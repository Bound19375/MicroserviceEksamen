using Auth.Application.Interface;
using Crosscutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Auth.Controllers
{
    [Route("/API/BoundCore/Auth")]
    public class AuthController : Controller
    {
        private readonly IAuthImplementation _auth;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthImplementation auth, IConfiguration configuration)
        {
            _auth = auth;
            _configuration = configuration;
        }

        [Authorize(Policy = "user")]
        [Authorize(Policy = "hwid")]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Auth([FromBody] AuthModelDTO model)
        {
            try
            {
                var result = await _auth.Auth(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message == "No Active Licenses")
                    return StatusCode(403, ex.Message);
                else
                    return StatusCode(500, ex.Message);
            }
        }
    }
}
