using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;

namespace WinAudioAssistant.Models
{
    public abstract class ManagedDevice
    {
        public enum IdentificationMethods
        {
            Strict,
            Loose,
            Custom,
        }

        [Flags]
        public enum IdentificationFlags
        {
            AudioEndpoint_GUID = 1 << 0,
            AudioEndpoint_FormFactor = 1 << 1,
            AudioEndpoint_JackSubType = 1 << 2,
            Device_ContainerId = 1 << 3,
            Device_DeviceDesc = 1 << 4,
            DeviceClass_IconPath = 1 << 5,
            DeviceInterface_FriendlyName = 1 << 6,
            HostDeviceDesc = 1 << 7,

            None = 0,
            Strict = AudioEndpoint_GUID,
            Loose = HostDeviceDesc | AudioEndpoint_FormFactor,
        }

        public string Name { get; set; } = "";
        public AudioEndpointInfo EndpointInfo { get; protected set; }
        public bool Enabled = true;
        public IdentificationMethods IdentificationMethod { get; set; } = IdentificationMethods.Loose;
        public IdentificationFlags CustomIdentificationFlags { get; set; } = IdentificationFlags.Loose;

        public abstract DeviceType DataFlow();
        public abstract void SetEndpoint(AudioEndpointInfo endpointInfo);

        /// <summary>
        /// The device checks if it should be active, and if there is a matching endpoint in the cache.
        /// </summary>
        /// <returns>Matching endpointInfo if it should be active, null if not.</returns>
        public AudioEndpointInfo? CheckShouldBeActive()
        {
            if (!Enabled) return null;
            // TODO: Additional activation conditions

            foreach (var endpoint in App.AudioEndpointManager.CachedEndpoints)
            {
                if (endpoint.DeviceState != DeviceState.Active) continue;
                if (endpoint.DataFlow != DataFlow()) continue;

                var identFlags = IdentificationMethod switch
                {
                    IdentificationMethods.Strict => IdentificationFlags.Strict,
                    IdentificationMethods.Loose => IdentificationFlags.Loose,
                    IdentificationMethods.Custom => CustomIdentificationFlags,
                    _ => throw new ArgumentOutOfRangeException(nameof(IdentificationMethod), IdentificationMethod, null)
                };

                // For each flag, check if the flag is set and if the property matches
                if ((identFlags & IdentificationFlags.AudioEndpoint_GUID) != 0 && endpoint.AudioEndpoint_GUID != EndpointInfo.AudioEndpoint_GUID) continue;
                if ((identFlags & IdentificationFlags.AudioEndpoint_FormFactor) != 0 && endpoint.AudioEndpoint_FormFactor != EndpointInfo.AudioEndpoint_FormFactor) continue;
                if ((identFlags & IdentificationFlags.AudioEndpoint_JackSubType) != 0 && endpoint.AudioEndpoint_JackSubType != EndpointInfo.AudioEndpoint_JackSubType) continue;
                if ((identFlags & IdentificationFlags.Device_ContainerId) != 0 && endpoint.Device_ContainerId != EndpointInfo.Device_ContainerId) continue;
                if ((identFlags & IdentificationFlags.Device_DeviceDesc) != 0 && endpoint.Device_DeviceDesc != EndpointInfo.Device_DeviceDesc) continue;
                if ((identFlags & IdentificationFlags.DeviceClass_IconPath) != 0 && endpoint.DeviceClass_IconPath != EndpointInfo.DeviceClass_IconPath) continue;
                if ((identFlags & IdentificationFlags.DeviceInterface_FriendlyName) != 0 && endpoint.DeviceInterface_FriendlyName != EndpointInfo.DeviceInterface_FriendlyName) continue;
                if ((identFlags & IdentificationFlags.HostDeviceDesc) != 0 && endpoint.HostDeviceDesc != EndpointInfo.HostDeviceDesc) continue;
                return endpoint;
            }

            return null;
        }
    }

    public class ManagedInputDevice : ManagedDevice
    {
        public ManagedInputDevice(AudioEndpointInfo endpointInfo)
        {
            Trace.Assert(endpointInfo.DataFlow == DataFlow(), "ManagedInputDevice created with mismatched DataFlow");
            EndpointInfo = endpointInfo;
        }

        public override DeviceType DataFlow() => DeviceType.Capture;

        public override void SetEndpoint(AudioEndpointInfo endpointInfo)
        {
            Trace.Assert(endpointInfo.DataFlow == DataFlow(), "ManagedInputDevice endpoint set to mismatched DataFlow");
            EndpointInfo = endpointInfo;
        }
    }

    public class ManagedOutputDevice : ManagedDevice
    {
        public ManagedOutputDevice(AudioEndpointInfo endpointInfo)
        {
            Trace.Assert(endpointInfo.DataFlow == DataFlow(), "ManagedInputDevice created with mismatched DataFlow");
            EndpointInfo = endpointInfo;
        }

        public override DeviceType DataFlow() => DeviceType.Playback;

        public override void SetEndpoint(AudioEndpointInfo endpointInfo)
        {
            Trace.Assert(endpointInfo.DataFlow == DataFlow(), "ManagedInputDevice endpoint set to mismatched DataFlow");
            EndpointInfo = endpointInfo;
        }
    }
}
