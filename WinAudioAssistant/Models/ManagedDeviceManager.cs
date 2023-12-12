using System.Collections.ObjectModel;
using System.Diagnostics;
using AudioSwitcher.AudioApi.CoreAudio;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Maintains the lists of managed devices, as well as the method to update the system default devices based on those lists.
    /// </summary>
    public class ManagedDeviceManager
    {
        #region Constructors
        /// <summary>
        /// Initializes the ManagedDeviceManager.
        /// </summary>
        public ManagedDeviceManager()
        {
            _readOnlyInputDevices = new ReadOnlyObservableCollection<ManagedInputDevice>(_inputDevices);
            _readOnlyOutputDevices = new ReadOnlyObservableCollection<ManagedOutputDevice>(_outputDevices);
            _readOnlyCommsInputDevices = new ReadOnlyObservableCollection<ManagedInputDevice>(_commsInputDevices);
            _readOnlyCommsOutputDevices = new ReadOnlyObservableCollection<ManagedOutputDevice>(_commsOutputDevices);
        }
        #endregion

        #region Private Fields
        // Internal lists of managed devices
        private readonly ObservableCollection<ManagedInputDevice> _inputDevices = new();
        private readonly ObservableCollection<ManagedOutputDevice> _outputDevices = new();
        private readonly ObservableCollection<ManagedInputDevice> _commsInputDevices = new();
        private readonly ObservableCollection<ManagedOutputDevice> _commsOutputDevices = new();

        // Internal lists of read-only wrappers
        private readonly ReadOnlyObservableCollection<ManagedInputDevice> _readOnlyInputDevices;
        private readonly ReadOnlyObservableCollection<ManagedOutputDevice> _readOnlyOutputDevices;
        private readonly ReadOnlyObservableCollection<ManagedInputDevice> _readOnlyCommsInputDevices;
        private readonly ReadOnlyObservableCollection<ManagedOutputDevice> _readOnlyCommsOutputDevices;
        #endregion

        #region Properties
        // Private properties to access the correct lists based on the SeparateCommsPriorityState
        private ObservableCollection<ManagedInputDevice> InputDevices => _inputDevices;
        private ObservableCollection<ManagedOutputDevice> OutputDevices => _outputDevices;
        private ObservableCollection<ManagedInputDevice> CommsInputDevices => _separateCommsPriorityState ? _commsInputDevices : _inputDevices;
        private ObservableCollection<ManagedOutputDevice> CommsOutputDevices => _separateCommsPriorityState ? _commsOutputDevices : _outputDevices;

        // Public properties to access the correct read-only lists based on the SeparateCommsPriorityState
        public ReadOnlyObservableCollection<ManagedInputDevice> ReadOnlyInputDevices => _readOnlyInputDevices;
        public ReadOnlyObservableCollection<ManagedOutputDevice> ReadOnlyOutputDevices => _readOnlyOutputDevices;
        public ReadOnlyObservableCollection<ManagedInputDevice> ReadOnlyCommsInputDevices => _separateCommsPriorityState ? _readOnlyCommsInputDevices : _readOnlyInputDevices;
        public ReadOnlyObservableCollection<ManagedOutputDevice> ReadOnlyCommsOutputDevices => _separateCommsPriorityState ? _readOnlyCommsOutputDevices : _readOnlyOutputDevices;

        private bool _separateCommsPriorityState = true;
        /// <summary>
        /// When true, the comms devices are kept separate from the primary devices.
        /// When false, the comms devices mirror the primary devices.
        /// </summary>
        public bool SeparateCommsPriorityState
        {
            get => _separateCommsPriorityState;
            set
            {
                if (_separateCommsPriorityState == value) return;
                if (value)
                {
                    // Set state to true, then copy non-comms devices to comms devices
                    _separateCommsPriorityState = true;
                    AddDevices(InputDevices, true);
                    AddDevices(OutputDevices, true);
                }
                else
                {
                    // Clear the comms devices, then set state to false
                    CommsInputDevices.Clear();
                    CommsOutputDevices.Clear();
                    _separateCommsPriorityState = false;
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a managed device to the appropriate list based on its type and the isComms parameter.
        /// </summary>
        /// <param name="device">ManagedDevice to add.</param>
        /// <param name="isComms">True if the device should be added to the appropriate comms list instead of the primary list.</param>
        public void AddDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? CommsInputDevices : InputDevices;
                if (collection.Contains(inputDevice)) return;
                collection.Add(inputDevice);
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? CommsOutputDevices : OutputDevices;
                if (collection.Contains(outputDevice)) return;
                collection.Add(outputDevice);
            }
            else
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Adds a list of managed devices to the appropriate list based on their type and the isComms parameter.
        /// </summary>
        /// <param name="devices">A list of ManagedDevices to add.</param>
        /// <param name="isComms">True if the managed devices should be added to the appropriate comms list instead of the primary list.</param>
        public void AddDevices(IEnumerable<ManagedDevice> devices, bool isComms)
        {
            foreach (var device in devices)
            {
                AddDevice(device, isComms);
            }
        }

        /// <summary>
        /// Adds a managed device to the specified index of the appropriate list based on its type and the isComms parameter.
        /// If the managed device already exists in the list, it is instead moved to the specified index.
        /// </summary>
        /// <param name="device">ManagedDevice to add.</param>
        /// <param name="isComms">True if the managed device should be added to the appropriate comms list instead of the primary list.</param>
        /// <param name="index">Index to insert or move the managed device to.</param>
        public void AddDeviceAt(ManagedDevice device, bool isComms, int index)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? CommsInputDevices : InputDevices;
                if (collection.Contains(inputDevice))
                {
                    collection.Move(collection.IndexOf(inputDevice), index);
                }
                else
                {
                    collection.Insert(index, inputDevice);
                }
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? CommsOutputDevices : OutputDevices;
                if (collection.Contains(outputDevice))
                {
                    collection.Move(collection.IndexOf(outputDevice), index);
                }
                else
                {
                    collection.Insert(index, outputDevice);
                }
            }
            else
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Removes a managed device from the appropriate list based on its type and the isComms parameter.
        /// </summary>
        /// <param name="device">ManagedDevice to remove.</param>
        /// <param name="isComms">True if the managed device should be removed from the appropriate comms list instead of the primary list.</param>
        public void RemoveDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? CommsInputDevices : InputDevices;
                collection.Remove(inputDevice);
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? CommsOutputDevices : OutputDevices;
                collection.Remove(outputDevice);
            }
            else
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Returns true if the managed device exists in the appropriate list based on its type and the isComms parameter.
        /// </summary>
        /// <param name="device">ManagedDevice to remove.</param>
        /// <param name="isComms">True if the managed device should be removed from the appropriate comms list instead of the primary list.</param>
        public bool HasDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? CommsInputDevices : InputDevices;
                return collection.Contains(inputDevice);
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? CommsOutputDevices : OutputDevices;
                return collection.Contains(outputDevice);
            }
            else
            {
                Debugger.Break();
                return false;
            }
        }

        /// <summary>
        /// Removes all managed devices from all lists, then repopulates them from the given lists.
        /// </summary>
        public void RepopulateDevices(IEnumerable<ManagedInputDevice>? inputDevices = null, IEnumerable<ManagedOutputDevice>? outputDevices = null,
                                      IEnumerable<ManagedInputDevice>? commsInputDevices = null, IEnumerable<ManagedOutputDevice>? commsOutputDevices = null)
        {
            InputDevices.Clear();
            OutputDevices.Clear();
            CommsInputDevices.Clear();
            CommsOutputDevices.Clear();

            if (inputDevices != null) AddDevices(inputDevices, false);
            if (outputDevices != null) AddDevices(outputDevices, false);
            if (commsInputDevices != null) AddDevices(commsInputDevices, true);
            if (commsOutputDevices != null) AddDevices(commsOutputDevices, true);
        }

        /// <summary>
        /// Updates the system default devices for all managed device lists.
        /// </summary>
        public void UpdateDefaultDevices()
        {
            bool error = false;
            Debug.WriteLine("");
            Debug.WriteLine("Updating default input devices:");
            error |= !UpdateDefaultDevice(InputDevices.ToList<ManagedDevice>(), false);
            Debug.WriteLine("Updating default output devices:");
            error |= !UpdateDefaultDevice(OutputDevices.ToList<ManagedDevice>(), false);
            Debug.WriteLine("Updating default comms input devices:");
            error |= !UpdateDefaultDevice(CommsInputDevices.ToList<ManagedDevice>(), true);
            Debug.WriteLine("Updating default comms output devices:");
            error |= !UpdateDefaultDevice(CommsOutputDevices.ToList<ManagedDevice>(), true);

            if (error)
                SystemEventsHandler.DispatchUpdateCachedEndpoints();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Iterates through the list of managed devices and sets the first matching device as the default.
        /// </summary>
        /// <returns>False if an error occured while assigning the default device. True if no errors occured.</returns>
        private static bool UpdateDefaultDevice(IEnumerable<ManagedDevice> managedDevices, bool isComms)
        {
            foreach (var managedDevice in managedDevices)
            {
                Debug.WriteLine($"Checking {managedDevice.Name}");
                // If CheckShouldBeActive returns an endpointInfo, then there was a match.
                if (managedDevice.CheckShouldBeActive() is AudioEndpointInfo endpointInfo)
                {
                    Debug.WriteLine($"Matched {endpointInfo.DeviceInterface_FriendlyName}");
                    // Get the real device that matches the cached endpointInfo
                    if (App.CoreAudioController.GetDevice(endpointInfo.AudioEndpoint_GUID) is CoreAudioDevice device)
                    {
                        // Check if it's already default
                        if (isComms && device.IsDefaultCommunicationsDevice) return true;
                        if (!isComms && device.IsDefaultDevice) return true;

                        // Set as default device
                        Debug.WriteLine("Setting as default device...");
                        try
                        {
                            bool result;
                            if (isComms) result = device.SetAsDefaultCommunications();
                            else result = device.SetAsDefault();
                            if (result) return true; //Succesfully set as default
                        }
                        catch (Exception e) when (e is ComInteropTimeoutException)
                        {
                            Debug.WriteLine("COM interop timeout.");
                        }

                        // Failed to set as default
                        Debug.WriteLine("Failed to set as default device.");
                        return false;
                    }
                    // Cached endpointInfo does not match a real device
                    Debug.WriteLine($"Matching physical device not found");
                    return false;
                }
                // No match or managedDevice is inactive, continue
            }
            // No matching devices found
            return true;
        }
        #endregion
    }
}
