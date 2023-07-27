using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models.DataPoints;

namespace MqttBridge.Processors;

public class InfluxProcessor : IProcessor
{
    public static void Register(IServiceCollection services)
    {
        services.AddSingleton<IProcessor, MongoProcessor>();
    }

    private readonly InfluxSettings _influxSettings;
    private readonly InfluxDBClient _influxClient;
    private readonly WriteApiAsync _writeApi;

    public InfluxProcessor(IOptions<InfluxSettings> influxSettings)
    {
        _influxSettings = influxSettings.Value;
        _influxClient = new InfluxDBClient(_influxSettings.Url, _influxSettings.Token);
        _writeApi = _influxClient.GetWriteApiAsync();
    }

    public async Task ProcessAsync(IReadOnlyCollection<MetricDataPoint> dataPoints)
    {
        List<PointData> pointData = CreatePointData(dataPoints);
        if (pointData.Count == 0)
        {
            return;
        }

        await _writeApi.WritePointsAsync(pointData, _influxSettings.Bucket, _influxSettings.Organization);
    }

    private List<PointData> CreatePointData(IReadOnlyCollection<MetricDataPoint> dataPoints)
    {
        List<PointData> pointData = new(dataPoints.Count);

        foreach (MetricDataPoint dataPoint in dataPoints)
        {
            PointData.Builder lineBuilder = PointData.Builder.Measurement(dataPoint.MetricGroup);

            foreach (IMetricKeyValue label in dataPoint.Labels)
            {
                lineBuilder = lineBuilder.Tag(label.Name, label.StringValue);
            }

            foreach (IMetricKeyValue value in dataPoint.Values)
            {
                AddField(value, lineBuilder);
            }

            pointData.Add(lineBuilder.ToPointData());
        }

        return pointData;
    }

    private void AddField(IMetricKeyValue metricValue, PointData.Builder builder)
    {
        switch (metricValue)
        {
            case IntegerKeyValue integerValue:
                builder.Field(metricValue.Name, integerValue.Value);
                return;
            case DoubleKeyValue doubleValue:
                builder.Field(metricValue.Name, doubleValue.Value);
                return;
            default:
                builder.Field(metricValue.Name, metricValue.StringValue);
                return;
        }
    }
}