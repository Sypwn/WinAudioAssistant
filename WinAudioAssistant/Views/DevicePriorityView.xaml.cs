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
using WinAudioAssistant.Models;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DevicePriorityView : Window
    {

        public DevicePriorityView()
        {
            InitializeComponent();
            OutputPriorityListBox.Tag = new ListBoxTag { Type = DeviceIOType.Output, IsComms = false };
            CommsOutputPriorityListBox.Tag = new ListBoxTag { Type = DeviceIOType.Output, IsComms = true };
            InputPriorityListBox.Tag = new ListBoxTag { Type = DeviceIOType.Input, IsComms = false };
            CommsInputPriorityListBox.Tag = new ListBoxTag { Type = DeviceIOType.Input, IsComms = true };

        }

        private void SeparateCommsPriorityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Height += MainGrid.RowDefinitions[1].ActualHeight + OutputPriorityListBox.ActualHeight;
        }

        private void SeparateCommsPriorityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Height -= MainGrid.RowDefinitions[1].ActualHeight + OutputPriorityListBox.ActualHeight;
        }

        private void PriorityListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is ListBox listBox && DataContext is DevicePriorityViewModel viewModel)
            {
                // Set ContextMenuListBox in ViewModel
                viewModel.ContextMenuListBox = listBox;
                // Notify RemoveDeviceCommand that its CanExecute result may have changed
                viewModel.RemoveDeviceCommand.RaiseCanExecuteChanged();

                /*
                if (listBox.SelectedItem == null)
                {
                    // Disable the "Remove" context menu item
                    ((MenuItem)listBox.ContextMenu.Items[1]).IsEnabled = false;
                }
                else
                {
                    // Enable the "Remove" context menu item
                    ((MenuItem)listBox.ContextMenu.Items[1]).IsEnabled = true;
                }
                */
            }
        }
    }
}