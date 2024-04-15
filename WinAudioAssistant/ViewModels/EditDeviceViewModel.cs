using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AudioSwitcher.AudioApi;
using WinAudioAssistant.Models;

namespace WinAudioAssistant.ViewModels
{
    /// <summary>
    /// Viewmodel for the Add/Edit Managed Device window.
    /// Allows the user to configure all avaiable settings for a managed device.
    /// </summary>
    public class EditDeviceViewModel : BaseViewModel
    {
        #region Constructors and Initialization
        /// <summary>
        /// Partial initialization of the viewmodel.
        /// Adds this viewmodel to the DevicePriorityViewModel's list of EditDeviceViewModels.
        /// </summary>
        public EditDeviceViewModel()
        {
            FilteredEndpoints = new(_filteredEndpoints);
            Debug.Assert(App.DevicePriorityViewModel is not null);
            Debug.Assert(App.DevicePriorityViewModel?.EditDeviceViewModels.Contains(this) == false);
            App.DevicePriorityViewModel?.EditDeviceViewModels.Add(this);

            AddApplicationConditionCommand = new RelayCommand(AddApplicationCondition);
            RemoveActivationConditionCommand = new RelayCommand(RemoveActivationCondition, CanRemoveActivationCondition);
        }

        /// <summary>
        /// Initializes the viewmodel to create a new managed device.
        /// </summary>
        /// <param name="dataFlow">The dataflow of the new managed device.</param>
        /// <param name="isComms">True if the managed device should be added to the communications device list.</param>
        public void InitializeNew(DeviceType dataFlow, bool isComms)
        {
            Debug.Assert(!_initialized);
            if (_initialized)
                return;
            _dataFlow = dataFlow;
            _isComms = isComms;
            WindowTitle = "New " + (_dataFlow == DeviceType.Capture ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";

            Initialize();
        }

        /// <summary>
        /// Initializes the viewmodel to edit an existing managed device.
        /// </summary>
        /// <param name="device">The managed device being edited.</param>
        /// <param name="isComms">True if the managed device is contained in the communications device list.</param>
        public void InitializeEdit(ManagedDevice device, bool isComms)
        {
            Debug.Assert(!_initialized);
            if (_initialized)
                return;
            _managedDevice = device;
            _dataFlow = device.DataFlow();
            _isComms = isComms;
            WindowTitle = "Edit " + (_dataFlow == DeviceType.Capture ? "Input" : "Output") + (isComms ? "Comms " : "") + " Managed Device";
            _managedDeviceNameChanged = device.Name != device.EndpointInfo.Device_DeviceDesc;

            _managedDeviceName = device.Name;
            _managedDeviceEndpointInfo = device.EndpointInfo;
            _managedDeviceIentificationMethod = device.IdentificationMethod;
            _managedDeviceCustomIdentificationFlags.Value = device.CustomIdentificationFlags & AvailableCustomIdentificationFlags; // Mask out any flags that are not available to be set

            Initialize();
        }

        /// <summary>
        /// Common initialization code for both InitializeNew and InitializeEdit.
        /// Registers event handlers.
        /// </summary>
        private void Initialize()
        {
            // When any IdentificationFlags checkbox is toggled, mark the viewmodel as having pending changes
            ManagedDeviceCustomIdentificationFlags.PropertyChanged += (_, _) => _pendingChanges = true;

            // When any endpoint list filter checkbox is toggled, or when the list of system endpoints is refreshed, update the filtered list of endpoints
            EndpointListFilter.PropertyChanged += (_, _) => UpdateFilteredEndpoints();
            SystemEventsHandler.RefreshedCachedEndpointsEvent += (_, _) => UpdateFilteredEndpoints();

            _initialized = true;
            UpdateFilteredEndpoints();
            OnPropertyChanged(string.Empty); // Refreshes all properties
        }
        #endregion

        #region Private Fields
        private bool _initialized = false; // Set to true when the viewmodel is initialized
        private bool _pendingChanges = false; // True when there are unsaved changes
        private DeviceType _dataFlow; // Data flow of the managed device being created/edited
        private bool _isComms = false; // True if the new managed device should be added to the communications device list
        private ManagedDevice? _managedDevice; // Reference to managed device being edited. Null if creating a new managed device.
        private bool _managedDeviceNameChanged = false; // True once the managed device name is changed by the user.
        private readonly ObservableCollection<AudioEndpointInfo> _filteredEndpoints = new(); // List of endpoints that match the current filter settings
        #endregion

        #region Managed Device Fields
        // Stored here until changes are applied
        private string _managedDeviceName = string.Empty;
        private AudioEndpointInfo? _managedDeviceEndpointInfo = null;
        private ManagedDevice.IdentificationMethods _managedDeviceIentificationMethod = ManagedDevice.IdentificationMethods.Loose;
        private readonly EnumFlags<ManagedDevice.IdentificationFlags> _managedDeviceCustomIdentificationFlags = new(ManagedDevice.IdentificationFlags.Loose);
        #endregion

        #region Public Properties
        public ManagedDevice? ManagedDevice { get => _managedDevice;} // Public accessor for the managed device being edited
        public ManagedDevice.IdentificationFlags AvailableCustomIdentificationFlags { get; set; } = ManagedDevice.IdentificationFlags.None; // A mask of the flags that are available to be selected in the UI
        #endregion

        #region UI Bound Properties
        public string WindowTitle { get; private set; } = "EditDeviceView"; // Bound to window title. Changes depending on whether we're creating or editing a managed device
        public string ManagedDeviceName // Bound to a text box. When the name is changed from the UI, we set _managedDeviceNameChanged to true.
        {
            get => _managedDeviceName;
            set
            {
                _managedDeviceName = value;
                _managedDeviceNameChanged = true;
                _pendingChanges = true;
                OnPropertyChanged(nameof(ManagedDeviceName));
            }
        }
        public ReadOnlyObservableCollection<AudioEndpointInfo> FilteredEndpoints { get; } // Bound to a combobox item source.
        public AudioEndpointInfo? ManagedDeviceEndpointInfo // Bound to a combobox selection. Selecting an endpoint also updates the managed device's name, unless a custom name has been provided.
        {
            get => _managedDeviceEndpointInfo;
            set
            {
                _managedDeviceEndpointInfo = value;
                _pendingChanges = true;
                if (!_managedDeviceNameChanged)
                {
                    _managedDeviceName = value?.Device_DeviceDesc ?? string.Empty;
                    OnPropertyChanged(nameof(ManagedDeviceName));
                }
                OnPropertyChanged(nameof(ManagedDeviceEndpointInfo));
            }
        }
        public EnumFlags<DeviceState> EndpointListFilter { get; } = new(DeviceState.Active); // Bound to multiple checkboxes, each of which toggles a flag in the enum.
        public ObservableCollection<DeviceIdentificationMethodOption> ManagedDeviceIdentificationMethods { get; } = new(); // Bound to a combobox item source. The list of identification methods. Populated by the view.
        public DeviceIdentificationMethodOption ManagedDeviceIdentificationMethod // Bound to a combobox selection.
        {
            get => ManagedDeviceIdentificationMethods.FirstOrDefault(item => item.Value == _managedDeviceIentificationMethod);
            set
            {
                _managedDeviceIentificationMethod = value.Value;
                _pendingChanges = true;
                OnPropertyChanged(nameof(ManagedDeviceIdentificationMethod));
                OnPropertyChanged(nameof(ShowCustomIdentificationFlags));
            }
        }
        public EnumFlags<ManagedDevice.IdentificationFlags> ManagedDeviceCustomIdentificationFlags { get => _managedDeviceCustomIdentificationFlags; } // Bound to multiple checkboxes, each of which toggles a flag in the enum.
        public bool ShowCustomIdentificationFlags => _managedDeviceIentificationMethod == ManagedDevice.IdentificationMethods.Custom; // Bound to the visibility of the custom identification flags checkboxes.
        public static BitmapSource ApplicationConditionIcon => App.IconManager.GetBitmapFromIconPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll,-153"), 16);
        public static BitmapSource HardwareConditionIcon => App.IconManager.GetBitmapFromIconPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "mmres.dll,-3018"), 16);
        public RelayCommand AddApplicationConditionCommand {  get; }
        public RelayCommand RemoveActivationConditionCommand { get; }
        public ObservableCollection<ActivationConditionViewModel> ActivationConditions { get; } = new();
        private ActivationConditionViewModel? _selectedActivationCondition;
        public ActivationConditionViewModel? SelectedActivationCondition
        {
            get => _selectedActivationCondition;
            set
            {
                _selectedActivationCondition = value;
                OnPropertyChanged(nameof(SelectedActivationCondition));
                RemoveActivationConditionCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        /// <remarks>
        /// If creating a new managed device, create the managed device and add it to the user settings.
        /// If editing an existing managed device, update that managed device from the private fields.
        /// </remarks>
        public override bool Apply()
        {
            // Applies changes to the managed device. Returns true if successful.
            Debug.Assert(_initialized);
            if (_initialized == false)
                return false;
            if (_managedDeviceEndpointInfo is null)
            {
                MessageBox.Show("Please select an audio endpoint.", "No Endpoint Selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (_managedDevice is null)
            {
                // Creating a new managed device
                _managedDevice = _dataFlow switch
                {
                    DeviceType.Capture => new ManagedInputDevice(_managedDeviceEndpointInfo.Value),
                    DeviceType.Playback => new ManagedOutputDevice(_managedDeviceEndpointInfo.Value),
                    _ => throw new NotImplementedException()
                };
                _managedDevice.Name = ManagedDeviceName;
                _managedDevice.IdentificationMethod = _managedDeviceIentificationMethod;
                _managedDevice.CustomIdentificationFlags = _managedDeviceCustomIdentificationFlags.Value & AvailableCustomIdentificationFlags;
                App.UserSettings.AddManagedDevice(_managedDevice, _isComms);
                Debug.Assert(App.DevicePriorityViewModel is not null);
                if (App.DevicePriorityViewModel is DevicePriorityViewModel viewModel)
                    viewModel.PendingChanges = true;
                _pendingChanges = false;
                return true;
            }
            else
            {
                // Editing an existing managed device
                if (App.UserSettings.HasManagedDevice(_managedDevice, _isComms))
                {
                    _managedDevice.Name = ManagedDeviceName;
                    _managedDevice.SetEndpoint(_managedDeviceEndpointInfo.Value);
                    _managedDevice.IdentificationMethod = _managedDeviceIentificationMethod;
                    _managedDevice.CustomIdentificationFlags = _managedDeviceCustomIdentificationFlags.Value & AvailableCustomIdentificationFlags;
                    Debug.Assert(App.DevicePriorityViewModel is not null);
                    if (App.DevicePriorityViewModel is DevicePriorityViewModel viewModel)
                        viewModel.PendingChanges = true;
                    _pendingChanges = false;
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

        /// <inheritdoc/>
        /// <remarks>
        /// No action required.
        /// </remarks>
        public override bool Discard() { return true; }

        /// <inheritdoc/>
        /// <remarks>
        /// Verifies that there are no pending changes.
        /// </remarks>
        public override bool ShouldClose()
        {
            if (_pendingChanges)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("You have unsaved changes. Are you sure you want to close this window?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                    _pendingChanges = false;
                else
                    return false;
            }
            return true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the filtered list of endpoints for use in the endpoint selection combobox, based on the current selection of filters.
        /// </summary>
        private void UpdateFilteredEndpoints()
        {
            // Note: _initialized could be true or false at this time
            if (!_initialized) return;

            // Generate a new filtered list of endpoints
            var filtered = App.AudioEndpointManager.CachedEndpoints
                .Where(item => (item.DataFlow == _dataFlow) && (item.DeviceState & EndpointListFilter.Value) != 0)
                .ToList();

            // These three steps must be performed in this order, or else things break.
            // Step 1: Put the currently selected endpoint at the top of the new list, moving it if it already exists
            if (_managedDeviceEndpointInfo != null)
            {
                filtered.Remove(_managedDeviceEndpointInfo.Value);
                filtered.Insert(0, _managedDeviceEndpointInfo.Value);
            }

            // Step 2: Replace the main list with the new list contents
            var previouslyPendingChanges = _pendingChanges;
            _filteredEndpoints.Clear();
            foreach (var item in filtered)
            {
                _filteredEndpoints.Add(item);
            }

            // Step 3: Re-select the endpoint.
            // If there was no previous selection, we'll select the first item in the list anyway.
            if (_filteredEndpoints.Count > 0) ManagedDeviceEndpointInfo = _filteredEndpoints[0];
            // But we don't want this to affect the state of PendingChanges
            _pendingChanges = previouslyPendingChanges;
        }

        private void AddApplicationCondition(object? parameter)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Applications (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "Select an application to add as a condition"
            };
            if (dialog.ShowDialog() == true)
            {
                var condition = new ActivationConditionViewModel(new ManagedDevice.ActivationCondition(ManagedDevice.ActivationConditionType.Application, dialog.FileName));
                ActivationConditions.Add(condition);
            }
        }

        private bool CanRemoveActivationCondition(object? parameter) => SelectedActivationCondition != null;

        private void RemoveActivationCondition(object? parameter)
        {
            if (SelectedActivationCondition != null)
                ActivationConditions.Remove(SelectedActivationCondition);
        }
        #endregion

        #region Destructor
        /// <inheritdoc/>
        /// <remarks>
        /// Removes this viewmodel from the DevicePriorityViewModel's list of EditDeviceViewModels.
        /// Unregisters event handlers.
        /// </remarks>
        public override void Cleanup()
        {
            Debug.Assert(App.DevicePriorityViewModel is not null);
            Debug.Assert(App.DevicePriorityViewModel?.EditDeviceViewModels.Contains(this) == true);
            App.DevicePriorityViewModel?.EditDeviceViewModels.Remove(this);

            Debug.Assert(_initialized);
            if (!_initialized) return;
            _initialized = false;

            EndpointListFilter.PropertyChanged -= (_, _) => UpdateFilteredEndpoints();
            ManagedDeviceCustomIdentificationFlags.PropertyChanged -= (_, _) => _pendingChanges = true;
            SystemEventsHandler.RefreshedCachedEndpointsEvent -= (_, _) => UpdateFilteredEndpoints();
        }
        #endregion

        #region Internal Types
        /// <summary>
        /// Represents a device identification method option.
        /// Used by the appropriate combobox in the view.
        /// </summary>
        public struct DeviceIdentificationMethodOption
        {
            public string Name { get; set; }
            public ManagedDevice.IdentificationMethods Value { get; set; }
            public string ToolTip { get; set; }
        }

        public class ActivationConditionViewModel
        {
            public ActivationConditionViewModel (ManagedDevice.ActivationCondition condition) => Condition = condition;
            public ManagedDevice.ActivationCondition Condition { get; }
            public BitmapSource TypeIcon => Condition.Type switch
            {
                ManagedDevice.ActivationConditionType.Application => ApplicationConditionIcon,
                ManagedDevice.ActivationConditionType.Hardware => HardwareConditionIcon,
                _ => throw new NotImplementedException()
            };
            public string Path => Condition.Type switch
            {
                ManagedDevice.ActivationConditionType.Application => Condition.ApplicationPath!,
                ManagedDevice.ActivationConditionType.Hardware => string.Empty,
                _ => throw new NotImplementedException()
            };
            
        }
        #endregion
    }
}
