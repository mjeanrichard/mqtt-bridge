using Microsoft.Extensions.Logging.Abstractions;
using MqttBridge.Models.Data.GasMeter;
using MqttBridge.Models.Input;
using MqttBridge.Subscription;
using NSubstitute;
using Shouldly;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Tests;

public class GasMeterSubscriberTests
{
    private static GasMeterMessage CreateMessage(double volume) =>
        new()
        {
            Id = "21072718",
            Address = 3,
            AccessNumber = 1,
            BatteryMillivolts = 2363,
            Manufacturer = 7910,
            Medium = 3,
            Milliseconds = 5117,
            Signature = "0",
            Status = "0",
            Version = 54,
            Volume = volume
        };

    [Test]
    public async Task ProcessAsync_PublishesMappedData_WhenVolumeIsPositive()
    {
        IPublisher publisher = Substitute.For<IPublisher>();
        List<GasMeterData> results = new();
        publisher.PublishAsync(Arg.Do<List<GasMeterData>>(list => results.AddRange(list))).Returns(Task.CompletedTask);

        GasMeterSubscriber subscriber = new(NullLogger<GasMeterSubscriber>.Instance, publisher);
        await subscriber.ProcessAsync(CreateMessage(6189.81));

        GasMeterData data = results.ShouldHaveSingleItem();
        data.DeviceId.ShouldBe("21072718");
        data.Battery.ShouldBe(2.363, 1e-9); // millivolts -> volts
        data.Volume.ShouldBe(6189.81);
        data.AccessNumber.ShouldBe(1);
        data.Manufacturer.ShouldBe(7910);
    }

    [Test]
    public async Task ProcessAsync_DoesNotPublish_WhenVolumeIsZero()
    {
        IPublisher publisher = Substitute.For<IPublisher>();
        GasMeterSubscriber subscriber = new(NullLogger<GasMeterSubscriber>.Instance, publisher);

        await subscriber.ProcessAsync(CreateMessage(0));

        await publisher.DidNotReceive().PublishAsync(Arg.Any<List<GasMeterData>>());
    }
}
