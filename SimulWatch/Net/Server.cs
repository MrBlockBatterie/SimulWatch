using System.Threading;
using System.Windows.Threading;

namespace SimulWatch.Net
{
    public class Server
    {
        private Host _host = new Host();
        private Client _client = new Client("localhost");
        Server()
        {
            InitHosting();
        }

        private void InitHosting()
        {
            Thread hostThread = new Thread(() => _host = new Host());
            Thread clientThread = new Thread(() => _client = new Client("localhost"));
            hostThread.Start();
            clientThread.Start();
        }
    }
}