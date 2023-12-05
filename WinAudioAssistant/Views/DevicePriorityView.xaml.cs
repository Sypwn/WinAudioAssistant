using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AudioSwitcher.AudioApi;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant.Views
{
    /// <summary>
    /// Interaction logic for DevicePriorityView.xaml
    /// </summary>
    public partial class DevicePriorityView : Window
    {
        public DevicePriorityView()
        {
            InitializeComponent();
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel)
                viewModel.CloseViewAction = Close;

            // Assign a tag to each ListBox, for use by the ViewModel
            OutputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Playback, IsComms = false };
            CommsOutputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Playback, IsComms = true };
            InputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Capture, IsComms = false };
            CommsInputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Capture, IsComms = true };
        }

        private void SeparateCommsPriorityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Height += MainGrid.RowDefinitions[1].ActualHeight + OutputPriorityListBox.ActualHeight;
        }

        private void SeparateCommsPriorityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Height -= MainGrid.RowDefinitions[1].ActualHeight + OutputPriorityListBox.ActualHeight;
        }


        private void PriorityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel)
                viewModel.PriorityListBox_SelectionChanged(sender, e);
        }

        private void PriorityListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Assert(sender is ListBox);
            if (sender is ListBox listBox)
            {
                // If the user clicks on empty space in the ListBox, unselect all items
                HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
                if (r.VisualHit.GetType() != typeof(ListBoxItem))
                    listBox.UnselectAll();
            }
        }

        private void PriorityListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel && viewModel.EditDeviceCommand.CanExecute(null))
                viewModel.EditDeviceCommand.Execute(null);
        }

        private void PriorityListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel)
                viewModel.PriorityListBox_ContextMenuOpening(sender, e);
        }
    }
}
