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
...

## Features
...

## Getting Started
...

### Prerequisites
...

### Server Setup
...

## Usage
...

## REST API Routes

### Request a Ride

Creates a new ride request in the system.

**Endpoint:**  
`POST https://api.driver.com/api/DriverManager/RequestDriver`

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
`POST https://api.client.com/api/DriverManager/DriverComplete`

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
