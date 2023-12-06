using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi.CoreAudio;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Maintains the lists of managed devices.
    /// </summary>
    public class ManagedDeviceManager
    {
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

        public ManagedDeviceManager()
        {
            _readOnlyInputDevices = new ReadOnlyObservableCollection<ManagedInputDevice>(_inputDevices);
            _readOnlyOutputDevices = new ReadOnlyObservableCollection<ManagedOutputDevice>(_outputDevices);
            _readOnlyCommsInputDevices = new ReadOnlyObservableCollection<ManagedInputDevice>(_commsInputDevices);
            _readOnlyCommsOutputDevices = new ReadOnlyObservableCollection<ManagedOutputDevice>(_commsOutputDevices);
        }

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

        public void AddDevices(IEnumerable<ManagedDevice> devices, bool isComms)
        {
            foreach (var device in devices)
            {
                AddDevice(device, isComms);
            }
        }

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


        public void RemoveDevice(ManagedDevice device, bool isComms)
        {
            if (device is ManagedInputDevice inputDevice)
            {
                var collection = isComms ? CommsInputDevices : InputDevices;
                if (collection.Contains(inputDevice))
                {
                    collection.Remove(inputDevice);
                }
            }
            else if (device is ManagedOutputDevice outputDevice)
            {
                var collection = isComms ? CommsOutputDevices : OutputDevices;
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
        /// Updates the default devices for all managed device lists.
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
                    if (App.CoreAudioController.GetDevice(endpointInfo.Guid) is CoreAudioDevice device)
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
    }
}
