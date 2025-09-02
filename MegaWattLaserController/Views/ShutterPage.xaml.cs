using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaserControllerApp.Services;
using LaserControllerApp.Models;
using System;
using System.Threading.Tasks;

namespace LaserControllerApp.Views
{
    public sealed partial class ShutterPage : Page
    {
        private readonly SerialPortManager _serialPortManager = SerialPortManager.Instance;

        public ShutterPage()
        {
            this.InitializeComponent();

            // Subscribe to response events
            if (_serialPortManager is ICommandResponseHandler responseHandler)
            {
                responseHandler.CommandResponseReceived += ResponseHandler_CommandResponseReceived;
            }
        }

        private void ResponseHandler_CommandResponseReceived(object sender, FpgaCommand e)
        {
            // Handle incoming responses on UI thread
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                ProcessResponse(e);
            });
        }

        private void ProcessResponse(FpgaCommand response)
        {
            switch (response.Command)
            {
                case FpgaCommandIds.lcdRxShutterConfig:
                    if (response.Data.Length > 0)
                    {
                        ShutterState state = (ShutterState)response.Data[0];
                        StatusTextBlock.Text = $"Shutter is {(state == ShutterState.Open ? "OPEN" : "CLOSED")}";
                        ShowSuccessMessage($"Shutter {(state == ShutterState.Open ? "opened" : "closed")} successfully");
                    }
                    break;

                case FpgaCommandIds.lcdRxBadCmd:
                    ShowErrorMessage("FPGA rejected shutter command");
                    break;

                default:
                    // Ignore other commands
                    break;
            }
        }

        private async void OpenShutter_Click(object sender, RoutedEventArgs e)
        {
            await SendShutterCommand(ShutterState.Open);
        }

        private async void CloseShutter_Click(object sender, RoutedEventArgs e)
        {
            await SendShutterCommand(ShutterState.Closed);
        }

        private async Task SendShutterCommand(ShutterState state)
        {
            if (!_serialPortManager.IsConnected)
            {
                ShowErrorMessage("Not connected to laser");
                return;
            }

            try
            {
                OpenShutterButton.IsEnabled = false;
                CloseShutterButton.IsEnabled = false;

                // For manual shutter control, we set both mode and state
                var command = new FpgaCommand
                {
                    Command = FpgaCommandIds.lcdTxShutterConfig,
                    Data = new byte[] { (byte)ShutterMode.Manual, (byte)state }
                };

                await _serialPortManager.SendCommandAsync(command);
                StatusTextBlock.Text = $"Sending shutter {(state == ShutterState.Open ? "open" : "close")} command...";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                OpenShutterButton.IsEnabled = true;
                CloseShutterButton.IsEnabled = true;
            }
        }

        private void ShowErrorMessage(string message)
        {
            StatusTextBlock.Text = message;
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Error;
            StatusInfoBar.IsOpen = true;
        }

        private void ShowSuccessMessage(string message)
        {
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Success;
            StatusInfoBar.IsOpen = true;
        }

        // Clean up event handler when page is unloaded
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_serialPortManager is ICommandResponseHandler responseHandler)
            {
                responseHandler.CommandResponseReceived -= ResponseHandler_CommandResponseReceived;
            }
        }
    }
}