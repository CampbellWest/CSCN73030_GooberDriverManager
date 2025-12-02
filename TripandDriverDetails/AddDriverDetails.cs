using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace DriverManagement
{
    public static class AddDriverDetails
    {
       // private static readonly string driverEndpoint = Config.DriverEndpoint;
        //private static readonly string apiKey = Config.SupabaseApiKey;
        private const string apiKey= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI";
        private const string driverEndpoint = "https://flpjmceqykalfwktysgi.supabase.co/rest/v1/Driver";

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
