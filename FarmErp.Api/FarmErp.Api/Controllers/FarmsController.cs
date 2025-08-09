using FarmErp.Api.Data;
using FarmErp.Api.Dtos;
using FarmErp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FarmErp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmsController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        private readonly GeometryFactory _geometryFactory;

        public FarmsController(ApplicationDbContext ctx)
        {
            _ctx = ctx;
            // Ensure SRID 4326
            _geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PolygonDto dto)
        {
            if (dto is null || dto.Coordinates is null || dto.Coordinates.Count < 3)
                return BadRequest("Need at least 3 coordinates to form a polygon.");

            var coords = dto.Coordinates.Select(c => new Coordinate(c.Lng, c.Lat)).ToList();

            // Ensure polygon closed (first == last)
            if (!coords.First().Equals2D(coords.Last()))
            {
                coords.Add(new Coordinate(coords.First()));
            }

            var polygon = _geometryFactory.CreatePolygon(coords.ToArray());

            if (!polygon.IsValid)
            {
                return BadRequest("Invalid polygon (self-intersection or other geometry error).");
            }

            var farm = new Farm
            {
                Name = dto.Name ?? "Unnamed Farm",
                Area = polygon
            };

            _ctx.Farms.Add(farm);
            await _ctx.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = farm.Id }, new { id = farm.Id });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var farm = await _ctx.Farms.FindAsync(id);
            if (farm is null) return NotFound();

            var coords = farm.Area.Coordinates.Select(c => new { lat = c.Y, lng = c.X }).ToList();
            return Ok(new { id = farm.Id, name = farm.Name, coordinates = coords });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _ctx.Farms
                .AsNoTracking()
                .Select(f => new
                {
                    id = f.Id,
                    name = f.Name
                }).ToListAsync();
            return Ok(list);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PolygonDto dto)
        {
            var farm = await _ctx.Farms.FindAsync(id);
            if (farm is null) return NotFound();

            if (dto.Coordinates is null || dto.Coordinates.Count < 3)
                return BadRequest("Need at least 3 coordinates to form a polygon.");

            var coords = dto.Coordinates.Select(c => new Coordinate(c.Lng, c.Lat)).ToList();
            if (!coords.First().Equals2D(coords.Last()))
                coords.Add(new Coordinate(coords.First()));

            var polygon = _geometryFactory.CreatePolygon(coords.ToArray());
            if (!polygon.IsValid) return BadRequest("Invalid polygon.");

            farm.Name = dto.Name ?? farm.Name;
            farm.Area = polygon;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var farm = await _ctx.Farms.FindAsync(id);
            if (farm is null) return NotFound();

            _ctx.Farms.Remove(farm);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
