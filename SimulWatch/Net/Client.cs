using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using CefSharp.DevTools.Runtime;

namespace SimulWatch.Net
{
    public class Client
    {
        private NetworkStream stream;
        private TcpClient client;
        public Client(string IP)
        {
            client = new TcpClient(IP, 7979);
            Debug.WriteLine("connecting to "+IP+" ...");
            Thread await = new Thread(() => AwaitCommands());
            await.Start();
        }

        private void AwaitCommands()
        {
            Await:
            stream = client.GetStream();
            /*
            byte[] length = new byte[1];
            stream.Read(length, 0, 1);
            byte[] data = new byte[length[0]];
            stream.Read(data, 1, length[0]);
            */
            byte[] data = new byte[2];
            stream.Read(data, 0, 2);
            MainWindow mainWindow = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                mainWindow = (MainWindow)App.Current.MainWindow;
            });
            switch ((SyncAction)data[0])
            {
                case SyncAction.Pause:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.mediaPlayer.Pause();
                    });
                    break;
                case SyncAction.Play:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.mediaPlayer.Play();
                    });
                    break;
                case SyncAction.SkipIntro:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.SkipIntro(null,null);
                    });
                    break;
            }
            goto Await;
        }
    }
}