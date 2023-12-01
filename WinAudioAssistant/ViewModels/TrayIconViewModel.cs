using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinAudioAssistant.Views;

namespace WinAudioAssistant.ViewModels
{
    class TrayIconViewModel
    {

        public ICommand DoubleClickCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand ExitCommand { get; }

        public TrayIconViewModel()
        {
            DoubleClickCommand = new RelayCommand(Settings);
            SettingsCommand = new RelayCommand(Settings);
            ExitCommand = new RelayCommand(Exit);
        }

        private void Settings(object? parameter)
        {
            foreach (var window in App.Current.Windows)
            {
                if (window is DevicePriorityView devicePriorityView)
                {
                    devicePriorityView.Focus();
                    return;
                }
            }
            new DevicePriorityView().Show();
        }

        private void Exit(object? parameter)
        {
            App.Current.Shutdown();
        }

    }
}
