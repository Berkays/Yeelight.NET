using System;
using System.Text;
using System.Linq;
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

        //Returns a list of devices in the local network
        public static async Task<List<Device>> DiscoverDevices(int pingTime = 20)
        {
            Dictionary<string, Device> devices = new Dictionary<string, Device>();

            using (UdpClient socket = new UdpClient())
            {
                socket.Client.ReceiveTimeout = 1000;

                IPAddress multicastAddress = IPAddress.Parse(MULTICAST_ADDRESS);

                IPEndPoint remoteEndPoint = new IPEndPoint(multicastAddress, DEVICE_PORT);
                IPEndPoint anyEndPoint = new IPEndPoint(IPAddress.Any, DEVICE_PORT);

                socket.JoinMulticastGroup(multicastAddress);

                await Task.Run(async () =>
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(dgram);
                    string localIp = NetworkUtils.GetLocalIPAddress();

                    for (int i = 0; i < pingTime; i++)
                    {
                        await socket.SendAsync(buffer, buffer.Length, remoteEndPoint);
                        await Task.Delay(50);
                        try
                        {
                            var response = await socket.ReceiveAsync();

                            var deviceIp = response.RemoteEndPoint.Address.ToString();

                            if (deviceIp == localIp || devices.ContainsKey(deviceIp))
                                continue;

                            var deviceInfo = Encoding.ASCII.GetString(response.Buffer);
                            var device = Device.Initialize(deviceInfo);

                            lock (devices)
                            {
                                devices.Add(deviceIp, device);
                            }

                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine(exc.ToString());
                            continue;
                        }
                        Task.Delay(200).Wait();
                    }
                });
            }

            return devices.Select(n => n.Value).ToList();
        }

        //Execute a command in the yeelight
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
                    client = null;

                    string responseJSON = Encoding.ASCII.GetString(buffer);
                    dynamic response = JsonConvert.DeserializeObject(responseJSON);
                    return response.result[0] == "ok";
                }
                else
                {
                    client.Close();
                    client = null;
                    return false;
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to device.");
                return false;
            }

        }

    }
}
