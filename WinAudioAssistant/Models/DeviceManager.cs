using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
