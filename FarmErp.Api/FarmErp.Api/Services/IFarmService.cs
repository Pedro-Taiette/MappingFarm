namespace FarmErp.Api.Services
{
    using FarmErp.Api.Dtos;
    using FarmErp.Api.Models;

    public interface IFarmService
    {
        Task<Farm> CreateAsync(PolygonDto dto, CancellationToken ct = default);
        Task<Farm?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<(int id, string name)>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(int id, PolygonDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
