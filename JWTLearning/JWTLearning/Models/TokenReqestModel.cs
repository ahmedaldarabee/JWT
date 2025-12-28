using System.ComponentModel.DataAnnotations;

namespace JWTLearning.Models
{
    public class TokenReqestModel {

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
