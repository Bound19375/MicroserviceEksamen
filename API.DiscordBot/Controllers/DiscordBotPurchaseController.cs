using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;

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
                SellixPayloadNormal.Root data = JsonConvert.DeserializeObject<SellixPayloadNormal.Root>(jsonString);
                await _handler.WebShopGrantWallet(data);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
