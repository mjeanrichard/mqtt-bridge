namespace MqttBridge.Models.Input.RemoconApi;

public enum DataItemIds
{
    PlantMode = 1,

    DhwMode = 2,

    DhwTemp = 3,

    OutsideTemp = 4,

    DhwStorageTemperature = 5,

    DhwTimeProgComfortTemp = 6,

    DhwTimeProgEconomyTemp = 7,

    Weather = 8,

    ChFlowTemp = 9,

    ZoneMode = 10, // 0x0000000A

    ZoneMeasuredTemp = 11, // 0x0000000B

    ZoneAntiFreezeTemp = 12, // 0x0000000C

    ZoneDesiredTemp = 13, // 0x0000000D

    ZoneComfortTemp = 14, // 0x0000000E

    ZoneEconomyTemp = 15, // 0x0000000F

    ZoneHeatRequest = 16, // 0x00000010

    IsZonePilotOn = 17, // 0x00000011

    AutomaticThermoregulation = 18, // 0x00000012

    Holiday = 19, // 0x00000013

    ZoneDeroga = 20, // 0x00000014

    ZoneFireplace = 21, // 0x00000015

    IsFlameOn = 22, // 0x00000016

    IsResistorOn = 23, // 0x00000017

    IsHeatingPumpOn = 24, // 0x00000018

    HeatingCircuitPressure = 25, // 0x00000019

    IsDhwBoost = 26, // 0x0000001A

    HybridTankChargeMode = 27, // 0x0000001B

    IsQuite = 28, // 0x0000001C

    HybridMode = 29, // 0x0000001D

    AntilegionellaOnOff = 30, // 0x0000001E

    AntilegionellaTemp = 31, // 0x0000001F

    AntilegionellaFreq = 32, // 0x00000020

    VirtTempSetpointHeat = 33, // 0x00000021

    VirtTempSetpointCool = 34, // 0x00000022

    VirtComfortTemp = 35, // 0x00000023

    VirtReducedTemp = 36, // 0x00000024

    VirtTempOffsetHeat = 37, // 0x00000025

    VirtTempOffsetCool = 38, // 0x00000026

    SlpMeasuredDhwTemp = 39, // 0x00000027

    SlpDesiredDhwTemp = 40, // 0x00000028

    SlpDhwComfortTemp = 41, // 0x00000029

    SlpDhwEconomyTemp = 42, // 0x0000002A

    SlpDhwGreenTemp = 43, // 0x0000002B

    SlpHcHpSupported = 44, // 0x0000002C

    SlpDhwProgOrManual = 45, // 0x0000002D

    SlpDhwOperMode = 46, // 0x0000002E

    SlpTemporaryBoostOnOff = 47, // 0x0000002F

    SlpAntilegionellaOnOff = 48, // 0x00000030

    SlpDhwTempMax = 49, // 0x00000031

    SlpDhwTempMin = 50, // 0x00000032

    SlpPreheatingOnOff = 51, // 0x00000033

    VmcState = 52, // 0x00000034

    VmcBoostMode = 53, // 0x00000035

    VmcImmediateBoost = 54, // 0x00000036

    VmcCovSetpoint = 55, // 0x00000037

    VmcHumiditySetpoint = 56, // 0x00000038

    BufferControlMode = 57, // 0x00000039

    BufferTimeProgComfortHeatingTemp = 58, // 0x0000003A

    BufferTimeProgComfortCoolingTemp = 59, // 0x0000003B

    BufferTimeProgEconomyHeatingTemp = 60, // 0x0000003C

    BufferTimeProgEconomyCoolingTemp = 61, // 0x0000003D

    ZoneComfortCoolingTemp = 62, // 0x0000003E

    ZoneEconomyCoolingTemp = 63, // 0x0000003F

    ZoneName = 64, // 0x00000040

    ChFlowSetpointTemp = 65, // 0x00000041

    PreHeatingOnOff = 66, // 0x00000042

    PreHeatingRunning = 67, // 0x00000043

    HeatingFlowTemp = 68, // 0x00000044

    CoolingFlowTemp = 69, // 0x00000045

    HeatingFlowOffset = 70, // 0x00000046

    CoolingFlowOffset = 71 // 0x00000047
}