using System.Configuration;
using System.Data;
using System.Windows;
using WinAudioAssistant.Models;
using AudioSwitcher.AudioApi.CoreAudio;
using WinAudioAssistant.Views;

namespace WinAudioAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly CoreAudioController CoreAudioController = new();
        public static readonly IconManager IconManager = new();
        public static readonly AudioEndpointManager AudioEndpointManager = new();
        public static readonly UserSettings UserSettings = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UserSettings.Startup();
            SystemEventsHandler.RegisterAllEvents();
            new TrayIconView();
        }
    }

}
