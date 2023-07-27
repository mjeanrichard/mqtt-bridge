using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttBridge.Configuration;
using MqttBridge.Converters;
using MqttBridge.Models;
using MqttBridge.Processors;
using MqttBridge.Subscription;

namespace MqttBridge;

internal class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<MqttSettings>(
            builder.Configuration.GetRequiredSection(MqttSettings.Name));

        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetRequiredSection(MongoDbSettings.Name));

        builder.Services.Configure<InfluxSettings>(
            builder.Configuration.GetRequiredSection(InfluxSettings.Name));

        ConfigureServices(builder.Services);

        using IHost host = builder.Build();
        await host.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSilverback()
            .WithConnectionToMessageBroker(options => options.AddMqtt())
            .AddEndpointsConfigurator<EndpointsConfigurator>()
            .AddSingletonSubscriber<ConvertingSubscriber<EnvSensorInfoConverter, EnvSensorInfo>>();

        services.AddSingleton<EnvSensorInfoConverter>();

        RegisterConverter<EnvSensorInfo, EnvSensorInfoConverter>(services);
        RegisterConverter<EnvSensorMeasurement, EnvSensorMeasurementConverter>(services);

        services.AddSingleton<IProcessor, MongoProcessor>();
        services.AddSingleton<IProcessor, InfluxProcessor>();
    }

    private static void RegisterConverter<TEntity, TConverter>(IServiceCollection services)
        where TConverter : class, IConverter<TEntity>
        where TEntity : class
    {
        services.AddSilverback().AddSingletonSubscriber<ConvertingSubscriber<TConverter, TEntity>>();

        services.AddSingleton<TConverter>();
    }
}