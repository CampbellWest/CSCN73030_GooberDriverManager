using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace DemoApi.Controllers;

// 

[ApiController]
[Route("api/[controller]/[action]")]
public class DriverManagerController : ControllerBase
{
    
	[HttpGet]
    public IActionResult TestAPI()
    {
        Thread.Sleep(10000);
	
        
        return Ok("Done");
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
		// then send this driver to the database group for the driver to be pulled later
        
        return Ok($"Drive With Ride Id: {rideId} Is Complete. Driver is Available Again.");
    }
}

public class ConfirmDriverRequest
{
    [JsonPropertyName("rideId")]    
    public int RideId { get; set; }
    
    [JsonPropertyName("driverAssigned")]    
    public bool DriverAssigned { get; set; }
    
    [JsonPropertyName("driverId")]    
    public int DriverId { get; set; }

    [JsonPropertyName("driverName")] 
    public string DriverName { get; set; } = "";

    [JsonPropertyName("carInformation")] 
    public string CarInfo { get; set; } = "";
    
    [JsonPropertyName("licensePlate")] 
    public string LicensePlate { get; set; } = "";

    [JsonPropertyName("currentLocation")]
    public LocationData CurrentLocation { get; set; } = default!;
}

public class RideRequest
{
    [JsonPropertyName("rideId")]    
    public int RideId { get; set; }
    
    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }

    [JsonPropertyName("timeStamp")] 
    public string TimeStamp { get; set; } = "";
    
    [JsonPropertyName("pickup")]
    public LocationData PickupLocation { get; set; } = default!;
    
    [JsonPropertyName("dropOff")]
    public LocationData DropOffLocation { get; set; } = default!;
    
    [JsonPropertyName("routeInformation")]
    public RouteInformation RouteInformation { get; set; } = default!;
    
    [JsonPropertyName("rideInformation")]
    public RideInformation RideInformation { get; set; } = default!;
    
    [JsonPropertyName("paymentInformation")]
    public PaymentInformation PaymentInformation { get; set; } = default!;
}

public class LocationData
{
    [JsonPropertyName("latitude")]    
    public double Latitude { get; set; }
    
    [JsonPropertyName("longitude")]    
    public double Longitude { get; set; }

    [JsonPropertyName("address")] 
    public string Address { get; set; } = "";
}

public class RouteInformation
{
    [JsonPropertyName("distance")]    
    public double Distance { get; set; }
    
    [JsonPropertyName("duration")]    
    public double Duration { get; set; }
}

public class RideInformation
{
    [JsonPropertyName("carType")] 
    public string CarType { get; set; } = "";
    
    [JsonPropertyName("petFriendly")]    
    public bool PetFriendly { get; set; } 
}

public class PaymentInformation
{
    [JsonPropertyName("currency")] 
    public string Curreny { get; set; } = "";
    
    [JsonPropertyName("amount")]    
    public double Amount { get; set; }
    
    [JsonPropertyName("paymentStatus")]    
    public bool PaymentStatus { get; set; }
}