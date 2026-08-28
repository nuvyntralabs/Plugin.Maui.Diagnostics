namespace Plugin.Maui.Diagnostics;

interface IEnvironmentCollector
{
    DeviceSnapshot Collect();
}
