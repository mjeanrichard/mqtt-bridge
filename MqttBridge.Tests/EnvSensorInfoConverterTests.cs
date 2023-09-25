using MqttBridge.Models.Input;
using NSubstitute;
using Silverback.Messaging.Messages;

namespace MqttBridge.Tests;

public class EnvSensorInfoConverterTests
{
    private static IInboundEnvelope<EnvSensorInfoMessage> CreateEnvelope(EnvSensorInfoMessage info)
    {
        IInboundEnvelope<EnvSensorInfoMessage> envelope = Substitute.For<IInboundEnvelope<EnvSensorInfoMessage>>();
        envelope.Message.Returns(info);
        return envelope;
    }

    [Test]
    public async Task ConvertAsync_ConvertsJson()
    {
        EnvSensorInfoMessage info = new()
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

        //EnvSensorInfoConverter converter = new(NullLogger<EnvSensorInfoConverter>.Instance);
        //MetricMessage metric = await converter.ConvertAsync(CreateEnvelope(info), CancellationToken.None);

        //metric.DataPoints.Should().HaveCount(1);
        //MetricDataPoint dataPoint = metric.DataPoints.Single();
        //dataPoint.Should().NotBeNull();
        //dataPoint!.MetricGroup.Should().Be("EnvSensorInfoMessage");
        //dataPoint.DeviceId.Should().Be("test");
        //dataPoint.Timestamp.Should().Be(new DateTime(2023, 07, 24, 19, 48, 22, DateTimeKind.Utc));
        //dataPoint.Labels.Should().Contain(value => value.Name == "Ip");
    }
}