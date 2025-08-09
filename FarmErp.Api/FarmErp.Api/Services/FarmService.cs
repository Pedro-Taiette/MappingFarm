using System.Linq;
using FarmErp.Api.Data;
using FarmErp.Api.Dtos;
using FarmErp.Api.GeometryUtils;
using FarmErp.Api.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FarmErp.Api.Services
{
    public class FarmService : IFarmService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly GeometryFactory _geometryFactory;

        public FarmService(ApplicationDbContext ctx)
        {
            _ctx = ctx;
            _geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        }

        public async Task<Farm> CreateAsync(PolygonDto dto, CancellationToken ct = default)
        {
            var polygon = BuildPolygonFromDto(dto);
            var farm = new Farm
            {
                Name = dto.Name ?? "Unnamed Farm",
                Area = polygon
            };
            _ctx.Farms.Add(farm);
            await _ctx.SaveChangesAsync(ct);
            return farm;
        }

        public async Task<Farm?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _ctx.Farms.FindAsync(new object?[] { id }, ct);
        }

        public async Task<List<(int id, string name)>> GetAllAsync(CancellationToken ct = default)
        {
            return await _ctx.Farms
                .AsNoTracking()
                .Select(f => new ValueTuple<int, string>(f.Id, f.Name))
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(int id, PolygonDto dto, CancellationToken ct = default)
        {
            var farm = await _ctx.Farms.FindAsync(new object?[] { id }, ct);
            if (farm is null) throw new KeyNotFoundException("Farm not found.");

            var polygon = BuildPolygonFromDto(dto);

            farm.Name = dto.Name ?? farm.Name;
            farm.Area = polygon;

            await _ctx.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var farm = await _ctx.Farms.FindAsync(new object?[] { id }, ct);
            if (farm is null) return;

            _ctx.Farms.Remove(farm);
            await _ctx.SaveChangesAsync(ct);
        }

        private Polygon BuildPolygonFromDto(PolygonDto dto)
        {
            if (dto is null || dto.Coordinates is null || dto.Coordinates.Count < 3)
                throw new ArgumentException("Need at least 3 coordinates to form a polygon.");

            var coords = dto.Coordinates.Select(c => new Coordinate(c.Lng, c.Lat)).ToList();

            if (!coords.First().Equals2D(coords.Last()))
                coords.Add(new Coordinate(coords.First()));

            var polygon = _geometryFactory.CreatePolygon(coords.ToArray());
            polygon.SRID = 4326;

            if (!polygon.IsValid)
                throw new ArgumentException("Invalid polygon (self-intersection or other geometry error).");

            polygon = GeographyOrientationHelper.NormalizeForSqlGeography(polygon);
            return polygon;
        }
    }
}
