namespace MqttBridge.Models.Data.Sensor;

public enum ResetReason
{
    Unknown,    //!< Reset reason can not be determined
    PowerOn,    //!< Reset due to power-on event
    External,        //!< Reset by external pin (not applicable for ESP32)
    Software,         //!< Software reset via esp_restart
    Panic,      //!< Software reset due to exception/panic
    WatchdogInterrupt,    //!< Reset (software or hardware) due to interrupt watchdog
    WatchdogTask,   //!< Reset due to task watchdog
    WatchdogOther,        //!< Reset due to other watchdogs
    DeepSleep,  //!< Reset after exiting deep sleep mode
    Brownout,   //!< Brownout reset (software or hardware)
    Sdio,       //!< Reset over SDIO
    Usb,        //!< Reset by USB peripheral
    JTag,       //!< Reset by JTAG
}