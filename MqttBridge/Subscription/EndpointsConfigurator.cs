using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models.Input;
using Silverback.Messaging.Configuration;

namespace MqttBridge.Subscription;

public class EndpointsConfigurator : IBrokerClientsConfigurator
{
    private readonly MqttSettings _mqttSettings;

    public EndpointsConfigurator(IOptions<MqttSettings> mqttOptions)
    {
        _mqttSettings = mqttOptions.Value;
    }

    public void Configure(BrokerClientsConfigurationBuilder builder)
    {
        builder
            .AddMqttClients(endpoints =>
            {
                endpoints.ConnectViaTcp(_mqttSettings.Host, _mqttSettings.Port);
                if (_mqttSettings.UseTls)
                {
                    endpoints.EnableTls(tls => tls.AllowUntrustedCertificates());
                }
                else
                {
                    endpoints.DisableTls();
                }

                if (!string.IsNullOrWhiteSpace(_mqttSettings.Username))
                {
                    endpoints.WithCredentials(_mqttSettings.Username, _mqttSettings.Password);
                }

                endpoints.AddMqttSubscription<FroniusDailyMessage>("MB_FroniusArchive" + _mqttSettings.ClientSuffix, "devices/philoweg/pva/daily", false);
                endpoints.AddMqttSubscription<EnvSensorInfoMessage>("MB_EnvSensorInfo" + _mqttSettings.ClientSuffix, "devices/philoweg/+/info", true);
                endpoints.AddMqttSubscription<EnvSensorMeasurement>("MB_EnvSensorMeasurement" + _mqttSettings.ClientSuffix, "devices/philoweg/+/sensors/+", true);
                endpoints.AddMqttSubscription<GasMeterMessage>("MB_GasMeter" + _mqttSettings.ClientSuffix, "devices/philoweg/gas/+", false);
                endpoints.AddMqttSubscription<MqttGatewayDeviceIdMessage>("MB_MqttGateway" + _mqttSettings.ClientSuffix, "devices/OMG_LILYGO/LORAtoMQTT/+", false);
                endpoints.AddMqttSubscription<MqttGatewayGenericMessage>("MB_MqttGateway2" + _mqttSettings.ClientSuffix, "devices/OMG_LILYGO/LORAtoMQTT", false);
                endpoints.AddMqttSubscription<HomeAssistantMessage>("MB_HA_BinarySensor" + _mqttSettings.ClientSuffix, "homeassistant/statestream/binary_sensor/#", false);
            });
    }
}