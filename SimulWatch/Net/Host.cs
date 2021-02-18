using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;

namespace SimulWatch.Net
{
    public class Host
    {

        private TcpClient client;
        private NetworkStream stream;
        public Host()
        {
            IPAddress localip = GetLocalIPAddress();
            TcpListener server = new TcpListener(localip, 7979);

            server.Start();
            Debug.WriteLine($"Server has started on {localip}.Waiting for a connection...", Environment.NewLine);
            
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow window = (MainWindow)App.Current.MainWindow;
                window.host = this;
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
                        1,
                        (byte)SyncAction.Pause
                    };
                    stream.Write(data,0,data.Length);
                    break;
                case SyncAction.Play:
                    data = new byte[]
                    {
                        1,
                        (byte)SyncAction.Play
                    };
                    stream.Write(data,0,data.Length);
                    break;
                case SyncAction.SkipIntro:
                    data = new byte[]
                    {
                        1,
                        (byte)SyncAction.SkipIntro
                    };
                    stream.Write(data,0,data.Length);
                    break;
            }
        }

        public void SendPackage(string source, SyncAction action = SyncAction.LoadSource)
        {
            
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