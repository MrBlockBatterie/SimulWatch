using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace SimulWatch.Net
{
    public class Client
    {

        private MainWindow _main = (MainWindow)Application.Current.MainWindow;
        public List<PackageData> OutgoingPackages = new List<PackageData>();
        private readonly Socket ClientSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private Thread RecieveThread = null;
        private byte[] Buff = new byte[2048];

        private const int PORT = 7979;

        public void Start(string ip)
        {
            ConnectToServer(ip);

            ClientSocket.BeginReceive(Buff, 0, 2048, SocketFlags.None, new AsyncCallback(ReceiveResponse),(object)null);


            // RecieveThread = new Thread(() =>
            // {
            //     ReceiveResponse();
            // });
            // RecieveThread.Start();
            RequestLoop();


            //Client.Exit();
        }

        private void ConnectToServer(string ip)
        {
            int num = 0;
            while (!ClientSocket.Connected)
            {
                try
                {
                    ++num;
                    Debug.WriteLine("Connection attempt " + num.ToString());
                    ClientSocket.Connect( IPAddress.Loopback/*IPAddress.Parse(ip)*/,7979);
                }
                catch (SocketException ex)
                {
                    Console.Clear();
                }
            }
            
            Debug.WriteLine("Connected");
        }

        private void RequestLoop()
        {
            while (true)
            {
                if (OutgoingPackages.Count > 0)
                {
                    SendRequest(OutgoingPackages.First());
                    
                }
            }
        }

        private void Exit()
        {
            //Client.SendData("exit");
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        public void SendRequest(PackageData data)
        {
            SendData(data);
            OutgoingPackages.Remove(data);
            //Client.Exit();
        }

        private void SendData(PackageData data)
        {
            Debug.WriteLine(ClientSocket.Connected);
            ClientSocket.Send(data.OutgoingData, 0, 2048, SocketFlags.None);
            
            
        }

        private void ReceiveResponse(IAsyncResult AR)
        {
            int length = ClientSocket.EndReceive(AR);
            Debug.WriteLine("Transmission lenght: "+length );
            // int length = Buff.Length;
            if (length == 0)
                return;
            byte[] bytes = new byte[length];
            Debug.WriteLine("recieved data");
            Array.Copy((Array)Buff, (Array)bytes, length);
            foreach (var b in bytes)
            {
                Debug.Write(b+" ");
                
            }
            Debug.Write(Environment.NewLine);
            switch ((SyncAction)bytes[5])
            {
                case SyncAction.Pause:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _main.MediaPlayer.Pause();
                    });
                    break;
                case SyncAction.Play:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _main.MediaPlayer.Play();
                    });
                    break;
                case SyncAction.SkipIntro:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _main.SkipIntro(null,null);
                    });
                    break;
                case SyncAction.LoadSource:
                    // int length = 0;
                    // for (int i = 0; i < 4; i++)
                    // {
                    //     length += bytes[i];
                    // }
                    Debug.WriteLine($"I Load Data");
                    length = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        length += bytes[i];
                    }
                    Debug.WriteLine($"Message is {length} bytes long");
                    byte[] stringData = new byte[length];
                    
                    Array.Copy((Array)bytes, 6 , stringData, 0 , length);
                    foreach (var b in stringData)
                    {
                        Debug.Write(b+" ");
                    }
                    string url = System.Text.Encoding.ASCII.GetString(stringData);
                    Debug.WriteLine($"encoded URL was {url}");
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _main.Player.Load(url);
                    });
                    break;
                case SyncAction.GoToStart:
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _main.ToStart(null,null);
                    });
                    break;
                default:
                    return;
            }
            
            
            Buff = new byte[2048];
            ClientSocket.BeginReceive(Buff, 0, 2048, SocketFlags.None, new AsyncCallback(ReceiveResponse),(object)null);
            
        }
        
    }
}