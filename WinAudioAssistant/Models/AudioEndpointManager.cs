using System.Collections.ObjectModel;
using System.IO;
using AudioSwitcher.AudioApi;

namespace WinAudioAssistant.Models
{
    /// <summary>
    /// Maintains a cache of audio endpoints (AudioEndpointInfo) that are registered in the system, which inclues their state.
    /// </summary>
    public class AudioEndpointManager
    {
        private readonly ObservableCollection<AudioEndpointInfo> _cachedEndpoints = new(); // Private list of cached endpoints
        public ReadOnlyObservableCollection<AudioEndpointInfo> CachedEndpoints => new(_cachedEndpoints); // Public read-only list of cached endpoints

        /// <summary>
        /// Refreshes the cache and updates the states of all audio endpoints.
        /// Should only be called by SystemEventsHandler.
        /// </summary>
        public void RefreshCachedEndpoints()
        {
            _cachedEndpoints.Clear();
            foreach (var device in App.CoreAudioController.GetDevices(DeviceState.All))
            {
                _cachedEndpoints.Add(new AudioEndpointInfo(device));
            }
        }

        /// <summary>
        /// Dump a list of all system audio endpoints to a file.
        /// Currently unused.
        /// </summary>
        public static void ListAllEndpoints()
        {
            using StreamWriter writer = new("endpoints.txt", append: false);
            foreach (var device in App.CoreAudioController.GetDevices(DeviceState.All))
            {
                writer.WriteLine("");
                writer.WriteLine("AudioEndpoint:");
                writer.WriteLine($"    DeviceType={device.DeviceType}");
                writer.WriteLine($"    FullName={device.FullName}");
                writer.WriteLine($"    GetHashCode={device.GetHashCode()}");
                writer.WriteLine($"    Icon={device.Icon}");
                writer.WriteLine($"    IconPath={device.IconPath}");
                writer.WriteLine($"    Id={device.Id}");
                writer.WriteLine($"    InterfaceName={device.InterfaceName}");
                writer.WriteLine($"    IsDefaultDevice={device.IsDefaultDevice}");
                writer.WriteLine($"    IsDefaultCommunicationsDevice={device.IsDefaultCommunicationsDevice}");
                writer.WriteLine($"    Name={device.Name}");
                writer.WriteLine($"    RealId={device.RealId}");
                writer.WriteLine($"    State={device.State}");

                writer.WriteLine("    Properties:");
                foreach (var property in device.Properties)
                {
                    var formatId = property.Key.FormatId;
                    var propertyId = property.Key.PropertyId;
                    string valueType;
                    string? value;
                    var propertyKeyDef = PropertyKeyDefLookup.Lookup(formatId, propertyId);

                    try
                    {
                        valueType = property.Value.GetType().ToString();
                        value = property.Value.ToString();
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
