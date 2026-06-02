# AnvizWebSDK

A REST API built with ASP.NET Core that acts as a bridge between Anviz biometric devices and web applications. It connects directly to Anviz devices over TCP, exposes their data via HTTP endpoints, and persists device configuration in a local database.

---

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Configuration](#configuration)
- [Running the API](#running-the-api)
- [API Reference](#api-reference)
- [Data Models](#data-models)
- [Notes](#notes)

---

## Overview

Anviz devices (fingerprint readers, card readers, etc.) communicate over a proprietary TCP protocol. This project wraps the [MxLabs/Anviz](https://github.com/MxLabs/Anviz) .NET SDK to expose that functionality as a simple JSON REST API, making it easy to integrate with any web application regardless of stack.

**Key capabilities:**
- List all registered devices and their connection status
- Retrieve aggregate statistics (user count, record count, fingerprint count, etc.)
- Pull all enrolled users from a device
- Pull all attendance records from a device
- Look up a specific user on a specific device

---

## Project Structure

```
AnvizWebAPI/
├── AnvizWebSDK/
│   ├── Controllers/
│   │   ├── DevicesController.cs       # All device-related endpoints
│   │   └── WeatherForecastController.cs
│   ├── Models/
│   │   ├── Device.cs                  # DB model: { Id, IpAddress }
│   │   ├── DeviceWithStats.cs         # Response model for /devices/stats
│   │   └── DeviceRegistry.cs
│   ├── AppDbContext.cs                # EF Core DB context
│   ├── Program.cs
│   └── appsettings.Development.json
├── AnvizWebSDK.slnx
└── LICENSE.txt
```

---

## Requirements

- [.NET 8+](https://dotnet.microsoft.com/download)
- Access to one or more Anviz devices on the local network
- A supported database (configured via `AppDbContext` — SQLite or SQL Server)

---

## Configuration

Edit `AnvizWebSDK/appsettings.Development.json` to set your database connection string and any other environment-specific values.

Devices are stored in the database with their IP addresses. Add your device records to the `Devices` table before making requests — the API will connect to each device dynamically per request using the stored IP.

---

## Running the API

```bash
cd AnvizWebSDK
dotnet run
```

By default the API listens on:
```
https://localhost:7236
http://localhost:5000
```

---

## API Reference

All endpoints are under the base path `/devices`.

---

### `GET /devices`

Returns a list of all devices registered in the database, with a live connection attempt for each.

**Response** — array of `AnvizDevice` objects (raw SDK representation):
```json
[
  {
    "id": 1,
    "ipAddress": "192.168.1.100",
    "deviceBiometricType": 1
  }
]
```

---

### `GET /devices/stats`

Returns statistics for every reachable device by calling `GetDownloadInformation()` on each.

**Response** — array of `DeviceWithStats`:
```json
[
  {
    "id": 1,
    "ipAddress": "192.168.1.100",
    "deviceBiometricType": 1,
    "userAmount": 42,
    "fingerPrintAmount": 38,
    "passwordAmount": 2,
    "cardAmount": 12,
    "allRecordAmount": 3800,
    "newRecordAmount": 14
  }
]
```

Devices that are unreachable are excluded from the response.

---

### `GET /devices/{id}/users`

Returns all enrolled users on the specified device.

**Path parameter:** `id` — the device's database ID (integer)

**Response** — array of `UserInfo` (from Anviz.SDK):
```json
[
  {
    "id": 1,
    "name": "Diana Oliveira",
    ...
  }
]
```

---

### `GET /devices/{id}/records`

Returns all attendance records stored on the specified device.

**Path parameter:** `id` — the device's database ID (integer)

**Response** — array of `RecordInfo` (from Anviz.SDK):
```json
[
  {
    "id": 1,
    "time": "2026-04-01T08:46:36",
    "backupCode": 0,
    "type": 0
  }
]
```

| Field        | Type     | Description                                      |
|--------------|----------|--------------------------------------------------|
| `id`         | `ulong`  | The user's ID on the device                      |
| `time`       | `string` | ISO 8601 datetime of the punch                   |
| `backupCode` | `int`    | Anviz internal code                              |
| `type`       | `int`    | `0` = Check-In &nbsp; `1` = Check-Out            |

---

### `GET /devices/{id}/statistics`

Returns the raw `Statistic` object from `GetDownloadInformation()` for a single device.

**Path parameter:** `id` — the device's database ID (integer)

Returns `404 Not Found` if the device ID does not exist or the device is unreachable.

**Response:**
```json
{
  "userAmount": 42,
  "fingerPrintAmount": 38,
  "passwordAmount": 2,
  "cardAmount": 12,
  "allRecordAmount": 3800,
  "newRecordAmount": 14
}
```

---

### `GET /devices/{id}/users/{userId}`

Returns a single enrolled user from the specified device.

**Path parameters:**
- `id` — device database ID (integer)
- `userId` — the user's ID on the device (integer, cast to `ulong` internally)

Returns `404 Not Found` if the user is not found on that device.

---

## Data Models

### `Device`
Stored in the database. Represents a known Anviz device.

| Field       | Type     | Description              |
|-------------|----------|--------------------------|
| `Id`        | `int`    | Primary key              |
| `IpAddress` | `string` | Device IP on the network |

### `DeviceWithStats`
Response model combining device identity with download statistics.

| Field                 | Type     |
|-----------------------|----------|
| `Id`                  | `int`    |
| `IpAddress`           | `string` |
| `DeviceBiometricType` | `enum`   |
| `UserAmount`          | `ulong`  |
| `FingerPrintAmount`   | `ulong`  |
| `PasswordAmount`      | `ulong`  |
| `CardAmount`          | `ulong`  |
| `AllRecordAmount`     | `ulong`  |
| `NewRecordAmount`     | `ulong`  |

---

## Notes

- **Connection per request:** Each API call re-connects to the device via TCP. This works fine for low-frequency polling but is not designed for high-throughput real-time streaming.
- **SSL in development:** The API runs with a self-signed dev cert on `https://localhost:7236`. When consuming from another app on the same machine, disable SSL verification in your HTTP client for local development.
- **Record `id` field:** The `id` in a `RecordInfo` is the device's internal user identifier (`ulong`). It maps to the `Userid` field in the Anviz database (`V_Record.Userid` / `Userinfo.Userid`).
- **No date filtering on records:** `GET /devices/{id}/records` returns all records on the device. Date filtering must be done on the consuming side.
