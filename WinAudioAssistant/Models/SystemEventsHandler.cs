using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using AudioSwitcher.AudioApi;
using System.Windows.Threading;


namespace WinAudioAssistant.Models
{
    public static class SystemEventsHandler
    {
        private enum DispatchAction
        {
            UpdateDefaultDevices,
            UpdateCachedEndpoints,
        }
        private static bool ready = false;
        private static readonly object activeDispatchLock = new();
        private static DispatchAction? activeDispatchAction;
        private static DispatcherOperation? activeDispatchOperation;

        public delegate void UpdatedDefaultDevicesEventHandler(object? sender, EventArgs e);
        public static event UpdatedDefaultDevicesEventHandler? UpdatedDefaultDevicesEvent;
        public delegate void UpdatedCachedEndpointsEventHandler(object? sender, EventArgs e);
        public static event UpdatedCachedEndpointsEventHandler? UpdatedCachedEndpointsEvent;

        private static void UpdateDefaultDevicesAction()
        {
            App.UserSettings.ManagedDevices.UpdateDefaultDevices();
            UpdatedDefaultDevicesEvent?.Invoke(null, EventArgs.Empty);
        }

        private static void UpdateCachedEndpointsAction()
        {
            App.AudioEndpointManager.UpdateCachedEndpoints();
            DispatchUpdateDefaultDevices();
            UpdatedCachedEndpointsEvent?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Queues an action to update the default devices.
        /// </summary>
        public static void DispatchUpdateDefaultDevices()
        {
            lock (activeDispatchLock)
            {
                Debug.Assert(ready); // This should not be called before RegisterAllEvents()
                if (!ready)
                    return;

                // Abort if there is already an operation pending
                // We don't care which action it is, since both will result in a call to UpdateDefaultDevices
                // The ready check is effectively a null check
                if (activeDispatchOperation!.Status == DispatcherOperationStatus.Pending)
                    return; 

                activeDispatchOperation = App.Current.Dispatcher.BeginInvoke(UpdateDefaultDevicesAction);
                activeDispatchAction = DispatchAction.UpdateDefaultDevices;
            }
        }

        /// <summary>
        /// Queues an action to update the cached endpoints. This will be followed by action to update the default devices.
        /// </summary>
        public static void DispatchUpdateCachedEndpoints()
        {
            lock (activeDispatchLock)
            {
                Debug.Assert(ready); // This should not be called before RegisterAllEvents()
                if (!ready)
                    return;

                // The ready check is effectively a null check
                if (activeDispatchOperation!.Status == DispatcherOperationStatus.Pending)
                {
                    if (activeDispatchAction == DispatchAction.UpdateCachedEndpoints)
                        return; // Already pending, and it's the same action, so we don't need to do anything
                    else
                        activeDispatchOperation.Abort(); // Already pending, but it's UpdateDefaultDevices, so we're going to replace it
                }

                activeDispatchOperation = App.Current.Dispatcher.BeginInvoke(UpdateCachedEndpointsAction);
                activeDispatchAction = DispatchAction.UpdateCachedEndpoints;
            }

        }

        public static void RegisterAllEvents()
        {
            // This ensures they aren't null during Dispatch...() calls, which saves a null check
            activeDispatchOperation = App.Current.Dispatcher.BeginInvoke(UpdateCachedEndpointsAction);
            activeDispatchAction = DispatchAction.UpdateCachedEndpoints;
            ready = true;

            RegisterAudioDeviceEvents();
            RegisterHardwareEvents();
            RegisterApplicationEvents();
        }

        // Audio device event handling
        private static void RegisterAudioDeviceEvents()
        {
            App.CoreAudioController.AudioDeviceChanged.Subscribe(OnDeviceChanged);
        }

        private static void OnDeviceChanged(DeviceChangedArgs args)
        {
            // Note: Gets called from other threads
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
                    DispatchUpdateCachedEndpoints();
                    return;
                case DeviceChangedType.DefaultChanged:
                    DispatchUpdateDefaultDevices();
                    return;
            }
        }

        // Hardware event handling
        private static void RegisterHardwareEvents()
        {
            // Set up WMI event watchers and associate with handler methods
        }

        // Application event handling
        private static void RegisterApplicationEvents()
        {
            // Set up application launch/close event handling
        }
    }
}