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
        public static IconManager IconManager { get; private set; } = new();
        public static AudioEndpointManager AudioEndpointManager { get; private set; } = new();  
        public static SystemEventsHandler SystemEventsHandler { get; private set; } = new();
#if DEBUG
#pragma warning disable CS8625
        // Give me NREs if I try to use this before it's properly loaded
        public static UserSettings UserSettings { get; private set; } = null;
#pragma warning restore CS8625
#else
        public static UserSettings UserSettings { get; private set; } = new();
#endif

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UserSettings = UserSettings.LoadAtStartup();

            SystemEventsHandler.RegisterAllEvents();
            AudioEndpointManager.UpdateCachedEndpoints();
        }
    }

}
