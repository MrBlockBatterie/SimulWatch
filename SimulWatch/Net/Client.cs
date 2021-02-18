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
        private string _ip;
        public Client(string IP)
        {
            _ip = IP;
            client = new TcpClient(IP, 7979);
            Debug.WriteLine("connecting to "+IP+"...");
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
            byte[] data = new byte[6];
            stream.Read(data, 0, 6);
            Debug.Write("Recieved bytes: ");
            foreach (var b in data)
            {
                Debug.Write(b+" ");
            }
            MainWindow mainWindow = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                mainWindow = (MainWindow)App.Current.MainWindow;
                mainWindow.Title += " {" + $"connected to {this._ip}" + "}";
            });
            Debug.WriteLine($"Recieved action was {data[5]}");
            switch ((SyncAction)data[5])
            {
                case SyncAction.Pause:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.MediaPlayer.Pause();
                    });
                    break;
                case SyncAction.Play:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.MediaPlayer.Play();
                    });
                    break;
                case SyncAction.SkipIntro:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.SkipIntro(null,null);
                    });
                    break;
                case SyncAction.LoadSource:
                    int length = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        length += data[i];
                    }
                    Debug.WriteLine($"Message is {length} bytes long");
                    byte[] stringData = new byte[length];
                    stream.Read(stringData, 0, length);
                    foreach (var b in stringData)
                    {
                        Debug.Write(b+" ");
                    }
                    string url = System.Text.Encoding.ASCII.GetString(stringData);
                    Debug.WriteLine($"encoded URL was {url}");
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.MediaPlayer.Play(url);
                    });
                    break;
                case SyncAction.GoToStart:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        mainWindow.ToStart(null,null);
                    });
                    break;
            }
            goto Await;
        }
    }
}