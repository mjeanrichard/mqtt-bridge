using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using MqttBridge.Converters;
using MqttBridge.Models;
using MqttBridge.Models.DataPoints;

using NSubstitute;

using Silverback.Messaging.Messages;

namespace MqttBridge.Tests;

public class EnvSensorInfoConverterTests
{
    private static IInboundEnvelope<EnvSensorInfo> CreateEnvelope(EnvSensorInfo info)
    {
        IInboundEnvelope<EnvSensorInfo> envelope = Substitute.For<IInboundEnvelope<EnvSensorInfo>>();
        envelope.Message.Returns(info);
        return envelope;
    }

    [Test]
    public async Task ConvertAsync_ConvertsJson()
    {
        EnvSensorInfo info = new()
        {
            Timestamp = 1690228102,
            Name = "test",
            Mac = "34:94:54:e6:fe:ac",
            Ip = "192.168.5.28",
            Rssi = 213,
            FwVersion = 53,
            SdState = 0,
            ConnectTime = 1113,
            ResetReason = 8,
            PowerGood = 1,
            NtpState = 0,
            Wifi = 2,
            Mqtt = 1
        };

        EnvSensorInfoConverter converter = new(NullLogger<EnvSensorInfoConverter>.Instance);
        IEnumerable<MetricDataPoint?> dataPoints = await converter.ConvertAsync(CreateEnvelope(info), CancellationToken.None);
        
        dataPoints.Should().HaveCount(1);
        MetricDataPoint dataPoint = dataPoints.Single();
        dataPoint.Should().NotBeNull();
        dataPoint!.MetricGroup.Should().Be("EnvSensorInfo");
        dataPoint.DeviceId.Should().Be("test");
        dataPoint.Timestamp.Should().Be(new DateTime(2023, 07, 24, 19, 48, 22, DateTimeKind.Utc));
        dataPoint.Labels.Should().Contain(value => value.Name == "Ip");
    }
}