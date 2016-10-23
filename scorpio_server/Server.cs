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
        NetworkSocket _socket = null;

        /*
         * Start server on a given adress
         */
        public void Start(string adress, int port)
        {
            _socket = NetworkSocket.InitializeProtocol();

            _socket.ListenForConnections(adress, port, OnNewConnection);
        }

        /* 
         * Event triggered on every new client connected to server
         */
        public void OnNewConnection(NetworkSocket socket)
        {
            socket.ProcessRequests(OnMessageReceived);
            Console.WriteLine("New Connection!");
        }

        /*
         * Event triggered on every new message recived
         */
        public void OnMessageReceived(NetworkMessage message, NetworkSocket connection_socket)
        {
            Console.WriteLine($"Recived message: {message.GetElementString("msg")}");
        }
    }
}
