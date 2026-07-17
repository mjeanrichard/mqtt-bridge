# mqtt-bridge

A .NET 10 console app that bridges home/solar IoT telemetry (installation codenamed **"philoweg"**) into storage (MongoDB) and monitoring (Prometheus).

## Architecture

Built around **Silverback 5.3.3** as an in-process message bus, fed over MQTT (`Silverback.Integration.MQTT`).

**Push pipeline (MQTT):**
1. `Subscription/EndpointsConfigurator` registers MQTT subscriptions to device topics. `Subscription/SilverbackExtensions.AddMqttSubscription` wires each topic to JSON deserialization (optionally camelCase).
2. Scoped **Subscribers** (`Subscription/*Subscriber.cs`) receive deserialized `Models/Input/*` messages, `Map` them to `Models/Data/*` models, and re-publish onto the internal bus.
3. Two singleton **Processors** (`Processors/`) consume the data models:
   - `MongoProcessor` — persists to MongoDB (via `Scrapers/MongoClientFactory`, `MongoScraper`).
   - `PrometheusProcessor` — pushes metrics via `PrometheusClient`.

**Pull pipeline (solar API):** `Scrapers/RemoconScraperService` periodically calls a solar API through `Clients/RemoconClient` (+ `AuthenticatingRestClient`).

**Data sources:** Fronius PV inverter archive (`devices/philoweg/pva/daily`), environmental sensors + info, gas meter, OpenMqttGateway / LoRa "PlantSense" soil sensors, Home Assistant statestream binary sensors.

## Run modes

CLI flags (System.CommandLine.DragonFruit, `Program.Main`):

- `--mqtt` — listen continuously on MQTT.
- `--remocon` — scrape the solar API.
- `--republish` — replay MongoDB → Prometheus via `ReprocessWorker`; combine with `--delete`, `--startDate`, `--endDate`.

## Configuration

`appsettings.json` + `appsettings.{Environment}.json` + environment variables, bound to `MqttSettings`, `MongoDbSettings`, `PrometheusSettings`, `RemoconSettings` (`Configuration/`). MongoDB uses a string enum convention, `IgnoreExtraElements`, and a custom `Processors/BsonDateOnlySerializer`.

## Building & testing

The repo root contains **both** `MqttBridge.sln` and `docker-compose.dcproj`, so a bare `dotnet build` fails with MSB1011. Always target the solution:

```
dotnet build MqttBridge.sln
dotnet test  MqttBridge.sln
```

Tests use **NUnit + Shouldly + NSubstitute**.
