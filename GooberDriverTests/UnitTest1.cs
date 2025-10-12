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
        // Arrange
        double lat = 43.5448;
        double lon = -80.2482;

        // Act
        double distance = DriverFinder.DriverFinder.CalculateDistance(lat, lon, lat, lon);

        // Assert
        Assert.Equal(0, distance, 2);
    }
}