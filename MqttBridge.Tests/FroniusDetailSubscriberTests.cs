using Microsoft.Extensions.Logging.Abstractions;
using MqttBridge.Models.Data.Pva;
using MqttBridge.Models.Input;
using MqttBridge.Subscription;
using NSubstitute;
using Shouldly;
using Silverback.Messaging.Publishing;

namespace MqttBridge.Tests;

public class FroniusDetailSubscriberTests
{
    private static FroniusDetailSubscriber CreateSubscriber() =>
        new(Substitute.For<IPublisher>(), NullLogger<FroniusDetailSubscriber>.Instance);

    private static PowerDataPoint Point(int seconds, double? produced = null, double? directlyConsumed = null) =>
        new()
        {
            Seconds = seconds,
            TimestampUtc = DateTime.UnixEpoch.AddSeconds(seconds),
            Produced = produced,
            DirectlyConsumed = directlyConsumed
        };

    private static FroniusDailyMessage MessageWith(params PowerDataPoint[] points)
    {
        FroniusDailyMessage message = new();
        message.Data.AddRange(points);
        return message;
    }

    [Test]
    public void Map_ConvertsInstantWattsToWattHours()
    {
        FroniusArchiveData data = CreateSubscriber().Map(MessageWith(Point(100, produced: 10))).ShouldHaveSingleItem();

        // ToWattHours multiplies the instantaneous power sample by the 12-second sampling interval.
        data.Instant.Produced.ShouldBe(120.0);
    }

    [Test]
    public void Map_DropsNegativeInstantValues()
    {
        FroniusArchiveData data = CreateSubscriber().Map(MessageWith(Point(100, produced: -5))).ShouldHaveSingleItem();

        data.Instant.Produced.ShouldBeNull();
    }

    [Test]
    public void Map_AccumulatesCumulativePerDayInSecondsOrder()
    {
        // Deliberately supplied out of order to verify the subscriber sorts by Seconds.
        List<FroniusArchiveData> data = CreateSubscriber()
            .Map(MessageWith(Point(200, directlyConsumed: 3), Point(100, directlyConsumed: 2)))
            .ToList();

        data.Count.ShouldBe(2);
        data[0].Seconds.ShouldBe(100);
        data[0].CumulativePerDay.DirectlyConsumed.ShouldBe(2.0);
        data[1].Seconds.ShouldBe(200);
        data[1].CumulativePerDay.DirectlyConsumed.ShouldBe(5.0);
    }

    [Test]
    public void Map_StopsAtBrokenInstantValue()
    {
        // 1000 W * 12 s = 12000 Wh, above the 10_000 instant guard, so mapping stops before this point.
        List<FroniusArchiveData> data = CreateSubscriber()
            .Map(MessageWith(
                Point(100, directlyConsumed: 10),
                Point(200, directlyConsumed: 1000),
                Point(300, directlyConsumed: 10)))
            .ToList();

        data.Count.ShouldBe(1);
        data[0].Seconds.ShouldBe(100);
    }

    [Test]
    public void Map_ReturnsEmpty_WhenNoDataPoints()
    {
        CreateSubscriber().Map(new FroniusDailyMessage()).ShouldBeEmpty();
    }
}
