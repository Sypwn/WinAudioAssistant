using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Imaging;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Newtonsoft.Json;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Contains a selection of properties for an audio endpoint, and methods to collect and update them.
    /// </summary>
    public struct AudioEndpointInfo
    {
        #region Constructors
        /// <summary>
        /// Deserialization constructor.
        /// Use named arguments for optional parameters. The order of arguments may change in the future.
        /// At minimum, an AudioEndpointInfo must have a DataFlow and AudioEndpoint_GUID. The rest may be populated later using UpdateFromSystem().
        /// </summary>
        [JsonConstructor]
        public AudioEndpointInfo(DeviceType dataFlow,
                                 Guid audioEndpoint_GUID,
                                 DeviceState? deviceState = null,
                                 FormFactorType? audioEndpoint_FormFactor = null,
                                 Guid? audioEndpoint_JackSubType = null,
                                 Guid? device_ContainerId = null,
                                 string? device_DeviceDesc = null,
                                 string? deviceClass_IconPath = null,
                                 string? deviceInterface_FriendlyName = null,
                                 string? hostDeviceDesc = null)
        {
            Trace.Assert(dataFlow != DeviceType.All, "AudioEndpointInfo created with DeviceType.All");
            DataFlow = dataFlow;
            AudioEndpoint_GUID = audioEndpoint_GUID;
            DeviceState = deviceState;
            AudioEndpoint_FormFactor = audioEndpoint_FormFactor;
            AudioEndpoint_JackSubType = audioEndpoint_JackSubType;
            Device_ContainerId = device_ContainerId;
            Device_DeviceDesc = device_DeviceDesc;
            DeviceClass_IconPath = deviceClass_IconPath;
            DeviceInterface_FriendlyName = deviceInterface_FriendlyName;
            HostDeviceDesc = hostDeviceDesc;
        }

        /// <summary>
        /// Collects the properties of a CoreAudioDevice to create an AudioEndpointInfo.
        /// </summary>
        /// <param name="device">CoreAudioDevice whose properties should be collected.</param>
        public AudioEndpointInfo(CoreAudioDevice device)
        {
            if (device.DeviceType == DeviceType.All)
                throw new ArgumentException("Invalid device type", nameof(device));
            if (device.Id == Guid.Empty)
                throw new ArgumentException("Invalid device GUID", nameof(device));
            DataFlow = device.DeviceType;
            AudioEndpoint_GUID = device.Id;
            UpdateFromDevice(device);
        }
        #endregion

        #region Endpoint Properties
        // These fields describe the endpoint. Most are populated from the device's (usually static) properties, DeviceState being the exception.
        public readonly DeviceType DataFlow { get; } // Capture (input) or playback (output)
        public readonly Guid AudioEndpoint_GUID { get; } // Globally unique to this endpoint, created by Windows when it is first connected
        [JsonIgnore]
        public DeviceState? DeviceState { get; private set;} // Active, unplugged, disabled, not present
        public FormFactorType? AudioEndpoint_FormFactor { get; private set; } // Speakers, headphones, headset, SPDIF, etc.
        public Guid? AudioEndpoint_JackSubType { get; private set; } // Contains a GUID for a type of jack, more specific than FormFactor
        public Guid? Device_ContainerId { get; private set; } // Perhaps points to the parent device? Not populated for virtual devices
        public string? Device_DeviceDesc { get; private set; } // The endpoint's name, which can be changed in the control panel
        public string? DeviceClass_IconPath { get; private set; } // Path to the icon for the endpoint's device class. Usually includes a resource identifier
        public string? DeviceInterface_FriendlyName { get; private set; } // Set by the driver, but may have a different value if there are duplicate devices
        public string? HostDeviceDesc { get; private set; } // (Actual property name unkown) Appears to be the name of the host device. Usually same as DeviceInterface_FriendlyName, but more consistent.
                                                            // REMINDER: Add new properties to deserialization constructor, and UpdateFromDevice(), and ManagedDevice.IdentificationFlags, and...
        #endregion

        #region Other Properties
        //[JsonIgnore]
        //public readonly string RealId => (DataFlow == DeviceType.Playback ? "{0.0.0.00000000}.{" : "{0.0.1.00000000}.{") + AudioEndpoint_GUID.ToString() + "}"; // Was used by previous API
        [JsonIgnore]
        public readonly Icon Icon => App.IconManager.GetIconFromIconPath(DeviceClass_IconPath);
        [JsonIgnore]
        public readonly BitmapSource IconBitmap => App.IconManager.GetBitmapFromIconPath(DeviceClass_IconPath);
        #endregion

        #region Public Methods
        /// <summary>
        /// Populate or refresh the properties of this AudioEndpointInfo from the system.
        /// </summary>
        /// <returns>True if a matching endpoint was found in the system.</returns>
        public bool UpdateFromSystem()
        {
            var device = App.CoreAudioController.GetDevice(AudioEndpoint_GUID);
            if (device?.Properties[PropertyKeys.AudioEndpoint_GUID] is Guid guid && guid == AudioEndpoint_GUID)
            {
                UpdateFromDevice(device);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Populate or refresh the properties of this AudioEndpointInfo from a CoreAudioDevice.
        /// </summary>
        /// <param name="device">CoreAudioDevice whose properties should be collected. The DataFlow and Endpoint GUID must match the existing info.</param>
        public void UpdateFromDevice(CoreAudioDevice device)
        {
            if (device.DeviceType != DataFlow)
                throw new ArgumentException("Device type mismatch", nameof(device));
            if (device.Id != AudioEndpoint_GUID)
                throw new ArgumentException("Device GUID mismatch", nameof(device));
            DeviceState = device.State;
            if (device.Properties[PropertyKeys.AudioEndpoint_FormFactor] is uint formFactor)
                AudioEndpoint_FormFactor = (FormFactorType)formFactor;
            if (Guid.TryParse(device.Properties[PropertyKeys.AudioEndpoint_JackSubType] as string, out var jackSubType)) // CoreAudioApi outputs this GUID as a string for some reason
                AudioEndpoint_JackSubType = jackSubType;
            if (device.Properties[PropertyKeys.Device_ContainerId] is Guid containerId) //&&
                //containerId != new Guid(0x0, 0x0, 0x0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff)) 
                // Devices that don't have a valid container (mainly virtual devices) have {00000000-0000-0000-FFFF-FFFFFFFFFFFF} instead of an empty GUID.
                // Previously I treated this as a null GUID, but I'll allow it to be stored as-is for now.
                Device_ContainerId = containerId;
            if (device.Properties[PropertyKeys.Device_DeviceDesc] is string deviceDesc)
                Device_DeviceDesc = deviceDesc;
            if (device.Properties[PropertyKeys.DeviceClass_IconPath] is string iconPath)
                DeviceClass_IconPath = iconPath;
            if (device.Properties[PropertyKeys.DeviceInterface_FriendlyName] is string friendlyName)
                DeviceInterface_FriendlyName = friendlyName;
            if (device.Properties[PropertyKeys.HostDeviceDesc] is string hostDeviceDesc)
                HostDeviceDesc = hostDeviceDesc;
        }
        #endregion

        #region Internal Types
        /// <summary>
        /// Endpoint form factors as defined in MMDEVICEAPI.H in Windows SDK
        /// </summary>
        public enum FormFactorType
        {
            RemoteNetworkDevice = 0,
            Speakers = 1,
            LineLevel = 2,
            Headphones = 3,
            Microphone = 4,
            Headset = 5,
            Handset = 6,
            UnknownDigitalPassthrough = 7,
            SPDIF = 8,
            DigitalAudioDisplayDevice = 9,
            UnknownFormFactor = 10,
        }

        /// <summary>
        /// Contains the Windows property keys for each property in AudioEndpointInfo.
        /// </summary>
        private static class PropertyKeys
        {
            public static readonly PropertyKey AudioEndpoint_GUID = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 4);
            public static readonly PropertyKey AudioEndpoint_FormFactor = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 0);
            public static readonly PropertyKey AudioEndpoint_JackSubType = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 8);
            public static readonly PropertyKey Device_ContainerId = new(new Guid(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c), 2);
            public static readonly PropertyKey Device_DeviceDesc = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 2);
            public static readonly PropertyKey DeviceClass_IconPath = new(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 12);
            public static readonly PropertyKey DeviceInterface_FriendlyName = new(new Guid(0x026e516e, 0xb814, 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 2);
            public static readonly PropertyKey HostDeviceDesc = new(new Guid(0xb3f8fa53, 0x0004, 0x438e, 0x90, 0x03, 0x51, 0xa4, 0x6e, 0x13, 0x9b, 0xfc), 6);
        }
        #endregion
    }
}
