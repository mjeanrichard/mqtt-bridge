using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MqttBridge.Configuration;
using MqttBridge.Converters;
using MqttBridge.Models;
using MqttBridge.Processors;
using MqttBridge.Subscription;

namespace MqttBridge;

public class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Configure().Build();
        await host.RunAsync();
    }

    public static HostApplicationBuilder Configure()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        IHostEnvironment env = builder.Environment;
        builder.Configuration
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

        builder.Services.Configure<MqttSettings>(
            builder.Configuration.GetRequiredSection(MqttSettings.Name));

        builder.Services.Configure<MongoDbSettings>(
            builder.Configuration.GetRequiredSection(MongoDbSettings.Name));

        builder.Services.Configure<InfluxSettings>(
            builder.Configuration.GetRequiredSection(InfluxSettings.Name));

        ConfigureServices(builder.Services);

        return builder;

    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton<IConverter<EnvSensorInfo>, EnvSensorInfoConverter>()
            .AddSingleton<IConverter<EnvSensorMeasurement>, EnvSensorMeasurementConverter>()
            .AddSingleton<IConverter<FroniusArchiveData>, FroniusArchiveDataConverter>()
            .AddSilverback()
            .WithConnectionToMessageBroker(options => options.AddMqtt())
            .AddEndpointsConfigurator<EndpointsConfigurator>();

        RegisterEntity<EnvSensorInfo>(services);
        RegisterEntity<EnvSensorMeasurement>(services);
        RegisterEntity<FroniusArchiveData>(services);
    }

    private static void RegisterEntity<TMessage>(IServiceCollection services) where TMessage : class
    {
        services.AddSilverback()
            .AddSingletonSubscriber<MongoProcessor<TMessage>>()
            .AddSingletonSubscriber<InfluxProcessor<TMessage>>();
    }
}