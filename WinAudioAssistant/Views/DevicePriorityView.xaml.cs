using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AudioSwitcher.AudioApi;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant.Views
{
    /// <summary>
    /// Interaction logic for DevicePriorityView.xaml
    /// </summary>
    public partial class DevicePriorityView : BaseView
    {
        /// <summary>
        /// Initializes the view.
        /// Assigns a ListBoxTag to each ListBox.
        /// </summary>
        public DevicePriorityView()
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel)
            {
                // The bindings don't take effect until after fade-in for some reason
                this.Width = viewModel.WindowWidth;
                this.Height = viewModel.WindowHeight;
            }

            // Assign a tag to each ListBox, for use by the ViewModel
            OutputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Playback, IsComms = false };
            CommsOutputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Playback, IsComms = true };
            InputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Capture, IsComms = false };
            CommsInputPriorityListBox.Tag = new ListBoxTag { DataFlow = DeviceType.Capture, IsComms = true };
        }

        public override void InitializeViewComponent() => InitializeComponent(); // Required to allow BaseView to call InitializeComponent()

        /// <summary>
        /// When SeparateCommsPriority is checked, expand the height of the window to show the Comms priority listbox.
        /// </summary>
        private void SeparateCommsPriorityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Height += MainGrid.RowDefinitions[1].ActualHeight + OutputPriorityListBox.ActualHeight;
        }

        /// <summary>
        /// When SeparateCommsPriority is unchecked, shrink the height of the window to hide the Comms priority listbox.
        /// </summary>
        private void SeparateCommsPriorityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Height -= MainGrid.RowDefinitions[1].ActualHeight + OutputPriorityListBox.ActualHeight;
        }

        /// <summary>
        /// When empty space is clicked in a listbox, clear the selection.
        /// </summary>
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

        /// <summary>
        /// When a listbox item is double-clicked, execute the EditDeviceCommand in the ViewModel, if valid.
        /// </summary>
        private void PriorityListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel && viewModel.EditDeviceCommand.CanExecute(null))
                viewModel.EditDeviceCommand.Execute(null);
        }

        /// <summary>
        /// When a listbox selection is changed, forward the event to the ViewModel.
        /// </summary>
        private void PriorityListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel)
                viewModel.PriorityListBox_SelectionChanged(sender, e);
        }

        /// <summary>
        /// When the context menu is opening, forward the event to the ViewModel.
        /// </summary>
        private void PriorityListBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Debug.Assert(DataContext is DevicePriorityViewModel);
            if (DataContext is DevicePriorityViewModel viewModel)
                viewModel.PriorityListBox_ContextMenuOpening(sender, e);
        }
    }
}
