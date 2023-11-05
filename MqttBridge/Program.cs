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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mqtt">Run continuously and listen on mqtt for messages.</param>
    /// <param name="remocon">Periodically load data from the solar API.</param>
    /// <param name="republish">Republish data from MongoDb to Prometheus</param>
    /// <param name="startDate">First date to scrape Data from.</param>
    /// <param name="endDate">Last Date to publish Data from</param>
    /// <returns></returns>
    private static async Task Main(bool mqtt, bool remocon, bool republish, DateOnly? startDate, DateOnly? endDate)
    {
        CommandLineOptions options = new() { Mqtt = mqtt, Remocon = remocon, Republish = republish, StartDate = startDate, EndDate = endDate };

        using IHost host = Configure(options).Build();

        ILogger logger = host.Services.GetRequiredService<ILogger>();
        if (republish)
        {
            logger.LogInformation("Republishing Data.");
            ReprocessWorker worker = host.Services.GetRequiredService<ReprocessWorker>();
            await worker.RunAsync(CancellationToken.None);
            logger.LogInformation("Done republishing Data.");
        }
        else
        {
            logger.LogInformation($"Starting workers (Mqtt: {options.Mqtt}, Remocon: {options.Remocon}).");
            await host.RunAsync();
        }
    }

    private static HostApplicationBuilder Configure(CommandLineOptions commandLineOptions)
    {
        HostApplicationBuilder host = ConfigureHost(commandLineOptions);

        if (commandLineOptions.Remocon)
        {
            host.Services.AddHostedService<RemoconScraperService>();
        }
        return host;
    }

    public static HostApplicationBuilder ConfigureHost(CommandLineOptions commandLineOptions)
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

        ConfigureServices(builder.Services, builder.Configuration, commandLineOptions);

        SetupMongoDb();

        return builder;
    }

    private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, CommandLineOptions commandLineOptions)
    {
        services.AddSingleton<RemoconClient>();
        services.AddSingleton<MongoClientFactory>();
        services.AddSingleton<MongoScraper>();
        services.AddSingleton(commandLineOptions);
        services.AddSingleton<ReprocessWorker>();

        services
            .AddSilverback()
            .AddScopedSubscriber<FroniusDailySubscriber>()
            .AddScopedSubscriber<EnvSensorSubscriber>()
            .AddScopedSubscriber<EnvSensorInfoSubscriber>()
            .AddScopedSubscriber<GasMeterSubscriber>()
            .AddSingletonSubscriber<MongoProcessor>()
            .AddSingletonSubscriber<PrometheusProcessor>();


        MqttSettings mqttSettings = new();
        configuration.GetRequiredSection(MqttSettings.Name).Bind(mqttSettings);
        if (commandLineOptions.Mqtt && mqttSettings.Enabled)
        {
            services.AddSilverback()
                .WithConnectionToMessageBroker(options => options.AddMqtt())
                .AddEndpointsConfigurator<EndpointsConfigurator>();
        }
    }

    public static void SetupMongoDb()
    {
        ConventionPack pack = new()
        {
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreExtraElementsConvention(true)
        };

        ConventionRegistry.Register("EnumStringConvention", pack, t => true);
        BsonSerializer.RegisterSerializer(BsonDateOnlySerializer.Instance);
    }
}