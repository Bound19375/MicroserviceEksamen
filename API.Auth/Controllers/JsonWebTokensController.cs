using Crosscutting.Configuration.JwtConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Auth.Controllers
{
    [Route("/API/BoundCore/Auth")]
    public class JsonWebTokensController : Controller
    {
        private readonly IConfiguration _configuration;
        public JsonWebTokensController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class JwtModel
        {
            public string RefreshToken { get; set; } = null!;
            public string? Hwid { get; set; }
        }

        [HttpPost("JwtRefreshAndGenerate")]
        [AllowAnonymous]
        public async Task<IActionResult> Generate([FromBody] JwtModel body)
        {
            var claims = new List<Claim>
            {
                new Claim("hwid", body.Hwid!),
                new Claim("role", "User"),
            };

            var jwt = await JwtApiResponse.JwtRefreshAndGenerate(claims, _configuration, body.RefreshToken, null!);

            return await Task.FromResult(Ok(jwt));
        }
    }
}
