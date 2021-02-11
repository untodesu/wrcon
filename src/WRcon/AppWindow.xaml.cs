using CoreRCON;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WRcon.Core;

namespace WRcon
{
    public partial class AppWindow : Window
    {
        private readonly object locker = new object();
        private readonly Config config = new Config();
        private RCON rcon = null;
        private bool busy = false;

        public AppWindow()
        {
            InitializeComponent();
            PrintText("WRcon {0}", System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
            SetClientStatus(ClientStatus.Disconnected);
            rconAddress.Text = config.Address;
            rconPort.Text = config.Port;
            rconPassword.Password = config.Password;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            config.Address = rconAddress.Text;
            config.Port = rconPort.Text;
            config.Password = rconPassword.Password;
            config.Save();
        }

        private void SetClientStatus(ClientStatus status)
        {
            lock(locker) {
                rconStatusLabel.Content = status.Text;
                rconStatusLabel.Foreground = status.Color;
            }
        }

        private void PrintText(string text)
        {
            lock(locker) {
                output.AppendText(text.Replace('\n', '\r') + "\r");
                output.ScrollToEnd();
            }
        }

        private void PrintText(string format, params object[] args)
        {
            lock(locker) {
                output.AppendText(String.Format(format, args).Replace('\n', '\r') + "\r");
                output.ScrollToEnd();
            }
        }

        private void RconPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Utils.IsNumeric(e.Text);
        }

        private void RconPort_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if(e.DataObject.GetDataPresent(typeof(string))) {
                if(!Utils.IsNumeric(e.DataObject.GetData(typeof(string)) as string))
                    e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private void RconAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                Connect_Click(sender, new RoutedEventArgs());
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if(rcon == null && !busy) {
                busy = true;

                SetClientStatus(ClientStatus.Connecting);
                PrintText("Connecting to {0}:{1}", rconAddress.Text, rconPort.Text);

                IPAddress[] ips = Dns.GetHostAddresses(rconAddress.Text);
                if(ips.Length == 0) {
                    SetClientStatus(ClientStatus.Disconnected);
                    PrintText("Error: Can't find any IP address for {0}", rconAddress.Text);
                    busy = false;
                    return;
                }

                try {
                    rcon = new RCON(ips[0], UInt16.Parse(rconPort.Text), rconPassword.Password);
                    rcon.OnDisconnected += Rcon_Disconnected;
                    await rcon.ConnectAsync();
                }
                catch(Exception exc) {
                    SetClientStatus(ClientStatus.Disconnected);
                    PrintText("Error: Can't connect ({0})", exc.Message);
                    rcon = null;
                    busy = false;
                    return;
                }

                SetClientStatus(ClientStatus.Connected);
                PrintText("Connected!");

                busy = false;
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if(!busy) {
                try { rcon?.Dispose(); }
                catch { }
            }
        }

        private void Rcon_Disconnected()
        {
            SetClientStatus(ClientStatus.Disconnected);
            PrintText("Disconnected!");
            rcon = null;
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if(rcon != null && !busy) {
                try {
                    if(!String.IsNullOrWhiteSpace(commandData.Text)) {
                        PrintText("> {0}", commandData.Text);
                        string response = await rcon.SendCommandAsync(commandData.Text);
                        if(!String.IsNullOrWhiteSpace(response))
                            PrintText(Regex.Replace(response, "[\\r\\n]+", "\r"));
                    }
                    commandData.Text = String.Empty;
                }
                catch { }
            }
        }

        private void CommandData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
                Send_Click(sender, new RoutedEventArgs());
        }
    }
}
