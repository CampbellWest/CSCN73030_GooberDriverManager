using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Generators;
using DemoApi.Resources;
using DriverManagement;
using DriverFinder;
using Microsoft.Win32.SafeHandles;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DriverManagerController : ControllerBase
{
    private const string SupabaseBaseUrl = "https://flpjmceqykalfwktysgi.supabase.co/rest/v1";
    private const string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZscGptY2VxeWthbGZ3a3R5c2dpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTkxMDEwMTMsImV4cCI6MjA3NDY3NzAxM30.X1rlQZeSvbrO0KE1LZdsrLvNS8YlpTborYoXG4JGsWI";
    private static List<ConfirmDriverRequest> RegisteredDrivers = new();
    
    [HttpGet]
    public IActionResult GenerateMoreDriversTest()
    {
        if (RegisteredDrivers.Count < 100)
        {
            for (int i = 0; i < 10; i++)
            {
                RegisteredDrivers.Add(DriverGenerator.GenerateDriver());
            }
        }

        return Ok(RegisteredDrivers);
    }

    private void GenerateDriversByTenUpToOneHundred()
    {
        if (RegisteredDrivers.Count < 100)
        {
            for (int i = 0; i < 10; i++)
            {
                RegisteredDrivers.Add(DriverGenerator.GenerateDriver());
            }
        }
    }
    private async Task<RideRequest?> BuildRideRequestFromTripAsync(int tripId)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {SupabaseApiKey}");

        // filters by id
        string url = $"{SupabaseBaseUrl}/Trip?id=eq.{tripId}";

        var response = await httpClient.GetAsync(url);
        var rawBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // Log full response body in the error so tests can see what went wrong
            throw new InvalidOperationException(
                $"Database API returned {response.StatusCode}: {rawBody}"
            );
        }

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            // No trip with that id
            return null;
        }

        var trip = root[0];

        double GetDoubleOrDefault(string name)
        {
            return trip.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null
                ? el.GetDouble()
                : 0.0;
        }

        bool GetBoolOrDefault(string name)
        {
            return trip.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.True;
        }

        string GetStringOrEmpty(string name)
        {
            return trip.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null
                ? (el.GetString() ?? string.Empty)
                : string.Empty;
        }

        int GetIntOrDefault(string name)
        {
            return trip.TryGetProperty(name, out var el) && el.ValueKind != JsonValueKind.Null
                ? el.GetInt32()
                : 0;
        }

        // Builds the RideRequest from trip table data
        var rideRequest = new RideRequest
        {
            RideId = trip.GetProperty("id").GetInt32(),
            ClientId = GetIntOrDefault("rider_id"),

            PickupLocation = new LocationData
            {
                Latitude = GetDoubleOrDefault("start_latitude"),
                Longitude = GetDoubleOrDefault("start_longitude"),
                Address = GetStringOrEmpty("start_location")
            },

            DropOffLocation = new LocationData
            {
                Address = GetStringOrEmpty("end_location")
            },

            RideInformation = new RideInformation
            {
                PetFriendly = GetBoolOrDefault("petFriendly"),
                CarType = GetStringOrEmpty("carType")
            }
            
        };

        return rideRequest;
    }

    // POST api/DriverManager/RequestDriver
    [HttpPost]
    public async Task<IActionResult> RequestDriver(TripIdRequest tripRequest)
    {
        if (tripRequest == null)
            return BadRequest("Trip request body is required.");

        RideRequest? rideRequest;

        try
        {
            rideRequest = await BuildRideRequestFromTripAsync(tripRequest.TripId);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        if (rideRequest == null)
            return NotFound($"Trip with ID {tripRequest.TripId} not found.");

        try
        {
            // Generate drivers
            GenerateDriversByTenUpToOneHundred();
            // Console.WriteLine("Registered drivers:");
            // foreach (var d in RegisteredDrivers)
            // {
            //     Console.WriteLine($"Driver {d.DriverId}");
            // }

            ConfirmDriverRequest? bestDriver = null;

            // Find the closest available driver
            while (true)
            {
                var filteredDrivers = DriverFinder.DriverFinder.FilterDrivers(rideRequest, RegisteredDrivers);
                bestDriver = DriverFinder.DriverFinder.FindClosestDriver(filteredDrivers, rideRequest.PickupLocation);

                if (bestDriver == null)
                {
                    // No suitable driver found yet, generate more
                    GenerateDriversByTenUpToOneHundred();
                }
                else
                {
                    break;
                }
            }

            // Add driver to database
            await AddDriverDetails.AddDriverAsync(
                id: bestDriver.DriverId,
                accountId: "142dc6ca-7d33-47ea-9b1d-53ac25c9b15f", // existing accountId
                rating: 5,
                availability: "available",
                licenseNumber: new Random().Next(10000, 99999),
                currentLocation: rideRequest.PickupLocation.Address
            );

            // Update trip with driverId
            await UpdateDriverIdForTrip.UpdateDriverIdAsync(
                tripId: rideRequest.RideId,
                driverId: bestDriver.DriverId
            );

            // Update driver object to reflect assigned ride
            bestDriver.RideId = rideRequest.RideId;
            bestDriver.DriverAssigned = true;
            bestDriver.CurrentLocation = rideRequest.PickupLocation;

            // Return combined response
            var response = new
            {
                rideId = rideRequest.RideId,
                driverAssigned = true,
                driver = bestDriver
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
        int driverId = new Random().Next(10000, 99999);
        await DriverManagement.AddDriverDetails.AddDriverAsync(
            id: driverId,
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
    public IActionResult ClearDriversTEST(string password)
    {
        if (password == "123")
        {
            RegisteredDrivers.Clear();
            return Ok();
        }

        return Ok();
    }
    
}