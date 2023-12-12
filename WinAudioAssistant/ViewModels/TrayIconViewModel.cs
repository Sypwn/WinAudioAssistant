using WinAudioAssistant.Views;

namespace WinAudioAssistant.ViewModels
{
    /// <summary>
    /// View model for the tray icon.
    /// Not a normal window, so it doesn't inherit from BaseViewModel.
    /// </summary>
    class TrayIconViewModel
    {
        /// <summary>
        /// Initializes the tray icon view model.
        /// </summary>
        public TrayIconViewModel()
        {
            DoubleClickCommand = new RelayCommand(Settings);
            SettingsCommand = new RelayCommand(Settings);
            ExitCommand = new RelayCommand(Exit);
        }

        public RelayCommand DoubleClickCommand { get; } // Bound to double clicking the tray icon
        public RelayCommand SettingsCommand { get; } // Bound to the settings option in the tray icon context menu
        public RelayCommand ExitCommand { get; } // Bound to the exit option in the tray icon context menu

        /// <summary>
        /// Opens or focuses the settings window.
        /// </summary>
        private void Settings(object? parameter)
        {
            if (App.DevicePriorityViewModel is not null)
                App.DevicePriorityViewModel.FocusViewAction();
            else
                new DevicePriorityView().Show();
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void Exit(object? parameter)
        {
            App.Current.Shutdown();
        }
    }
}
