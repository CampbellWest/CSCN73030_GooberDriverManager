using System.Text.Json;
using DemoApi.Resources;

namespace Generators;

public static class CarGenerator
{
    private static readonly Random rand = new();

    private static readonly string filePath =
        Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CarMakeModel.json");

    private static readonly Dictionary<string, Dictionary<string, CarInformation>> carData = LoadCarData();

    private static Dictionary<string, Dictionary<string, CarInformation>> LoadCarData()
    {
        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CarInformation>>>(json)
               ?? new Dictionary<string, Dictionary<string, CarInformation>>();
    }

    public static (string make, string model, int year, int seats) GetRandomCar()
    {
        var makes = carData.Keys.ToList();
        var make = makes[rand.Next(makes.Count)];

        var models = carData[make].Keys.ToList();
        var model = models[rand.Next(models.Count)];

        var seats = carData[make][model].seats;

        var year = rand.Next(2000, 2025);

        return (make, model, year, seats);
    }
}