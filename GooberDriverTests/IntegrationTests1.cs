using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
}
