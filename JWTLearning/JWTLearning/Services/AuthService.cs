using JWTLearning.Helpers;
using JWTLearning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTLearning.Services {

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;

        // IOptions<JWT> jwt: to set jwt values from appsetting into JWT values
        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt) {
            _userManager = userManager;
            this._jwt = jwt.Value; // .value that be main jwt object as one block!
        }

        //RegisterModel model: be as a data that send by user
        public async Task<AuthModel> RegisterAsync(RegisterModel model ) {

            if ( await _userManager.FindByEmailAsync(model.Email) != null){
                return new AuthModel { Message = "this email is already exist!" };
            }

            if (await _userManager.FindByNameAsync(model.UserName) != null){
                return new AuthModel { Message = "this user name is already exist!" };
            }

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

            // return user with fully successed operations
            return new AuthModel {
                Email = user.Email,
                Expireation = jwtToken.ValidTo,
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
            
            
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles) {
                roleClaims.Add(new Claim("roles",role));
            }
        
            var claims = new[]
            {
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
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: singingCredentials
             );

            return jwtSecurityToken; // return token after secure it !

        }
    }
}
