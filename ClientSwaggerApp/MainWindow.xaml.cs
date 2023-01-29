using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientSwaggerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 27001;

        public MainWindow()
        {
            InitializeComponent();

        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
            RequestLoop();
        }


        private void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(IPAddress.Parse("10.2.13.15"), PORT);
                }
                catch (SocketException)
                {
                }
            }

            MessageBox.Show("Connected");

            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            App.Current.Dispatcher.Invoke(() =>
            {
                ParseForView(text);
            });
        }

        private void ParseForView(string text)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var commands = text.Split('\n');
                var result = commands.ToList();
                result.Remove("");
                foreach (var item in result)
                {
                    Button button = new Button();
                    button.FontSize = 22;
                    button.Margin = new Thickness(0, 10, 0, 0);
                    button.Content = item;
                    button.Click += Button_Click1;
                    CommandsStackPanel.Children.Add(button);
                }
            });
        }

        TextBox textBox = new TextBox();
        public string SelectedCommand { get; set; }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if (sender is Button bt)
            {
                var content = bt.Content.ToString();
                var result = content.Remove(content.Length - 1, 1);
                SelectedCommand = result;
                var splitResult = result.Split('\\');
                if (splitResult.Length > 2)
                {
                    textBox.Width = 200;
                    textBox.Height = 60;
                    textBox.Text = "*" + splitResult[2];
                    textBox.FontSize = 22;
                    if (paramsStackPanel.Children.Count > 3)
                    {
                        paramsStackPanel.Children.RemoveAt(3);
                        paramsStackPanel.Children.RemoveAt(3);
                    }
                    paramsStackPanel.Children.Add(textBox);

                    Button button = new Button();
                    button.FontSize = 22;
                    button.Margin = new Thickness(0, 10, 0, 0);
                    button.Content = "Execute";
                    button.Click += Button_Click2; ;
                    paramsStackPanel.Children.Add(button);

                }
                else
                {
                    if (paramsStackPanel.Children.Count > 3)
                    {
                        paramsStackPanel.Children.RemoveAt(3);
                        paramsStackPanel.Children.RemoveAt(3);
                    }
                    SendString(result);
                }
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            var result = SelectedCommand.Split('\\');
            var resultText = result[0] + "\\"+result[1] + "\\" + textBox.Text;
            if (SelectedCommand.Contains("json"))
            {
                resultText= result[0] + "\\" + result[1] + " " + textBox.Text;
            }
            SendString(resultText);
        }

        private void RequestLoop()
        {

            var receiver = Task.Run(() =>
            {

                while (true)
                {
                    ReceiveResponse();

                }
            });

            //Task.WaitAll(receiver);
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            App.Current.Dispatcher.Invoke(() =>
            {
                ResponseTxtb.Text = text;
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SendString(requestTxtb.Text);
        }
    }
}
