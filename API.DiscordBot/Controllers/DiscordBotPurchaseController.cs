using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace API.DiscordBot.Controllers
{
    [Route("API/BoundCore/BoundBot/")]
    public class DiscordBotPurchaseController : Controller
    {
        private readonly IDiscordGatewayBuyHandlerImplementation _handler;
        public DiscordBotPurchaseController(IDiscordGatewayBuyHandlerImplementation handler)
        {
            _handler = handler;
        }

        [HttpPost("GrantLicenseOrder")]
        public async Task<IActionResult> PassToDB([FromBody] JsonObject json)
        {
            try
            {
                var result = await _handler.GrantLicense(json);

                if (result)
                    return Ok();

                return StatusCode(StatusCodes.Status500InternalServerError, "Serialization Failure");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
