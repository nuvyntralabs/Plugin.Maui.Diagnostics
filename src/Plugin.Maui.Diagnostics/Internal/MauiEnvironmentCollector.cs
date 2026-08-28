#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
#endif

namespace Plugin.Maui.Diagnostics;

sealed class MauiEnvironmentCollector : IEnvironmentCollector
{
    public DeviceSnapshot Collect()
    {
        var builder = new DeviceSnapshotBuilder
        {
            DebuggerAttached = Debugger.IsAttached,
            AppUsedMemoryBytes = GC.GetTotalMemory(false)
        };

#if ANDROID || IOS
        Try(() => CollectDevice(builder));
        Try(() => CollectApp(builder));
        Try(() => CollectBattery(builder));
        Try(() => CollectNetwork(builder));
#endif
        return builder.Build();
    }

#if ANDROID || IOS
    static void CollectDevice(DeviceSnapshotBuilder builder)
    {
        builder.Manufacturer = DeviceInfo.Current.Manufacturer;
        builder.Model = DeviceInfo.Current.Model;
        builder.Platform = DeviceInfo.Current.Platform.ToString();
        builder.OsVersion = DeviceInfo.Current.VersionString;
        builder.Idiom = DeviceInfo.Current.Idiom.ToString();
        builder.IsVirtualDevice = DeviceInfo.Current.DeviceType == DeviceType.Virtual;
    }

    static void CollectApp(DeviceSnapshotBuilder builder)
    {
        builder.AppVersion = AppInfo.Current.VersionString;
        builder.AppBuild = AppInfo.Current.BuildString;
        builder.PackageName = AppInfo.Current.PackageName;
    }

    static void CollectBattery(DeviceSnapshotBuilder builder)
    {
        var level = Battery.ChargeLevel;
        builder.BatteryPercent = level < 0 ? null : Math.Clamp(level * 100d, 0, 100);
        builder.BatteryState = Battery.State switch
        {
            BatteryState.Charging => BatteryChargeState.Charging,
            BatteryState.Discharging => BatteryChargeState.Discharging,
            BatteryState.Full => BatteryChargeState.Full,
            BatteryState.NotCharging => BatteryChargeState.NotCharging,
            BatteryState.NotPresent => BatteryChargeState.NotPresent,
            _ => BatteryChargeState.Unknown
        };
        builder.EnergySaverOn = Battery.EnergySaverStatus switch
        {
            EnergySaverStatus.On => true,
            EnergySaverStatus.Off => false,
            _ => null
        };
    }

    static void CollectNetwork(DeviceSnapshotBuilder builder)
    {
        var access = Connectivity.Current.NetworkAccess;
        builder.HasNetwork = access switch
        {
            NetworkAccess.None => false,
            NetworkAccess.Unknown => null,
            _ => true
        };
        builder.HasInternet = access switch
        {
            NetworkAccess.Internet or NetworkAccess.ConstrainedInternet => true,
            NetworkAccess.Unknown => null,
            _ => false
        };
        builder.IsConstrained = access is NetworkAccess.ConstrainedInternet;

        var profiles = Connectivity.Current.ConnectionProfiles.Select(static profile => profile.ToString()).ToArray();
        builder.ConnectionProfiles.AddRange(profiles);
        builder.IsExpensive = profiles.Contains("Cellular", StringComparer.OrdinalIgnoreCase);
    }

    static void Try(Action collect)
    {
        try
        {
            collect();
        }
        catch
        {
            // Device APIs can throw on unsupported hosts.
        }
    }
#endif
}
