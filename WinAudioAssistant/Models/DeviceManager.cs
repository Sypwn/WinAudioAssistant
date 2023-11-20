using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.Models
{
    public class DeviceManager
    {

        private ObservableCollection<InputDevice> _inputDevices;
        private ObservableCollection<InputDevice> _commsInputDevices;
        private ObservableCollection<OutputDevice> _outputDevices;
        private ObservableCollection<OutputDevice> _commsOutputDevices;

        public ReadOnlyObservableCollection<InputDevice> InputDevices { get; private set; }
        public ReadOnlyObservableCollection<InputDevice> CommsInputDevices { get; private set; }
        public ReadOnlyObservableCollection<OutputDevice> OutputDevices { get; private set; }
        public ReadOnlyObservableCollection<OutputDevice> CommsOutputDevices { get; private set;  }

        private bool _separateCommsPriorityState;

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

        public bool GetSeparateCommsPriority()
        {
            return _separateCommsPriorityState;
        }

        public void SetSeparateCommsPriority(bool value)
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

        public void AddDevice(Device device)
        {
            if (device is InputDevice)
            {
                _inputDevices.Add((InputDevice)device);
            }
            else if (device is OutputDevice)
            {
                _outputDevices.Add((OutputDevice)device);
            }
        }

        public void AddDeviceAt(Device device, int index)
        {
            if (device is InputDevice)
            {
                // If device is already present, move it to the new index
                if (_inputDevices.Contains((InputDevice)device))
                {
                    _inputDevices.Move(_inputDevices.IndexOf((InputDevice)device), index);
                }
                else
                {
                    _inputDevices.Insert(index, (InputDevice)device);
                }
            }
            else if (device is OutputDevice)
            {
                if (_outputDevices.Contains((OutputDevice)device))
                {
                    _outputDevices.Move(_outputDevices.IndexOf((OutputDevice)device), index);
                }
                else
                {
                    _outputDevices.Insert(index, (OutputDevice)device);
                }
            }
        }

        public void AddCommsDevice(Device device)
        {
            if (device is InputDevice)
            {
                _commsInputDevices.Add((InputDevice)device);
            }
            else if (device is OutputDevice)
            {
                _commsOutputDevices.Add((OutputDevice)device);
            }
        }

        public void AddCommsDeviceAt(Device device, int index)
        {
            if (device is InputDevice)
            {
                // If device is already present, move it to the new index
                if (_commsInputDevices.Contains((InputDevice)device))
                {
                    _commsInputDevices.Move(_commsInputDevices.IndexOf((InputDevice)device), index);
                }
                else
                {
                    _commsInputDevices.Insert(index, (InputDevice)device);
                }
            }
            else if (device is OutputDevice)
            {
                if (_commsOutputDevices.Contains((OutputDevice)device))
                {
                    _commsOutputDevices.Move(_commsOutputDevices.IndexOf((OutputDevice)device), index);
                }
                else
                {
                    _commsOutputDevices.Insert(index, (OutputDevice)device);
                }
            }
        }

        public void RemoveDevice(Device device)
        {
            if (device is InputDevice)
            {
                _inputDevices.Remove((InputDevice)device);
            }
            else if (device is OutputDevice)
            {
                _outputDevices.Remove((OutputDevice)device);
            }
        }

        public void RemoveCommsDevice(Device device)
        {
            if (device is InputDevice)
            {
                _commsInputDevices.Remove((InputDevice)device);
            }
            else if (device is OutputDevice)
            {
                _commsOutputDevices.Remove((OutputDevice)device);
            }
        }
    }
}
