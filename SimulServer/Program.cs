using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SimulWatch.Net;
using SimulWatch.Utility;

namespace SimulServer
{
    public static class Server
    {
        private static readonly Socket serverSocket =
            new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 7979;
        private static readonly byte[] buffer = new byte[2048];

        public static void Main()
        {
            Server.SetupServer();
            Console.ReadLine();
            Server.TestSend();
            Server.CloseAllSockets();
        }

        private static void TestSend()
        {
            var data = new byte[]
            {
                0,
                0,
                0,
                0,
                0,
                (byte)SyncAction.LoadSource
            };
                        
            byte[] bytes = Encoding.ASCII.GetBytes("https://youtu.be/HNwWQ9zGT-8");
            data = data.Concatenate(bytes);
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            Server.serverSocket.Bind((EndPoint)new IPEndPoint(IPAddress.Any, 7979));
            Server.serverSocket.Listen(0);
            Server.serverSocket.BeginAccept(new AsyncCallback(Server.AcceptCallback), (object)null);
            Console.WriteLine("Server setup complete");
        }

        private static void CloseAllSockets()
        {
            foreach (Socket clientSocket in Server.clientSockets)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }

            Server.serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;
            try
            {
                socket = Server.serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException ex)
            {
                return;
            }

            Server.clientSockets.Add(socket);
            socket.BeginReceive(Server.buffer, 0, 2048, SocketFlags.None, new AsyncCallback(Server.ReceiveCallback),
                (object)socket);
            Console.WriteLine("Client connected, waiting for request...");
            Server.serverSocket.BeginAccept(new AsyncCallback(Server.AcceptCallback), (object)null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket asyncState = (Socket)AR.AsyncState;
            Thread.Sleep(100);
            int length;
            try
            {
                length = asyncState.EndReceive(AR);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Client forcefully disconnected");
                asyncState.Close();
                Server.clientSockets.Remove(asyncState);
                return;
            }

            byte[] bytes1 = new byte[length];
            Array.Copy((Array)Server.buffer, (Array)bytes1, length);
            Console.WriteLine($"Recieved {bytes1}, sending out");

            foreach (Socket socket in clientSockets)
            {
                socket.Send(bytes1);
            }

            asyncState.BeginReceive(Server.buffer, 0, 2048, SocketFlags.None, new AsyncCallback(Server.ReceiveCallback),
                (object)asyncState);
        }
    }
}