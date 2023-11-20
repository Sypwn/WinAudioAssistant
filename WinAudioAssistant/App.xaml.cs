using System.Configuration;
using System.Data;
using System.Windows;
using WinAudioAssistant.Models;

namespace WinAudioAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static UserSettings UserSettings { get; private set; } = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            UserSettings.DeviceManager.AddDevice(new OutputDevice("Headphones"));
            UserSettings.DeviceManager.AddDevice(new OutputDevice("Speakers"));
            UserSettings.DeviceManager.AddDevice(new InputDevice("Microphone"));
            UserSettings.DeviceManager.AddDevice(new InputDevice("Webcam"));
            UserSettings.DeviceManager.AddDevice(new InputDevice("Line In"));
        }
    }

}
