using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Vlc.DotNet.Wpf;

namespace SimulWatch
{
    public partial class FullscreenWindow : Window
    {
        public FullscreenWindow()
        {
            InitializeComponent();
            
        }

        private void FullscreenWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var mainWindow = (MainWindow)App.Current.MainWindow;
            var Vlc = (VlcControl)Grid.Children[0];
            Grid.Children.Remove(Vlc);
            mainWindow.Dock.Children.Add(Vlc);

        }

        private void FullscreenWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}