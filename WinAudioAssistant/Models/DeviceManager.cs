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
    public class DeviceManager
    {
        // Internal collections of devices
        // If separate comms priority is disabled, those references will be the same as the normal collections
        private ObservableCollection<InputDevice> _inputDevices;
        private ObservableCollection<InputDevice> _commsInputDevices;
        private ObservableCollection<OutputDevice> _outputDevices;
        private ObservableCollection<OutputDevice> _commsOutputDevices;

        // Public read-only references to the internal collections
        public ReadOnlyObservableCollection<InputDevice> InputDevices { get; private set; }
        public ReadOnlyObservableCollection<InputDevice> CommsInputDevices { get; private set; }
        public ReadOnlyObservableCollection<OutputDevice> OutputDevices { get; private set; }
        public ReadOnlyObservableCollection<OutputDevice> CommsOutputDevices { get; private set;  }

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
                    // Copy the current devices to new lists of comms devices
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

        public void AddDevice(Device device, bool isComms)
        {
            if (device is InputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
                if (collection.Contains(inputDevice)) return;
                collection.Add(inputDevice);
            }
            else if (device is OutputDevice outputDevice)
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

        public void AddDeviceAt(Device device, bool isComms, int index)
        {
            if (device is InputDevice inputDevice)
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
            else if (device is OutputDevice outputDevice)
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


        public void RemoveDevice(Device device, bool isComms)
        {
            if (device is InputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
                if (collection.Contains(inputDevice))
                {
                    collection.Remove(inputDevice);
                }
            }
            else if (device is OutputDevice outputDevice)
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

        public bool HasDevice(Device device, bool isComms)
        {
            if (device is InputDevice inputDevice)
            {
                var collection = isComms ? _commsInputDevices : _inputDevices;
                return collection.Contains(inputDevice);
            }
            else if (device is OutputDevice outputDevice)
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
