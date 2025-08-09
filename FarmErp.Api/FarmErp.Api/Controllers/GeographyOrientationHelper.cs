using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace FarmErp.Api.GeometryUtils
{
    // This helper ensures that polygons are stored in the correct orientation for SQL Server geography type.
    public static class GeographyOrientationHelper
    {
        public static Polygon NormalizeForSqlGeography(Polygon poly)
        {
            if (poly == null) return poly!;

            var factory = poly.Factory;
            if (poly.SRID != 4326)
                factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            // Exterior ring -> CCW
            var shellRing = (LinearRing)poly.ExteriorRing;
            var shellCoords = shellRing.Coordinates;
            if (!Orientation.IsCCW(shellCoords))
                shellCoords = shellCoords.Reverse().ToArray();

            var shellSeq = factory.CoordinateSequenceFactory.Create(shellCoords);
            shellRing = factory.CreateLinearRing(shellSeq);

            // Holes -> CW
            var holes = new LinearRing[poly.NumInteriorRings];
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                var holeRing = (LinearRing)poly.GetInteriorRingN(i);
                var holeCoords = holeRing.Coordinates;
                if (Orientation.IsCCW(holeCoords)) // invert to CW
                    holeCoords = holeCoords.Reverse().ToArray();

                var holeSeq = factory.CoordinateSequenceFactory.Create(holeCoords);
                holes[i] = factory.CreateLinearRing(holeSeq);
            }

            var fixedPoly = factory.CreatePolygon(shellRing, holes);
            fixedPoly.SRID = 4326;
            return fixedPoly;
        }
    }
}