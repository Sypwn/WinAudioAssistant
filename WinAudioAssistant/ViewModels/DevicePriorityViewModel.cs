using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WinAudioAssistant.Models;

namespace WinAudioAssistant.ViewModels
{
    public struct ListBoxTag
    {
        public DeviceIOType Type;
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
            if (dropInfo.Data is Device device &&
                dropInfo.DragInfo.VisualSource is ListBox sourceBox &&
                dropInfo.VisualTarget is ListBox targetBox &&
                targetBox.Tag is ListBoxTag targetTag)
            {
                // Confirm that the device I/O type matches the listbox I/O type
                if (device.Type == targetTag.Type)
                {
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
            if (dropInfo.Data is Device device &&
                dropInfo.VisualTarget is ListBox targetBox &&
                targetBox.Tag is ListBoxTag targetTag)
            {
                if (targetTag.IsComms)
                {
                    App.UserSettings.DeviceManager.AddCommsDeviceAt(device, dropInfo.InsertIndex == 0 ? 0 : dropInfo.InsertIndex - 1);
                }
                else
                {
                    App.UserSettings.DeviceManager.AddDeviceAt(device, dropInfo.InsertIndex == 0 ? 0 : dropInfo.InsertIndex - 1);
                }
            }
        }

        public bool SeparateCommsPriority
        {
            get => App.UserSettings.SeparateCommsPriority;
            set
            {
                App.UserSettings.SeparateCommsPriority = value;
                OnPropertyChanged(nameof(SeparateCommsPriority));
                OnPropertyChanged(nameof(CommsInputDevices));
                OnPropertyChanged(nameof(CommsOutputDevices));
            }
        }

        public ReadOnlyObservableCollection<InputDevice> InputDevices => App.UserSettings.DeviceManager.InputDevices;
        public ReadOnlyObservableCollection<InputDevice> CommsInputDevices => App.UserSettings.DeviceManager.CommsInputDevices;
        public ReadOnlyObservableCollection<OutputDevice> OutputDevices => App.UserSettings.DeviceManager.OutputDevices;
        public ReadOnlyObservableCollection<OutputDevice> CommsOutputDevices => App.UserSettings.DeviceManager.CommsOutputDevices;

        public ListBox? ContextMenuListBox { get; set; }

        public RelayCommand RemoveDeviceCommand { get; }

        public DevicePriorityViewModel()
        {
            RemoveDeviceCommand = new RelayCommand(RemoveDevice, CanRemoveDevice);
        }

        private void RemoveDevice(object? parameter)
        {
            if (ContextMenuListBox?.SelectedItem is Device device)
            {
                if (ContextMenuListBox.Tag is ListBoxTag tag && tag.IsComms)
                {
                    App.UserSettings.DeviceManager.RemoveCommsDevice(device);
                }
                else
                {
                    App.UserSettings.DeviceManager.RemoveDevice(device);
                }
            }
        }

        private bool CanRemoveDevice(object? parameter)
        {
            return ContextMenuListBox?.SelectedItem is Device;
        }

    }
}
