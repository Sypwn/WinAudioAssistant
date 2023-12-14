using System.Diagnostics;
﻿using System.Collections.ObjectModel;
using AudioSwitcher.AudioApi;
using Newtonsoft.Json;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Base class for a managed audio device. These are what the user configures in the UI.
    /// It is effectively a wrapper for an AudioEndpointInfo, but with configurable properties and methods to locate a fitting active endpoint,
    /// even one whose GUID does not match exactly.
    /// </summary>
    public abstract class ManagedDevice
    {
        #region Constructors
        /// <summary>
        /// Creates a new ManagedDevice with the given AudioEndpointInfo, while first verifying that the DataFlow matches.
        /// </summary>
        /// <param name="endpointInfo"></param>
        public ManagedDevice(AudioEndpointInfo endpointInfo)
        {
            Trace.Assert(endpointInfo.DataFlow == DataFlow(), "ManagedDevice created with mismatched DataFlow");
            EndpointInfo = endpointInfo;
            Conditions = new ReadOnlyCollection<ActivationCondition>(_conditions);
        }
        #endregion

        #region Private Fields
        [JsonProperty(PropertyName = "Conditions")]
        private readonly List<ActivationCondition> _conditions = new(); // Conditions that must be met for this managed device to be active
        #endregion

        #region Properties
        public AudioEndpointInfo EndpointInfo { get; protected set; } // Contains the known properties of the intended endpoint, used to compare against active endpoints
        public string Name { get; set; } = ""; // User configured name for this managed device
        public bool Enabled { get; set; } = true; // Allows the user to disable this managed device without deleting it
        public IdentificationMethods IdentificationMethod { get; set; } = IdentificationMethods.Loose; // General setting for how the managed device is matched to an active endpoint
        public IdentificationFlags CustomIdentificationFlags { get; set; } = IdentificationFlags.Loose; // When IdentificationMethod is set to Custom, these flags determine which properties are checked
        [JsonIgnore]
        public ReadOnlyCollection<ActivationCondition> Conditions { get; } // Public access to the list of conditions
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the DataFlow/DeviceType of this managed device, as determined by the derived class.
        /// </summary>
        /// <returns>DeviceType.Capture or DeviceType.Playback</returns>
        public abstract DeviceType DataFlow();

        /// <summary>
        /// Updates the EndpointInfo property with the given AudioEndpointInfo, while first verifying that the DataFlow matches.
        /// </summary>
        /// <param name="endpointInfo">New AudioEndpointInfo for this managed device.</param>
        public void SetEndpoint(AudioEndpointInfo endpointInfo)
        {
            if (endpointInfo.DataFlow != DataFlow())
                throw new ArgumentException("ManagedDevice endpoint set to mismatched DataFlow", nameof(endpointInfo));
            EndpointInfo = endpointInfo;
        }

        public void AddCondition(ActivationCondition condition) => _conditions.Add(condition);

        public void RemoveCondition(ActivationCondition condition) => _conditions.Remove(condition);

        /// <summary>
        /// The device checks if it should be active, and if there is a matching endpoint in the cache.
        /// </summary>
        /// <returns>AudioEndpointInfo for a matching active endpoint if it finds one, otherwise null.</returns>
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
        #endregion

        #region Internal Types
        /// <summary>
        /// Generic configurable techniques for matching an endpoint to this managed device.
        /// </summary>
        public enum IdentificationMethods
        {
            Strict,
            Loose,
            Custom,
        }

        /// <summary>
        /// Bit flags used to determine which AudioEndpointInfo properties to use when matching an endpoint to this managed device.
        /// The "Strict" and "Loose" flags determine which properties are checked for the respective IdentificationMethods.
        /// </summary>
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

        /// <summary>
        /// Types of conditions that can be used to determine if this managed device should be active.
        /// </summary>
        public enum ActivationConditionType
        {
            Application, // An instance of the application at ApplicationPath must be running
            Hardware, // A hardware device must be connected
        }

        /// <summary>
        /// A condition that must be met for this managed device to be active.
        /// Each condition will register itself with SystemEventsHandler, which will keep the State property updated.
        /// </summary>
        public struct ActivationCondition
        {
            public ActivationCondition(ActivationConditionType type, string applicationPath)
            {
                Type = type;
                switch (type)
                {
                    case ActivationConditionType.Application:
                        ApplicationPath = applicationPath;
                        break;
                    case ActivationConditionType.Hardware:
                        ApplicationPath = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
                SystemEventsHandler.RegisterCondition(this);
            }

            [JsonIgnore]
            public object Lock { get; } = new();
            public ActivationConditionType Type { get; }
            [JsonIgnore]
            public bool State { get; set; }
            public string? ApplicationPath { get; }
        }
        #endregion
    }

    /// <summary>
    /// Managed device that is explicitly a capture/input device.
    /// Derived classes are used to better ensure that capture and playback devices are never internally mixed up.
    /// </summary>
    public class ManagedInputDevice(AudioEndpointInfo endpointInfo) : ManagedDevice(endpointInfo)
    {
        public override DeviceType DataFlow() => DeviceType.Capture;
    }

    /// <summary>
    /// Managed device that is explicitly a playback/output device.
    /// Derived classes are used to better ensure that capture and playback devices are never internally mixed up.
    /// </summary>
    public class ManagedOutputDevice(AudioEndpointInfo endpointInfo) : ManagedDevice(endpointInfo)
    {
        /// <inheritdoc/>
        public override DeviceType DataFlow() => DeviceType.Playback;
    }
}
