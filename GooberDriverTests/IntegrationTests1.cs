using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using DemoApi;
using DemoApi.Resources;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DemoApi.GooberDriverTests
{
   
    public class DriverManagerControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DriverManagerControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Create an in-memory HttpClient for the DemoApi application
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAvailableDrivers_NoDrivers_ReturnsNotFound()
        {
            // Arrange
            var clearResponse = await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");
            clearResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _client.GetAsync("/api/DriverManager/GetAvailableDrivers");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("No available drivers at the moment.", message);
        }

       
        [Fact]
        public async Task GetAvailableDrivers_WhenDriversExist_ReturnsOkWithAvailableDrivers()
        {
            // Arrange: clears and then generates new drivers
            await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");

            var generateResponse = await _client.GetAsync("/api/DriverManager/GenerateMoreDriversTest");
            generateResponse.EnsureSuccessStatusCode();

            // Act: calls the real endpoint
            var response = await _client.GetAsync("/api/DriverManager/GetAvailableDrivers");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var drivers = await response.Content.ReadFromJsonAsync<List<ConfirmDriverRequest>>();

            Assert.NotNull(drivers);
            Assert.NotEmpty(drivers!);
            // all drivers returned should be available
            Assert.All(drivers!, d => Assert.True(d.IsAvailable));
        }


        [Fact]
        public async Task UpdateDriverAvailability_UnknownDriver_ReturnsNotFound()
        {
            // Arrange
            await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");

            int bogusDriverId = 999_999;

            // Act
            var response = await _client.PutAsync(
                $"/api/DriverManager/UpdateDriverAvailability?driverId={bogusDriverId}&isAvailable=false",
                content: null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Driver with ID {bogusDriverId} not found.", body);
        }


        /*
        [Fact]
        
        public async Task UpdateDriverAvailability_ExistingDriver_MakesDriverUnavailable()
        {
            // Arrange
            await _client.GetAsync("/api/DriverManager/ClearDriversTEST?password=123");
            var generateResponse = await _client.GetAsync("/api/DriverManager/GenerateMoreDriversTest");
            generateResponse.EnsureSuccessStatusCode();

            // Gets list of available drivers first
            var initialResponse = await _client.GetAsync("/api/DriverManager/GetAvailableDrivers");
            initialResponse.EnsureSuccessStatusCode();

            var initialDrivers = await initialResponse.Content.ReadFromJsonAsync<List<ConfirmDriverRequest>>();
            Assert.NotNull(initialDrivers);
            Assert.NotEmpty(initialDrivers!);

            var target = initialDrivers!.First();
            int driverId = target.DriverId;

            // Act
            var updateResponse = await _client.PutAsync(
                $"/api/DriverManager/UpdateDriverAvailability?driverId={driverId}&isAvailable=false",
                content: null);
            updateResponse.EnsureSuccessStatusCode();

            // Assert
            var afterResponse = await _client.GetAsync("/api/DriverManager/GetAvailableDrivers");
            if (afterResponse.StatusCode == HttpStatusCode.NotFound)
            {
                var msg = await afterResponse.Content.ReadAsStringAsync();
                Assert.Contains("No available drivers", msg);
            }
            else
            {
                afterResponse.EnsureSuccessStatusCode();
                var afterDrivers = await afterResponse.Content.ReadFromJsonAsync<List<ConfirmDriverRequest>>();
                Assert.NotNull(afterDrivers);
                Assert.DoesNotContain(afterDrivers!, d => d.DriverId == driverId);
                Assert.All(afterDrivers!, d => Assert.True(d.IsAvailable));
            }
        }
    */

        //Ride info integration tests would go here
    }

    public class IntegrationTests
    {
        private const string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI";
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

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"Failed to delete driver: {errorMsg}");
            }
        }

        private async Task DeleteTripAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{TripEndpoint}?id=eq.{id}");
            request.Headers.Add("apikey", SupabaseApiKey);
            request.Headers.Add("Authorization", $"Bearer {SupabaseApiKey}");

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"Failed to delete trip: {errorMsg}");
            }
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
            try{
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
                //Console.WriteLine("Supabase Driver Error: " + error); 
            }

            Assert.True(response.IsSuccessStatusCode, "Driver was NOT created in Supabase");

            // VERIFY: fetch the driver by license_number
                var driverData = await GetDriverAsync(licenseNumber);
                //Console.WriteLine("Driver Data: " + driverData);
                Assert.Contains("Testlocation", driverData);
                Assert.Contains(licenseNumber.ToString(), driverData);
            }
             finally
            {
                await DeleteDriverAsync(driverId); // cleanup
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

                Assert.True(response.IsSuccessStatusCode, "Driver with special characters should be created");

                // Verify content
                var driverData = await GetDriverAsync(licenseNumber);
                Assert.Contains("Québec City", driverData);
            }
            finally
            {
                await DeleteDriverAsync(driverId);
            }
        }

        // [Fact]
        // public async Task UpdateTripDriverId_ShouldUpdateSuccessfully()
        // {
        //     int tripId = 1; // existing trip already in DB
        //     int newDriverId = new Random().Next(10000, 99999);

        //     var updatePayload = new { driver_id = newDriverId };

        //     var response = await _client.PatchAsync(
        //         $"{TripEndpoint}?id=eq.{tripId}",
        //         JsonContent(updatePayload)
        //     );

        //     Assert.True(response.IsSuccessStatusCode, "DriverId update failed.");

        //     var tripData = await GetTripAsync(tripId);
        //     Assert.Contains(newDriverId.ToString(), tripData);

        // }

        // [Fact]
        // public async Task UpdateTripDriverId_ShouldUpdateSuccessfully()
        // {
        //     // 1. Insert driver
        //     int newDriverId = new Random().Next(10000, 99999);
        //     int licenseNumber = new Random().Next(100000, 999999);

        //     var driverPayload = new
        //     {
        //         id = newDriverId,
        //         account_id = "142dc6ca-7d33-47ea-9b1d-53ac25c9b15f",
        //         rating = 5,
        //         availability_status = "available",
        //         license_number = licenseNumber,
        //         current_location = "TempLocation"
        //     };

        //     var driverResponse = await _client.PostAsync(DriverEndpoint, JsonContent(driverPayload));
        //     driverResponse.EnsureSuccessStatusCode();

        //     // 2. Insert trip
        //     var tripPayload = new
        //     {
        //         rider_id = 1,
        //         driver_id = (int?)null,
        //         start_location = "Point A",
        //         end_location = "Point B",
        //         time_started = DateTime.UtcNow.ToString("o")
        //     };

        //     var tripResponse = await _client.PostAsync(TripEndpoint, JsonContent(tripPayload));
        //     tripResponse.EnsureSuccessStatusCode();

        //     var insertedTripJson = await tripResponse.Content.ReadAsStringAsync();
        //     var insertedTrip = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(insertedTripJson);
        //     int tripId = ((JsonElement)insertedTrip![0]["id"]).GetInt32();

        //     // 3. Update trip using Supabase client
        //     var updateResult = await supabase
        //         .From<Trip>()
        //         .Where(x => x.id == tripId)
        //         .Set(x => x.driver_id, newDriverId)
        //         .Update();

        //     Console.WriteLine("Update result: " + JsonSerializer.Serialize(updateResult));

        //     // 4. Verify update
        //     var updatedTrip = updateResult.FirstOrDefault();
        //     Assert.NotNull(updatedTrip);
        //     Assert.Equal(newDriverId, updatedTrip.driver_id);

        //     // 5. Cleanup
        //     await DeleteDriverAsync(newDriverId);
        //     await DeleteTripAsync(tripId);
        // }




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

            // Re-fetch the trip to confirm it does NOT exist
            var verifyResponse = await _client.GetAsync($"{TripEndpoint}?id=eq.{invalidTripId}");
            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();

            // If trip does not exist, the returned array will be empty: "[]"
            Assert.Equal("[]", verifyContent.Trim().ToString());




            // Ensure the update did NOT crash or silently succeed on a nonexistent trip.
            Assert.True(response.IsSuccessStatusCode, 
                "PATCH should return a valid HTTP status even if no rows exist, but update must not actually modify anything.");

        }


    }
}
