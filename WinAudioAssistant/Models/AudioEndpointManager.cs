using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Maintains a list of active audio endpoints on the system.
    /// </summary>
    public class AudioEndpointManager
    {
        private ObservableCollection<AudioEndpointInfo> _cachedEndpoints { get; } = new();
        public ReadOnlyObservableCollection<AudioEndpointInfo> CachedEndpoints => new(_cachedEndpoints);

        public AudioEndpointManager()
        {
            UpdateCachedEndpoints();
        }

        public void UpdateCachedEndpoints()
        {
            _cachedEndpoints.Clear();
            var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All))
            {
                _cachedEndpoints.Add(new AudioEndpointInfo(device));
            }
        }

        /// <summary>
        /// Dump a list of all system audio endpoints to a file.
        /// </summary>
        public static void ListAllEndpoints()
        {
            using StreamWriter writer = new("endpoints.txt", append: false);
            var enumerator = new MMDeviceEnumerator();
            foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.All))
            {
                writer.WriteLine("");
                writer.WriteLine("AudioEndpoint:");
                writer.WriteLine($"    ID={device.ID}");
                writer.WriteLine($"    DataFlow={device.DataFlow}");
                writer.WriteLine($"    State={device.State}");
                writer.WriteLine($"    FriendlyName={device.FriendlyName}");
                writer.WriteLine($"    DeviceFriendlyName={device.DeviceFriendlyName}");
                writer.WriteLine($"    InstanceID={device.InstanceId}");
                writer.WriteLine($"    DeviceTopology.DeviceId={device.DeviceTopology.DeviceId}");
                writer.WriteLine($"    DeviceTopology.ConnectorCount={device.DeviceTopology.ConnectorCount}");
                writer.WriteLine("    Properties:");
                for (int i = 0; i < device.Properties.Count; i++)
                {
                    var formatId = device.Properties[i].Key.formatId;
                    var propertyId = device.Properties[i].Key.propertyId;
                    string valueType;
                    string? value;
                    var propertyKeyDef = PropertyKeyDefLookup.Lookup(formatId, propertyId);

                    try
                    {
                        valueType = device.Properties[i].Value.GetType().ToString();
                        value = device.Properties[i].Value.ToString();
                    }
                    catch (NotImplementedException)
                    {
                        // Skip this value as its value type is not supported
                        valueType = "Unsupported";
                        value = null;
                    }
                    if (propertyKeyDef != null)
                        writer.WriteLine($"        PropertyName={propertyKeyDef?.Name}, ValueType={valueType}" + (value != null ? $", Value={value}" : ""));
                    else
                        writer.WriteLine($"        FormatId={formatId}, PropertyId={propertyId}, ValueType={valueType}" + (value != null ? $", Value={value}" : ""));
                }
            }
        }
    }
}
