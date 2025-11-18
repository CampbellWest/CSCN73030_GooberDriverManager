using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DemoApi.Controllers;
using DemoApi.Resources;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DemoApi.GooberDriverTests
{
    public class GetDriverandRideTests
    {
       
        [Fact]
        public void GetAvailableDrivers_NoDrivers_ReturnsNotFound()
        {
            // Arrange
            var controller = new DriverManagerController();

            controller.ClearDriversTEST("123");

            // Act
            var result = controller.GetAvailableDrivers();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No available drivers at the moment.", notFound.Value);
        }

        
        [Fact]
        public void GetAvailableDrivers_ReturnsOnlyAvailableDrivers()
        {
            // Arrange
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            // Seed drivers into the controller
            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);

            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            Assert.NotEmpty(allDrivers);

            // Mark one driver as unavailable
            var driverToDisable = allDrivers.First();
            var updateResult = controller.UpdateDriverAvailability(driverToDisable.DriverId, false);
            Assert.IsType<OkObjectResult>(updateResult);

            // Act
            var getResult = controller.GetAvailableDrivers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(getResult);
            var availableDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(ok.Value);

            // there should be fewer available drivers than total drivers
            Assert.True(availableDrivers.Count < allDrivers.Count);


            // all returned drivers must be available
            Assert.All(availableDrivers, d => Assert.True(d.IsAvailable));
        }

        
       [Fact]
       public void UpdateDriverAvailability_ExistingDriver_UpdatesFlagAndReturnsOk()
       {
        // Arrange
        var controller = new DriverManagerController();
        controller.ClearDriversTEST("123");
        
        var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
        Assert.NotNull(generateResult);
        
        var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
        var target = allDrivers.First();
        Assert.True(target.IsAvailable); // default from generator
        
        // Act
        var result = controller.UpdateDriverAvailability(target.DriverId, false);
        
        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var message = Assert.IsType<string>(ok.Value);
        Assert.Contains($"Driver with ID {target.DriverId} availability updated to False", message);
        
        var getResult = controller.GetAvailableDrivers();
        
        if (getResult is NotFoundObjectResult)
        {
        // No drivers are available anymore
        }
        else
        {
            var okGet = Assert.IsType<OkObjectResult>(getResult);
            var availableDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(okGet.Value);
            Assert.All(availableDrivers, d => Assert.True(d.IsAvailable));
        }   
         }


        
        [Fact]
        public void UpdateDriverAvailability_NonExistingDriver_ReturnsNotFound()
        {
            // Arrange
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            int bogusDriverId = 999999;

            // Act
            var result = controller.UpdateDriverAvailability(bogusDriverId, false);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var message = Assert.IsType<string>(notFound.Value);
            Assert.Equal($"Driver with ID {bogusDriverId} not found.", message);
        }

        
        [Fact]
        public void RideInformation_JsonDeserialization_MapsCarTypeAndPetFriendly()
        {
            // Arrange
            string json = """
            [
              {
                "carType": "XL",
                "petFriendly": true
              }
            ]
            """;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Act
            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            // Assert
            Assert.NotNull(vehicles);
            var info = Assert.Single(vehicles!);

            Assert.Equal("XL", info.CarType);
            Assert.True(info.PetFriendly);
        }

        /*
        [Fact]
        public void GetAvailableDrivers_AllDriversUnavailable_ReturnsNotFound()
        {
            // Arrange
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            // Seed drivers into the controller
            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);

            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            Assert.NotEmpty(allDrivers);

            // Mark all drivers as unavailable
            foreach (var driver in allDrivers)
            {
                var updateResult = controller.UpdateDriverAvailability(driver.DriverId, false);
                Assert.IsType<OkObjectResult>(updateResult);
            }

            // Act
            var getResult = controller.GetAvailableDrivers();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(getResult);
            Assert.Equal("No available drivers at the moment.", notFound.Value);
        }
        */

        [Fact]
        public void UpdateDriverAvailability_ToggleDriverBackToAvailable()
        {
            // Arrange
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");
            
            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);
            
            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            Assert.NotEmpty(allDrivers);
            
            var target = allDrivers.First();

            // Initial state: all drivers should be available
            var initialAvailable = controller.GetAvailableDrivers() as OkObjectResult;
            Assert.NotNull(initialAvailable);
            var initialList = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(initialAvailable!.Value);
            int initialCount = initialList.Count;

            
            var setFalseResult = controller.UpdateDriverAvailability(target.DriverId, false);
            Assert.IsType<OkObjectResult>(setFalseResult);

            var afterFalse = controller.GetAvailableDrivers();
            
            if (afterFalse is OkObjectResult okAfterFalse)
            {
                var availableAfterFalse = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(okAfterFalse.Value);
                Assert.True(availableAfterFalse.Count < initialCount);
                Assert.All(availableAfterFalse, d => Assert.True(d.IsAvailable));
                }
                else
                {
                    
                    Assert.IsType<NotFoundObjectResult>(afterFalse);
                }

            
            var setTrueResult = controller.UpdateDriverAvailability(target.DriverId, true);
            Assert.IsType<OkObjectResult>(setTrueResult);

            
            var finalResult = controller.GetAvailableDrivers();
            if (finalResult is OkObjectResult okFinal)
            {
                var finalList = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(okFinal.Value);
                Assert.NotEmpty(finalList);
                Assert.All(finalList, d => Assert.True(d.IsAvailable));
                }
                else
                {
                    Assert.True(false, "Expected at least one available driver after toggling back to true.");
                    }
        }
        
        [Fact]
        public void RideInformation_JsonDeserialization_IsCaseInsensitive(){
            // Arrange
            string json = """
            [
              {
                "CarType": "Regular",
                "PetFriendly": false
              }
            ]
            """;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Act
            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            // Assert
            Assert.NotNull(vehicles);
            var info = Assert.Single(vehicles!);

            Assert.Equal("Regular", info.CarType);
            Assert.False(info.PetFriendly);
        }

        [Fact]
        public void RideInformation_JsonDeserialization_EmptyArray_ReturnsEmptyList(){
            // Arrange
            string json = "[]";

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Act
            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            // Assert
            Assert.NotNull(vehicles);
            Assert.Empty(vehicles!);
        }

        [Fact]
        public void RideInformation_JsonDeserialization_IgnoresExtraFields(){
            // Arrange
            string json = """
            [
              {
                "carType": "XL",
                "petFriendly": true,
                "colour": "Purple",
                "numberOfSeats": 7
              }
            ]
            """;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Act
            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            // Assert
            Assert.NotNull(vehicles);
            var info = Assert.Single(vehicles!);

            Assert.Equal("XL", info.CarType);
            Assert.True(info.PetFriendly);
        }
        
    }
}
