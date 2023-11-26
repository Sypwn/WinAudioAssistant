using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Maintains the lists of managed devices.
    /// </summary>
    public class DeviceManager
    {
        // Internal collections of managed devices
        // If separate comms priority is disabled, those references will be the same as the normal collections
        private ObservableCollection<ManagedInputDevice> _inputDevices;
        private ObservableCollection<ManagedInputDevice> _commsInputDevices;
        private ObservableCollection<ManagedOutputDevice> _outputDevices;
        private ObservableCollection<ManagedOutputDevice> _commsOutputDevices;

        // Public read-only references to the internal collections
        public ReadOnlyObservableCollection<ManagedInputDevice> InputDevices { get; private set; }
        public ReadOnlyObservableCollection<ManagedInputDevice> CommsInputDevices { get; private set; }
        public ReadOnlyObservableCollection<ManagedOutputDevice> OutputDevices { get; private set; }
        public ReadOnlyObservableCollection<ManagedOutputDevice> CommsOutputDevices { get; private set;  }

        private bool _separateCommsPriorityState;

        public bool SeparateCommsPriorityState
        {
            get => _separateCommsPriorityState;
            set
            {
                if (_separateCommsPriorityState == value) return;
                _separateCommsPriorityState = value;

                if (value)
                {
                    // Copy the current managed devices to new lists of comms devices
                    _commsInputDevices = new(_inputDevices);
                    CommsInputDevices = new(_commsInputDevices);

                    _commsOutputDevices = new(_outputDevices);
                    CommsOutputDevices = new(_commsOutputDevices);
                }
                else
                {
                    // Comms devices reference the same lists as the normal devices
                    _commsInputDevices = _inputDevices;
                    CommsInputDevices = InputDevices;

                    _commsOutputDevices = _outputDevices;
                    CommsOutputDevices = OutputDevices;
                }
            }
        }

        public DeviceManager()
        {
            _separateCommsPriorityState = false;

            // Initially configured with separate comms priority off
            _inputDevices = new();
            InputDevices = new(_inputDevices);

            _outputDevices = new();
            OutputDevices = new(_outputDevices);

            _commsInputDevices = _inputDevices;
            CommsInputDevices = InputDevices;

            _commsOutputDevices = _outputDevices;
            CommsOutputDevices = OutputDevices;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void UpdateDefaultDevices()
        {
            bool error = false;
            Debug.WriteLine("");
            Debug.WriteLine("Updating default input devices:");
            error |= !UpdateDefaultDevice(_inputDevices.ToList<ManagedDevice>(), false);
            Debug.WriteLine("Updating default output devices:");
            error |= !UpdateDefaultDevice(_outputDevices.ToList<ManagedDevice>(), false);
            Debug.WriteLine("Updating default comms input devices:");
            error |= !UpdateDefaultDevice(_commsInputDevices.ToList<ManagedDevice>(), true);
            Debug.WriteLine("Updating default comms output devices:");
            error |= !UpdateDefaultDevice(_commsOutputDevices.ToList<ManagedDevice>(), true);

            if (error)
                App.Current.Dispatcher.BeginInvoke(App.AudioEndpointManager.UpdateCachedEndpoints);
        }

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

        public void AddDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
                if (collection.Contains(inputDevice)) return;
                collection.Add(inputDevice);
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? _commsOutputDevices : _outputDevices;
                if (collection.Contains(outputDevice)) return;
                collection.Add(outputDevice);
            }
            else
            {
                Debugger.Break();
            }
        }

        public void AddDeviceAt(ManagedDevice device, bool isComms, int index)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
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
                var collection = isComms ? _commsOutputDevices : _outputDevices;
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


        public void RemoveDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
                if (collection.Contains(inputDevice))
                {
                    collection.Remove(inputDevice);
                }
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? _commsOutputDevices : _outputDevices;
                if (collection.Contains(outputDevice))
                {
                    collection.Remove(outputDevice);
                }
            }
            else
            {
                Debugger.Break();
            }
        }

        public bool HasDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
                return collection.Contains(inputDevice);
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? _commsOutputDevices : _outputDevices;
                return collection.Contains(outputDevice);
            }
            else
            {
                Debugger.Break();
                return false;
            }
        }
    }
}
