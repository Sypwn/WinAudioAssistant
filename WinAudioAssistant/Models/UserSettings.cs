using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinAudioAssistant.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserSettings
    {
        [JsonProperty]
        public bool SeparateCommsPriority
        {
            get => ManagedDevices.SeparateCommsPriorityState;
            set => ManagedDevices.SeparateCommsPriorityState = value;
        }

        public ManagedDeviceManager ManagedDevices { get; }

        public UserSettings()
        {
            ManagedDevices = new(false);
        }

        [JsonConstructor]
        public UserSettings(bool separateCommsPriority, 
                            IEnumerable<ManagedInputDevice> inputDevices,
                            IEnumerable<ManagedOutputDevice> outputDevices,
                            IEnumerable<ManagedInputDevice>? commsInputDevices = null,
                            IEnumerable<ManagedOutputDevice>? commsOutputDevices = null)
        {
            ManagedDevices = new(separateCommsPriority);
            ManagedDevices.AddDevices(inputDevices, false);
            ManagedDevices.AddDevices(outputDevices, false);
            if (commsInputDevices is not null)
                ManagedDevices.AddDevices(commsInputDevices, true);
            if (commsOutputDevices is not null)
                ManagedDevices.AddDevices(commsOutputDevices, true);
        }

        // All this just to make sure SeparateCommsPriority is set before managed devices are populated
        [JsonProperty(PropertyName = "InputDevices")]
        private ReadOnlyObservableCollection<ManagedInputDevice> InputDevicesToSerialize{ get => ManagedDevices.InputDevices; }
        [JsonProperty(PropertyName = "OutputDevices")]
        private ReadOnlyObservableCollection<ManagedOutputDevice> OutputDevicesToSerialize { get => ManagedDevices.OutputDevices; }
        [JsonProperty(PropertyName = "CommsInputDevices")]
        private ReadOnlyObservableCollection<ManagedInputDevice>? CommsInputDevicesToSerialize { get => SeparateCommsPriority ? ManagedDevices.CommsInputDevices : null; }
        [JsonProperty(PropertyName = "CommsOutputDevices")]
        private ReadOnlyObservableCollection<ManagedOutputDevice>? CommsOutputDevicesToSerialize { get => SeparateCommsPriority ? ManagedDevices.CommsOutputDevices : null; }

        public void SaveToFile()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText("settings.json", json);
        }

        public static UserSettings LoadAtStartup()
        {
            if (File.Exists("settings.json"))
            {
                string json = File.ReadAllText("settings.json");
                if (JsonConvert.DeserializeObject<UserSettings>(json) is UserSettings settings)
                    return settings;
            }
            return new();
        }
    }
}
