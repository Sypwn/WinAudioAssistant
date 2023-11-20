using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.Models
{
    public class UserSettings
    {
        public bool SeparateCommsPriority
        {
            get => DeviceManager.GetSeparateCommsPriority();
            set => DeviceManager.SetSeparateCommsPriority(value);
        }

        public DeviceManager DeviceManager { get; } = new();
    }
}
