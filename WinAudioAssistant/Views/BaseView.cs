using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant.Views
{
    public abstract class BaseView : Window
    {
        public abstract void InitializeViewComponent();

        public BaseView()
        {
            InitializeViewComponent(); // Should direct to InitializeComponent() in the derived class
            Debug.Assert(DataContext is BaseViewModel);
            if (DataContext is BaseViewModel viewModel)
            {
                viewModel.CloseViewAction = Close;
                viewModel.FocusViewAction = () => Focus(); // Focus returns bool, so we wrap it in a lambda
                this.Unloaded += (_, _) => viewModel.Cleanup();
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.Assert(DataContext is BaseViewModel);
            if (DataContext is BaseViewModel viewModel && !viewModel.ShouldClose())
                e.Cancel = true;
        }
    }
}
