using Xunit;
using DriverFinder;
using DemoApi.Resources;
using System.Text.Json;
using DriverManagement;

namespace DemoApi.GooberDriverTests;

public class DriverFinderTests
{
    [Fact]
    public void CalculateDistance_SameLocation_ReturnsZero()
    {
        // Arrange
        double lat = 43.5448;
        double lon = -80.2482;

        // Act
        double distance = DriverFinder.DriverFinder.CalculateDistance(lat, lon, lat, lon);

        // Assert
        Assert.Equal(0, distance, 2);
    }

}
public class TripandDriverTests
    {

         private static readonly Random random = new();
        
        // ---- Test for AddDriverDetails ----
            [Fact]
            public async Task AddDriver_ShouldSucceed_WithValidInput()
            {
                // Arrange
                int id = random.Next(1000, 9999); // unique test id
                string accountId = "142dc6ca-7d33-47ea-9b1d-53ac25c9b15f";
                int rating = 5;
                string availability = "available"; 
                int licenseNumber = 99999;
                string currentLocation = "Test Location";

                // Act
                var exception = await Record.ExceptionAsync(() =>
                    AddDriverDetails.AddDriverAsync(id, accountId, rating, availability, licenseNumber, currentLocation)
                );

                // Assert
                Assert.Null(exception); // test passes if no exception
            }

            [Fact]
            public async Task AddDriver_ShouldFail_WithInvalidAvailability()
            {
                // Arrange
                int driverId = random.Next(1000, 9999);
                string accountId = Guid.NewGuid().ToString();
                int rating = 5;
                string invalidAvailability = "busy"; // Not valid enum
                int licenseNumber = 123456;
                string currentLocation = "123 King St, Waterloo";

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    AddDriverDetails.AddDriverAsync(driverId, accountId, rating, invalidAvailability, licenseNumber, currentLocation)
                );

                // Assert
                Assert.Contains("invalid input value for enum availability_status", ex.Message);

            }

            [Fact]
            public async Task AddDriver_ShouldFail_WhenAvailabilityIsNull()
            {
                // Arrange
                int id = new Random().Next(1000, 9999);
                string accountId = Guid.NewGuid().ToString();
                int rating = 4;
                string? availability = null; // missing value
                int licenseNumber = 12345;
                string currentLocation = "Unknown";

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    AddDriverDetails.AddDriverAsync(id, accountId, rating, availability!, licenseNumber, currentLocation)
                );

                // Assert
                Assert.Contains("Failed to add driver", ex.Message);
            }


            [Fact]
            public async Task AddDriver_ShouldFail_WithInvalidRating()
            {
                // Arrange
                int id = new Random().Next(1000, 9999);
                string accountId = Guid.NewGuid().ToString();
                int rating = 10; // out of range 
                string availability = "available";
                int licenseNumber = 56789;
                string currentLocation = "Test City";

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    AddDriverDetails.AddDriverAsync(id, accountId, rating, availability, licenseNumber, currentLocation)
                );

                // Assert
               Assert.Contains("Failed to add driver", ex.Message);

            }

            // ---- Test for AddTripDetails ----
            [Fact]
            public async Task AddTrip_ShouldSucceed_WithValidInput()
            {
                // Arrange
                int id = random.Next(1000, 9999);
                int driverId = 1; 
                int clientId = 1;
                string startLocation = "Start Test";
                string endLocation = "End Test";
                DateTime timeStarted = DateTime.UtcNow;
                DateTime timeCompleted = DateTime.UtcNow.AddMinutes(30);
                string status = "Completed";

                // Act
                var exception = await Record.ExceptionAsync(() =>
                    AddTripDetails.AddTripAsync(id, driverId, clientId, startLocation, endLocation, timeStarted, timeCompleted, status)
                );

                // Assert
                Assert.Null(exception);
            }

            [Fact]
            public async Task AddTrip_ShouldFail_WithNonexistentDriver()
            {
                // Arrange
                int tripId = random.Next(1000, 9999);
                int driverId = random.Next(1000, 9999); // Nonexistent driver
                int clientId = 1;
                string startLocation = "100 University Ave";
                string endLocation = "200 King St";
                DateTime startTime = DateTime.UtcNow;
                DateTime endTime = startTime.AddMinutes(30);
                string status = "Completed";

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    AddTripDetails.AddTripAsync(tripId, driverId, clientId, startLocation, endLocation, startTime, endTime, status)
                );

                // Assert
                Assert.Contains("23503", ex.Message);
            }

            [Fact]
            public async Task AddTrip_ShouldFail_WithDuplicateTripId()
            {
                // Arrange
                int tripId = 4; 
                int driverId = 1; // Existing driver
                int clientId = 1;
                string startLocation = "100 University Ave";
                string endLocation = "200 King St";
                DateTime startTime = DateTime.UtcNow;
                DateTime endTime = startTime.AddMinutes(30);
                string status = "Completed";

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    AddTripDetails.AddTripAsync(tripId, driverId, clientId, startLocation, endLocation, startTime, endTime, status)
                );

                // Assert
                Assert.Contains("duplicate key value violates unique constraint", ex.Message);
            }


            [Fact]
            public async Task AddTrip_ShouldFail_WithInvalidStatus()
            {
                // Arrange
                int id = new Random().Next(1000, 9999);
                int driverId = 1;
                int clientId = 1;
                string startLocation = "A";
                string endLocation = "B";
                DateTime startTime = DateTime.UtcNow;
                DateTime endTime = startTime.AddMinutes(15);
                string invalidStatus = "UnknownStatus";

                // Act
                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    AddTripDetails.AddTripAsync(id, driverId, clientId, startLocation, endLocation, startTime, endTime, invalidStatus)
                );

                // Assert
                Assert.Contains("invalid input value for enum", ex.Message);
            }

         
}

