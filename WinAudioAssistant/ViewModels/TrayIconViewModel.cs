using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinAudioAssistant.Views;

namespace WinAudioAssistant.ViewModels
{
    class TrayIconViewModel : ViewModel
    {
        public RelayCommand DoubleClickCommand { get; }
        public RelayCommand SettingsCommand { get; }
        public RelayCommand ExitCommand { get; }

        public TrayIconViewModel()
        {
            DoubleClickCommand = new RelayCommand(Settings);
            SettingsCommand = new RelayCommand(Settings);
            ExitCommand = new RelayCommand(Exit);
        }

        private void Settings(object? parameter)
        {
            if (App.DevicePriorityViewModel is not null)
                App.DevicePriorityViewModel.FocusViewAction();
            else
                new DevicePriorityView().Show();
        }

        private void Exit(object? parameter)
        {
            App.Current.Shutdown();
        }

    }
}
