using System.Configuration;
using System.Data;
using System.Windows;
using WinAudioAssistant.Models;
using AudioSwitcher.AudioApi.CoreAudio;

namespace WinAudioAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static CoreAudioController CoreAudioController { get; private set; } = new();
        public static UserSettings UserSettings { get; private set; } = new();
        public static AudioEndpointManager AudioEndpointManager { get; private set; } = new();  
        public static SystemEventsHandler SystemEventsHandler { get; private set; } = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }

}
