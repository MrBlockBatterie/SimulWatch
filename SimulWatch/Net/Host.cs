using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using SimulWatch.Utility;

namespace SimulWatch.Net
{
    public class Host
    {

        private TcpClient client = default(TcpClient);
        private NetworkStream stream;
        public Host()
        {
            IPAddress ipAddress = Dns.Resolve("93.215.210.210").AddressList[0];
            IPAddress localip = GetLocalIPAddress();
            TcpListener server = new TcpListener(localip, 7979);

            server.Start();
            Debug.WriteLine($"Server has started on {localip} Waiting for a connection...", Environment.NewLine);
            
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow window = (MainWindow)App.Current.MainWindow;
                window.Host = this;
            });
            client = server.AcceptTcpClient();

            stream = client.GetStream();
            Debug.WriteLine("A client connected.");
        }

        public void SendPackage(SyncAction action)
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

        public void SendPackage(string source, SyncAction action = SyncAction.LoadSource)
        {
            if (action == SyncAction.LoadSource)
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
        
    }
    
}