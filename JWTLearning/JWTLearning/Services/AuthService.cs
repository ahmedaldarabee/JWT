using JWTLearning.Helpers;
using JWTLearning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// you needed to write it manually when you want to build refresh token generation!
using System.Security.Cryptography;

namespace JWTLearning.Services {

    public class AuthService : IAuthService {
        //readonly: same const idea but flexibility in define a value to it, where we can add value through initially like _userManager = userManager [ as a idea!! ] or in constructor

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        #region "Registration"

        // IOptions<JWT> jwt: to set jwt values from appsetting into JWT values
        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jwt,
            RoleManager<IdentityRole> roleManager) {

            _userManager = userManager;
            _roleManager = roleManager;
            this._jwt = jwt.Value; // .value that be main jwt object as one block!
        }

        //RegisterModel model: be as a data that send by user
        public async Task<AuthModel> RegisterAsync(RegisterModel model ) {

            if (
                await _userManager.FindByEmailAsync(model.Email) != null || 
                await _userManager.FindByNameAsync(model.UserName) != null)
                return new AuthModel { Message = "this user name or email is already exist!" };
            
            // create new user
            var user = new ApplicationUser {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) {
                var errors = string.Empty;

                foreach (var error in result.Errors) {
                    errors += $"{error.Description}, ";
                }

                return new AuthModel { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            // after we register user, now we want to create Token to it!
            var jwtToken = await CreateJWTToken(user);

            // return user with fully successes operations
            return new AuthModel {
                Email = user.Email,
                //Expireation = jwtToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" }, // This section: just for this case [ For testing not in real! ]
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                
                // convert token from object into JWT-string formate
                // [1] : to be valid show main JWT Component: header.payload/Claims.signature formate
                // [2] : to be testable in API's testing tools like Postman,Swagger,Browser, ... where these tools does't understand token as object!

                UserName = user.UserName,
            };
        }

        // i want from you to clarify next code:
        private async Task<JwtSecurityToken> CreateJWTToken(ApplicationUser user) {

            //userClaims: that be as user data
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles) {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }
        
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid",user.Id)

            }
            .Union(userClaims)
            .Union(userClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var singingCredentials = new SigningCredentials(symmetricSecurityKey,SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: singingCredentials
             );

            return jwtSecurityToken; // return token after secure it !
        }

        #endregion

        #region "Login"
        public async Task<AuthModel> GetTokenAsync(TokenReqestModel model) {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);

            if ( user is null || !await _userManager.CheckPasswordAsync(user, model.Password)){
                authModel.Message = "Email or Password is incorrected";
            }
            
            var jwtToken = await CreateJWTToken(user);
            var roleList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authModel.Email = user.Email;
            authModel.UserName = user.UserName;
            //authModel.Expireation = jwtToken.ValidTo;
            authModel.Roles = roleList.ToList();

            // if user have refresh token and be as active, so we needed to assign this refresh token into 
            // - our Auth Model , else we want to generate new refresh token and assign into our auth model!
            
            if (user.RefreshTokens.Any(token => token.isActive)){
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(token => token.isActive);
                authModel.RefreshToken = activeRefreshToken.Token;
                authModel.RefreshTokenExpireation = activeRefreshToken.ExpiresOn;
            }
            else{
                var newRefreshToken = GenerateRefreshToken();
                authModel.RefreshToken = newRefreshToken.Token;
                authModel.RefreshTokenExpireation = newRefreshToken.ExpiresOn;

                // now after we assigning new refresh token value,
                // we want to update our database
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }

            return authModel;
        }
        #endregion

        #region "User Roles"

        public async Task<string> AddRoleAsync(AddRoleModel model) {

            // get user data from database
            var user = await _userManager.FindByIdAsync(model.UserId);

            // get user role from database
            if (user is null || ! await _roleManager.RoleExistsAsync(model.Role)) {
                return "Invalid user id or role";
            }
            //IsInRoleAsync: to check if this user that have this role
            if (await _userManager.IsInRoleAsync(user, model.Role)) {
                return "user already assigned to this role";
            }

            var result = await _userManager.AddToRoleAsync(user,model.Role);

            return result.Succeeded ? string.Empty : "sorry, something went wrong!";
        }

        #endregion

        #region "Generate Refresh Token"
        // once you want to select class as a return type, the return type be as class fields        
        private RefreshToken GenerateRefreshToken(){
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);

            return new RefreshToken {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreateOn = DateTime.UtcNow
            };
        }
        #endregion
    }

    #region "Project Notes"
    /*
        another notes: 
            FindByEmailAsync: Gets a user by email; returns null if not found.
            FindByNameAsync: Gets a user by username; returns null if not found.
            CreateAsync: Creates a new user and saves it to the database.
            GetClaimsAsync: Retrieves all claims assigned to the user.
            CheckPasswordAsync: Verifies whether the provided password is correct.
            GetRolesAsync: Retrieves all roles assigned to the user.
            FindByIdAsync: Gets a user by ID; returns null if not found.
            RoleExistsAsync: Checks whether a role exists in the system.
            IsInRoleAsync: Checks whether the user belongs to a specific role.
            AddToRoleAsync: Assigns a role to the user.
     */
    #endregion
}
