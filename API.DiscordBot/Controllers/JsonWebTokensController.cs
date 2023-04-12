using Crosscutting;
using Crosscutting.Configuration.JwtConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.DiscordBot.Controllers
{

    [Route("API/DiscordBot/")]
    public class JsonWebTokensController : Controller
    {
        private readonly IConfiguration _configuration;

        public JsonWebTokensController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("JwtRefreshAndGenerate")]
        [AllowAnonymous]
        public async Task<IActionResult> Generate([FromBody] DiscordModelDto model)
        {
            const string adminRoleId = "860603777790771211";
            const string staffRoleId = "860628656259203092";

            var claims = new List<Claim>
            {
                new("role", "User"),
            };

            var roles = new HashSet<string>(model.Roles!);

            if (roles.Contains(adminRoleId) || roles.Contains("Mod"))
            {
                claims.Add(new Claim("admin", "admin"));
            }

            if (roles.Contains(staffRoleId) || roles.Contains("Staff"))
            {
                claims.Add(new Claim("staff", "staff"));
            }

            var jwt = await JwtApiResponse.JwtRefreshAndGenerate(claims, _configuration, model.RefreshToken, model.DiscordId);

            return await Task.FromResult(Ok(jwt));
        }
    }
}
