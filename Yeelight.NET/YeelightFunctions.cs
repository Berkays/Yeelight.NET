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

            bool isSuccesful = SendCommand(device, 0, "set_power", new dynamic[] { newState, "smooth", duration });

            if (isSuccesful)
                device[DeviceProperty.Power] = newState;

            return device;
        }

        public static Device Blink(this Device device, int duration = 500, int delay = 4000, int count = 2)
        {
            for (int i = 0; i < count; i++)
            {
                device.Toggle(duration);
                device.WaitCmd(delay);
                device.Toggle(duration);
                device.WaitCmd(delay);
            }

            return device;
        }

        public static Device SetColorTemperature(this Device device, int temperature, int duration = 500)
        {
            if (!(temperature >= 1700 && temperature <= 6500))
                return device;

            if (device.isPowered)
            {
                bool isSuccesful = SendCommand(device, 0, "set_ct_abx", new dynamic[] { temperature, "smooth", duration });

                if (isSuccesful)
                {
                    device[DeviceProperty.ColorTemperature] = temperature;
                    device[DeviceProperty.ColorMode] = 2;
                }
            }

            return device;
        }

        public static Device SetRgbColor(this Device device, int r, int g, int b, int duration = 500)
        {
            int rgb = (r * 65536) + (g * 256) + b;

            if (device.isPowered)
            {
                bool isSuccesful = SendCommand(device, 0, "set_rgb", new dynamic[] { rgb, "smooth", duration });

                if (isSuccesful)
                {
                    device[DeviceProperty.RGB] = rgb;
                    device[DeviceProperty.ColorMode] = 1;
                }
            }

            return device;
        }
        public static Device SetRgbColor(this Device device, int rgb, int duration = 500)
        {
            if (device.isPowered)
            {
                bool isSuccesful = SendCommand(device, 0, "set_rgb", new dynamic[] { rgb, "smooth", duration });

                if (isSuccesful)
                {
                    device[DeviceProperty.RGB] = rgb;
                    device[DeviceProperty.ColorMode] = 1;
                }
            }

            return device;
        }

        public static Device SetBrightness(this Device device, int brightness, int duration = 500)
        {
            brightness = Math.Max(1, Math.Min(100, Math.Abs(brightness)));

            if (device.isPowered)
            {
                bool isSuccesful = SendCommand(device, 0, "set_bright", new dynamic[] { brightness, "smooth", duration });

                if (isSuccesful)
                    device[DeviceProperty.Brightness] = brightness;
            }

            return device;
        }

        public static Device SetName(this Device device, string name)
        {
            bool isSuccesful = SendCommand(device, 0, "set_name", new dynamic[] { name });

            if (isSuccesful)
                device[DeviceProperty.Name] = name;

            return device;
        }

        public static Device WaitCmd(this Device device, int duration = 500)
        {
            Task.Delay(duration).Wait();
            return device;
        }
    }
}
