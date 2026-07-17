using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MqttBridge.Models.Data.Sensor;
using MqttBridge.Models.Input;
using MqttBridge.Subscription;
using NSubstitute;
using Shouldly;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Tests;

public class EnvSensorSubscriberTests
{
    private static EnvSensorSubscriber CreateSubscriber() =>
        new(NullLogger<EnvSensorSubscriber>.Instance, Substitute.For<IPublisher>());

    private static EnvSensorMeasurement CreateMeasurement(string name, string device, double value, long timestamp = 1690228102) =>
        new()
        {
            Name = name,
            Device = device,
            Timestamp = timestamp,
            Measurements = new Dictionary<string, JsonElement>
            {
                ["value"] = JsonSerializer.SerializeToElement(value)
            }
        };

    [Test]
    public void Map_MapsTemperatureWithUnitAndTimestamp()
    {
        EnvSensorData data = CreateSubscriber()
            .Map(CreateMeasurement("temp", "philoweg", 21.5))
            .ShouldHaveSingleItem();

        data.Name.ShouldBe("temp");
        data.Device.ShouldBe("philoweg");
        data.Value.ShouldBe(21.5);
        data.Unit.ShouldBe(Units.DegreesCelsius);
        data.Type.ShouldBe(MeasurementType.Temperatur);
        data.IsTestDevice.ShouldBeFalse();
        data.TimestampUtc.ShouldBe(new DateTime(2023, 07, 24, 19, 48, 22, DateTimeKind.Utc));
    }

    [Test]
    public void Map_ConvertsDistanceFromMillimetersToMeters()
    {
        EnvSensorData data = CreateSubscriber()
            .Map(CreateMeasurement("distance", "philoweg", 2000))
            .ShouldHaveSingleItem();

        data.Value.ShouldBe(2.0);
        data.Unit.ShouldBe(Units.Meter);
        data.Type.ShouldBe(MeasurementType.Distance);
    }

    [Test]
    public void Map_FlagsTestDevices_CaseInsensitively()
    {
        EnvSensorData data = CreateSubscriber()
            .Map(CreateMeasurement("humidity", "TestSensor01", 55))
            .ShouldHaveSingleItem();

        data.IsTestDevice.ShouldBeTrue();
    }

    [Test]
    public void Map_ReturnsEmpty_WhenMeasurementsAreNull()
    {
        EnvSensorMeasurement message = new() { Name = "temp", Device = "philoweg" };

        CreateSubscriber().Map(message).ShouldBeEmpty();
    }

    [Test]
    public void Map_UnknownMeasurementName_StillCarriesRawValue()
    {
        // Unknown names are logged as warnings but must not throw and must preserve the raw value.
        EnvSensorData data = CreateSubscriber()
            .Map(CreateMeasurement("frobnicate", "philoweg", 7))
            .ShouldHaveSingleItem();

        data.Value.ShouldBe(7);
    }
}
