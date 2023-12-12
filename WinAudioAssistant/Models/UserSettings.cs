using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Contains all properties that should be saved and loaded from the settings file, including the managed device lists.
    /// Handles the serialization and deserialization of these properties.
    /// </summary>
    public class UserSettings
    {
        #region Private Fields
        string _settingsPath = string.Empty; // The path to the settings file, once located
        [JsonIgnore]
        private readonly ManagedDeviceManager _managedDevices = new(); // Private ManagedDeviceManager that handles the managed device lists
        #endregion

        #region Public Properties
        /// <summary>
        /// When true, the comms devices are kept separate from the primary devices.
        /// When false, the comms devices mirror the primary devices.
        /// </summary>
        public bool SeparateCommsPriority
        {
            get { return _managedDevices.SeparateCommsPriorityState; }
            set { _managedDevices.SeparateCommsPriorityState = value; }
        }
        public double PriorityConfigurationWindowWidth { get; set; } = 600; // Saves the width of the priority configuration window
        public double PriorityConfigurationWindowHeight { get; set; } = 300; // Saves the height of the priority configuration window
        #endregion

        #region ManagedDeviceManager Accessors and Methods
        // These exposed accessors allow the UI to read the managed device lists
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedInputDevice> ManagedInputDevices => _managedDevices.ReadOnlyInputDevices;
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedOutputDevice> ManagedOutputDevices => _managedDevices.ReadOnlyOutputDevices;
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedInputDevice> ManagedCommsInputDevices => _managedDevices.ReadOnlyCommsInputDevices;
        [JsonIgnore]
        public ReadOnlyObservableCollection<ManagedOutputDevice> ManagedCommsOutputDevices => _managedDevices.ReadOnlyCommsOutputDevices;

        // These lists are used exclusively for serialization and deserialization of the managed device lists
        [JsonProperty(PropertyName = "ManagedInputDevices")]
        private List<ManagedInputDevice>? _inputDevicesForSerialization;
        [JsonProperty(PropertyName = "ManagedOutputDevices")]
        private List<ManagedOutputDevice>? _outputDevicesForSerialization;
        [JsonProperty(PropertyName = "ManagedCommsInputDevices")]
        private List<ManagedInputDevice>? _commsInputDevicesForSerialization;
        [JsonProperty(PropertyName = "ManagedCommsOutputDevices")]
        private List<ManagedOutputDevice>? _commsOutputDevicesForSerialization;

        // These exposed methods allow the UI to add, remove, and check for the existence of managed devices

        /// <inheritdoc cref="ManagedDeviceManager.AddDevice(ManagedDevice, bool)"/>
        public void AddManagedDevice(ManagedDevice device, bool isComms) => _managedDevices.AddDevice(device, isComms);

        /// <inheritdoc cref="ManagedDeviceManager.AddDeviceAt(ManagedDevice, bool, int)"/>
        public void AddManagedDeviceAt(ManagedDevice device, bool isComms, int index) => _managedDevices.AddDeviceAt(device, isComms, index);

        /// <inheritdoc cref="ManagedDeviceManager.RemoveDevice(ManagedDevice, bool)"/>
        public void RemoveManagedDevice(ManagedDevice device, bool isComms) => _managedDevices.RemoveDevice(device, isComms);

        /// <inheritdoc cref="ManagedDeviceManager.HasDevice(ManagedDevice, bool)"/>
        public bool HasManagedDevice(ManagedDevice device, bool isComms) => _managedDevices.HasDevice(device, isComms);

        /// <inheritdoc cref="ManagedDeviceManager.UpdateDefaultDevices()"/>
        public void UpdateDefaultDevices() => _managedDevices.UpdateDefaultDevices(); // Should only be called by SystemEventsHandler
        #endregion

        #region Public Serialization Methods
        /// <summary>
        /// Loads the settings file during application startup.
        /// </summary>
        public void Startup()
        {
            _settingsPath = "settings.json"; // TODO: Locate the appropriate path to the settings file.
            if (File.Exists(_settingsPath) && Deserialize())
                return;
            
            // Default setting overrides
            SeparateCommsPriority = false; // This must default to true during construction
        }

        /// <summary>
        /// Loads the settings file during application runtime.
        /// </summary>
        /// <returns>True if the settings were successfully loaded.</returns>
        public bool Load()
        {
            if (Deserialize())
            {
                SystemEventsHandler.DispatchUpdateDefaultDevices();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the settings file during application runtime.
        /// </summary>
        /// <returns>True if the settings were successfully saved.</returns>
        public bool Save()
        {
            return Serialize();
        }
        #endregion

        #region Private Serialization Methods
        /// <summary>
        /// Serializes the settings to the settings file.
        /// </summary>
        /// <returns>True if successful.</returns>
        private bool Serialize()
        {
            // First, populate the temporary serialization lists with the current managed device lists
            _inputDevicesForSerialization = ManagedInputDevices.ToList();
            _outputDevicesForSerialization = ManagedOutputDevices.ToList();
            _commsInputDevicesForSerialization = SeparateCommsPriority ? ManagedCommsInputDevices.ToList() : null;
            _commsOutputDevicesForSerialization = SeparateCommsPriority ? ManagedCommsOutputDevices.ToList() : null;

            try
            {
                // Then serialize the settings to the file
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                // Finally, clear the temporary serialization lists
                _inputDevicesForSerialization = null;
                _outputDevicesForSerialization = null;
                _commsInputDevicesForSerialization = null;
                _commsOutputDevicesForSerialization = null;
            }
            return true;
        }

        /// <summary>
        /// Deserializes the settings from the settings file.
        /// </summary>
        /// <returns>True if successful.</returns>
        private bool Deserialize()
        {
            try
            {
                // First, deserialize the settings from the file
                string json = File.ReadAllText(_settingsPath);
                JsonConvert.PopulateObject(json, this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            // Then repopulate the managed device lists from the temporary serialization lists
            _managedDevices.RepopulateDevices(inputDevices: _inputDevicesForSerialization, outputDevices: _outputDevicesForSerialization,
                                             commsInputDevices: _commsInputDevicesForSerialization, commsOutputDevices: _commsOutputDevicesForSerialization);

            // Finally, clear the temporary serialization lists
            _inputDevicesForSerialization = null;
            _outputDevicesForSerialization = null;
            _commsInputDevicesForSerialization = null;
            _commsOutputDevicesForSerialization = null;
            return true;
        }
        #endregion
    }
}
