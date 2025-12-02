using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DriverManagement
{
    public class UpdateDriverIdForTrip
    {
        //private static readonly string tripEndpoint = Config.TripEndpoint;
        //private static readonly string apiKey = Config.SupabaseApiKey;
        private const string apiKey= "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI";
        private const string tripEndpoint = "https://flpjmceqykalfwktysgi.supabase.co/rest/v1/Trip";

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            return client;
        }

        
        public static async Task UpdateDriverIdAsync(int tripId, int driverId)
        {
            using var client = CreateClient();

            var payload = new
            {
                driver_id = driverId
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{tripEndpoint}?id=eq.{tripId}")
            {
                Content = content
            };

            var response = await client.PatchAsync(
                $"{tripEndpoint}?id=eq.{tripId}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update driver for trip {tripId}: {errorMsg}");
            }
        }
    }
}
