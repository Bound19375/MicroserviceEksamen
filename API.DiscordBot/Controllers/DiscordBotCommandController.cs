using Crosscutting;
using DiscordBot.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace API.DiscordBot.Controllers
{
    [Route("API/DiscordBot/Command")]
    public class DiscordBotCommandController : Controller
    {
        private readonly IDiscordBotCommandImplementation _discord;
        public DiscordBotCommandController(IDiscordBotCommandImplementation discord)
        {
            _discord = discord;
        }

        [HttpPost("StaffLicense")]
        public async Task<IActionResult> StaffLicense([FromBody] DiscordModelDTO model)
        {
            try
            {
                var result = await _discord.GetStaffLicense(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateDiscord")]
        public async Task<IActionResult> UpdateDiscordAndRole([FromBody] DiscordModelDTO model)
        {
            try
            {
                var result = await _discord.UpdateDiscordAndRole(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateHwid")]
        public async Task<IActionResult> UpdateHWID([FromBody] DiscordModelDTO model)
        {
            try
            {
                var result = await _discord.UpdateHWID(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
