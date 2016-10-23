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
        public delegate void NewMessageRecivedCallback(NetworkMessage message, NetworkSocket connection_socket);

        NetworkSocket() { }
        NetworkSocket(Socket socket)
        {
            _socket = socket;
        }
        
        /* 
         * Creates a new socket of specified protocol type
         */
        public static NetworkSocket InitializeProtocol(ConnectionType connection_type = ConnectionType.TCP)
        {
            Socket socket = null;

            if (connection_type == ConnectionType.TCP)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            return new NetworkSocket(socket);
        }

        /*
         * Tries to connect to server specified running on adress server_adress:port
         */
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

        /*
         * Asynchonously wait for new connections and fire a callback when a new connection occurs
         */
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

        public void ProcessRequests(NewMessageRecivedCallback msg_recived_callback)
        {
            Task.Run(() =>
           {
               byte[] stack_buffer = new byte[0];
               int message_size = -1;

               while (true)
               {
                   byte[] buffer = new byte[4096];
                   int bytes_recived = _socket.Receive(buffer);
                   buffer = buffer.Take(bytes_recived).ToArray();

                   while (buffer.Length > 0)
                   {
                       if (message_size - stack_buffer.Length <= 0)
                       {
                           NetworkMessage.HeaderInfo header_info = NetworkMessage.SearchForMessage(buffer);

                           if (header_info.return_code == NetworkMessage.HeaderInfo.ReturnCode.HeaderFound)
                           {
                               message_size = header_info.data_size;

                               stack_buffer = buffer.Skip(header_info.data_start_index).ToArray();
                               buffer = buffer.Skip(header_info.data_start_index + message_size).ToArray();
                           }
                           else if (header_info.return_code == NetworkMessage.HeaderInfo.ReturnCode.HeaderFoundPartially)
                           {
                               byte[] tmp_buffer = new byte[2048];
                               int tmp_bytes_received = _socket.Receive(tmp_buffer);
                               buffer = buffer.Concat(tmp_buffer.Take(tmp_bytes_received)).ToArray();
                           }
                       }
                       else
                       {
                           int skip_size = message_size - stack_buffer.Length;

                           stack_buffer = stack_buffer.Concat(buffer).ToArray();
                           buffer = buffer.Skip(skip_size).ToArray();
                       }


                       if (stack_buffer.Length >= message_size)
                       {
                           
                           msg_recived_callback(new NetworkMessage(stack_buffer.Take(message_size).ToArray()), this);
                           message_size = -1;
                           stack_buffer = new byte[0];
                       }
                   }
               }
           });
        }

        public void Send(NetworkMessage message)
        {
            _socket.Send(message.GetSerialized());
        }

        static void Ping(Socket socket)
        {
            byte[] ping_message = Encoding.ASCII.GetBytes("<PING>");
            socket.Send(ping_message);
        }
    }
}
