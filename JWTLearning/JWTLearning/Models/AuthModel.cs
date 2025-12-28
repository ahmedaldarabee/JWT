using System.Text.Json.Serialization;

namespace JWTLearning.Models {

    public class AuthModel {
        public string Message { get; set; }
        
        public bool IsAuthenticated { get; set; }
        
        public string UserName { get; set; }
        
        public string Email { get; set; }

        public List<string> Roles { get; set; }
        
        public string Token { get; set; }
        
        [JsonIgnore]
        public string? RefreshToken { get; set; }

        // Why JsonIgnore,
        // that be to hold main refresh token into server to
        //  one: avoid hacking and build new access token
        //  two: avoid show it into json response

        public DateTime RefreshTokenExpireation { get; set; }
    }
}
