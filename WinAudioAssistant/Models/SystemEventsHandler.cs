using System.Diagnostics;
using System.Windows.Threading;
using AudioSwitcher.AudioApi;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Handles system events, and dispatches actions to update the application state.
    /// All requests to update audio endpoints or system default audio devices should be routed through this class,
    /// as it ensures that they are handled by the UI thread, and that they are not queued unnecessarily.
    /// </summary>
    public static class SystemEventsHandler
    {
        #region Private Fields
        private static bool _ready = false; // Set to true after RegisterAllEvents() is called
        private static readonly object _activeDispatchLock = new(); // Used to synchronize access to _activeDispatchOperation and _activeDispatchAction
        private static DispatchAction? _activeDispatchAction; // The type of opertation that is currently pending
        private static DispatcherOperation? _activeDispatchOperation; // The reference to the operation that is currently pending
        #endregion

        #region Public Properties
        public delegate void UpdatedDefaultDevicesEventHandler(object? sender, EventArgs e);
        /// <summary>
        /// Raised after the system default audio devices are updated.
        /// </summary>
        public static event UpdatedDefaultDevicesEventHandler? UpdatedDefaultDevicesEvent;
        public delegate void UpdatedCachedEndpointsEventHandler(object? sender, EventArgs e);
        /// <summary>
        /// Raised after the cached audio endpoints are updated.
        /// </summary>
        public static event UpdatedCachedEndpointsEventHandler? UpdatedCachedEndpointsEvent;
        #endregion

        #region Public Methods
        /// <summary>
        /// Registers all event handlers for system events.
        /// Must be called before any other methods in this class.
        /// </summary>
        public static void RegisterAllEvents()
        {
            // Startiing with a UpdateCachedEndpointsAction dispatch ensures these fields aren't null during Dispatch...() calls, which saves a null check
            _activeDispatchOperation = App.Current.Dispatcher.BeginInvoke(UpdateCachedEndpointsAction);
            _activeDispatchAction = DispatchAction.UpdateCachedEndpoints;
            _ready = true;

            RegisterAudioDeviceEvents();
            RegisterHardwareEvents();
            RegisterApplicationEvents();
        }

        /// <summary>
        /// Queues an action to update the system default devices.
        /// </summary>
        public static void DispatchUpdateDefaultDevices()
        {
            lock (_activeDispatchLock)
            {
                Debug.Assert(_ready); // This should not be called before RegisterAllEvents()
                if (!_ready)
                    return;

                // Abort if there is already an operation pending
                // We don't care which action it is, since both will result in a call to UpdateDefaultDevices
                // The ready check is effectively a null check
                if (_activeDispatchOperation!.Status == DispatcherOperationStatus.Pending)
                    return; 

                _activeDispatchOperation = App.Current.Dispatcher.BeginInvoke(UpdateDefaultDevicesAction);
                _activeDispatchAction = DispatchAction.UpdateDefaultDevices;
            }
        }

        /// <summary>
        /// Queues an action to update the cached endpoints. This will be followed by action to update the default devices.
        /// </summary>
        public static void DispatchUpdateCachedEndpoints()
        {
            lock (_activeDispatchLock)
            {
                Debug.Assert(_ready); // This should not be called before RegisterAllEvents()
                if (!_ready)
                    return;

                // The ready check is effectively a null check
                if (_activeDispatchOperation!.Status == DispatcherOperationStatus.Pending)
                {
                    if (_activeDispatchAction == DispatchAction.UpdateCachedEndpoints)
                        return; // Already pending, and it's the same action, so we don't need to do anything
                    else
                        _activeDispatchOperation.Abort(); // Already pending, but it's UpdateDefaultDevices, so we're going to replace it
                }

                _activeDispatchOperation = App.Current.Dispatcher.BeginInvoke(UpdateCachedEndpointsAction);
                _activeDispatchAction = DispatchAction.UpdateCachedEndpoints;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method is called by the dispatcher on the UI thread to update the system default audio devices.
        /// </summary>
        private static void UpdateDefaultDevicesAction()
        {
            App.UserSettings.UpdateDefaultDevices();
            UpdatedDefaultDevicesEvent?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// This method is called by the dispatcher on the UI thread to update the cached audio endpoints.
        /// </summary>
        private static void UpdateCachedEndpointsAction()
        {
            App.AudioEndpointManager.UpdateCachedEndpoints();
            DispatchUpdateDefaultDevices();
            UpdatedCachedEndpointsEvent?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Registers event handlers for audio device events.
        /// </summary>
        private static void RegisterAudioDeviceEvents() => App.CoreAudioController.AudioDeviceChanged.Subscribe(OnDeviceChanged);

        /// <summary>
        /// This method is called by a CoreAudioApi thread when an audio device event is raised.
        /// </summary>
        private static void OnDeviceChanged(DeviceChangedArgs args)
        {
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

        /// <summary>
        /// Registers event handlers for hardware change events.
        /// </summary>
        private static void RegisterHardwareEvents()
        {
            // TODO: Set up WMI event watchers and associate with handler methods
        }

        /// <summary>
        /// Registers event handlers for application launch/close events.
        /// </summary>
        private static void RegisterApplicationEvents()
        {
            // TODO: Set up application launch/close event handling
        }
        #endregion

        #region Internal Types
        /// <summary>
        /// The types of operations that can be dispatched to the UI thread.
        /// </summary>
        private enum DispatchAction
        {
            UpdateDefaultDevices,
            UpdateCachedEndpoints,
        }
        #endregion
    }
}