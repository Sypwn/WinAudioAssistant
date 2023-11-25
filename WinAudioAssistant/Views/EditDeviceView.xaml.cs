﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinAudioAssistant.ViewModels;

namespace WinAudioAssistant.Views
{
    /// <summary>
    /// Interaction logic for EditDeviceView.xaml
    /// </summary>
    public partial class EditDeviceView : Window
    {
        public EditDeviceView()
        {
            InitializeComponent();
        }

        // Too lazy to make these into Commands right now.
        private void RefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            ((EditDeviceViewModel)DataContext).RefreshDevices();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ((EditDeviceViewModel)DataContext).Apply();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (((EditDeviceViewModel)DataContext).Apply())
            {
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
