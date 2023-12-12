using System.Diagnostics;
using System.Windows;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant.Views
{
    /// <summary>
    /// Base class window-style views.
    /// </summary>
    public abstract class BaseView : Window
    {
        /// <summary>
        /// Initializes the base view, and sets up the BaseViewModel.
        /// </summary>
        public BaseView()
        {
            // The ViewModel doesn't exist until InitializeComponent() is called. But that method is only generated for the derived class.
            // So we use InitializeViewComponent() to call it from here.
            InitializeViewComponent();
            Debug.Assert(DataContext is BaseViewModel);
            if (DataContext is BaseViewModel viewModel)
            {
                viewModel.CloseViewAction = Close;
                viewModel.FocusViewAction = () => Focus(); // Focus returns bool, so we wrap it in a lambda
                this.Unloaded += (_, _) => viewModel.Cleanup();
            }
        }

        public abstract void InitializeViewComponent(); // The dervived class must direct this to InitializeComponent()

        /// <summary>
        /// Event handler for the window closing event.
        /// Intercepts the event and checks if the viewmodel should close.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.Assert(DataContext is BaseViewModel);
            if (DataContext is BaseViewModel viewModel && !viewModel.ShouldClose())
                e.Cancel = true;
        }
    }
}
