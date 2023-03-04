using Auth.Application.Interface;
using Crosscut;
using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Crosscutting;

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

        [HttpGet("JwtGenerate/{hwid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Generate(string hwid)
        {
            var claims = new List<Claim>
            {
                new Claim("hwid", hwid),
                new Claim("role", "User"),
            };


            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signingCredentials
            );;

            return await Task.FromResult(Ok(new JwtSecurityTokenHandler().WriteToken(token)));
        }

        [HttpPost("Authenticate")]
        [Authorize(Roles = "User")]
        [Authorize(Policy = "hwid")]
        public async  Task<IActionResult> Auth([FromBody] AuthModelDTO model)
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
