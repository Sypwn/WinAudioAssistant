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
        public string Name { get; set; } = "";
        public AudioEndpointInfo EndpointInfo { get; protected set; }

        public abstract DeviceType DataFlow();
        public abstract void SetEndpoint(AudioEndpointInfo endpointInfo);

        /// <summary>
        /// The device checks if it should be active, and if there is a matching endpoint in the cache.
        /// </summary>
        /// <returns>Matching endpointInfo if it should be active, null if not.</returns>
        public AudioEndpointInfo? CheckShouldBeActive()
        {
            // TODO: Activation conditions

            foreach (var endpoint in App.AudioEndpointManager.CachedEndpoints)
            {
                if (endpoint.DeviceState != DeviceState.Active) continue;

                // TODO: Additional identification methods
                if (endpoint.Guid == EndpointInfo.Guid)
                {
                    return endpoint;
                }
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
