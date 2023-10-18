using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MqttBridge.Configuration;
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

        builder.Services.Configure<PrometheusSettings>(
            builder.Configuration.GetRequiredSection(PrometheusSettings.Name));

        ConfigureServices(builder.Services);

        SetupMongoDb();

        return builder;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSilverback()
            .AddScopedSubscriber<FroniusDailySubscriber>()
            .AddScopedSubscriber<EnvSensorSubscriber>()
            .AddScopedSubscriber<EnvSensorInfoSubscriber>()
            .AddScopedSubscriber<GasMeterSubscriber>()
            .AddSingletonSubscriber<MongoProcessor>()
            .AddSingletonSubscriber<PrometheusProcessor>()
            .WithConnectionToMessageBroker(options => options.AddMqtt())
            .AddEndpointsConfigurator<EndpointsConfigurator>();
    }

    private static void SetupMongoDb()
    {
        ConventionPack pack = new()
        {
            new EnumRepresentationConvention(BsonType.String)
        };

        ConventionRegistry.Register("EnumStringConvention", pack, t => true);
    }
}