using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace scorpio_server
{
    class Client
    {
        public NetworkSocket socket = null;

        public void Connect(string ip, int port)
        {
            socket = NetworkSocket.InitializeProtocol();
            socket.ConnectToServer(ip, port);
        }

        public void SendMessage(string message)
        {
            NetworkMessage msg = new NetworkMessage();
            msg.AttachString(message, "msg");

            socket.Send(msg);
        }

        public void Disconnect()
        {
            
        }

    }
}
