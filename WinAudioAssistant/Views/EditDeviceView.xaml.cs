using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.Assert(DataContext is EditDeviceViewModel);
            if (DataContext is EditDeviceViewModel viewModel)
                viewModel.CloseViewAction = Close;
        }
    }
}
