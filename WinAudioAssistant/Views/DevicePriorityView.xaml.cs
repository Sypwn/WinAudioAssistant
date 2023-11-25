﻿using NAudio.CoreAudioApi;
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
            OutputPriorityListBox.Tag = new ListBoxTag { DataFlow = DataFlow.Render, IsComms = false };
            CommsOutputPriorityListBox.Tag = new ListBoxTag { DataFlow = DataFlow.Render, IsComms = true };
            InputPriorityListBox.Tag = new ListBoxTag { DataFlow = DataFlow.Capture, IsComms = false };
            CommsInputPriorityListBox.Tag = new ListBoxTag { DataFlow = DataFlow.Capture, IsComms = true };

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
                // Notify DeviceCommands that their CanExecute result may have changed
                viewModel.EditDeviceCommand.RaiseCanExecuteChanged();
                viewModel.RemoveDeviceCommand.RaiseCanExecuteChanged();
            }
        }
    }
}