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
    public class EditDeviceViewModel : BaseViewModel
    {
        public struct DeviceIdentificationMethodOption
        {
            public string Name { get; set; }
            public ManagedDevice.IdentificationMethods Value { get; set; }
            public string ToolTip { get; set; }
        }
        public const int IconSize = 32;

        // === ViewModel properties ===
        public RelayCommand RefreshDevicesCommand { get; }
        private bool _initialized = false;
        private ManagedDevice? _managedDevice; // Original managed device reference. Null if creating a new one
        public ManagedDevice? ManagedDevice { get => _managedDevice;}
        public DeviceType DataFlow { get; private set; }
        public bool IsComms { get; private set; } = false;

        // === Managed device fields ===
        // Stored here until changes are applied
        private string _managedDeviceName = string.Empty;
        private AudioEndpointInfo? _managedDeviceEndpointInfo = null;
        private ManagedDevice.IdentificationMethods _managedDeviceIentificationMethod = ManagedDevice.IdentificationMethods.Loose;
        private EnumFlags<ManagedDevice.IdentificationFlags> _managedDeviceCustomIdentificationFlags = new(ManagedDevice.IdentificationFlags.Loose);

        // === UI bound properties ===
        public string WindowTitle { get; private set; } = "EditDeviceView";
        // The name starts empty, but is set to the endpoint's name when one is selected, until a custom name is provided.
        private bool _managedDeviceNameChanged = false;
        public string ManagedDeviceName
        {
            get => _managedDeviceName;
            set
            {
                _managedDeviceName = value;
                _managedDeviceNameChanged = true;
                OnPropertyChanged(nameof(ManagedDeviceName));
            }
        }
        public AudioEndpointInfo? ManagedDeviceEndpointInfo
        {
            get => _managedDeviceEndpointInfo;
            set
            {
                _managedDeviceEndpointInfo = value;
                if (!_managedDeviceNameChanged)
                {
                    _managedDeviceName = value?.Device_DeviceDesc ?? string.Empty;
                    OnPropertyChanged(nameof(ManagedDeviceName));
                }
                OnPropertyChanged(nameof(ManagedDeviceEndpointInfo));
            }
        }
        public EnumFlags<DeviceState> EndpointListFilter { get; } = new(DeviceState.Active);

        private ObservableCollection<AudioEndpointInfo> _filteredEndpoints = new();
        public ReadOnlyObservableCollection<AudioEndpointInfo> FilteredEndpoints => new(_filteredEndpoints);
        public ObservableCollection<DeviceIdentificationMethodOption> ManagedDeviceIdentificationMethods { get; } = new(); // Populated in the view
        public DeviceIdentificationMethodOption ManagedDeviceIdentificationMethod
        {
            get => ManagedDeviceIdentificationMethods.FirstOrDefault(item => item.Value == _managedDeviceIentificationMethod);
            set
            {
                _managedDeviceIentificationMethod = value.Value;
                OnPropertyChanged(nameof(ManagedDeviceIdentificationMethod));
                OnPropertyChanged(nameof(ShowCustomIdentificationFlags));
            }
        }
        public EnumFlags<ManagedDevice.IdentificationFlags> ManagedDeviceCustomIdentificationFlags { get => _managedDeviceCustomIdentificationFlags; }
        public bool ShowCustomIdentificationFlags => _managedDeviceIentificationMethod == ManagedDevice.IdentificationMethods.Custom;
        public ManagedDevice.IdentificationFlags AvailableCustomIdentificationFlags = ManagedDevice.IdentificationFlags.None; // Set by the View

        public EditDeviceViewModel()
        {
            Debug.Assert(App.DevicePriorityViewModel is not null);
            Debug.Assert(App.DevicePriorityViewModel?.EditDeviceViewModels.Contains(this) == false);
            App.DevicePriorityViewModel?.EditDeviceViewModels.Add(this);
            RefreshDevicesCommand = new RelayCommand(RefreshDevices);
        }

        public void InitializeEdit(ManagedDevice device, bool isComms)
        {
            //Editing an existing managed device
            Debug.Assert(!_initialized);
            if (_initialized) return;
            _managedDevice = device;
            DataFlow = device.DataFlow();
            IsComms = isComms;
            WindowTitle = "Edit " + (DataFlow == DeviceType.Capture ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            _managedDeviceName = device.Name;
            _managedDeviceEndpointInfo = device.EndpointInfo;
            _managedDeviceIentificationMethod = device.IdentificationMethod;
            _managedDeviceCustomIdentificationFlags.Value = device.CustomIdentificationFlags & AvailableCustomIdentificationFlags; // Mask out any flags that are not available to be set

            Initialize();
        }

        public void InitializeNew(DeviceType dataFlow, bool isComms)
        {
            //Creating a new managed device
            Debug.Assert(!_initialized);
            if (_initialized) return;
            DataFlow = dataFlow;
            IsComms = isComms;
            WindowTitle = "New " + (DataFlow == DeviceType.Capture ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            Initialize();
        }

        private void Initialize()
        {
            // When an endpoint list filter checkbox is toggled, or when the list of unpoints is updated, update the filtered list of endpoints
            EndpointListFilter.PropertyChanged += (_, _) => UpdateFilteredEndpoints();
            SystemEventsHandler.UpdatedCachedEndpointsEvent += (_, _) => UpdateFilteredEndpoints();

            UpdateFilteredEndpoints();
            _initialized = true;
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }

        private void UpdateFilteredEndpoints()
        {
            // Note: _initialized could be true or false at this time

            // Generate a new filtered list of endpoints
            var filtered = App.AudioEndpointManager.CachedEndpoints
                .Where(item => (item.DataFlow == DataFlow) && (item.DeviceState & EndpointListFilter.Value) != 0)
                .ToList();

            // These three steps must be performed in this order, or else things break.
            // Step 1: Put the currently selected endpoint at the top of the list, moving it if it already exists
            if (_managedDeviceEndpointInfo != null)
            {
                filtered.Remove(_managedDeviceEndpointInfo.Value);
                filtered.Insert(0, _managedDeviceEndpointInfo.Value);
            }

            // Step 2: Update the filtered list with the new contents
            _filteredEndpoints.Clear();
            foreach (var item in filtered)
            {
                _filteredEndpoints.Add(item);
            }

            // Step 3: Re-select the endpoint.
            // Even if there was no previous selection, this will select the first item in the list.
            if (_filteredEndpoints.Count > 0) ManagedDeviceEndpointInfo = _filteredEndpoints[0];
        }

        public void RefreshDevices(object? parameter)
        {
            SystemEventsHandler.DispatchUpdateCachedEndpoints();
        }

        public override bool Apply()
        {
            // Applies changes to the managed device. Returns true if successful.
            Debug.Assert(_initialized);
            if (_initialized == false) return false;
            if (_managedDeviceEndpointInfo is null)
            {
                MessageBox.Show("Please select an audio endpoint.", "No Endpoint Selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (_managedDevice is null)
            {
                // Creating a new managed device
                _managedDevice = DataFlow switch
                {
                    DeviceType.Capture => new ManagedInputDevice(_managedDeviceEndpointInfo.Value),
                    DeviceType.Playback => new ManagedOutputDevice(_managedDeviceEndpointInfo.Value),
                    _ => throw new NotImplementedException()
                };
                _managedDevice.Name = ManagedDeviceName;
                _managedDevice.IdentificationMethod = _managedDeviceIentificationMethod;
                _managedDevice.CustomIdentificationFlags = _managedDeviceCustomIdentificationFlags.Value & AvailableCustomIdentificationFlags;
                App.UserSettings.AddManagedDevice(_managedDevice, IsComms);
                return true;
            }
            else
            {
                // Editing an existing managed device
                if (App.UserSettings.HasManagedDevice(_managedDevice, IsComms))
                {
                    _managedDevice.Name = ManagedDeviceName;
                    _managedDevice.SetEndpoint(_managedDeviceEndpointInfo.Value);
                    _managedDevice.IdentificationMethod = _managedDeviceIentificationMethod;
                    _managedDevice.CustomIdentificationFlags = _managedDeviceCustomIdentificationFlags.Value & AvailableCustomIdentificationFlags;
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

        public override bool Discard() { return true; }

        public override bool ShouldClose() { return true; }

        public override void Cleanup()
        {
            Debug.Assert(App.DevicePriorityViewModel is not null);
            Debug.Assert(App.DevicePriorityViewModel?.EditDeviceViewModels.Contains(this) == true);
            App.DevicePriorityViewModel?.EditDeviceViewModels.Remove(this);

            Debug.Assert(_initialized);
            if (!_initialized) return;
            _initialized = false;

            EndpointListFilter.PropertyChanged -= (_, _) => UpdateFilteredEndpoints();
            SystemEventsHandler.UpdatedCachedEndpointsEvent -= (_, _) => UpdateFilteredEndpoints();
        }
    }
}
