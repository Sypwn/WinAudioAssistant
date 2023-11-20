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
        }
    }

}
