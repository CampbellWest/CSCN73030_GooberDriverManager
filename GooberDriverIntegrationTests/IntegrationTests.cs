using Xunit;
using DriverFinder;
using DemoApi.Resources;
using System.Text.Json;
using DriverManagement;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;

namespace DriverManagement.Tests.Integration
{
    public class IntegrationTests
    {
        private readonly MockSupabaseClient _mockClient = new();

        private async Task AddDriverAsync(int id, string accountId, int rating, string availability, int licenseNumber, string? currentLocation)
        {
            var driverObj = new
            {
                id = id,
                account_id = accountId,
                rating = rating,
                availability_status = availability,
                license_number = licenseNumber,
                current_location = currentLocation
            };

            string json = JsonSerializer.Serialize(driverObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _mockClient.PostAsync("https://mock.supabase/Driver", content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to add driver: {errorMsg}");
            }
        }

        private async Task AddTripAsync(int id, int driverId, int clientId, string startLocation, string endLocation, DateTime timeStarted, DateTime timeCompleted, string status)
        {
            var tripObj = new
            {
                id = id,
                driver_id = driverId,
                rider_id = clientId,
                start_location = startLocation,
                end_location = endLocation,
                time_started = timeStarted.ToString("yyyy-MM-ddTHH:mm:ss"),
                time_completed = timeCompleted.ToString("yyyy-MM-ddTHH:mm:ss"),
                status = status
            };

            string json = JsonSerializer.Serialize(tripObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _mockClient.PostAsync("https://mock.supabase/Trip", content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to add trip: {errorMsg}");
            }
        }

        [Fact]
        public async Task AddDriver_ShouldSucceed_WithValidInput()
        {
            var exception = await Record.ExceptionAsync(() =>
                AddDriverAsync(1, "test-account-id", 5, "available", 11111, "Test Location")
            );

            Assert.Null(exception);
        }

        [Fact]
        public async Task AddDriver_ShouldFail_WithDuplicateLicenseNumber()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                AddDriverAsync(2, "test-account-id", 5, "available", 12345, "Test Location")
            );

            Assert.Contains("duplicate key", ex.Message);
        }

        [Fact]
        public async Task AddTrip_ShouldFail_WhenStartLocationMissing()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                AddTripAsync(1, 1, 1, "", "End", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10), "Completed")
            );

            Assert.Contains("null value", ex.Message);
        }

        [Fact]
        public async Task AddDriver_ShouldFail_WhenRatingIsOutOfRange()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                AddDriverAsync(3, "acc-001", 10, "available", 98765, "Toronto")
            );

            Assert.Contains("rating", ex.Message);
        }

        [Fact]
        public async Task AddDriver_ShouldFail_WhenLicenseNumberIsNegative()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                AddDriverAsync(4, "acc-002", 4, "available", -111, "Waterloo")
            );

            Assert.Contains("license", ex.Message);
        }
        [Fact]
        public async Task AddTrip_ShouldFail_WhenDriverDoesNotExist()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                AddTripAsync(2, 9999, 10, "Kitchener", "Cambridge", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(15), "Scheduled")
            );

            Assert.Contains("foreign key", ex.Message);
        }

        [Fact]
        public async Task AddTrip_ShouldFail_WhenEndTimeBeforeStartTime()
        {
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                AddTripAsync(3, 1, 2, "Start", "End", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-5), "Completed")
            );

            Assert.Contains("time", ex.Message);
        }

        [Fact]
        public async Task AddDriver_ShouldSucceed_WithSpecialCharactersInLocation()
        {
            var exception = await Record.ExceptionAsync(() =>
                AddDriverAsync(5, "acc-003", 5, "available", 55555, "Québec City 🌆")
            );

            Assert.Null(exception);
        }


    }
}
