using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace YeelightNET;

public static class NetworkUtils
{
    //Return local ip address of the network interface
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    //Return local ip address from full address
    public static string getAddress(string fullAddress)
    {
        Regex regex = new Regex(@"\d{3}.\d{3}.\d{1,3}.\d{1,3}");

        Match match = regex.Match(fullAddress);

        if (match.Success)
            return match.Value;

        return String.Empty;
    }

    //Return port number from full address
    public static int getPort(string fullAddress)
    {
        Regex regex = new Regex(@":(\d{1,5})");

        Match match = regex.Match(fullAddress);

        try
        {
            if (match.Success)
                return int.Parse(match.Groups[1].Value);

            return 0;
        }
        catch
        {
            return 0;
        }
    }
}