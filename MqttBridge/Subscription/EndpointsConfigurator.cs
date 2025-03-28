using System.Text.Json;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models.Input;
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
                            config.ConnectViaTcp(_mqttSettings.Host, _mqttSettings.Port);
                            if (_mqttSettings.UseTls)
                            {
                                config.EnableTls(tlsOptionBuilder => tlsOptionBuilder.WithAllowUntrustedCertificates());
                            }
                            else
                            {
                                config.DisableTls();
                            }

                            if (!string.IsNullOrWhiteSpace(_mqttSettings.Username))
                            {
                                config.WithCredentials(_mqttSettings.Username, _mqttSettings.Password);
                            }
                        }
                    )
                    .AddMqttInbound<FroniusDailyMessage>("devices/philoweg/pva/daily", "MB_FroniusArchive" + _mqttSettings.ClientSuffix, CreateDefaultSerializer<FroniusDailyMessage>())
                    .AddMqttInbound<EnvSensorInfoMessage>("devices/philoweg/+/info", "MB_EnvSensorInfo" + _mqttSettings.ClientSuffix, CreateCamelCaseSerializer<EnvSensorInfoMessage>())
                    .AddMqttInbound<EnvSensorMeasurement>("devices/philoweg/+/sensors/+", "MB_EnvSensorMeasurement" + _mqttSettings.ClientSuffix, CreateCamelCaseSerializer<EnvSensorMeasurement>())
                    .AddMqttInbound<GasMeterMessage>("devices/philoweg/gas/+", "MB_GasMeter" + _mqttSettings.ClientSuffix, CreateDefaultSerializer<GasMeterMessage>())
                    .AddMqttInbound<MqttGatewayDeviceIdMessage>("devices/OMG_LILYGO/LORAtoMQTT/+", "MB_MqttGateway" + _mqttSettings.ClientSuffix, CreateDefaultSerializer<MqttGatewayDeviceIdMessage>())
                    .AddMqttInbound<MqttGatewayGenericMessage>("devices/OMG_LILYGO/LORAtoMQTT", "MB_MqttGateway2" + _mqttSettings.ClientSuffix, CreateDefaultSerializer<MqttGatewayGenericMessage>())
                    .AddMqttInbound<HomeAssistantMessage>("homeassistant/statestream/binary_sensor/#", "MB_HA_BinarySensor" + _mqttSettings.ClientSuffix, CreateDefaultSerializer<HomeAssistantMessage>())
            );
    }
}