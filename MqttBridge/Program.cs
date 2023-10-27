using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MqttBridge.Clients;
using MqttBridge.Configuration;
using MqttBridge.Processors;
using MqttBridge.Scrapers;
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

        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.ColorBehavior = LoggerColorBehavior.Disabled;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss ";
        });

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

        builder.Services.Configure<RemoconSettings>(
            builder.Configuration.GetRequiredSection(RemoconSettings.Name));

        ConfigureServices(builder.Services);

        SetupMongoDb();

        return builder;
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<RemoconClient>();
        services.AddHostedService<RemoconScraperService>();

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

    public static void SetupMongoDb()
    {
        ConventionPack pack = new()
        {
            new EnumRepresentationConvention(BsonType.String)
        };

        ConventionRegistry.Register("EnumStringConvention", pack, t => true);
        BsonSerializer.RegisterSerializer(BsonDateOnlySerializer.Instance);
    }
}