namespace FarmErp.Api.Dtos
{
    public class PolygonDto
    {
        public string? Name { get; set; }
        public List<LatLngDto> Coordinates { get; set; } = new();
    }

    public class LatLngDto
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
