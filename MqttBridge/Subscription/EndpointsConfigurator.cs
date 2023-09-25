using System.Text.Json;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models;
using MqttBridge.Models.Input;
using MQTTnet.Client;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Serialization;

namespace MqttBridge.Subscription;

public class EndpointsConfigurator : IEndpointsConfigurator
{
    private static JsonMessageSerializer<TMessage> CreateCamelCaseSerializer<TMessage>()
    {
        return new JsonMessageSerializer<TMessage>()
        {
            Options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        };
    }

    private static JsonMessageSerializer<TMessage> CreateDefaultSerializer<TMessage>()
    {
        return new JsonMessageSerializer<TMessage>();
    }

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
                    .AddMqttInbound<FroniusDailyModel>("devices/philoweg/pva/daily", "MB_FroniusArchive" + _mqttSettings.ClientSuffix, CreateDefaultSerializer<FroniusDailyModel>())
                    //.AddMqttInbound<EnvSensorInfo>("devices/philoweg/+/info", "MB_EnvSensorInfo" + _mqttSettings.ClientSuffix, CreateCamelCaseSerializer<EnvSensorInfo>())
                    .AddMqttInbound<EnvSensorMeasurement>("devices/philoweg/+/sensors/+", "MB_EnvSensorMeasurement" + _mqttSettings.ClientSuffix, CreateCamelCaseSerializer<EnvSensorMeasurement>()));
    }
}