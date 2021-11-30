using System.Windows;
using SimulWatch.Net;

namespace SimulWatch
{
    public partial class DebugWindow : Window
    {
        public MainWindow Main;
        public DebugWindow(MainWindow main)
        {
            InitializeComponent();
            Main = main;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Main.FullScreenMode(null,null);
        }

        private void ServerStartButton(object sender, RoutedEventArgs e)
        {
            Server.Start();
        }
    }
}