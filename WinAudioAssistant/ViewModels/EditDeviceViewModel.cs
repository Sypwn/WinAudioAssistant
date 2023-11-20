using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinAudioAssistant.Models;

namespace WinAudioAssistant.ViewModels
{
    public class EditDeviceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _initialized = false;
        private Device? _device; // Original device reference. Null if creating a new device
        public string WindowTitle { get; private set; } = "EditDeviceView";
        public DeviceIOType? IOType { get; private set; }
        public bool IsComms { get; private set; } = false;

        public string DeviceName { get; set; } = string.Empty;

        public void Initialize(Device device, bool isComms)
        {
            if (_initialized) return;
            //Editing an existing device
            _device = device;
            IOType = device.Type();
            IsComms = isComms;
            WindowTitle = "Edit " + (IOType == DeviceIOType.Input ? "Input" : "Output") + (isComms ? "Comms " : "") + " Device";

            DeviceName = device.Name;

            _initialized = true;
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }

        public void Initialize(DeviceIOType type, bool isComms)
        {
            if (_initialized) return;
            //Creating a new device
            IOType = type;
            IsComms = isComms;
            WindowTitle = "New " + (IOType == DeviceIOType.Input ? "Input" : "Output") + (isComms ? "Comms " : "") + " Device";

            _initialized = true;
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }


        public void Apply()
        {
            Debug.Assert(_initialized);
            if (_initialized == false) return;
            if (_device is null)
            {
                // Creating a new device
                _device = IOType switch
                {
                    DeviceIOType.Input => new InputDevice(),
                    DeviceIOType.Output => new OutputDevice(),
                    _ => throw new NotImplementedException()
                };
                _device.Name = DeviceName;
                App.UserSettings.DeviceManager.AddDevice(_device, IsComms);
            }
            else
            {
                // Editing an existing device
                if (App.UserSettings.DeviceManager.HasDevice(_device, IsComms))
                {
                    _device.Name = DeviceName;
                }
                else
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("The device you are editing no longer exists. Would you like to save these changes as a new device?", "Device Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _device = null;
                        Apply();
                    }
                }
            }
        }
    }
}
