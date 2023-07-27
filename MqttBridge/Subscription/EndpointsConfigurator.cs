using System.Text.Json;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Configuration.Mqtt;
using Silverback.Messaging.Serialization;

namespace MqttBridge.Subscription;

public class EndpointsConfigurator : IEndpointsConfigurator
{
    private readonly MqttSettings _mqttSettings;

    public EndpointsConfigurator(IOptions<MqttSettings> mqttOptions)
    {
        _mqttSettings = mqttOptions.Value;
    }

    public void Configure(IEndpointsConfigurationBuilder builder)
    {
        builder
            .AddMqttEndpoints(
                endpoints => endpoints

                    // Configure the client options
                    .Configure(
                        config =>
                        {
                            config
                                .WithClientId("samples.basic.consumer")
                                .ConnectViaTcp(client =>
                                {
                                    client.Server = _mqttSettings.Host;
                                    client.Port = _mqttSettings.Port;
                                    client.TlsOptions = new MqttClientTlsOptions() { UseTls = _mqttSettings.UseTls, CertificateValidationHandler = args => true };
                                });

                            if (!string.IsNullOrWhiteSpace(_mqttSettings.Username))
                            {
                                config.WithCredentials(_mqttSettings.Username, _mqttSettings.Password);
                            }
                        }
                    )

                    // Consume the samples/basic topic
                    .AddMqttInbound<EnvSensorInfo>("devices/philoweg/+/info", "MB_EnvSensorInfo")
                    .AddMqttInbound<EnvSensorMeasurement>("devices/philoweg/+/sensors/+", "MB_EnvSensorMeasurement"));
    }
}

public static class SilverbackExtensions
{
    private static JsonMessageSerializer<TMessage> CreateSerializer<TMessage>()
    {
        return new JsonMessageSerializer<TMessage>()
        {
            Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        };
    }

    public static IMqttEndpointsConfigurationBuilder AddMqttInbound<TMessage>(this IMqttEndpointsConfigurationBuilder builder, string topic, string clientId)
    {
        builder.AddInbound(
            endpoint => endpoint
                .Configure(builder => builder.WithClientId(clientId))
                .ConsumeFrom(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .DisableMessageValidation()
                .UseSerializer(CreateSerializer<TMessage>())
                .OnError(policy => policy.Skip()));

        return builder;
    }
}