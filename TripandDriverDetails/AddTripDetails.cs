using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace DriverManagement
{
    public class AddTripDetails
    {   
        // Supabase endpoint for Trip table
        private static readonly string tripEndpoint = "https://flpjmceqykalfwktysgi.supabase.co/rest/v1/Trip";
        // Supabase public API key, since database team is using supabase, we need this key to get authentication to access the database.
        private static readonly string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI"; //public api key for authentication

         private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            return client;
        }

        // Add trip details to the database
        public static async Task AddTripAsync(int id, int driverId, int ClientId, string startLocation, string endLocation, DateTime timeStarted, DateTime timeCompleted, string status)
        {
            using var client = CreateClient();

            var tripObj = new
            {
                id = id,
                driver_id = driverId,
                rider_id = ClientId,
                start_location = startLocation,
                end_location = endLocation,
                time_started = timeStarted.ToString("yyyy-MM-ddTHH:mm:ss"),
                time_completed = timeCompleted.ToString("yyyy-MM-ddTHH:mm:ss"),
                status = status
            };

            // Serialize object to JSON
            string json = JsonSerializer.Serialize(tripObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send POST request to Supabase
            var response = await client.PostAsync(tripEndpoint, content);

            if (response.IsSuccessStatusCode)
                Console.WriteLine($"Trip {id} added successfully.");
            else
                Console.WriteLine($"Failed to add trip: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
