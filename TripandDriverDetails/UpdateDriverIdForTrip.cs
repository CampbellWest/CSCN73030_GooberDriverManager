using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DriverManagement
{
    public class UpdateDriverIdForTrip
    {
        private static readonly string tripEndpoint = Config.TripEndpoint;
        private static readonly string apiKey = Config.SupabaseApiKey;

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
