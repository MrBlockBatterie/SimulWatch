using System.Threading;
using System.Windows;
using SimulWatch.Net;

namespace SimulWatch
{
    public partial class ConnectionWindow : Window
    {
        public ConnectionWindow()
        {
            InitializeComponent();
        }

        private void ConnectToHost(object sender, RoutedEventArgs e)
        {
            Client client;
            Thread clientThread = new Thread(() => 
                Dispatcher.Invoke(() =>
                {
                    client = new Client(TextBox.Text);
                }));
            clientThread.Start();
            this.Close();
        }
    }
}