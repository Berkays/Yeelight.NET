using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json;

namespace YeelightNET
{
    public class Yeelight
    {
        private const string dgram = "M-SEARCH * HTTP/1.1\r\nMAN: \"ssdp:discover\"\r\nST: wifi_bulb\r\n"; //Yeelight udp datagram
        private const string MULTICAST_ADDRESS = "239.255.255.250"; //Yeelight multicast address
        private const int DEVICE_PORT = 1982; //Yeelight comm port

        public static async Task<List<Device>> DiscoverDevices()
        {
            List<Device> devices = new List<Device>();

            UdpClient client = new UdpClient(DEVICE_PORT);

            IPAddress multicastAddress = IPAddress.Parse(MULTICAST_ADDRESS);

            IPEndPoint remoteEndPoint = new IPEndPoint(multicastAddress, DEVICE_PORT);
            IPEndPoint anyEndPoint = new IPEndPoint(IPAddress.Any, DEVICE_PORT);

            client.JoinMulticastGroup(multicastAddress);

            try
            {
                var receiveTask = Task.Run(() =>
                {
                    string localIp = NetworkUtils.GetLocalIPAddress();

                    while (true)
                    {
                        var response = client.Receive(ref anyEndPoint);

                        //Pass if sender is same as receiver
                        if (anyEndPoint.Address.ToString() == localIp)
                            continue;

                        Device device = new Device(Encoding.ASCII.GetString(response));

                        if (devices.Contains(device) == false)
                            devices.Add(device);

                        break;
                    }
                });

                var sendTask = Task.Run(() =>
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(dgram);

                    //Send 5 times with 250 ms delay
                    for (int i = 0; i < 5; i++)
                    {
                        client.Send(buffer, buffer.Length, remoteEndPoint);
                        Task.Delay(250).Wait();
                    }
                });

                //Wait for sending to finish
                await sendTask;
            }
            catch
            {
                //Handle connection exceptions
            }
            finally
            {
                client.Close();
            }

            return devices;
        }

        public static bool SendCommand(Device device, int id, string method, dynamic[] parameters)
        {
            var obj = new { id = id, method = method, @params = parameters };

            //Yeelight requires \r\n delimiters at the end of json data
            string json = JsonConvert.SerializeObject(obj) + "\r\n";

            //Full address of yeelight bulb
            string location = (string)device[DeviceProperty.Location];

            string ip = NetworkUtils.getAddress(location);
            int port = NetworkUtils.getPort(location);

            if (string.IsNullOrEmpty(ip) || port == 0)
                return false;

            try
            {
                TcpClient client = new TcpClient();

                client.Connect(ip, port);

                if (client.Connected)
                {
                    //Send command
                    byte[] buffer = Encoding.ASCII.GetBytes(json);
                    client.Client.Send(buffer);

                    //Receive response
                    buffer = new byte[128];
                    client.Client.Receive(buffer);

                    client.Close();

                    string responseJSON = Encoding.ASCII.GetString(buffer);
                    dynamic response = JsonConvert.DeserializeObject(responseJSON);
                    return response.result[0] == "ok";
                }
                else
                {
                    return false;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to server.");
                return false;
            }

        }

        public class Device : IEquatable<Device>
        {
            private Dictionary<DeviceProperty, dynamic> DeviceValues = new Dictionary<DeviceProperty, dynamic>();

            public bool isPowered
            {
                get
                {
                    return this[DeviceProperty.Power].ToString() == "on";
                }
            }

            public Device(string data)
            {
                this.getProperties(data);
            }

            //Indexer for Dictionary
            public dynamic this[DeviceProperty dp]
            {
                get
                {
                    if (this.DeviceValues.ContainsKey(dp))
                        return this.DeviceValues[dp];
                    else
                        return null;
                }

                set
                {
                    if (this.DeviceValues.ContainsKey(dp))
                        this.DeviceValues[dp] = value;
                }
            }

            //Returns dynamic value in concrete type
            public T getValue<T>(DeviceProperty dp)
            {
                if (this.DeviceValues.ContainsKey(dp))
                    return (T)this.DeviceValues[dp];
                else
                    return default(T);
            }

            public bool Equals(Device other)
            {
                if (this[DeviceProperty.Id] == other[DeviceProperty.Id])
                    return true;
                else
                    return false;
            }

            private void getProperties(string data)
            {
                string[] set = data.Trim('\n').Split('\r');
                var propArray = (int[])Enum.GetValues(typeof(DeviceProperty));
                foreach (var i in propArray)
                {
                    string val = parseValue(set[i]);
                    try
                    {
                        DeviceValues.Add((DeviceProperty)i, int.Parse(val));
                    }
                    catch
                    {
                        DeviceValues.Add((DeviceProperty)i, val);
                    }
                }
            }

            private string parseValue(string raw)
            {
                int startPos = raw.IndexOf(':') + 1;
                return raw.Substring(startPos).Trim();
            }
        }

        public enum DeviceProperty
        {
            Location = 4,
            Id = 6,
            Power = 10,
            Brightness = 11,
            ColorMode = 12,
            ColorTemperature = 13,
            RGB = 14,
            Hue = 15,
            Saturation = 16
        }
    }

    public static class NetworkUtils
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string getAddress(string fullAddress)
        {
            Regex regex = new Regex(@"\d{3}.\d{3}.\d{1,3}.\d{1,3}");

            Match match = regex.Match(fullAddress);

            if (match.Success)
                return match.Value;
            else
                return String.Empty;
        }

        public static int getPort(string fullAddress)
        {
            Regex regex = new Regex(@":(\d{1,5})");

            Match match = regex.Match(fullAddress);

            try
            {
                if (match.Success)
                    return int.Parse(match.Groups[1].Value);
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
