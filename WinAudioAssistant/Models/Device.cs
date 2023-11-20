using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.Models
{
    public enum DeviceIOType
    {
        Input,
        Output
    }

    public abstract class Device
    {
        public string Name { get; set; } = "";
        public DeviceIOType Type { get; protected set; }
    }

    public class InputDevice : Device
    {
        public InputDevice()
        {
            Type = DeviceIOType.Input;
        }

        public InputDevice(string name)
        {
            Name = name;
            Type = DeviceIOType.Input;
        }
        
    }

    public class OutputDevice : Device
    {
        public OutputDevice()
        {
            Type = DeviceIOType.Output;
        }

        public OutputDevice(string name)
        {
            Name = name;
            Type = DeviceIOType.Output;
        }
    }
}
