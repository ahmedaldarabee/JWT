using JWTLearning.Models;

namespace JWTLearning.Services
{
    public interface IAuthService {
        Task<AuthModel> RegisterAsync(RegisterModel model);

        Task<AuthModel> GetTokenAsync(TokenReqestModel model);

        Task<string> AddRoleAsync(AddRoleModel model);
    }
}
