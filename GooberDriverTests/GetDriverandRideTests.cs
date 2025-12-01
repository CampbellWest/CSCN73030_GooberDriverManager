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
            var controller = new DriverManagerController();

            controller.ClearDriversTEST("123");

            var result = controller.GetAvailableDrivers();

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No available drivers at the moment.", notFound.Value);
        }

        
        [Fact]
        public void GetAvailableDrivers_ReturnsOnlyAvailableDrivers()
        {
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);

            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            Assert.NotEmpty(allDrivers);

            var driverToDisable = allDrivers.First();
            var updateResult = controller.UpdateDriverAvailability(driverToDisable.DriverId, false);
            Assert.IsType<OkObjectResult>(updateResult);

            var getResult = controller.GetAvailableDrivers();

            var ok = Assert.IsType<OkObjectResult>(getResult);
            var availableDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(ok.Value);

            Assert.True(availableDrivers.Count < allDrivers.Count);
            Assert.All(availableDrivers, d => Assert.True(d.IsAvailable));
        }

        
        [Fact]
        public void UpdateDriverAvailability_ExistingDriver_UpdatesFlagAndReturnsOk()
        {
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);

            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            var target = allDrivers.First();
            Assert.True(target.IsAvailable);

            var result = controller.UpdateDriverAvailability(target.DriverId, false);

            var ok = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<string>(ok.Value);
            Assert.Contains($"Driver with ID {target.DriverId} availability updated to False", message);

            var getResult = controller.GetAvailableDrivers();

            if (getResult is NotFoundObjectResult)
            {
                // No drivers are available anymore.
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
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            int bogusDriverId = 999_999;

            var result = controller.UpdateDriverAvailability(bogusDriverId, false);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var message = Assert.IsType<string>(notFound.Value);
            Assert.Equal($"Driver with ID {bogusDriverId} not found.", message);
        }

        [Fact]
        public void GetAvailableDrivers_AllDriversUnavailable_ReturnsNotFoundOrOnlyAvailableDrivers()
        {
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);

            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            Assert.NotEmpty(allDrivers);

            foreach (var driver in allDrivers)
            {
                var updateResult = controller.UpdateDriverAvailability(driver.DriverId, false);
                Assert.IsType<OkObjectResult>(updateResult);
            }

            var getResult = controller.GetAvailableDrivers();

            if (getResult is NotFoundObjectResult notFound)
            {
                Assert.Equal("No available drivers at the moment.", notFound.Value);
            }
            else
            {
                var ok = Assert.IsType<OkObjectResult>(getResult);
                var remaining = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(ok.Value);
                Assert.All(remaining, d => Assert.True(d.IsAvailable));
            }
        }

        [Fact]
        public void UpdateDriverAvailability_ToggleDriverBackToAvailable()
        {
            var controller = new DriverManagerController();
            controller.ClearDriversTEST("123");

            var generateResult = controller.GenerateMoreDriversTest() as OkObjectResult;
            Assert.NotNull(generateResult);

            var allDrivers = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(generateResult!.Value);
            Assert.NotEmpty(allDrivers);

            var target = allDrivers.First();

            var initialAvailableResult = controller.GetAvailableDrivers() as OkObjectResult;
            Assert.NotNull(initialAvailableResult);

            var initialList = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(initialAvailableResult!.Value);
            int initialCount = initialList.Count;

            var setFalseResult = controller.UpdateDriverAvailability(target.DriverId, false);
            Assert.IsType<OkObjectResult>(setFalseResult);

            var afterFalse = controller.GetAvailableDrivers();
            if (afterFalse is OkObjectResult okAfterFalse)
            {
                var availableAfterFalse = Assert.IsAssignableFrom<List<ConfirmDriverRequest>>(okAfterFalse.Value);
                Assert.True(availableAfterFalse.Count <= initialCount);
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
                Assert.Fail("Expected at least one available driver after toggling back to true.");
            }
        }

        [Fact]
        public void RideInformation_JsonDeserialization_MapsCarTypeAndPetFriendly()
        {
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

            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            Assert.NotNull(vehicles);
            var info = Assert.Single(vehicles!);

            Assert.Equal("XL", info.CarType);
            Assert.True(info.PetFriendly);
        }

        [Fact]
        public void RideInformation_JsonDeserialization_IsCaseInsensitive()
        {
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

            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            Assert.NotNull(vehicles);
            var info = Assert.Single(vehicles!);

            Assert.Equal("Regular", info.CarType);
            Assert.False(info.PetFriendly);
        }

        [Fact]
        public void RideInformation_JsonDeserialization_EmptyArray_ReturnsEmptyList()
        {
            string json = "[]";

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            Assert.NotNull(vehicles);
            Assert.Empty(vehicles!);
        }

        [Fact]
        public void RideInformation_JsonDeserialization_IgnoresExtraFields()
        {
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

            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, options);

            Assert.NotNull(vehicles);
            var info = Assert.Single(vehicles!);

            Assert.Equal("XL", info.CarType);
            Assert.True(info.PetFriendly);
        }
    }
}
