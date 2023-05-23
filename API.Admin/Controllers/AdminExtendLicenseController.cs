using System.Net;
using Admin.Application.Interface;
using Crosscutting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Admin.Controllers
{
    [Route("/API/BoundCore/Admin/")]
    public class AdminExtendLicenseController : Controller
    {
        private readonly IAdminExtendLicensesImplementation _license;
        public AdminExtendLicenseController(IAdminExtendLicensesImplementation license)
        {
            _license = license;
        }

        public class ExtendLicenseModel
        {
            public int MinutesToExtend { get; set; }
            public string? DiscordId { get; set; }
        }

        [HttpPost("ExtendLicense")]
        [Authorize(Policy = "admin")]
        public async Task<IActionResult> ExtendLicense([FromBody] ExtendLicenseModel model)
        {
            try
            {
                var result = await _license.ExtendLicense(model.MinutesToExtend, model.DiscordId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
