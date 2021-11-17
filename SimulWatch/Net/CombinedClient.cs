using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SimulWatch.Utility;

namespace SimulWatch.Net
{
    public class CombinedClient
    {
       private NetworkStream stream;
        private TcpClient client;
        private string _ip;
        public CombinedClient(string IP)
        {
            _ip = IP;
            client = new TcpClient(IP, 7979);
            Debug.WriteLine("connecting to "+IP+"...");
            Thread await = new Thread(() => AwaitCommands());
            MainWindow mainWindow = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                mainWindow = (MainWindow)App.Current.MainWindow;
                mainWindow.Title += " {" + $"connected to {this._ip}" + "}";
            });
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
        public void SendPackage(SyncAction action)
        {
            using (var stream = client.GetStream())
            {
                byte[] data;
                switch (action)
                {
                    case SyncAction.Pause:
                        data = new byte[]
                        {
                            1, 0, 0, 0, 0,
                            (byte) SyncAction.Pause
                        };
                        stream.Write(data,0,data.Length);
                        break;
                    case SyncAction.Play:
                        data = new byte[]
                        {
                            1,0,0,0,0,
                            (byte)SyncAction.Play
                        };
                        stream.Write(data,0,data.Length);
                        break;
                    case SyncAction.SkipIntro:
                        data = new byte[]
                        {
                            1,0,0,0,0,
                            (byte)SyncAction.SkipIntro
                        };
                        stream.Write(data,0,data.Length);
                        break;
                    case SyncAction.GoToStart:
                        data = new byte[]
                        {
                            1,0,0,0,0,
                            (byte)SyncAction.GoToStart
                        };
                        stream.Write(data,0,data.Length);
                        break;
                }
            }
        }

        public void SendPackage(string source, SyncAction action = SyncAction.LoadSource)
        {
            if (action == SyncAction.LoadSource)
            {
                using (var stream = client.GetStream())
                {
                    Debug.WriteLine($"Message is {source.Length} bytes long");
                    int stringLength = source.Length;
                    byte[] length = new Byte[]{0,0,0,0,0};
                    if (stringLength > 255)
                    {
                        length[0] = 255;
                        int index = 1;
                    
                        if (stringLength - 255 <= 255)
                        {
                            length[1] = (byte)(stringLength % 255);
                        }
                        while (stringLength - 255 >= 255)
                        {
                            length[index] = 255;
                            stringLength -= 255;
                            index++;
                            if (stringLength - 255 <= 255)
                            {
                                length[index] = (byte)(stringLength - 255);
                            }
                        }
                    }
                    else
                    {
                        length[0] = (byte)stringLength;
                    }
                    byte[] lengthBytes = new byte[]
                    {
                        length[0],
                        length[1],
                        length[2],
                        length[3],
                        length[4],
                        (byte)SyncAction.LoadSource
                    };
                    byte[] bytes = Encoding.ASCII.GetBytes(source);
                    byte[] finalData = lengthBytes.Concatenate(bytes);
                    stream.Write(finalData,0,finalData.Length);
                }
            }
        }
    }
}