using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWTLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SecuredController : ControllerBase {

        [HttpGet("TokenTesting")]
        public IActionResult GetData(){
            return Ok("token worked successfully!");
        }
    }
}
