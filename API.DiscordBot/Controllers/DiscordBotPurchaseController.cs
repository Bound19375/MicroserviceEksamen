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
                string? jsonString = json.ToString() ?? throw new Exception("JsonObject Is Invalid!");
                SellixPayloadNormal.Root data = JsonConvert.DeserializeObject<SellixPayloadNormal.Root>(jsonString) ?? throw new Exception("Invalid Json Object");
                await _handler.GrantLicense(data);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
