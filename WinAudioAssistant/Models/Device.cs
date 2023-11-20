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
        public abstract DeviceIOType Type();
    }

    public class InputDevice : Device
    {
        public InputDevice() { }

        public override DeviceIOType Type() => DeviceIOType.Input;
    }

    public class OutputDevice : Device
    {
        public OutputDevice() { }

        public override DeviceIOType Type() => DeviceIOType.Output;
    }
}
