using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using YeelightNET;

namespace Tests;

public class Tests
{
    [SkippableFact]
    public async Task Test1()
    {
        Console.WriteLine("Discovering Devices...");

        List<Device> devices = await Yeelight.DiscoverDevices(5000);

        Skip.If(devices.Count == 0, "No device found in network");

        Assert.True(devices.Count > 0);
        Assert.True(devices[0][DeviceProperty.Location].StartsWith("yeelight://"));
    }
}