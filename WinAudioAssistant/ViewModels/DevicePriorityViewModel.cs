using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GongSolutions.Wpf.DragDrop;
using AudioSwitcher.AudioApi;
using WinAudioAssistant.Models;
using WinAudioAssistant.Views;

namespace WinAudioAssistant.ViewModels
{
    // Assigned to each ListBox in the view code-behind
    public struct ListBoxTag
    {
        public DeviceType DataFlow;
        public bool IsComms;
    }

    public class DevicePriorityViewModel : ViewModel, IDropTarget
    {
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            // Confirm that the data is a device and the source and target are listboxes
            if (dropInfo.Data is ManagedDevice device &&
                dropInfo.DragInfo.VisualSource is ListBox sourceBox &&
                dropInfo.VisualTarget is ListBox targetBox &&
                targetBox.Tag is ListBoxTag targetTag)
            {
                // Confirm that the device I/O type matches the listbox I/O type
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

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ManagedDevice device &&
                dropInfo.VisualTarget is ListBox targetBox &&
                targetBox.Tag is ListBoxTag targetTag)
            {
                App.UserSettings.AddManagedDeviceAt(device, targetTag.IsComms,
                    dropInfo.InsertIndex == 0 ? 0 : dropInfo.InsertIndex - 1); //Fix index off-by-one when dropping.
            }
        }

        public bool SeparateCommsPriority
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
                OnPropertyChanged(nameof(SeparateCommsPriority));

                // When this is toggled, the references to the comms devices change (DeviceManager.SeparateCommsPriorityState)
                OnPropertyChanged(nameof(CommsInputDevices));
                OnPropertyChanged(nameof(CommsOutputDevices));
            }
        }

        public ReadOnlyObservableCollection<ManagedInputDevice> InputDevices => App.UserSettings.ManagedInputDevices;
        public ReadOnlyObservableCollection<ManagedInputDevice> CommsInputDevices => App.UserSettings.ManagedCommsInputDevices;
        public ReadOnlyObservableCollection<ManagedOutputDevice> OutputDevices => App.UserSettings.ManagedOutputDevices;
        public ReadOnlyObservableCollection<ManagedOutputDevice> CommsOutputDevices => App.UserSettings.ManagedCommsOutputDevices;

        // Updated in the view whenever the context menu is opened
        public ListBox? ActiveListBox { get; set; } // Tracks which ListBox was most recently interacted with
        public Action CloseViewAction { get; set; } = () => { }; // Set in the view

        public RelayCommand AddDeviceCommand { get; }
        public RelayCommand EditDeviceCommand { get; }
        public RelayCommand RemoveDeviceCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand ApplyCommand { get; }

        public DevicePriorityViewModel()
        {
            AddDeviceCommand = new RelayCommand(AddDevice);
            EditDeviceCommand = new RelayCommand(EditDevice, CanEditDevice);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice, CanRemoveDevice);
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
            ApplyCommand = new RelayCommand((object? p) => { Apply(p); }); // Apply() has bool return type
        }

        private void AddDevice(object? parameter)
        {
            Debug.Assert(ActiveListBox?.Tag is ListBoxTag);
            if (ActiveListBox?.Tag is ListBoxTag tag)
            {
                var editDeviceView = new EditDeviceView();
                ((EditDeviceViewModel)editDeviceView.DataContext).Initialize(tag.DataFlow, tag.IsComms);
                editDeviceView.Show();
            }
        }

        private void EditDevice(object? parameter)
        {
            Debug.Assert(ActiveListBox?.SelectedItem is ManagedDevice);
            Debug.Assert(ActiveListBox.Tag is ListBoxTag);
            if (ActiveListBox?.SelectedItem is ManagedDevice device && ActiveListBox.Tag is ListBoxTag tag)
            {
                // Check if device is already being edited, and if so focus the window.
                foreach (var window in App.Current.Windows)
                {
                    if (window is EditDeviceView editDeviceView &&
                        editDeviceView.DataContext is EditDeviceViewModel editDeviceViewModel &&
                        editDeviceViewModel.ManagedDevice == device)
                    {
                        editDeviceView.Focus();
                        return;
                    }
                }
                var newEditDeviceView = new EditDeviceView();
                ((EditDeviceViewModel)newEditDeviceView.DataContext).Initialize(device, tag.IsComms);
                newEditDeviceView.Show();
            }
        }

        private bool CanEditDevice(object? parameter)
        {
            return ActiveListBox?.SelectedItem is ManagedDevice;
        }

        private void RemoveDevice(object? parameter)
        {
            Debug.Assert(ActiveListBox?.SelectedItem is ManagedDevice);
            Debug.Assert(ActiveListBox.Tag is ListBoxTag);
            if (ActiveListBox?.SelectedItem is ManagedDevice device && ActiveListBox.Tag is ListBoxTag tag)
            {
                App.UserSettings.RemoveManagedDevice(device, tag.IsComms);
            }
        }

        private bool CanRemoveDevice(object? parameter)
        {
            return ActiveListBox?.SelectedItem is ManagedDevice;
        }

        public void Ok(object? parameter)
        {
            if (Apply(parameter))
                CloseViewAction();
        }

        public void Cancel(object? parameter)
        {
            App.UserSettings.Load();
            CloseViewAction();
        }

        public bool Apply(object? parameter)
        {
            App.UserSettings.Save();
            return true;
        }

        public void PriorityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // We can't move this to the setter of a single one-way bound SelectedItem property because we need to know which ListBox it's in
            // We could instead create 4 separate SelectedItem properties, but that's a lot of code duplication
            Debug.Assert(sender is ListBox);
            if (sender is ListBox listBox)
            {
                ActiveListBox = listBox;
                // Notify DeviceCommands that their CanExecute result may have changed
                EditDeviceCommand.RaiseCanExecuteChanged();
                RemoveDeviceCommand.RaiseCanExecuteChanged();
            }
        }

        public void PriorityListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Debug.Assert(sender is ListBox);
            if (sender is ListBox listBox)
            {
                ActiveListBox = listBox;
                // Notify DeviceCommands that their CanExecute result may have changed
                EditDeviceCommand.RaiseCanExecuteChanged();
                RemoveDeviceCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
