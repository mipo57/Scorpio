using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace scorpio_server
{
    enum ConnectionType { UDP, TCP }
    class NetworkSocket
    {
        Socket _socket = null;
        CancellationTokenSource _listening_cancelation = new CancellationTokenSource();

        public delegate void NewConnectionCallback(NetworkSocket socket);
        public delegate void NewMessageRecivedCallback(NetworkSocket socket, NetworkMessage message);

        NetworkSocket() { }
        NetworkSocket(Socket socket)
        {
            _socket = socket;
        }

        public static NetworkSocket InitializeProtocol(ConnectionType connection_type = ConnectionType.UDP)
        {
            Socket socket = null;

            if (connection_type == ConnectionType.TCP)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            return new NetworkSocket(socket);
        }

        public bool ConnectToServer(string server_adress, int port)
        {
            _socket.Connect(IPAddress.Parse(server_adress), port);

            if (_socket.Connected)
            {
                Console.WriteLine("Connected to {0}:{1}", server_adress, port);
                return true;
            }
            else
            {
                Console.WriteLine("Warning: Connection to {0}:{1} failed!", server_adress, port);
                return false;
            }
        }

        public void ListenForConnections(string IP, int port, NewConnectionCallback new_connection_callback, int max_connections = 1)
        {
            _socket.Bind(new IPEndPoint(IPAddress.Parse(IP), port));
            _socket.Listen(max_connections);

            Task.Run(() =>
            {
                while (true)
                {
                    Socket new_connection = _socket.Accept();
                    new_connection_callback(new NetworkSocket(new_connection));
                }
            });
        }

        public void StopListeningForConnections()
        {
            _listening_cancelation.Cancel();
        }

        static void ProcessRequests(NewMessageRecivedCallback msg_recived_callback)
        {
            byte[] buffer;
            List<byte> full_msg = new List<byte>();


        }
    /*
        static void ProcessRequests()
        {
            byte[] buffer = new byte[1024];
            string message_queue = "";

            while (true)
            {
                int num_bytes_recived = socket.Receive(buffer);

                if (num_bytes_recived > 0)
                {
                    string msg = Encoding.ASCII.GetString(buffer, 0, num_bytes_recived);

                    string[] messages = msg.Split(new char[] { Constants.C_END_OF_MESSAGE }, StringSplitOptions.None);
                    messages[0] = message_queue + messages[0];
                    message_queue = messages.Last();

                    foreach (string str in messages.Take(messages.Length - 1))
                    {
                        Console.WriteLine("Message from client {0}:{1} - {2}", (socket.RemoteEndPoint as IPEndPoint).Address, (socket.RemoteEndPoint as IPEndPoint).Port, str);
                    }

                    string return_msg = msg.ToUpper();
                    byte[] return_msg_en = Encoding.ASCII.GetBytes(return_msg);
                    socket.Send(return_msg_en);
                }
            }
        }
        */

        static void Ping(Socket socket)
        {
            byte[] ping_message = Encoding.ASCII.GetBytes("<PING>");
            socket.Send(ping_message);
        }

        public static void Run(Socket connection_socket)
        {
            CancellationToken cancelation_token = new CancellationToken();
            Task.Run(() => { ProcessRequests(connection_socket); }, cancelation_token);

            int last_second = DateTime.Now.Second;
            while (connection_socket.Connected)
            {
                if (DateTime.Now.Second != last_second && DateTime.Now.Second % 3 == 0)
                {
                    Ping(connection_socket);
                    last_second = DateTime.Now.Second;
                }
            }

            cancelation_token.ThrowIfCancellationRequested();
            Console.WriteLine("Client {0}:{1} disconnected!", (connection_socket.RemoteEndPoint as IPEndPoint).Address, (connection_socket.RemoteEndPoint as IPEndPoint).Port);
        }

        public static void Send(Socket socket, string message)
        {
            string msg = message + Constants.C_END_OF_MESSAGE;
            byte[] message_encoded = Encoding.ASCII.GetBytes(msg);

            socket.Send(message_encoded);
        }
    }
}
