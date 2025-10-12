using System.Text.Json;
using System.Xml;
using DemoApi.Resources;

namespace DriverFinder;

public class DriverFinder
{
    private const double EarthRadiusMeters = 6371e3;
    
    public static double CalculateDistance(double lat1Degrees, double lon1Degrees, double lat2Degrees, double lon2Degrees)
    {
       
        // convert lat/long from degrees to radians
        double latRad = ToRadians(lat1Degrees);
        double lonRad = ToRadians(lon1Degrees);
        double lat2Rad = ToRadians(lat2Degrees);
        double lon2Rad = ToRadians(lon2Degrees);
        
        // difference between lats and longs
        double difLat = latRad -lat2Rad;
        double difLon = lonRad - lon2Rad;
        
        //Haversine formula
        double a = Math.Pow(Math.Sin(difLat / 2), 2) +
                   Math.Cos(latRad) * Math.Cos(lat2Rad) *
                   Math.Pow(Math.Sin(difLon / 2), 2);
        
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        double distance = c * EarthRadiusMeters;
        
        return (distance);
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    // filter drivers who do not match ride request details
    private static ConfirmDriverRequest[] FilterDrivers(RideRequest rideRequest, ConfirmDriverRequest[] driverList)
    {
        bool isPetFriendly = rideRequest.RideInformation.PetFriendly;
        string carType = rideRequest.RideInformation.CarType;
        var validDrivers = new List<ConfirmDriverRequest>();
        
        foreach (var driver in driverList)
        {
            
            if (string.IsNullOrEmpty(driver.CarInfo)) continue;

            // Deserialize the CarInfo JSON string into a CarInformation object
            CarInformation? carInfo;
            try
            {
                carInfo = JsonSerializer.Deserialize<CarInformation>(driver.CarInfo);
            }
            catch
            {
                continue; // skip driver if carInfo is invalid
            }

            if (carInfo == null) continue;

            if (carInfo.IsPetFriendly == isPetFriendly && carInfo.CarType == carType)
            {
                validDrivers.Add(driver);
            }
        }

        return validDrivers.ToArray();
    }
    private static ConfirmDriverRequest FindClosestDriver(ConfirmDriverRequest[] driverList, LocationData pickupLocation)
    {
        
        double shortestDriverDistance = 0;
        ConfirmDriverRequest? closestDriver = null;

        
        //loop through array, calculate driver distance from 
        foreach (var driver in driverList)
        {
            double driveLat = driver.CurrentLocation.Latitude;
            double driveLon = driver.CurrentLocation.Longitude;
            double  currentDriverDistance = CalculateDistance(driveLat, driveLon, 
                pickupLocation.Latitude, pickupLocation.Longitude);

            if (currentDriverDistance < shortestDriverDistance | shortestDriverDistance == 0)
            {
                shortestDriverDistance =  currentDriverDistance;
                closestDriver = driver;
                
            }
        }

        return closestDriver;
    }
    
}

