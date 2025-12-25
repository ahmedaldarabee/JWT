using System.ComponentModel.DataAnnotations;

namespace JWTLearning.Models
{
    public class TokenReqestModel {

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
