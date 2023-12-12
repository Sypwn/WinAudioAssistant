using System.Windows;
using WinAudioAssistant.Models;
using AudioSwitcher.AudioApi.CoreAudio;
using WinAudioAssistant.Views;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Static model instances
        public static readonly CoreAudioController CoreAudioController = new();
        public static readonly IconManager IconManager = new();
        public static readonly AudioEndpointManager AudioEndpointManager = new();
        public static readonly UserSettings UserSettings = new();

        public static DevicePriorityViewModel? DevicePriorityViewModel { get; set; } // Reference to the DevicePriorityViewModel instance, if one is open.

        /// <summary>
        /// Application entry point.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UserSettings.Startup();
            SystemEventsHandler.RegisterAllEvents();
            _ = new TrayIconView();
        }
    }
}
