using DemoApi.Resources;
using System.Security.Cryptography;

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
            },
            DriverId = GenerateDriverId(),
            
        };

        return carInfo;
    }

    private static int GenerateDriverId()
    {
        byte[] randomBytes = new byte[4]; 
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        int secureId = BitConverter.ToInt32(randomBytes, 0);
        if (secureId < 0)
        {
            secureId = -secureId;
        }
        return secureId;
    }
}