using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JWTLearning.Models
{
    public class ApplicationUser: IdentityUser {

        [Required,MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        // why we build here list of refresh token?
            //because each user that may be have multi session where
            // may be use mobile or laptop to another device to login in this application so for every device that have token also may be user try to login with difference browser also the same idea where every browser that have a difference token!
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
}
