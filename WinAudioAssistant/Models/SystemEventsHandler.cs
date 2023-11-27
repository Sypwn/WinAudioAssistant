using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AudioSwitcher.AudioApi;


namespace WinAudioAssistant.Models
{
    public class SystemEventsHandler
    {
        public void RegisterAllEvents()
        {
            RegisterAudioDeviceEvents();
            RegisterHardwareEvents();
            RegisterApplicationEvents();
        }

        // Audio device event handling
        private void RegisterAudioDeviceEvents()
        {
            App.CoreAudioController.AudioDeviceChanged.Subscribe(OnDeviceChanged);
        }

        public static void OnDeviceChanged(DeviceChangedArgs args)
        {
            // Note: Gets called from other threads
            // TODO: Optimize
            Debug.WriteLine("{0} - {1}", args.Device.Name, args.ChangedType.ToString());
            switch (args.ChangedType)
            {
                case DeviceChangedType.PropertyChanged:
                case DeviceChangedType.MuteChanged:
                case DeviceChangedType.VolumeChanged:
                case DeviceChangedType.PeakValueChanged:
                    return; // These changes are ignored
                case DeviceChangedType.DeviceAdded:
                case DeviceChangedType.DeviceRemoved:
                case DeviceChangedType.StateChanged:
                    App.Current.Dispatcher.BeginInvoke(App.AudioEndpointManager.UpdateCachedEndpoints);
                    return;
                case DeviceChangedType.DefaultChanged:
                    App.Current.Dispatcher.BeginInvoke(App.UserSettings.ManagedDevices.UpdateDefaultDevices);
                    return;
            }
        }

        // Hardware event handling
        private void RegisterHardwareEvents()
        {
            // Set up WMI event watchers and associate with handler methods
        }

        // Application event handling
        private void RegisterApplicationEvents()
        {
            // Set up application launch/close event handling
        }
    }
}