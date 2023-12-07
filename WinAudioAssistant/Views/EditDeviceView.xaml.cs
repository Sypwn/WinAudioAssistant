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
using WinAudioAssistant.Models;

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
            {
                viewModel.CloseViewAction = Close;
                viewModel.FocusViewAction = () => Focus(); // Focus returns bool, so we need to wrap it in a lambda
                this.Unloaded += (_,_) => viewModel.Cleanup();

                viewModel.ManagedDeviceIdentificationMethods.Add(new EditDeviceViewModel.DeviceIdentificationMethodOption
                {
                    Name = "Strict",
                    Value = ManagedDevice.IdentificationMethods.Strict,
                    ToolTip = "Identify this endpoint by its unique GUID. This ensures there are no identification conflicts with other similar devices, " +
                              "however its GUID may change when changing ports or sockets, updating or reinstalling drivers, etc."
                });
                viewModel.ManagedDeviceIdentificationMethods.Add(new EditDeviceViewModel.DeviceIdentificationMethodOption
                {
                    Name = "Loose",
                    Value = ManagedDevice.IdentificationMethods.Loose,
                    ToolTip = "Identify this endpoint based on its hardware description and jack type. This allows the endpoint to continue to be detected across " +
                              "multiple USB ports, for example, but will create an identification conflict if multiple similar devices are connected."
                });
                viewModel.ManagedDeviceIdentificationMethods.Add(new EditDeviceViewModel.DeviceIdentificationMethodOption
                {
                    Name = "Custom",
                    Value = ManagedDevice.IdentificationMethods.Custom,
                    ToolTip = "Customize the method used to identify this endpoint. (Advanced)\n\nAll checked properties must match the saved endpoint information " +
                              "for a positive identification. You can hover over an endpoint in the previous selection dropdown to see detailed info about it."
                });

                // The ViewModel must know which custom flags are available to be selected in the UI, to ensure unavailable flags are not somehow left active
                viewModel.AvailableCustomIdentificationFlags = ManagedDevice.IdentificationFlags.AudioEndpoint_FormFactor |
                                                               ManagedDevice.IdentificationFlags.AudioEndpoint_JackSubType |
                                                               ManagedDevice.IdentificationFlags.Device_ContainerId |
                                                               ManagedDevice.IdentificationFlags.Device_DeviceDesc |
                                                               ManagedDevice.IdentificationFlags.DeviceInterface_FriendlyName |
                                                               ManagedDevice.IdentificationFlags.HostDeviceDesc;
            }
        }

        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            // Lock the window height to the content height
            SizeToContent = SizeToContent.Height;
        }
    }
}
