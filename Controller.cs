using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Generators;
using DemoApi.Resources;
using DriverManagement;

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
    public async Task<IActionResult> RequestDriver(RideRequest rideRequest)
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

        // calling AddTripDetails.AddTripAsync
        // Pass in all required parameters from the ride request and assigned driver.
        // This will send the trip info to the database once writes are enabled.

        await AddTripDetails.AddTripAsync(
            id: rideRequest.RideId,
            driverId: ConfirmRequest.DriverId,
            riderId: rideRequest.RiderId,
            startLocation: "108 University Ave E, Waterloo", // fake data for now
            endLocation: "220 King St N, Waterloo",         //fake data for now
            timeStarted: DateTime.UtcNow,
            timeCompleted: DateTime.UtcNow.AddMinutes(30),
            status: "Completed"
        );


        return Ok(ConfirmRequest);
    }

    // POST api/DriverManager/DriveComplete
    [HttpPost]
    public async Task<IActionResult> DriveComplete(int rideId)
    {

        // Find the database entry with the matching rideId 
        // and set it to null now that driver is free to be used again 
        // while also updating the current coordinates to the destination coordinates of the trip 
        // then send this driver to the database 

        // calling AddDriverDetails.AddDriverAsync
        //using fake data      
        await AddDriverDetails.AddDriverAsync(
            id: 123,
            accountId: "160ca31",
            rating: 5,
            availability: "available",
            licenseNumber: 12345,
            currentLocation: "220 King St N, Waterloo"
        );


        return Ok($"Drive With Ride Id: {rideId} Is Complete. Driver is Available Again.");
    }

    [HttpGet]
    public IActionResult GetAvailableDrivers()
    {
        // Filters all drivers who are available
        var availableDrivers = RegisteredDrivers.Where(d => d.IsAvailable).ToList();

        // returns a message if no available drivers
        if (!availableDrivers.Any())
            return NotFound("No available drivers at the moment.");

        return Ok(availableDrivers);
    }

    // PUT api/DriverManager/UpdateDriverAvailability
    [HttpPut]
    public IActionResult UpdateDriverAvailability(int driverId, bool isAvailable)
    {
        // Finds the driver by ID
        var driver = RegisteredDrivers.FirstOrDefault(d => d.DriverId == driverId);

        if (driver == null)
            return NotFound($"Driver with ID {driverId} not found.");

        driver.IsAvailable = isAvailable;

        return Ok($"Driver with ID {driverId} availability updated to {isAvailable}.");
    }

    [HttpGet]
    public async Task<IActionResult> GetRideInfo(int rideId)
    {
        using var httpClient = new HttpClient();

        //I will replace with actual api url once I figure out from database team what it is
        string dbApiUrl = $".../api/rides/getRideInfo?rideId={rideId}";

        var response = await httpClient.GetAsync(dbApiUrl);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error fetching ride details.");
        }

        //response body as json
        var jsonResponse = await response.Content.ReadAsStringAsync();

        //deserialize json to RideResponse object
        var rideResponse = JsonSerializer.Deserialize<RideResponse>(jsonResponse, new JsonSerializerOptions
        {
            //incase of unmatching names
            PropertyNameCaseInsensitive = true
        });

        if (rideResponse == null)
        {
            return NotFound("Ride details not found.");
        }

        return Ok(rideResponse);
    }

}