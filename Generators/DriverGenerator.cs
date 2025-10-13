using DemoApi.Resources;

namespace Generators;

public static class DriverGenerator
{
    public static ConfirmDriverRequest GenerateDriver() // maybe take in number to generate
    {
        var (make, model, year, seats) = CarGenerator.GetRandomCar();
        var (latitude, longitude) = LocationGenerator.GenerateLocation();

        var carType = "";

        if (seats <= 5)
            carType = "X";
        else
            carType = "XL";
        
        bool isPetFriendly = new Random().Next(1, 11) > 7;

        var carInfo = new ConfirmDriverRequest
        {
            DriverName = NameGenerator.GenerateName(),
            LicensePlate = LicensePlateGenerator.GeneratePlate(),
            CarDescription = $"{year} {make} {model}",
            CarInfo = new CarInformation()
            {
                seats = seats,
                IsPetFriendly = isPetFriendly,
                CarType = carType,
            },
            CurrentLocation = new LocationData()
            {
                Latitude = Math.Round(latitude, 5),
                Longitude = Math.Round(longitude, 5)
            }
        };

        return carInfo;
    }
}