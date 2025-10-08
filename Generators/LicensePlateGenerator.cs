using System;
using System.Text;

namespace Generators;

public static class LicensePlateGenerator
{
    private static readonly Random rand = new();

    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    public static string GeneratePlate()
    {
        StringBuilder plate = new StringBuilder();

        for (int i = 0; i < 3; i++)
            plate.Append(Letters[rand.Next(Letters.Length)]);

        plate.Append(' '); 
        
        plate.Append(Letters[rand.Next(Letters.Length)]);
        
        for (int i = 0; i < 3; i++)
            plate.Append(rand.Next(10)); 

        return plate.ToString();
    }
}