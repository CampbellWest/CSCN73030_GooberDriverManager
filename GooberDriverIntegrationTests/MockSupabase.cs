using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System;

namespace DriverManagement.Tests.Integration
{
    public class MockSupabaseClient
    {
        public async Task<HttpResponseMessage> PostAsync(string endpoint, StringContent content)
        {
            var jsonString = await content.ReadAsStringAsync();
            var jsonObj = JsonSerializer.Deserialize<JsonElement>(jsonString);

            // --- DRIVER VALIDATION ---
            if (endpoint.Contains("Driver", StringComparison.OrdinalIgnoreCase))
            {
                // Check license number
                if (jsonObj.TryGetProperty("license_number", out var licenseNumberProp))
                {
                    int licenseNumber = licenseNumberProp.GetInt32();

                    if (licenseNumber == 12345)
                    {
                        return CreateError(HttpStatusCode.Conflict, "duplicate key");
                    }

                    if (licenseNumber < 0)
                    {
                        return CreateError(HttpStatusCode.BadRequest, "license number must be positive");
                    }
                }

                // Check rating
                if (jsonObj.TryGetProperty("rating", out var ratingProp))
                {
                    int rating = ratingProp.GetInt32();

                    if (rating < 1 || rating > 5)
                    {
                        return CreateError(HttpStatusCode.BadRequest, "invalid rating value");
                    }
                }

                // Check availability
                if (jsonObj.TryGetProperty("availability_status", out var availabilityProp))
                {
                    string availability = availabilityProp.GetString()!;
                    var valid = new[] { "available", "unavailable", "busy" };
                    if (!Array.Exists(valid, v => v.Equals(availability, StringComparison.OrdinalIgnoreCase)))
                    {
                        return CreateError(HttpStatusCode.BadRequest, "invalid availability status");
                    }
                }
            }

            // --- TRIP VALIDATION ---
            if (endpoint.Contains("Trip", StringComparison.OrdinalIgnoreCase))
            {
                string start = jsonObj.GetProperty("start_location").GetString() ?? "";
                string end = jsonObj.GetProperty("end_location").GetString() ?? "";

                DateTime startTime = DateTime.Parse(jsonObj.GetProperty("time_started").GetString() ?? "");
                DateTime endTime = DateTime.Parse(jsonObj.GetProperty("time_completed").GetString() ?? "");

                int driverId = jsonObj.GetProperty("driver_id").GetInt32();
                string status = jsonObj.GetProperty("status").GetString() ?? "";

                // Start location missing
                if (string.IsNullOrWhiteSpace(start))
                    return CreateError(HttpStatusCode.BadRequest, "null value for start location");

                // Same start and end location
                if (start.Equals(end, StringComparison.OrdinalIgnoreCase))
                    return CreateError(HttpStatusCode.BadRequest, "start and end locations cannot be same");

                // End time before start time
                if (endTime < startTime)
                    return CreateError(HttpStatusCode.BadRequest, "end time cannot be before start time");

                // Nonexistent driver
                if (driverId == 9999)
                    return CreateError(HttpStatusCode.BadRequest, "foreign key constraint (driver does not exist)");

                // Invalid status
                var validStatuses = new[] { "Completed", "Scheduled", "Cancelled" };
                if (!Array.Exists(validStatuses, s => s.Equals(status, StringComparison.OrdinalIgnoreCase)))
                    return CreateError(HttpStatusCode.BadRequest, "invalid status value");
            }

            // Default successful response
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }

        private HttpResponseMessage CreateError(HttpStatusCode code, string message)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(
                    $"{{\"error\":\"{message}\"}}",
                    Encoding.UTF8,
                    "application/json"
                )
            };
        }
    }
}
