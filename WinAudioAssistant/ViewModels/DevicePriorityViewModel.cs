using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using AudioSwitcher.AudioApi;
using WinAudioAssistant.Models;
using WinAudioAssistant.Views;

namespace WinAudioAssistant.ViewModels
{
    /// <summary>
    /// Viewmodel for the device priority configuration window.
    /// Contains a pair of ListBoxes for each managed device dataflow (input/output) and another pair for comms managed devices.
    /// Managed devices can be rearranged within each ListBox, and copied between ListBoxes of the same dataflow.
    /// Managed devices can be added, edited, and removed from each ListBox.
    /// </summary>
    public class DevicePriorityViewModel : BaseViewModel, IDropTarget
    {
        #region Constructors
        /// <summary>
        /// Initializes the DevicePriorityViewModel.
        /// Stores itself as a static reference in the App class.
        /// </summary>
        public DevicePriorityViewModel()
        {
            Debug.Assert(App.DevicePriorityViewModel is null);
            App.DevicePriorityViewModel = this;
            AddDeviceCommand = new RelayCommand(AddDevice);
            EditDeviceCommand = new RelayCommand(EditDevice, CanEditDevice);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice, CanRemoveDevice);
        }
        #endregion

        #region Internal Properties
        public bool PendingChanges { get; set; } = false; // True when there are unsaved changes
        public List<EditDeviceViewModel> EditDeviceViewModels { get; set; } = new(); // Contains a list of child Edit Managed Device dialogs
        public ListBox? ActiveListBox { get; set; } // Tracks which ListBox was most recently interacted with
        #endregion

        #region UI Bound Properties
        public double WindowWidth //Bound to the width of the window, which is saved to user settings.
        {
            get => App.UserSettings.PriorityConfigurationWindowWidth;
            set
            {
                App.UserSettings.PriorityConfigurationWindowWidth = value;
                OnPropertyChanged(nameof(WindowWidth));
            }
        } 
        public double WindowHeight // Bound to the height of the window, which is saved to user settings.
        {
            get => App.UserSettings.PriorityConfigurationWindowHeight;
            set
            {
                App.UserSettings.PriorityConfigurationWindowHeight = value;
                OnPropertyChanged(nameof(WindowHeight));
            }
        } 
        public bool SeparateCommsPriority // Bound to the checkbox that determines whether comms devices are managed separately from primary devices
        {
            get => App.UserSettings.SeparateCommsPriority;
            set
            {
                if (value == false && // If we're unchecking the box and...
                    (App.UserSettings.ManagedCommsOutputDevices.Except(App.UserSettings.ManagedOutputDevices).Any() || // there are any unique managed Comms output devices, or...
                     App.UserSettings.ManagedCommsInputDevices.Except(App.UserSettings.ManagedInputDevices).Any())) // there are any unique managed Comms input devices
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Unchecking this box will clear your managed Comms devices, and instead make them match your primary devices." +
                        "\nAre you sure you want to do this?", "Clear Managed Comms Devices", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (messageBoxResult != MessageBoxResult.Yes) return;
                }
                App.UserSettings.SeparateCommsPriority = value;
                PendingChanges = true;
                OnPropertyChanged(nameof(SeparateCommsPriority));

                // When this is toggled, the references to the comms devices change (DeviceManager.SeparateCommsPriorityState)
                OnPropertyChanged(nameof(CommsInputDevices));
                OnPropertyChanged(nameof(CommsOutputDevices));
            }
        }

        // Properties containing the lists of managed devices, bound to each ListBox
        public static ReadOnlyObservableCollection<ManagedInputDevice> InputDevices => App.UserSettings.ManagedInputDevices;
        public static ReadOnlyObservableCollection<ManagedInputDevice> CommsInputDevices => App.UserSettings.ManagedCommsInputDevices;
        public static ReadOnlyObservableCollection<ManagedOutputDevice> OutputDevices => App.UserSettings.ManagedOutputDevices;
        public static ReadOnlyObservableCollection<ManagedOutputDevice> CommsOutputDevices => App.UserSettings.ManagedCommsOutputDevices;

        public RelayCommand AddDeviceCommand { get; } // Bound to the command to add a new managed device to a ListBox
        public RelayCommand EditDeviceCommand { get; } // Bound to the command to edit an existing managed device in a ListBox
        public RelayCommand RemoveDeviceCommand { get; } // Bound to the command to remove an existing managed device from a ListBox
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        /// <remarks>
        /// Informs the UserSettings class to save all settings to the user settings file.
        /// </remarks>
        public override bool Apply()
        {
            {
                SystemEventsHandler.DispatchUpdateDefaultDevices();
                if (App.UserSettings.Save())
                {
                    PendingChanges = false;
                    return true;
                }
                return false;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Informs the UserSettings class to load all settings from the user settings file, effectively reverting the application state.
        /// </remarks>
        public override bool Discard()
        {
            return App.UserSettings.Load();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Verifies that there are no active EditDeviceViewModels, and that there are no pending changes.
        /// </remarks>
        public override bool ShouldClose()
        {
            if (EditDeviceViewModels.Count > 0)
            {
                EditDeviceViewModels[0].FocusViewAction();
                return false;
            }
            if (PendingChanges)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("You have unsaved changes. Are you sure you want to close this window?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (messageBoxResult == MessageBoxResult.Yes)
                    PendingChanges = false;
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Called when the selection within a ListBox changes.
        /// Updates the active ListBox property, and notifies the Edit and Remove Device commands that their CanExecute result may have changed.
        /// </summary>
        public void PriorityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // We can't move this to the setter of a single one-way bound SelectedItem property because we need to know which ListBox it's in
            // We could instead create 4 separate SelectedItem properties, but that's a lot of code duplication
            Debug.Assert(sender is ListBox);
            if (sender is ListBox listBox)
            {
                ActiveListBox = listBox;
                EditDeviceCommand.RaiseCanExecuteChanged();
                RemoveDeviceCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Called when the context menu of a ListBox is opening (from right click or from context menu key).
        /// Updates the active ListBox property, and notifies the Edit and Remove Device commands that their CanExecute result may have changed.
        /// </summary>
        public void PriorityListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Debug.Assert(sender is ListBox);
            if (sender is ListBox listBox)
            {
                ActiveListBox = listBox;
                EditDeviceCommand.RaiseCanExecuteChanged();
                RemoveDeviceCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called when the Add Device command is activated.
        /// Creates a new EditDeviceView, and initializes the EditDeviceViewModel to create a new managed device.
        /// </summary>
        private void AddDevice(object? parameter)
        {
            Debug.Assert(ActiveListBox?.Tag is ListBoxTag);
            if (ActiveListBox?.Tag is ListBoxTag tag)
            {
                var editDeviceView = new EditDeviceView();
                Debug.Assert(editDeviceView.DataContext is EditDeviceViewModel);
                if (editDeviceView.DataContext is EditDeviceViewModel editDeviceViewModel)
                {
                    editDeviceViewModel.InitializeNew(tag.DataFlow, tag.IsComms);
                    editDeviceView.Show();
                }
                else
                {
                    editDeviceView.Close();
                }
            }
        }

        /// <summary>
        /// Called when the Edit Device command is activated.
        /// Creates a new EditDeviceView, and initializes the EditDeviceViewModel to reference an existing managed device.
        /// </summary>
        private void EditDevice(object? parameter)
        {
            Debug.Assert(ActiveListBox?.SelectedItem is ManagedDevice);
            Debug.Assert(ActiveListBox.Tag is ListBoxTag);
            if (ActiveListBox?.SelectedItem is ManagedDevice device && ActiveListBox.Tag is ListBoxTag tag)
            {
                // Check if device is already being edited, and if so focus the window.
                var existingViewModel = EditDeviceViewModels.Find(item => item.ManagedDevice == device);
                if (existingViewModel is not null)
                {
                    existingViewModel.FocusViewAction();
                    return;
                }

                var editDeviceView = new EditDeviceView();
                Debug.Assert(editDeviceView.DataContext is EditDeviceViewModel);
                if (editDeviceView.DataContext is EditDeviceViewModel editDeviceViewModel)
                {
                    editDeviceViewModel.InitializeEdit(device, tag.IsComms);
                    editDeviceView.Show();
                }
                else
                {
                    editDeviceView.Close();
                }
            }
        }

        /// <summary>
        /// Called to check if the Edit Device command should be available or grayed out.
        /// Verifies that a managed device is selected in the active ListBox.
        /// </summary>
        /// <returns>True if the Edit Device command should be made available</returns>
        private bool CanEditDevice(object? parameter)
        {
            return ActiveListBox?.SelectedItem is ManagedDevice;
        }

        /// <summary>
        /// Called when the Remove Device command is activated.
        /// Removes the selected managed device from the active ListBox, via a call to the UserSettings.
        /// </summary>
        private void RemoveDevice(object? parameter)
        {
            Debug.Assert(ActiveListBox?.SelectedItem is ManagedDevice);
            Debug.Assert(ActiveListBox.Tag is ListBoxTag);
            if (ActiveListBox?.SelectedItem is ManagedDevice device && ActiveListBox.Tag is ListBoxTag tag)
            {
                App.UserSettings.RemoveManagedDevice(device, tag.IsComms);
                PendingChanges = true;
            }
        }

        /// <summary>
        /// Called to check if the Remove Device command should be available or grayed out.
        /// Verifies that a managed device is selected in the active ListBox.
        /// </summary>
        /// <returns>True if the Remove Device command should be made available</returns>
        private bool CanRemoveDevice(object? parameter)
        {
            return ActiveListBox?.SelectedItem is ManagedDevice;
        }
        #endregion

        #region Interface Methods
        /// <summary>
        /// Called when a managed device is dragged over a ListBox.
        /// </summary>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            // Confirm that the data is a device and the source and target are listboxes
            if (dropInfo.Data is ManagedDevice device &&
                dropInfo.DragInfo.VisualSource is ListBox sourceBox &&
                dropInfo.VisualTarget is ListBox targetBox &&
                targetBox.Tag is ListBoxTag targetTag)
            {
                // Confirm that the device dataflow matches the listbox dataflow
                if (device.DataFlow() == targetTag.DataFlow)
                {
                    // Show the caret when dragging over the listbox
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

                    // If dragging within the same listbox, we're moving
                    if (sourceBox == targetBox)
                    {
                        dropInfo.Effects = System.Windows.DragDropEffects.Move;
                        return;
                    }
                    // If dragging between listboxes, we're probably copying
                    else
                    {
                        dropInfo.Effects = System.Windows.DragDropEffects.Copy;
                        return;
                    }
                }
            }
            dropInfo.Effects = System.Windows.DragDropEffects.None;
        }

        /// <summary>
        /// Called when a managed device is dropped on a ListBox.
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ManagedDevice device &&
                dropInfo.VisualTarget is ListBox targetBox &&
                targetBox.Tag is ListBoxTag targetTag)
            {
                App.UserSettings.AddManagedDeviceAt(device, targetTag.IsComms,
                    dropInfo.InsertIndex == 0 ? 0 : dropInfo.InsertIndex - 1); //Fix index off-by-one when dropping.
                PendingChanges = true;
            }
        }
        #endregion

        #region Destructor
        /// <inheritdoc/>
        /// <remarks>
        /// Removes the static reference to itself in the App class.
        /// </remarks>
        public override void Cleanup()
        {
            Debug.Assert(App.DevicePriorityViewModel == this);
            App.DevicePriorityViewModel = null;
        }
        #endregion

        #region Internal Types
        /// <summary>
        /// A struct to store the dataflow and whether the ListBox represents comms devices.
        /// The View will assign one of these to the Tag property of each ListBox.
        /// </summary>
        public struct ListBoxTag
        {
            public DeviceType DataFlow;
            public bool IsComms;
        }
        #endregion
    }
}
