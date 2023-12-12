using System.Diagnostics;
using System.Windows;
using WinAudioAssistant.ViewModels;
using WinAudioAssistant.Models;

namespace WinAudioAssistant.Views
{
    /// <summary>
    /// Interaction logic for EditDeviceView.xaml
    /// </summary>
    public partial class EditDeviceView : BaseView
    {
        /// <summary>
        /// Initializes the view.
        /// Populates the DeviceIdentificationMethods list.
        /// Sets the available custom identification flags.
        /// </summary>
        public EditDeviceView()
        {
            Debug.Assert(DataContext is EditDeviceViewModel);
            if (DataContext is EditDeviceViewModel viewModel)
            {
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

        protected override void InitializeViewComponent() => InitializeComponent(); // Required to allow BaseView to call InitializeComponent()

        /// <summary>
        /// When the window layout changes, re-enable SizeToContent.Height.
        /// This effectively locks the window height to the content height, while allowing the width to be resized.
        /// </summary>
        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Height;
        }
    }
}
