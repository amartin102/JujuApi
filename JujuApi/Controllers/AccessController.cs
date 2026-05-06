using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.JwtSecutiry;

namespace JujuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly JwtExtensions _jwt;

        public AccessController(JwtExtensions jwt)
        {
            _jwt = jwt;
        }

        [HttpGet]
        [Route("GenerarToken")]
        public async Task<IActionResult> GenerarToken()
        {
            await Task.Delay(1000);
            return StatusCode(StatusCodes.Status200OK, new { isSuccess = true, token = _jwt.GenerateToken() });
        }

    }
}
