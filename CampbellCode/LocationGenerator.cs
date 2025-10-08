namespace Generators;

public static class LocationGenerator
{
    private static readonly Random rand = new();
    private const double EarthRadiusMeters = 6371e3;
    
    private static double WaterlooLatitude = 43.461655;
    private static double WaterlooLongitude = -80.521417;
    private static int MeterFromCoordinateCenter = 7500;
    
    public static (double lat, double lon) GenerateLocation()
    {
        double radiusInRadians = MeterFromCoordinateCenter / EarthRadiusMeters;

        double randomDistanceFactor = Math.Sqrt(rand.NextDouble());
        double randomAngle = 2 * Math.PI * rand.NextDouble();

        double offsetX = radiusInRadians * randomDistanceFactor * Math.Cos(randomAngle);
        double offsetY = radiusInRadians * randomDistanceFactor * Math.Sin(randomAngle);

        double cosLat = Math.Cos(WaterlooLatitude * Math.PI / 180);
        double adjustedX = offsetX / cosLat;

        double newLat = WaterlooLatitude + (offsetY * 180 / Math.PI);
        double newLon = WaterlooLongitude + (adjustedX * 180 / Math.PI);

        return (newLat, newLon);
    }
}