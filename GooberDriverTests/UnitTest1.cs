using Xunit;
using DriverFinder;
using DemoApi.Resources;
using System.Text.Json;

namespace DemoApi.GooberDriverTests;

public class DriverFinderTests
{
    [Fact]
    public void CalculateDistance_SameLocation_ReturnsZero()
    {
        // arrange
        double lat = 43.5448;
        double lon = -80.2482;

        // act
        double distance = DriverFinder.DriverFinder.CalculateDistance(lat, lon, lat, lon);

        // assert
        Assert.Equal(0, distance, 2);
    }
    [Fact]
    public void ToRadians_180_ReturnsPI()
    {
        double rad = double.DegreesToRadians(180);
        
        Assert.Equal(Math.PI, rad, 2);
    }
    [Fact]
    public void ToRadians_ZeroDegrees_ReturnsZero()
    {
        // arrange
        double degrees = 0;
        
        // act
        double result = DriverFinder.DriverFinder.ToRadians(degrees);
        
        // assert
        Assert.Equal(0, result, 10);
    }
    
    [Fact]
    public void ToRadians_90Degrees_ReturnsPiOverTwo()
    {
        // arrange
        double degrees = 90;
        
        // act
        double result = DriverFinder.DriverFinder.ToRadians(degrees);
        
        // assert
        Assert.Equal(Math.PI / 2, result, 10);
    }
    
    [Fact]
    public void ToRadians_NegativeDegrees_ReturnsNegativeRadians()
    {
        // arrange
        double degrees = -45;
        
        // act
        double result = DriverFinder.DriverFinder.ToRadians(degrees);
        
        // assert
        Assert.Equal(-Math.PI / 4, result, 10);
    }
    
 
    [Fact]
    public void CalculateDistance_TorontoToOttawa_ReturnsApproximateDistance()
    {
        // arrange - toronto & ottwa
        double torontoLat = 43.6532;
        double torontoLon = -79.3832;
        double ottawaLat = 45.4215;
        double ottawaLon = -75.6972;
        
        // act
        double distance = DriverFinder.DriverFinder.CalculateDistance(
            torontoLat, torontoLon, ottawaLat, ottawaLon);
        
        // assert - should be 350-400 km
        Assert.InRange(distance, 350000, 400000);
    }
    
    [Fact]
    public void CalculateDistance_GuelphLocations_ReturnsReasonableDistance()
    {
        // arrange - 2 spots in guelph
        double lat1 = 43.5448;
        double lon1 = -80.2482;
        double lat2 = 43.5310;
        double lon2 = -80.2260;
        
        // act
        double distance = DriverFinder.DriverFinder.CalculateDistance(lat1, lon1, lat2, lon2);
        
        // assert - should be 1-10km
        Assert.InRange(distance, 1000, 10000);
    }
    
    [Fact]
    public void CalculateDistance_OppositeOrder_ReturnsSameDistance()
    {
        // arrange
        double lat1 = 43.5448;
        double lon1 = -80.2482;
        double lat2 = 43.5310;
        double lon2 = -80.2260;
        
        // act
        double distance1 = DriverFinder.DriverFinder.CalculateDistance(lat1, lon1, lat2, lon2);
        double distance2 = DriverFinder.DriverFinder.CalculateDistance(lat2, lon2, lat1, lon1);
        
        // assert
        Assert.Equal(distance1, distance2, 1);
    }
    
    #region FilterDrivers Tests
    
    [Fact]
    public void FilterDrivers_MatchingCarType_ReturnsDriver()
    {
        // arrange
        var rideRequest = new RideRequest
        {
            RideInformation = new RideInformation
            {
                CarType = "regular",
                PetFriendly = false
            }
        };
        
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5, Longitude = -80.2 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FilterDrivers(rideRequest, driverList);
        
        // assert
        Assert.Single(result);
    }
    
    [Fact]
    public void FilterDrivers_MismatchedCarType_ReturnsEmpty()
    {
        // arrange
        var rideRequest = new RideRequest
        {
            RideInformation = new RideInformation
            {
                CarType = "xl",
                PetFriendly = false
            }
        };
        
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5, Longitude = -80.2 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FilterDrivers(rideRequest, driverList);
        
        // assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void FilterDrivers_PetFriendlyRequest_RequiresPetFriendlyCar()
    {
        // arrange
        var rideRequest = new RideRequest
        {
            RideInformation = new RideInformation
            {
                CarType = "regular",
                PetFriendly = true
            }
        };
        
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5, Longitude = -80.2 }
            },
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = true },
                CurrentLocation = new LocationData { Latitude = 43.6, Longitude = -80.3 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FilterDrivers(rideRequest, driverList);
        
        // assert
        Assert.Single(result);
        Assert.True(result[0].CarInfo.IsPetFriendly);
    }
    
    [Fact]
    public void FilterDrivers_NoPetRequest_AcceptsBothPetFriendlyAndNonPetFriendly()
    {
        // arrange
        var rideRequest = new RideRequest
        {
            RideInformation = new RideInformation
            {
                CarType = "regular",
                PetFriendly = false
            }
        };
        
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5, Longitude = -80.2 }
            },
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = true },
                CurrentLocation = new LocationData { Latitude = 43.6, Longitude = -80.3 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FilterDrivers(rideRequest, driverList);
        
        // assert
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public void FilterDrivers_EmptyDriverList_ReturnsEmpty()
    {
        // arrange
        var rideRequest = new RideRequest
        {
            RideInformation = new RideInformation
            {
                CarType = "regular",
                PetFriendly = false
            }
        };
        
        var driverList = new List<ConfirmDriverRequest>();
        
        // act
        var result = DriverFinder.DriverFinder.FilterDrivers(rideRequest, driverList);
        
        // assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void FilterDrivers_MultipleMatchingDrivers_ReturnsAll()
    {
        // arrange
        var rideRequest = new RideRequest
        {
            RideInformation = new RideInformation
            {
                CarType = "xl",
                PetFriendly = false
            }
        };
        
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "xl", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5, Longitude = -80.2 }
            },
            new()
            {
                CarInfo = new CarInformation { CarType = "xl", IsPetFriendly = true },
                CurrentLocation = new LocationData { Latitude = 43.6, Longitude = -80.3 }
            },
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.7, Longitude = -80.4 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FilterDrivers(rideRequest, driverList);
        
        // assert
        Assert.Equal(2, result.Count);
        Assert.All(result, driver => Assert.Equal("xl", driver.CarInfo.CarType));
    }
    
    #endregion
    
    #region FindClosestDriver Tests
    
    [Fact]
    public void FindClosestDriver_SingleDriver_ReturnsThatDriver()
    {
        // arrange
        var pickup = new LocationData { Latitude = 43.5448, Longitude = -80.2482 };
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5310, Longitude = -80.2260 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FindClosestDriver(driverList, pickup);
        
        // assert
        Assert.NotNull(result);
        Assert.Equal(driverList[0], result);
    }
    
    [Fact]
    public void FindClosestDriver_MultipleDrivers_ReturnsClosest()
    {
        // arrange
        var pickup = new LocationData { Latitude = 43.5448, Longitude = -80.2482 };
        
        var closeDriver = new ConfirmDriverRequest
        {
            CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
            CurrentLocation = new LocationData { Latitude = 43.5450, Longitude = -80.2480 } // Very close
        };
        
        var farDriver = new ConfirmDriverRequest
        {
            CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
            CurrentLocation = new LocationData { Latitude = 43.6532, Longitude = -79.3832 } // Much farther
        };
        
        var driverList = new List<ConfirmDriverRequest> { farDriver, closeDriver };
        
        // act
        var result = DriverFinder.DriverFinder.FindClosestDriver(driverList, pickup);
        
        // assert
        Assert.Equal(closeDriver, result);
    }
    
    [Fact]
    public void FindClosestDriver_EmptyList_ThrowsException()
    {
        // arrange
        var pickup = new LocationData { Latitude = 43.5448, Longitude = -80.2482 };
        var driverList = new List<ConfirmDriverRequest>();
        
        // act & Assert
        var exception = Assert.Throws<Exception>(() => 
            DriverFinder.DriverFinder.FindClosestDriver(driverList, pickup));
        
        Assert.Equal("No drivers available", exception.Message);
    }
    
    [Fact]
    public void FindClosestDriver_DriverAtSameLocation_ReturnsDriver()
    {
        // arrange
        var pickup = new LocationData { Latitude = 43.5448, Longitude = -80.2482 };
        var driverList = new List<ConfirmDriverRequest>
        {
            new()
            {
                CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
                CurrentLocation = new LocationData { Latitude = 43.5448, Longitude = -80.2482 }
            }
        };
        
        // act
        var result = DriverFinder.DriverFinder.FindClosestDriver(driverList, pickup);
        
        // assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public void FindClosestDriver_ThreeDrivers_ReturnsMiddleDistance()
    {
        // arrange
        var pickup = new LocationData { Latitude = 43.5448, Longitude = -80.2482 };
        
        var driver1 = new ConfirmDriverRequest
        {
            CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
            CurrentLocation = new LocationData { Latitude = 43.5500, Longitude = -80.2500 }
        };
        
        var driver2 = new ConfirmDriverRequest
        {
            CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
            CurrentLocation = new LocationData { Latitude = 43.5449, Longitude = -80.2483 } // Closest
        };
        
        var driver3 = new ConfirmDriverRequest
        {
            CarInfo = new CarInformation { CarType = "regular", IsPetFriendly = false },
            CurrentLocation = new LocationData { Latitude = 43.6000, Longitude = -80.3000 }
        };
        
        var driverList = new List<ConfirmDriverRequest> { driver1, driver2, driver3 };
        
        // act
        var result = DriverFinder.DriverFinder.FindClosestDriver(driverList, pickup);
        
        // assert
        Assert.Equal(driver2, result);
    }
    
    #endregion
}