﻿using GongSolutions.Wpf.DragDrop;
using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WinAudioAssistant.Models;
using WinAudioAssistant.Views;

namespace WinAudioAssistant.ViewModels
{
    // Assigned to each ListBox in the view code-behind
    public struct ListBoxTag
    {
        public DataFlow DataFlow;
        public bool IsComms;
    }

    public class DevicePriorityViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                App.UserSettings.DeviceManager.AddDeviceAt(device, targetTag.IsComms,
                    dropInfo.InsertIndex == 0 ? 0 : dropInfo.InsertIndex - 1); //Fix index off-by-one when dropping.
            }
        }

        public bool SeparateCommsPriority
        {
            get => App.UserSettings.SeparateCommsPriority;
            set
            {
                App.UserSettings.SeparateCommsPriority = value;
                OnPropertyChanged(nameof(SeparateCommsPriority));

                // When this is toggled, the references to the comms devices change (DeviceManager.SeparateCommsPriorityState)
                OnPropertyChanged(nameof(CommsInputDevices));
                OnPropertyChanged(nameof(CommsOutputDevices));
            }
        }

        public ReadOnlyObservableCollection<ManagedInputDevice> InputDevices => App.UserSettings.DeviceManager.InputDevices;
        public ReadOnlyObservableCollection<ManagedInputDevice> CommsInputDevices => App.UserSettings.DeviceManager.CommsInputDevices;
        public ReadOnlyObservableCollection<ManagedOutputDevice> OutputDevices => App.UserSettings.DeviceManager.OutputDevices;
        public ReadOnlyObservableCollection<ManagedOutputDevice> CommsOutputDevices => App.UserSettings.DeviceManager.CommsOutputDevices;

        // Updated in the view whenever the context menu is opened
        public ListBox? ContextMenuListBox { get; set; }

        public RelayCommand AddDeviceCommand { get; }
        public RelayCommand EditDeviceCommand { get; }
        public RelayCommand RemoveDeviceCommand { get; }

        public DevicePriorityViewModel()
        {
            AddDeviceCommand = new RelayCommand(AddDevice);
            EditDeviceCommand = new RelayCommand(EditDevice, CanEditDevice);
            RemoveDeviceCommand = new RelayCommand(RemoveDevice, CanRemoveDevice);
        }

        private void AddDevice(object? parameter)
        {
            Debug.Assert(ContextMenuListBox?.Tag is ListBoxTag);
            if (ContextMenuListBox?.Tag is ListBoxTag tag)
            {
                var editDeviceView = new EditDeviceView();
                ((EditDeviceViewModel)editDeviceView.DataContext).Initialize(tag.DataFlow, tag.IsComms);
                editDeviceView.Show();
            }
        }

        private void EditDevice(object? parameter)
        {
            Debug.Assert(ContextMenuListBox?.SelectedItem is ManagedDevice);
            Debug.Assert(ContextMenuListBox.Tag is ListBoxTag);
            if (ContextMenuListBox?.SelectedItem is ManagedDevice device && ContextMenuListBox.Tag is ListBoxTag tag)
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
            Debug.Assert(ContextMenuListBox is ListBox);
            return ContextMenuListBox?.SelectedItem is ManagedDevice;
        }

        private void RemoveDevice(object? parameter)
        {
            Debug.Assert(ContextMenuListBox?.SelectedItem is ManagedDevice);
            Debug.Assert(ContextMenuListBox.Tag is ListBoxTag);
            if (ContextMenuListBox?.SelectedItem is ManagedDevice device && ContextMenuListBox.Tag is ListBoxTag tag)
            {
                App.UserSettings.DeviceManager.RemoveDevice(device, tag.IsComms);
            }
        }

        private bool CanRemoveDevice(object? parameter)
        {
            Debug.Assert(ContextMenuListBox is ListBox);
            return ContextMenuListBox?.SelectedItem is ManagedDevice;
        }

    }
}
