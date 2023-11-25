using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.Models
{
    public enum EndpointFormFactor
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

    public struct AudioEndpointInfo
    {
        public readonly DataFlow DataFlow { get; }
        public readonly Guid AudioEndpoint_GUID { get; } // Globally unique to this endpoint
        public DeviceState? DeviceState { get; private set;}
        public EndpointFormFactor? AudioEndpoint_FormFactor { get; private set; } // Speakers, headphones, headset, SPDIF, etc.
        public Guid? AudioEndpoint_JackSubType { get; private set; } // Contains a GUID for a type of jack, more specific than FormFactor
        public Guid? Device_ContainerId { get; private set; } // Perhaps points to the parent device? Not populated for virtual devices
        public string? Device_DeviceDesc { get; private set; } // The endpoint's name, which can be changed in the control panel
        public string? DeviceClass_IconPath { get; private set; }
        public string? DeviceInterface_FriendlyName { get; private set; } // Set by the driver, but may have a different value if there are duplicate devices
        public string? HostDeviceDesc { get; private set; } // (Actual property name unkown) Appears to be the name of the host device. Usually same as DeviceInterface_FriendlyName.

        public readonly string ID => (DataFlow == DataFlow.Render ? "{0.0.0.00000000}.{" : "{0.0.1.00000000}.{") + AudioEndpoint_GUID.ToString() + "}";

        /// <summary>
        /// Use named arguments for optional parameters. The order of arguments may change in the future.
        /// </summary>
        /// <remarks>Test remark.</remarks>
        public AudioEndpointInfo(DataFlow dataFlow,
                                 Guid audioEndpoint_GUID,
                                 EndpointFormFactor? audioEndpoint_FormFactor = null,
                                 Guid? audioEndpoint_JackSubType = null,
                                 Guid? device_ContainerId = null,
                                 string? device_DeviceDesc = null,
                                 string? deviceClass_IconPath = null,
                                 string? deviceInterface_FriendlyName = null,
                                 string? hostDeviceDesc = null)
        {
            Trace.Assert(dataFlow != DataFlow.All, "AudioEndpointInfo created with DataFlow.All");
            DataFlow = dataFlow;
            AudioEndpoint_GUID = audioEndpoint_GUID;
            AudioEndpoint_FormFactor = audioEndpoint_FormFactor;
            AudioEndpoint_JackSubType = audioEndpoint_JackSubType;
            Device_ContainerId = device_ContainerId;
            Device_DeviceDesc = device_DeviceDesc;
            DeviceClass_IconPath = deviceClass_IconPath;
            DeviceInterface_FriendlyName = deviceInterface_FriendlyName;
            HostDeviceDesc = hostDeviceDesc;
        }
        public AudioEndpointInfo(MMDevice device)
        {
            Trace.Assert(device.DataFlow != DataFlow.All, "AudioEndpointInfo created with DataFlow.All");
            DataFlow = device.DataFlow;
            if (Guid.TryParse(device.Properties[propertyKeys.AudioEndpoint_GUID]?.Value as string, out var guid))
            {
                AudioEndpoint_GUID = guid;
            }
            else
            {
                AudioEndpoint_GUID = Guid.Empty;
            }
            Debug.Assert(AudioEndpoint_GUID != Guid.Empty, "AudioEndpointInfo created with empty GUID");
            UpdateFromDevice(device);
        }

        /// <summary>
        /// Populate or refresh the properties of this AudioEndpointInfo from the system.
        /// </summary>
        /// <returns>True if a matching endpoint was found in the system.</returns>
        public bool UpdateFromSystem()
        {
            var device = new MMDeviceEnumerator().GetDevice(ID);
            if (device?.Properties[propertyKeys.AudioEndpoint_GUID]?.Value is Guid guid && guid == AudioEndpoint_GUID)
            {
                UpdateFromDevice(device);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateFromDevice(MMDevice device)
        {
            DeviceState = device.State;
            if (device.Properties[propertyKeys.AudioEndpoint_FormFactor]?.Value is uint formFactor)
                AudioEndpoint_FormFactor = (EndpointFormFactor)formFactor;
            if (Guid.TryParse(device.Properties[propertyKeys.AudioEndpoint_JackSubType]?.Value as string, out var jackSubType))
                AudioEndpoint_JackSubType = jackSubType;
            if (device.Properties[propertyKeys.Device_ContainerId]?.Value is Guid containerId &&
                containerId != new Guid(0x0, 0x0, 0x0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff))
                Device_ContainerId = containerId;
            if (device.Properties[propertyKeys.Device_DeviceDesc]?.Value is string deviceDesc)
                Device_DeviceDesc = deviceDesc;
            if (device.Properties[propertyKeys.DeviceClass_IconPath]?.Value is string iconPath)
                DeviceClass_IconPath = iconPath;
            if (device.Properties[propertyKeys.DeviceInterface_FriendlyName]?.Value is string friendlyName)
                DeviceInterface_FriendlyName = friendlyName;
            if (device.Properties[propertyKeys.Device_DeviceDesc]?.Value is string hostDeviceDesc)
                HostDeviceDesc = hostDeviceDesc;
        }

        private static class propertyKeys
        {
            internal static readonly PropertyKey AudioEndpoint_GUID = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 4);
            internal static readonly PropertyKey AudioEndpoint_FormFactor = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 0);
            internal static readonly PropertyKey AudioEndpoint_JackSubType = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 8);
            internal static readonly PropertyKey Device_ContainerId = new(new Guid(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c), 2);
            internal static readonly PropertyKey Device_DeviceDesc = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 2);
            internal static readonly PropertyKey DeviceClass_IconPath = new(new Guid(0x259abffc, 0x50a7, 0x47ce, 0xaf, 0x8, 0x68, 0xc9, 0xa7, 0xd7, 0x33, 0x66), 12);
            internal static readonly PropertyKey DeviceInterface_FriendlyName = new(new Guid(0x026e516e, 0xb814, 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22), 2);
            internal static readonly PropertyKey HostDeviceDesc = new(new Guid(0xb3f8fa53, 0x0004, 0x438e, 0x90, 0x03, 0x51, 0xa4, 0x6e, 0x13, 0x9b, 0xfc), 6);
        }

    }


}
