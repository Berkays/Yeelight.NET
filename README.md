
# Yeelight.NET

C#.NET Api used to control xiaomi yeelight devices over the Local Area Network.

## Installation

Using Nuget Package Manager:

    Install-Package YeelightNET

## Usage

Reference the compiled class or insert the source code into your project.

### Discovering devices
**IMPORTANT !**

In order to discover and control the xiaomi yeelight devices in the local network, you have to download and install yeelight app and enable the LAN control for each device.

Android Application: https://play.google.com/store/apps/details?id=com.yeelight.cherry&hl=en
IOS Application: https://itunes.apple.com/gb/app/yeelight/id977125608?mt=8

To get a list of devices, simply use the async function:

    List<Device> devices = await Yeelight.DiscoverDevices();
    OR 
    List<Device> devices = Yeelight.DiscoverDevices().Result; //for synchronous
You should also let firewall to allow request in the first run.

### Controlling

The API designed in a fluent fashion so you can chain methods to control devices with an organized code.
Ex: `devices[0].Toggle().SetBrightness(20).WaitCmd(2000).SetRgbColor(0,255,0).WaitCmd(5000).SetBrightness(5).SetColorTemperature(2000);`

These device functions are extension methods on device class in YeelightFunctions static class.

The **onPropertyChanged** event can be used to notify when a property changes through indexer of Device class.

### Reading values

 Read values through indexer of **Device** class. It takes **DeviceProperty** enum for parameter.
 Ex:

     mDevice[Yeelight.DeviceProperty.Brightness]
     mDevice[Yeelight.DeviceProperty.ColorTemperature]
     mDevice[Yeelight.DeviceProperty.Name]
## Yeelight Documentation
https://www.yeelight.com/download/Yeelight_Inter-Operation_Spec.pdf
## License
This project is licensed under the GNUv3 License - see the [LICENSE.md](LICENSE.md) file for details.
## Contact
*Contact for any bugs or requests.*

berkaygursoy@gmail.com
