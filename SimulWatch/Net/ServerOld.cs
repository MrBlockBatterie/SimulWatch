using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;

namespace SimulWatch.Net
{
    public class ServerOld
    {
        public TcpClient ClientOut;
        public TcpClient ClientIn;
        
        
        private TcpClient dummyClient = default(TcpClient);
        private List<TcpClient> clientListIn = new List<TcpClient>();
        private List<TcpClient> clientListOut = new List<TcpClient>();
        private List<NetworkStream> clientNStreamsIn = new List<NetworkStream>();
        private List<NetworkStream> clientNStreamsOut = new List<NetworkStream>();
        private List<Thread> clientThreads = new List<Thread>();
        public DebugWindow Window;
        public ServerOld(DebugWindow debugWindow)
        {
            Window = debugWindow;
            Thread start = new Thread(() => InitHosting());
            start.Start();
        }

        private void InitHosting()
        {
            Thread serverIn = new Thread(() => AcceptClientsIn());
            Thread serverOut = new Thread(() => AcceptClientsOut());
            serverIn.Start();
            serverOut.Start();
            
            ClientIn = new TcpClient("localhost", 7978);
            ClientOut = new TcpClient("localhost", 7979);
            
        }

        private void AcceptClientsIn()
        {
            IPAddress localip = GetLocalIPAddress();
            TcpListener serverIn = new TcpListener(7978);
            
            
            TcpClient localDummy;
            
            
            
            serverIn.Start();
            App.Current.Dispatcher.Invoke(() => {Window.LogBox.AppendText($"Server has started on {localip} Waiting for a connection... {Environment.NewLine}"); });
            
            
            A:
            localDummy = serverIn.AcceptTcpClient();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                clientListIn.Add(localDummy);
                clientNStreamsIn.Add(clientListIn[clientListIn.Count-1].GetStream()); 
            });
            clientThreads.Add(new Thread(() => CallAndResponse(clientNStreamsIn[clientNStreamsIn.Count-1])));
            clientThreads.Last().Start();
           
            
            
            goto A;
        }
        private void AcceptClientsOut()
        {
            IPAddress localip = GetLocalIPAddress();
            TcpListener serverOut = new TcpListener(7979);
            
            
            
            TcpClient localDummy;
            
            
            serverOut.Start();
            
            App.Current.Dispatcher.Invoke(() => {Window.LogBox.AppendText($"Server has started on {localip} Waiting for a connection... {Environment.NewLine}"); });
            
            
            A:
            localDummy = serverOut.AcceptTcpClient();
            App.Current.Dispatcher.Invoke(() =>
            {
                clientListOut.Add(localDummy);
                clientNStreamsOut.Add(clientListOut[clientListOut.Count-1].GetStream());
                App.Current.Dispatcher.Invoke(() => {Window.LogBox.AppendText($"A client connected with IP {((IPEndPoint)localDummy.Client.RemoteEndPoint).Address.ToString()} {Environment.NewLine}"); });
            });
            
            
            goto A;
        }

        private void CallAndResponse(NetworkStream stream)
        {
            Await:
            //stream = ClientIn.GetStream();
            /*
            byte[] length = new byte[1];
            stream.Read(length, 0, 1);
            byte[] data = new byte[length[0]];
            stream.Read(data, 1, length[0]);
            */
            byte[] data = new byte[6];
            try
            {
                stream.Read(data, 0, 6);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            foreach (var localstream in clientNStreamsOut)
            {
                try
                {
                    localstream.Write(data,0,6);
                    string bytes = "";
                    foreach (var b in data)
                    {
                        bytes += b.ToString();
                    }
                    App.Current.Dispatcher.Invoke(() => {Window.LogBox.AppendText($"Sending data {bytes} {Environment.NewLine}"); });

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
            goto Await;
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