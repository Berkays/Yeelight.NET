using System;
using System.Collections.Generic;

namespace YeelightNET;
public class Device : IEquatable<Device>
{
    public event Action<DeviceProperty> onPropertyChanged;

    //Dictionary holds device properties. Can be accessed with an indexer
    private Dictionary<DeviceProperty, dynamic> DeviceValues = new Dictionary<DeviceProperty, dynamic>();

    //A shorthand for converting power state to a boolean
    public bool isPowered
    {
        get
        {
            return this[DeviceProperty.Power].ToString() == "on";
        }
    }

    //Initializes a new instance of a device
    public static Device Initialize(string data)
    {
        Device device = new Device();
        device.getProperties(data);
        return device;
    }

    //Indexer for Dictionary
    public dynamic this[DeviceProperty dp]
    {
        get
        {
            if (this.DeviceValues.ContainsKey(dp))
                return this.DeviceValues[dp];

            return null;
        }

        set
        {
            if (this.DeviceValues.ContainsKey(dp))
            {
                this.DeviceValues[dp] = value;

                onPropertyChanged?.Invoke(dp);
            }
        }
    }

    //Return copy of current state
    public Dictionary<DeviceProperty, dynamic> getState()
    {
        return new Dictionary<DeviceProperty, dynamic>(DeviceValues);
    }

    //Returns dynamic value in generic type
    public T getValue<T>(DeviceProperty dp)
    {
        if (this.DeviceValues.ContainsKey(dp))
            return (T)this.DeviceValues[dp];

        return default(T);
    }

    public bool Equals(Device other)
    {
        if (this[DeviceProperty.Id] == other[DeviceProperty.Id])
            return true;

        return false;
    }

    //Parses values from udp response and fills dictionary
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
    Saturation = 16,
    Name = 17
}