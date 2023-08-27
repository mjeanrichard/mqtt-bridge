using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models;
using MQTTnet.Client;
using Silverback.Messaging.Configuration;

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