using NetTopologySuite.Geometries;

namespace FarmErp.Api.Models
{
    public class Farm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // SRID 4326 (WGS84) --- stored as geography
        public Polygon Area { get; set; } = default!;
    }
}
