using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EchoTravel.Api.Services;

namespace EchoTravel.Api.Controllers
{
    [ApiController]
    [Route("api/ingest")]
    public class IngestController : ControllerBase
    {
        private readonly AnnouncementsService _announcementsService;

        public IngestController(AnnouncementsService announcementsService)
        {
            _announcementsService = announcementsService;
        }

        [HttpPost("{trainId}")]
        public async Task<IActionResult> Post(string trainId, [FromQuery] string? car, [FromQuery] string to = "en", CancellationToken ct = default)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
            {
                return BadRequest("No file");
            }

            await using var stream = file.OpenReadStream();
            await _announcementsService.ProcessAsync(trainId, stream, file.FileName, file.ContentType ?? "audio/wav", to, ct);

            return Ok(new { ok = true });
        }
    }
}
