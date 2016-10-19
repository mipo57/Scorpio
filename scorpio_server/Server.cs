using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace scorpio_server
{
    class Server
    {
        public Socket socket = null;
        public List<NetworkSocket> active_connections = null;

        public void Listen(string adress, int port, int max_connections = 10)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(adress), port));

            socket.Listen(max_connections);

            while (true)
            {
                Socket new_connection = socket.Accept();
                Task.Run(() => { NetworkSocket.Run(new_connection); });
            }
        }
    }
}
