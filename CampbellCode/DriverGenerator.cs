using DemoApi.Resources;

namespace Generators;

public static class DriverGenerator
{
    public static ConfirmDriverRequest GenerateDriver() // maybe take in number to generate
    {
        var (make, model, year, seats) = CarGenerator.GetRandomCar();
        
        var (latitude, longitude) = LocationGenerator.GenerateLocation();

        var carInfo = new ConfirmDriverRequest
        {
            DriverName = NameGenerator.GenerateName(),
            CarInfo = $"{year} {make} {model}: {seats} seats",
            LicensePlate = LicensePlateGenerator.GeneratePlate(), 
            CurrentLocation = new LocationData()
            {
                Latitude = Math.Round(latitude, 5),
                Longitude = Math.Round(longitude, 5)
            }
        };
        
        return carInfo;
    }
}