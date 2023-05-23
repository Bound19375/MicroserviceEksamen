using System.Security.Claims;
using System.Text.Json.Nodes;
using Crosscutting.Configuration.JwtConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Admin.Controllers
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
        public async Task<IActionResult> Generate([FromBody] JsonObject model)
        {
            const string adminRoleId = "860603777790771211";
            const string staffRoleId = "860628656259203092";

            var claims = new List<Claim>
            {
                new("role", "User"),
                new("role", "Admin")
            };


            var jwt = await JwtApiResponse.JwtRefreshAndGenerate(claims, _configuration, model[0]?.ToString() ?? string.Empty, null!);

            return await Task.FromResult(Ok(jwt));
        }
    }
}
