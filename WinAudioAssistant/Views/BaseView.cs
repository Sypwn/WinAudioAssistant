using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant.Views
{
    /// <summary>
    /// Base class window-style views.
    /// </summary>
    public abstract partial class BaseView : Window
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
                viewModel.FocusViewAction = ForceFocus;
                viewModel.FlashViewAction = Flash;
                this.Unloaded += (_, _) => viewModel.Cleanup();
            }
        }

        protected abstract void InitializeViewComponent(); // The dervived class must direct this to InitializeComponent()

        /// <summary>
        /// Restores the window if it is minimized, and then focuses it.
        /// </summary>
        public void ForceFocus()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            Activate();
            Focus();
        }

        /// <summary>
        /// Plays the system chime and flashes the window, similar to a modal dialog.
        /// </summary>
        public void Flash()
        {
            const uint FLASHW_CAPTION = 1; // Flash the window caption
            const uint FLASHW_TRAY = 2; // Flash the taskbar button
            //const uint FLASHW_TIMERNOFG = 12; // Flash continuously until the window comes to the foreground
            FLASHWINFO fwInfo = new()
            {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle,
                dwFlags = FLASHW_CAPTION | FLASHW_TRAY,
                uCount = 5,
                dwTimeout = 60,
            };

            System.Media.SystemSounds.Beep.Play();
            FlashWindowEx(ref fwInfo);
        }

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

        /// <summary>
        /// External method to call FlashWindowEx
        /// </summary>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool FlashWindowEx(ref FLASHWINFO pwfi);

        /// <summary>
        /// Struct for the pfwi parameter of FlashWindowEx, defined in WINUSER.H
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }
    }
}
