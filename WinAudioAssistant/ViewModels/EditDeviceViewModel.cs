using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AudioSwitcher.AudioApi;
using WinAudioAssistant.Models;

namespace WinAudioAssistant.ViewModels
{
    public class EditDeviceViewModel : INotifyPropertyChanged
    {
        public const int IconSize = 32;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Fundamental properties
        private bool _initialized = false;
        private ManagedDevice? _managedDevice; // Original managed device reference. Null if creating a new one
        public ManagedDevice? ManagedDevice { get => _managedDevice;}
        public DeviceType DataFlow { get; private set; }
        public bool IsComms { get; private set; } = false;

        // Managed device properties, stored here until changes are applied
        private string _managedDeviceName = string.Empty;
        private bool _managedDeviceNameChanged = false;
        public string ManagedDeviceName
        { 
            get => _managedDeviceName;
            set { _managedDeviceName = value; _managedDeviceNameChanged = true; OnPropertyChanged(nameof(ManagedDeviceName));}
        }
        private AudioEndpointInfo? _managedDeviceEndpoint = null;
        public AudioEndpointInfo? ManagedDeviceEndpoint
        {
            get => _managedDeviceEndpoint;
            set {
                _managedDeviceEndpoint = value;
                if (!_managedDeviceNameChanged)
                {
                    _managedDeviceName = value?.Device_DeviceDesc ?? string.Empty;
                    OnPropertyChanged(nameof(ManagedDeviceName));
                }
                OnPropertyChanged(nameof(ManagedDeviceEndpoint));
            }
        }

        // UI bound properties
        public string WindowTitle { get; private set; } = "EditDeviceView";
        private DeviceState _endpointStateFilter = DeviceState.Active;

        public bool FilterActive
        {
            get => (_endpointStateFilter & DeviceState.Active) == DeviceState.Active;
            set
            {
                if (value) _endpointStateFilter |= DeviceState.Active;
                else _endpointStateFilter &= ~DeviceState.Active;
                OnPropertyChanged(nameof(FilterActive));
                UpdateFilteredEndpoints();
            }
        }
        public bool FilterDisabled
        {
            get => (_endpointStateFilter & DeviceState.Disabled) == DeviceState.Disabled;
            set
            {
                if (value) _endpointStateFilter |= DeviceState.Disabled;
                else _endpointStateFilter &= ~DeviceState.Disabled;
                OnPropertyChanged(nameof(FilterDisabled));
                UpdateFilteredEndpoints();
            }
        }
        public bool FilterNotPresent
        {
            get => (_endpointStateFilter & DeviceState.NotPresent) == DeviceState.NotPresent;
            set
            {
                if (value) _endpointStateFilter |= DeviceState.NotPresent;
                else _endpointStateFilter &= ~DeviceState.NotPresent;
                OnPropertyChanged(nameof(FilterNotPresent));
                UpdateFilteredEndpoints();
            }
        }
        public bool FilterUnplugged
        {
            get => (_endpointStateFilter & DeviceState.Unplugged) == DeviceState.Unplugged;
            set
            {
                if (value) _endpointStateFilter |= DeviceState.Unplugged;
                else _endpointStateFilter &= ~DeviceState.Unplugged;
                OnPropertyChanged(nameof(FilterUnplugged));
                UpdateFilteredEndpoints();
            }
        }

        private ObservableCollection<AudioEndpointInfo> _filteredEndpoints = new();
        public ReadOnlyObservableCollection<AudioEndpointInfo> FilteredEndpoints => new(_filteredEndpoints);


        public void Initialize(ManagedDevice device, bool isComms)
        {
            //Editing an existing managed device
            Debug.Assert(!_initialized);
            if (_initialized) return;
            _managedDevice = device;
            DataFlow = device.DataFlow();
            IsComms = isComms;
            WindowTitle = "Edit " + (DataFlow == DeviceType.Capture ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            ManagedDeviceName = device.Name;
            ManagedDeviceEndpoint = device.EndpointInfo;

            _initialized = true;
            UpdateFilteredEndpoints();
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }

        public void Initialize(DeviceType dataFlow, bool isComms)
        {
            //Creating a new managed device
            Debug.Assert(!_initialized);
            if (_initialized) return;
            DataFlow = dataFlow;
            IsComms = isComms;
            WindowTitle = "New " + (DataFlow == DeviceType.Capture ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            _initialized = true;
            UpdateFilteredEndpoints();
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }

        private void UpdateFilteredEndpoints()
        {
            // Note: _initialized could be true or false at this time

            // Generate a new filtered list of endpoints
            var filtered = App.AudioEndpointManager.CachedEndpoints
                .Where(item => (item.DataFlow == DataFlow) && (item.DeviceState & _endpointStateFilter) != 0)
                .ToList();

            // These three steps must be performed in this order, or else things break.
            // Step 1: Put the currently selected endpoint at the top of the list, moving it if it already exists
            if (_managedDeviceEndpoint != null)
            {
                filtered.Remove(_managedDeviceEndpoint.Value);
                filtered.Insert(0, _managedDeviceEndpoint.Value);
            }

            // Step 2: Update the filtered list with the new contents
            _filteredEndpoints.Clear();
            foreach (var item in filtered)
            {
                _filteredEndpoints.Add(item);
            }

            // Step 3: Re-select the endpoint.
            // Even if there was no previous selection, this will select the first item in the list.
            if (_filteredEndpoints.Count > 0) ManagedDeviceEndpoint = _filteredEndpoints[0];
        }

        public void RefreshDevices()
        {
            App.AudioEndpointManager.UpdateCachedEndpoints();
            UpdateFilteredEndpoints();
        }

        public bool Apply()
        {
            // Applies changes to the managed device. Returns true if successful.
            Debug.Assert(_initialized);
            if (_initialized == false) return false;
            if (ManagedDeviceEndpoint is null)
            {
                MessageBox.Show("Please select an audio endpoint.", "No Endpoint Selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (_managedDevice is null)
            {
                // Creating a new managed device
                _managedDevice = DataFlow switch
                {
                    DeviceType.Capture => new ManagedInputDevice(ManagedDeviceEndpoint.Value),
                    DeviceType.Playback => new ManagedOutputDevice(ManagedDeviceEndpoint.Value),
                    _ => throw new NotImplementedException()
                };
                _managedDevice.Name = ManagedDeviceName;
                App.UserSettings.ManagedDevices.AddDevice(_managedDevice, IsComms);
                return true;
            }
            else
            {
                // Editing an existing managed device
                if (App.UserSettings.ManagedDevices.HasDevice(_managedDevice, IsComms))
                {
                    _managedDevice.Name = ManagedDeviceName;
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
