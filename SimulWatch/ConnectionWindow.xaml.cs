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
            Client client = new Client();
            ((MainWindow)App.Current.MainWindow).CombinedClient = client;
            
            Thread clientThread = new Thread(() =>
                {
                    string cip = "";
                    Dispatcher.Invoke(() =>
                    {
                        cip = TextBox.Text;
                    });
                    client.Start(cip);
                }
            );
            
            clientThread.Start();
        }
    }
}