using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Generators;
using DemoApi.Resources;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DriverManagerController : ControllerBase
{
    private static List<ConfirmDriverRequest> RegisteredDrivers = new();
    
	[HttpGet]
    public IActionResult TestAPI()
    {
        if (RegisteredDrivers.Count < 100)
        {
            for (int i = 0; i < 10; i++)
            {
                RegisteredDrivers.Add(DriverGenerator.GenerateDriver());
            }
        }
        
        return Ok(RegisteredDrivers.Count);
    }
    
    // POST api/DriverManager/RequestDriver
    [HttpPost]
    public IActionResult RequestDriver(RideRequest rideRequest)
    {
        var ConfirmRequest = new ConfirmDriverRequest
        {
            RideId = rideRequest.RideId,
            DriverAssigned = true,
            DriverId = 123,
            DriverName = "John Driver",
            CarInfo = "Red 2017 Honda Civic",
            LicensePlate = "ABCD 123",
            CurrentLocation = new LocationData
            {
                Latitude = 60.123,
                Longitude = -70.123
            }
        };
        
        return Ok(ConfirmRequest);
    }
    
    // POST api/DriverManager/DriveComplete
    [HttpPost]
    public IActionResult DriveComplete(int rideId)
    {
        // Find the database entry with the matching rideId 
        // and set it to null now that driver is free to be used again 
        // while also updating the current coordinates to the destination coordinates of the trip 
		// then send this driver to the database 
        
        return Ok($"Drive With Ride Id: {rideId} Is Complete. Driver is Available Again.");
    }
}