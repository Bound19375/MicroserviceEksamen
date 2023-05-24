using System.Security.Claims;
using System.Text.Json.Nodes;
using Crosscutting.Configuration.JwtConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Admin.Controllers
{

    [Route("/API/BoundCore/Admin/")]
    public class JsonWebTokensController : Controller
    {
        private readonly IConfiguration _configuration;

        public JsonWebTokensController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class APIJWTModel
        {
            public string? RefreshToken { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("JwtRefreshAndGenerate")]
        [AllowAnonymous]
        public async Task<IActionResult> Generate([FromBody] APIJWTModel model)
        {
            try
            {
                if (model.Password != null)
                {
                    var claims = new List<Claim>
                    {
                        new("user", "user"),
                        new("admin", "admin")
                    };

                    var jwt = await JwtApiResponse.JwtRefreshAndGenerate(claims, _configuration, model.RefreshToken!, null!);

                    return await Task.FromResult(Ok(jwt));
                }

                return BadRequest("Unsuccessful Admin JWT Generation");

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
