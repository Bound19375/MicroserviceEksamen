using Crosscutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.DiscordBot.Controllers {

    [Route("API/DiscordBot/")]
    public class JsonWebTokensController : Controller 
    {
        private readonly IConfiguration _configuration;

        public JsonWebTokensController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("JwtGenerate")]
        [AllowAnonymous]
        public async Task<IActionResult> Generate([FromBody] DiscordModelDTO model) 
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

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signingCredentials
            );

            return await Task.FromResult(Ok(new JwtSecurityTokenHandler().WriteToken(token)));
        }
    }
}
