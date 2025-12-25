using JWTLearning.Models;
using JWTLearning.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWTLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) { 
            _authService = authService;
        }

        [HttpPost("registeration")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model) {
            // to check if received values are valid based on RegisterModel data annotations
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await _authService.RegisterAsync(model);
            if (!result.IsAuthenticated) return BadRequest(result.Message);
            
            return Ok(new {
                Token = result.Token,
                Expireation = result.Expireation
            });
        }

        [HttpPost("Logging")]
        public async Task<IActionResult> LoginUser([FromBody] TokenReqestModel model){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            var result = await _authService.GetTokenAsync(model);

            if (!result.IsAuthenticated) {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model){
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.AddRoleAsync(model);
            return !string.IsNullOrEmpty(result) ? BadRequest(result) : Ok(model);
        }
        
    }
}
