using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Generators;
using DemoApi.Resources;
using DriverManagement;
using Microsoft.Win32.SafeHandles;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DriverManagerController : ControllerBase
{
    private static List<ConfirmDriverRequest> RegisteredDrivers = new();

    [HttpGet]
    public IActionResult GenerateaMoreDriversTest()
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
            ClientId: rideRequest.ClientId,
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

    // GET api/DriverManager/GetRideInfo?driverId=123
    [HttpGet]
    public async Task<IActionResult> GetRideInfo(int driverId)
    {
        using var httpClient = new HttpClient();

        //Supabase endpoint for the Vehicles table
        string dbApiUrl = $"https://flpjmceqykalfwktysgi.supabase.co/rest/v1/Vehicles?driver_id=eq.{driverId}";

        //public api key for authentication
        string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI";

        // includes the apikey in the request header
        httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            var response = await httpClient.GetAsync(dbApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, $"Database API returned {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();

            //Supabase returns a json structure
            var vehicles = JsonSerializer.Deserialize<List<RideInformation>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (vehicles == null || !vehicles.Any())
            {
                return NotFound($"No vehicle found for driver ID {driverId}");
            }

            // returns filtered RideInformation
            return Ok(vehicles.First());
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error contacting database API: {ex.Message}");
        }
    }


}