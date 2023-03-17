using DiscordBot.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.DiscordBot.Controllers
{
    [Route("API/DiscordBot/Query")]
    public class DiscordBotQueryController : Controller
    {
        private readonly IDiscordBotQueryImplementation _discord;
        public DiscordBotQueryController(IDiscordBotQueryImplementation discord)
        {
            _discord = discord;
        }

        [HttpPost("Authenticate")]
        [Authorize(Roles = "admin")]
        [Authorize(Roles = "staff")]
        [HttpGet("CheckDB/{username}/{id}")]
        public async Task<IActionResult> CheckDB(string username, string id)
        {
            try
            {
                var result = await _discord.CheckDB(username, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Authenticate")]
        [Authorize(Roles = "User")]
        [HttpGet("CheckMe/{username}/{id}")]
        public async Task<IActionResult> CheckMe(string username, string id)
        {
            try
            {
                var result = await _discord.CheckDB(username, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
