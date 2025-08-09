using FarmErp.Api.Dtos;
using FarmErp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmErp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmsController : ControllerBase
    {
        private readonly IFarmService _service;

        public FarmsController(IFarmService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PolygonDto dto, CancellationToken ct)
        {
            try
            {
                var farm = await _service.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = farm.Id }, new { id = farm.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var farm = await _service.GetByIdAsync(id, ct);
            if (farm is null) return NotFound();

            var coords = farm.Area.Coordinates.Select(c => new { lat = c.Y, lng = c.X }).ToList();
            return Ok(new { id = farm.Id, name = farm.Name, coordinates = coords });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _service.GetAllAsync(ct);
            return Ok(list.Select(i => new { id = i.id, name = i.name }));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PolygonDto dto, CancellationToken ct)
        {
            try
            {
                await _service.UpdateAsync(id, dto, ct);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
