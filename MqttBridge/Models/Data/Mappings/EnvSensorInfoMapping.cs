using Microsoft.Extensions.Logging;

namespace MqttBridge.Models.Data.Mappings;

public static class EnvSensorInfoMapping
{
    public static NtpState MapNtpState(int ntpState, ILogger logger)
    {
        switch (ntpState)
        {
            case 0: return NtpState.Reset;
            case 1: return NtpState.Completed;
            case 2: return NtpState.InProgress;
            default:
                logger.LogWarning($"Unknown NtpState value {ntpState}.");
                return NtpState.Unknown;
        }
    }

    public static WifiState MapWifiState(int wifiState, ILogger logger)
    {
        switch (wifiState)
        {
            case 0: return WifiState.Disconnected;
            case 1: return WifiState.Connecting;
            case 2: return WifiState.Connected;
            default:
                logger.LogWarning($"Unknown WifiState value {wifiState}.");
                return WifiState.Unknown;
        }
    }

    public static ResetReason MapResetReason(int resetReason, ILogger logger)
    {
        switch (resetReason)
        {
            case 0: return ResetReason.Unknown;
            case 1: return ResetReason.PowerOn;
            case 2: return ResetReason.External;
            case 3: return ResetReason.Software;
            case 4: return ResetReason.Panic;
            case 5: return ResetReason.WatchdogInterrupt;
            case 6: return ResetReason.WatchdogTask;
            case 7: return ResetReason.WatchdogOther;
            case 8: return ResetReason.DeepSleep;
            case 9: return ResetReason.Brownout;
            case 10: return ResetReason.Sdio;
            case 11: return ResetReason.Usb;
            case 12: return ResetReason.JTag;
            default:
                logger.LogWarning($"Unknown ResetReason value {resetReason}.");
                return ResetReason.Unknown;
        }
    }
}