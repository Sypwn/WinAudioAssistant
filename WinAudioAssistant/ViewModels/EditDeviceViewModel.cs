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
        private ManagedDevice? _managedDevice; // Original managed device reference. Null if creating a new one
        public string WindowTitle { get; private set; } = "EditDeviceView";
        public DeviceIOType? IOType { get; private set; }
        public bool IsComms { get; private set; } = false;

        public string DeviceName { get; set; } = string.Empty;

        public void Initialize(ManagedDevice device, bool isComms)
        {
            //Editing an existing managed device
            Debug.Assert(!_initialized);
            if (_initialized) return;
            _managedDevice = device;
            IOType = device.Type();
            IsComms = isComms;
            WindowTitle = "Edit " + (IOType == DeviceIOType.Input ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            DeviceName = device.Name;

            _initialized = true;
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }

        public void Initialize(DeviceIOType type, bool isComms)
        {
            //Creating a new managed device
            Debug.Assert(!_initialized);
            if (_initialized) return;
            IOType = type;
            IsComms = isComms;
            WindowTitle = "New " + (IOType == DeviceIOType.Input ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            _initialized = true;
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }


        public bool Apply()
        {
            // Applies changes to the managed device. Returns true if successful.
            Debug.Assert(_initialized);
            if (_initialized == false) return false;
            if (_managedDevice is null)
            {
                // Creating a new managed device
                _managedDevice = IOType switch
                {
                    DeviceIOType.Input => new ManagedInputDevice(),
                    DeviceIOType.Output => new ManagedOutputDevice(),
                    _ => throw new NotImplementedException()
                };
                _managedDevice.Name = DeviceName;
                App.UserSettings.DeviceManager.AddDevice(_managedDevice, IsComms);
                return true;
            }
            else
            {
                // Editing an existing managed device
                if (App.UserSettings.DeviceManager.HasDevice(_managedDevice, IsComms))
                {
                    _managedDevice.Name = DeviceName;
                    return true;
                }
                else
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("The managed device you are editing no longer exists. Would you like to save these changes as a new managed device?", "Managed Device Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _managedDevice = null;
                        return Apply();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
