using JWTLearning.Models;

namespace JWTLearning.Services
{
    public interface IAuthService {
        Task<AuthModel> RegisterAsync(RegisterModel model);
    }
}
