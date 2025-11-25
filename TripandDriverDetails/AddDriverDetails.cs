using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace DriverManagement
{
    public static class AddDriverDetails
    {
        private static readonly string driverEndpoint = Config.DriverEndpoint;
        private static readonly string apiKey = Config.SupabaseApiKey;

         private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            return client;
        }

        // Add driver details to the database
        public static async Task AddDriverAsync(int id, string accountId, int rating, string availability, int licenseNumber, string? currentLocation)
        {
            using var client = CreateClient();

            var driverObj = new
            {
                id = id,
                account_id = accountId,
                rating = rating,
                availability_status = availability,
                license_number = licenseNumber,
                current_location = currentLocation
            };
            // Serialize object to JSON
            string json = JsonSerializer.Serialize(driverObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send POST request to Supabase
            var response = await client.PostAsync(driverEndpoint, content);

             if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to add driver: {errorMsg}");
            }
            // if (response.IsSuccessStatusCode)
            //     Console.WriteLine($"Driver {accountId} added successfully.");
            // else
            //     Console.WriteLine($"Failed to add driver: {await response.Content.ReadAsStringAsync()}");
        }
    
    }
}
