# Gewber Driver Management

A fleet management system that dispatches drivers, allowing users to request a ride from one location to another.

[Click here for GitHub](https://github.com/CampbellWest/CSCN73030_GewberDriverManager)

## Team Members
- Cambell
- Jordan
- Navjeet
- Paul
- Shal

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Server Setup](#server-setup)
- [Usage](#usage)
- [REST API Routes](#rest-api-routes)
  - [Request a Ride](#request-a-ride)
  - [Complete a Ride](#complete-a-ride)
  
## Overview
This module handles all driver-related operations in the system, including driver registration, ride assignment, trip completion, and retrieval of ride details. It interacts with the Database team (Supabase backend) using REST API endpoints to store and retrieve driver and trip data. This module ensures smooth communication between the ride request system and the backend database, managing driver availability and assignments dynamically.

Other modules can access our functions using our endpoints in our [REST API Routes](#rest-api-routes) section.

## Features
•	Generating and managing driver data in memory for simulation/testing.
•	Handling ride requests and assigning the most suitable driver.
•	Communicating with Supabase to log trip and driver details.
•	Updating driver status (available/busy) based on ride completion.
•	Fetching ride information from the Supabase database.

## Requirements

| Requirement | Description |
| :--- | :--- |
| REQ-0001 | The system shall generate test drivers in batches of 10 until 100 total are registered. |
| REQ-0002 | The system shall filter and assign the closest available driver to each ride request. |
| REQ-0003 | The system shall send a trip entry to Supabase when a driver is assigned to a ride. |
| REQ-0004 | The system shall send a driver entry to Supabase when a ride is completed. |
| REQ-0005 | The system shall mark a driver as available again once a ride is completed. |
| REQ-0006 | The system shall fetch ride or vehicle information from Supabase for a given driver ID. |
| REQ-0007 | The system shall return a driver ID when a ride is requested. |
| REQ-0008 | The system shall return the driver latitude and longitude when a ride is requested. |
| REQ-0009 | The system shall generate tests drivers with random car make and model. |
| REQ-0010 | The system shall generate tests drivers with random passenger capacity. |
| REQ-0011 | The system shall generate tests drivers with random pet friendly vehicles as a boolean. |

## Getting Started
...

### Prerequisites
...

### Server Setup
Project is deployed and the UI is accessible through: 
http://10.172.55.21:7500/swagger

You can access our JSON structure for all the endpoints from the link above.

## Usage
...

## REST API Routes

### Request a Ride

Creates a new ride request in the system.

**Endpoint:**  
`POST http://10.172.55.21:7500/api/DriverManager/RequestDriver`

**Attributes**

- `ride_id` (integer) — Unique identifier for the ride.

**From the RideID we will need the following data from the database**
- `clientId` (integer) — Identifier for the client requesting the ride.  
- `timestamp` (ISO 8601 string) — Time when the ride request was created.  
- `pickup` (object) — Pickup location details.  
  - `latitude` (float)  
  - `longitude` (float)  
  - `address` (string)  
- `dropOff` (object) — Drop-off location details.  
  - `latitude` (float)  
  - `longitude` (float)  
  - `address` (string)   
- `rideInformation` (object) — Ride preferences.  
  - `carType` (string) — Type of car requested.  
  - `petFriendly` (boolean) — Whether pets are allowed.  


**Example**

```json
{
  "ride_id": 30,
  "clientId": 1,
  "timestamp": "2025-09-18T16:45:00Z",
  "pickup": {
    "latitude": 43.5448,
    "longitude": -80.2482,
    "address": "108 University Ave E, Waterloo"
  },
  "dropOff": {
    "latitude": 43.4723,
    "longitude": -80.5449,
    "address": "220 King St N, Waterloo"
  },
  "routeInformation": {
    "distance_km": 23.4,
    "duration_min": 28
  },
  "rideInformation": {
    "carType": "XL",
    "petFriendly": true
  },
}
```

### Complete a Ride

Marks a ride as completed and finalizes the trip.  
This endpoint provides details of the driver and vehicle once the ride has been successfully completed.

**Endpoint:**  
`POST http://10.172.55.21:7500/api/DriverManager/DriverComplete`

**Attributes**

- `rideId` (integer) — Unique identifier for the ride.  
- `driverId` (integer) — Identifier for the driver who completed the ride.  
- `current_location` (object) — Final reported location for the ride.  
  - `latitude` (float) — Latitude coordinate.  
  - `longitude` (float) — Longitude coordinate.  
  - `address` (string) — Street address of the final location.  

**Example**

```json
{
  "rideId": 123,
  "driverId": 123,
  "current_location": {
    "latitude": 60.123,
    "longitude": -70.123,
    "address": "108 University Ave E, Waterloo"
  }
}
```

## Docker
### Build docker image with docker [username]/[image name]
```docker build -t <username>/drivermanagement .```

### Push docker image to account [username]/[image name]
```docker push <username>/drivermanagement```

### Compose docker locally
```docker compose up --build```
