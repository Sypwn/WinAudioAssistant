using System;
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

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Too lazy to make this a Command right now.
            ((EditDeviceViewModel)DataContext).Apply();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ((EditDeviceViewModel)DataContext).Apply();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
