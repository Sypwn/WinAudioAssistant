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
    public class UserSettings
    {
        public bool SeparateCommsPriority
        {
            get { return ManagedDevices.SeparateCommsPriorityState; }
            set { ManagedDevices.SeparateCommsPriorityState = value; }
        }
        public double PriorityConfigurationWindowWidth { get; set; } = 600;
        public double PriorityConfigurationWindowHeight { get; set; } = 300;

        [JsonIgnore]
        private ManagedDeviceManager ManagedDevices { get; } = new();
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedInputDevice> ManagedInputDevices => ManagedDevices.ReadOnlyInputDevices;
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedOutputDevice> ManagedOutputDevices => ManagedDevices.ReadOnlyOutputDevices;
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedInputDevice> ManagedCommsInputDevices => ManagedDevices.ReadOnlyCommsInputDevices;
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedOutputDevice> ManagedCommsOutputDevices => ManagedDevices.ReadOnlyCommsOutputDevices;
        public void AddManagedDevice(ManagedDevice device, bool isComms) => ManagedDevices.AddDevice(device, isComms);
        public void AddManagedDeviceAt(ManagedDevice device, bool isComms, int index) => ManagedDevices.AddDeviceAt(device, isComms, index);
        public void RemoveManagedDevice(ManagedDevice device, bool isComms) => ManagedDevices.RemoveDevice(device, isComms);
        public bool HasManagedDevice(ManagedDevice device, bool isComms) => ManagedDevices.HasDevice(device, isComms);
        public void UpdateDefaultDevices() => ManagedDevices.UpdateDefaultDevices(); // Should only be called by SystemEventsHandler

        [JsonProperty(PropertyName = "ManagedInputDevices")]
        private List<ManagedInputDevice>? InputDevicesForSerialization;
        [JsonProperty(PropertyName = "ManagedOutputDevices")]
        private List<ManagedOutputDevice>? OutputDevicesForSerialization;
        [JsonProperty(PropertyName = "ManagedCommsInputDevices")]
        private List<ManagedInputDevice>? CommsInputDevicesForSerialization;
        [JsonProperty(PropertyName = "ManagedCommsOutputDevices")]
        private List<ManagedOutputDevice>? CommsOutputDevicesForSerialization;

        private bool SaveToFile(string filename)
        {
            InputDevicesForSerialization = ManagedInputDevices.ToList();
            OutputDevicesForSerialization = ManagedOutputDevices.ToList();
            CommsInputDevicesForSerialization = SeparateCommsPriority ? ManagedCommsInputDevices.ToList() : null;
            CommsOutputDevicesForSerialization = SeparateCommsPriority ? ManagedCommsOutputDevices.ToList() : null;

            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filename, json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        private bool LoadFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    string json = File.ReadAllText(filename);
                    JsonConvert.PopulateObject(json, this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }

                ManagedDevices.RepopulateDevices(inputDevices: InputDevicesForSerialization, outputDevices: OutputDevicesForSerialization,
                                                 commsInputDevices: CommsInputDevicesForSerialization, commsOutputDevices: CommsOutputDevicesForSerialization);
                InputDevicesForSerialization = null;
                OutputDevicesForSerialization = null;
                CommsInputDevicesForSerialization = null;
                CommsOutputDevicesForSerialization = null;
                return true;
            }
            return false;
        }

        public void Startup()
        {
            if (!LoadFromFile("settings.json"))
            {
                SeparateCommsPriority = false;
            }
        }

        public bool Save()
        {
            return SaveToFile("settings.json");
        }

        public bool Load()
        {
            if (LoadFromFile("settings.json"))
            {
                SystemEventsHandler.DispatchUpdateDefaultDevices();
                return true;
            }
            return false;
        }
    }
}
