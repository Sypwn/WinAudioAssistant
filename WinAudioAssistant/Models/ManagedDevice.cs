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

    public abstract class ManagedDevice
    {
        public string Name { get; set; } = "";
        public abstract DeviceIOType Type();
    }

    public class ManagedInputDevice : ManagedDevice
    {
        public ManagedInputDevice() { }

        public override DeviceIOType Type() => DeviceIOType.Input;
    }

    public class ManagedOutputDevice : ManagedDevice
    {
        public ManagedOutputDevice() { }

        public override DeviceIOType Type() => DeviceIOType.Output;
    }
}
