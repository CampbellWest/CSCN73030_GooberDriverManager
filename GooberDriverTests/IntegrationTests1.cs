using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DemoApi;
using DemoApi.Resources;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DemoApi.GooberDriverTests
{
    // Integration tests against the DemoApi itself
    public class DriverManagerControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DriverManagerControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAvailableDrivers_NoDrivers_ReturnsNotFound()
        {
            var clearResponse = await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");
            clearResponse.EnsureSuccessStatusCode();

            var response = await _client.GetAsync("/api/DriverManager/GetAvailableDrivers");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("No available drivers at the moment.", message);
        }

        [Fact]
        public async Task GetAvailableDrivers_WhenDriversExist_ReturnsOkWithAvailableDrivers()
        {
            await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");

            var generateResponse = await _client.GetAsync("/api/DriverManager/GenerateMoreDriversTest");
            generateResponse.EnsureSuccessStatusCode();

            var response = await _client.GetAsync("/api/DriverManager/GetAvailableDrivers");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var drivers = await response.Content.ReadFromJsonAsync<List<ConfirmDriverRequest>>();

            Assert.NotNull(drivers);
            Assert.NotEmpty(drivers!);
            Assert.All(drivers!, d => Assert.True(d.IsAvailable));
        }

        [Fact]
        public async Task UpdateDriverAvailability_UnknownDriver_ReturnsNotFound()
        {
            await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");

            int bogusDriverId = 999_999;

            var response = await _client.PutAsync(
                $"/api/DriverManager/UpdateDriverAvailability?driverId={bogusDriverId}&isAvailable=false",
                content: null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Driver with ID {bogusDriverId} not found.", body);
        }

        [Fact]
        public async Task RequestDriver_UnknownTrip_ReturnsNotFound()
        {
            var payload = new TripIdRequest { TripId = 99999999 }; // Non-existent trip ID

            var response = await _client.PostAsJsonAsync("/api/DriverManager/RequestDriver", payload);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    // Integration tests directly against Supabase
    public class IntegrationTests
    {
        private const string SupabaseApiKey =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI";

        private const string DriverEndpoint = "https://flpjmceqykalfwktysgi.supabase.co/rest/v1/Driver";
        private const string TripEndpoint = "https://flpjmceqykalfwktysgi.supabase.co/rest/v1/Trip";

        private readonly HttpClient _client;

        public IntegrationTests()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseApiKey);
            _client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }

        private StringContent JsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task DeleteDriverAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{DriverEndpoint}?id=eq.{id}");
            request.Headers.Add("apikey", SupabaseApiKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseApiKey}");

            _ = await _client.SendAsync(request);
        }

        private async Task DeleteTripAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{TripEndpoint}?id=eq.{id}");
            request.Headers.Add("apikey", SupabaseApiKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseApiKey}");

            _ = await _client.SendAsync(request);
        }

        private async Task<string> GetDriverAsync(int licenseNumber)
        {
            var response = await _client.GetAsync($"{DriverEndpoint}?license_number=eq.{licenseNumber}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetTripAsync(int tripId)
        {
            var response = await _client.GetAsync($"{TripEndpoint}?id=eq.{tripId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task AddDriver_ShouldCreateDriver()
        {
            int driverId = new Random().Next(10000, 99999);
            int licenseNumber = new Random().Next(100000, 999999);

            try
            {
                var payload = new
                {
                    id = driverId,
                    account_id = "142dc6ca-7d33-47ea-9b1d-53ac25c9b15f",
                    rating = 5,
                    availability_status = "available",
                    license_number = licenseNumber,
                    current_location = "Testlocation"
                };

                var response = await _client.PostAsync(DriverEndpoint, JsonContent(payload));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Supabase Driver Error: " + error);
                }

                Assert.True(response.IsSuccessStatusCode, "Driver was NOT created in Supabase");

                var driverData = await GetDriverAsync(licenseNumber);
                Assert.Contains("Testlocation", driverData);
                Assert.Contains(licenseNumber.ToString(), driverData);
            }
            finally
            {
                await DeleteDriverAsync(driverId);
            }
        }

        [Fact]
        public async Task AddDriver_ShouldSucceed_WithSpecialCharactersInLocation()
        {
            int driverId = new Random().Next(10000, 99999);
            int licenseNumber = new Random().Next(100000, 999999);

            try
            {
                var payload = new
                {
                    id = driverId,
                    account_id = "142dc6ca-7d33-47ea-9b1d-53ac25c9b15f",
                    rating = 5,
                    availability_status = "available",
                    license_number = licenseNumber,
                    current_location = "Québec City 🌆"
                };

                var response = await _client.PostAsync(DriverEndpoint, JsonContent(payload));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Supabase Driver Error: " + error);
                }

                Assert.True(response.IsSuccessStatusCode,
                    "Driver with special characters should be created");

                var driverData = await GetDriverAsync(licenseNumber);
                Assert.Contains("Québec City", driverData);
            }
            finally
            {
                await DeleteDriverAsync(driverId);
            }
        }

        [Fact]
        public async Task UpdateDriverId_ShouldFail_WhenTripDoesNotExist()
        {
            int invalidTripId = 999999;
            int newDriverId = new Random().Next(10000, 99999);

            var updatePayload = new { driver_id = newDriverId };

            var response = await _client.PatchAsync(
                $"{TripEndpoint}?id=eq.{invalidTripId}",
                JsonContent(updatePayload)
            );

            var verifyResponse = await _client.GetAsync($"{TripEndpoint}?id=eq.{invalidTripId}");
            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();

            Assert.Equal("[]", verifyContent.Trim());

            Assert.True(response.IsSuccessStatusCode,
                "PATCH should return a valid HTTP status even if no rows exist.");
        }
    }
}
