using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using SimulWatch.Utility;

namespace SimulWatch.Net
{
    public class Host
    {

        private TcpClient client = default(TcpClient);
        private List<TcpClient> clientList = new List<TcpClient>();
        private List<NetworkStream> clientNStreams = new List<NetworkStream>();
        private List<Thread> clientThreads = new List<Thread>();
        //private NetworkStream stream;
        public Host() //Deprecated
        {
            IPAddress ipAddress = Dns.Resolve("93.215.210.210").AddressList[0];
            IPAddress localip = GetLocalIPAddress();
            TcpListener server = new TcpListener(7979);
            

            server.Start();
            Debug.WriteLine($"Server has started on {localip} Waiting for a connection...", Environment.NewLine);
            
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow window = (MainWindow)App.Current.MainWindow;
                //window.C = this;
            });

            A:
            clientList.Add(server.AcceptTcpClient());
            clientNStreams.Add(clientList[clientList.Count-1].GetStream());
            Debug.WriteLine("A client connected.");
            clientThreads.Add(new Thread(() => AwaitCommands(clientNStreams[clientNStreams.Count-1])));
            clientThreads.Last().Start();
            goto A;
        }

        public void SendPackage(SyncAction action)
        {
            foreach (var stream in clientNStreams)
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
                foreach (var stream in clientNStreams)
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
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("err");
        }
        
        private void AwaitCommands(NetworkStream stream)
        {
            Await:
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
                        SendPackage(SyncAction.Pause);
                    });
                    break;
                case SyncAction.Play:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SendPackage(SyncAction.Play);
                    });
                    break;
                case SyncAction.SkipIntro:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SendPackage(SyncAction.SkipIntro);
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
                        SendPackage(url);
                    });
                    break;
                case SyncAction.GoToStart:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        SendPackage(SyncAction.GoToStart);
                    });
                    break;
            }
            goto Await;
        }
        
    }
    
}