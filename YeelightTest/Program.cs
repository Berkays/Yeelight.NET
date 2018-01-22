using System;
using System.Threading.Tasks;

using YeelightNET;

namespace YeelightTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var Devices = Yeelight.DiscoverDevices().Result;
            foreach (var device in Devices)
            {
                Console.WriteLine("Device: {0}, State: {1}", device[Yeelight.DeviceProperty.Id], device[Yeelight.DeviceProperty.Power]);
            }

            if (Devices.Count == 0)
            {
                Console.WriteLine("Couldn't find any devices. Make sure to enable lan control over yeelight app");
                return;
            }

            while (true)
            {
                Console.Write("\nCommand: ");
                var c = Console.ReadLine();

                if (c == "toggle")
                {
                    Devices[0].Toggle();
                }
                else if (c == "blink")
                {
                    Task.Run(() => Devices[0].Blink());
                }
                else if (c == "ct")
                {
                    Console.Write("\nNew Temperature: ");
                    int temp = int.Parse(Console.ReadLine());
                    Devices[0].SetColorTemperature(temp);
                }
                else if (c == "rgb")
                {
                    Console.Write("\nNew Color(R,G,B): ");
                    string[] rgb = Console.ReadLine().Split(',');
                    Devices[0].SetRgbColor(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2]));
                }
                else if (c == "bright")
                {
                    Console.Write("\nNew Brightness: ");
                    int bright = int.Parse(Console.ReadLine());
                    Devices[0].SetBrightness(bright);
                }
                else if (c == "test")
                {
                    Console.Write("\nTesting...");
                    Devices[0].Toggle().Wait().Result.SetBrightness(10).Wait().Result.SetRgbColor(0, 255, 0).Wait().Result.Blink().Result.Wait().Result.Toggle();
                }
                else if (c == "status")
                {
                    Console.WriteLine("\nStatus:");

                    Console.WriteLine("Id: {0}", Devices[0][Yeelight.DeviceProperty.Id]);
                    Console.WriteLine("Location: {0}", Devices[0][Yeelight.DeviceProperty.Location]);
                    Console.WriteLine("State: {0}", Devices[0][Yeelight.DeviceProperty.Power]);
                    Console.WriteLine("Brightness: {0}", Devices[0][Yeelight.DeviceProperty.Brightness]);
                    Console.WriteLine("Color Mode: {0}", Devices[0][Yeelight.DeviceProperty.ColorMode]);
                    Console.WriteLine("RGB: {0},{1},{2}", Devices[0][Yeelight.DeviceProperty.RGB] >> 16, (Devices[0][Yeelight.DeviceProperty.RGB] >> 8) & 255, Devices[0][Yeelight.DeviceProperty.RGB] & 255);
                    Console.WriteLine("Color Temperature: {0} K", Devices[0][Yeelight.DeviceProperty.ColorTemperature]);
                }
                else if (c == "quit")
                {
                    break;
                }
            }

        }
    }
}
