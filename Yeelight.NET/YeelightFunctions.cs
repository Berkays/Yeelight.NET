using System;
using System.Threading.Tasks;
using static YeelightNET.Yeelight;

namespace YeelightNET
{
    //Device extensions
    public static class YeelightFunctions
    {
        public static Device Toggle(this Device device, int duration = 500)
        {
            string newState = "on";
            if (device.isPowered)
                newState = "off";

            bool isSuccesfull = SendCommand(device, 0, "set_power", new dynamic[] { newState, "smooth", duration });

            device[DeviceProperty.Power] = newState;

            return device;
        }

        public static async Task<Device> Blink(this Device device, int duration = 500, int delay = 4000, int count = 2)
        {
            for (int i = 0; i < count; i++)
            {
                Toggle(device, duration);
                await Task.Delay(delay);
                Toggle(device, duration);
                await Task.Delay(delay);
            }

            return device;
        }

        public static Device SetColorTemperature(this Device device, int temperature, int duration = 500)
        {
            if (!(temperature >= 1700 && temperature <= 6500))
                return device;

            if (device.isPowered)
            {
                SendCommand(device, 0, "set_ct_abx", new dynamic[] { temperature,"smooth",duration });

                device[DeviceProperty.ColorTemperature] = temperature;
            }

            return device;
        }

        public static Device SetRgbColor(this Device device, int r,int g,int b, int duration = 500)
        {
            int rgb = (r * 65536) + (g * 256) + b; 

            if (device.isPowered)
            {
                SendCommand(device, 0, "set_rgb", new dynamic[] { rgb, "smooth", duration });

                device[DeviceProperty.RGB] = rgb;
            }

            return device;
        }

        public static Device SetBrightness(this Device device, int brightness, int duration = 500)
        {
            brightness = Math.Max(1,Math.Min(100,Math.Abs(brightness)));

            if (device.isPowered)
            {
                SendCommand(device, 0, "set_bright", new dynamic[] { brightness, "smooth", duration });

                device[DeviceProperty.Brightness] = brightness;
            }

            return device;
        }

        public static async Task<Device> Wait(this Device device, int duration = 3000)
        {
            await Task.Delay(duration);
            return device;
        }
    }
}
