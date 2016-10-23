using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace scorpio_lib
{
    public enum ConnectionType { UDP, TCP }
    public class NetworkSocket
    {
        Socket _socket = null;

        List<NetworkSocket> _connected_sockets = new List<NetworkSocket>();

        bool _listening_for_connections = false;
        bool _processing_messages = false;

        public delegate void NewConnectionCallback(NetworkSocket socket);
        public delegate void NewMessageRecivedCallback(NetworkMessage message, NetworkSocket connection_socket);

        public string ConnectedIP
        {
            get
            {
                if (_socket == null)
                    return "0.0.0.0";

                return (_socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            }
        }

        public int ConnectedPort
        {
            get
            {
                if (_socket == null)
                    return -1;

                return (_socket.RemoteEndPoint as IPEndPoint).Port;
            }
        }

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
            if (_socket == null)
                return false;

            try
            {
                _socket.Connect(IPAddress.Parse(server_adress), port);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while trying to connect to {server_adress}:{port} - {e.Message}");
                return false;
            }

            if (_socket.Connected)
            {
                Console.WriteLine($"Connected to {server_adress}:{port}");
                return true;
            }
            else
            {
                Console.WriteLine($"Warning: Connection to {server_adress}:{port} failed!");
                return false;
            }
        }

        /*
         * Asynchonously wait for new connections and fire a callback when a new connection occurs
         */
        public void ListenForConnections(string IP, int port, NewConnectionCallback new_connection_callback, uint ping_timestamp = 500, int max_pending_connections = 1)
        {
            if (_socket == null)
                return;

            try
            {
                _socket.Bind(new IPEndPoint(IPAddress.Parse(IP), port));
                _socket.Listen(max_pending_connections);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error occured while trying to setup server on {IP}:{port} - {e.Message}");
                return;
            }

            _listening_for_connections = true;

            // save every connection socket and fire callback when new connection occurs
            Task.Run(() =>
            {
                while (_listening_for_connections)
                {
                    NetworkSocket new_connection = new NetworkSocket( _socket.Accept() );
                    lock (_connected_sockets)
                    {
                        _connected_sockets.Add(new_connection);
                    }
                    new_connection_callback(new_connection);
                }
            });

            // ping to make sure all connections are active
            Task.Run( () =>
            {
                while (_listening_for_connections)
                {
                    Thread.Sleep((int)ping_timestamp);
                    lock (_connected_sockets)
                    {
                        List<NetworkSocket> sockets_to_remove = new List<NetworkSocket>();
                        foreach (NetworkSocket socket in _connected_sockets)
                        {
                            if (!socket.Ping())
                            {
                                socket.Disconnect();
                                Console.WriteLine("Socket Disconnected!");

                                sockets_to_remove.Add(socket);
                            }
                        }

                        // remove inactive sockets
                        foreach (NetworkSocket socket in sockets_to_remove)
                            _connected_sockets.Remove(socket);
                    }
                }
            });
        }

        /* 
         * Stops listening for connections. Also stops 
         */
        public void StopListeningForConnections()
        {
            _listening_for_connections = false;
        }

        public void ProcessRequests(NewMessageRecivedCallback msg_recived_callback, int buffer_size = 4096)
        {
            if (buffer_size < 2)
                throw new Exception("NetworkSocket::ProcessRequests: Buffer size is too small, needs to be larger than 2 bytes");

            _processing_messages = true;

            Task.Run(() =>
           {
               byte[] stack_buffer = new byte[buffer_size];
               int message_size = -1;
               bool is_receiving_messege = false;

               while (_processing_messages)
               {
                   byte[] buffer = new byte[buffer_size];

                   // wait for new data stream
                   try
                   {
                       int bytes_recived = _socket.Receive(buffer);
                       buffer = buffer.Take(bytes_recived).ToArray();
                   }
                   catch (SocketException e)
                   {
                       Disconnect();

                       return;
                   }

                   // process received data 
                   while (buffer.Length > 0)
                   {
                       // search for header if not currently receiving message
                       if (!is_receiving_messege)
                       {
                           // search received data for valid messages
                           NetworkMessage.HeaderInfo header_info = NetworkMessage.SearchForMessage(buffer);

                           // trim useless data
                           if (header_info.data_start_index > 0)
                               buffer = buffer.Skip(header_info.data_start_index).ToArray();

                           // start receiving data if full header was found in buffer
                           if (header_info.return_code == NetworkMessage.HeaderInfo.ReturnCode.HeaderFound)
                           {
                               message_size = header_info.data_size;

                               stack_buffer = buffer;
                               buffer = buffer.Skip(message_size).ToArray();

                               is_receiving_messege = true;
                           }
                           // request more data if found a part of header at the end of buffer
                           else if (header_info.return_code == NetworkMessage.HeaderInfo.ReturnCode.HeaderFoundPartially)
                           {
                               byte[] tmp_buffer = new byte[buffer_size / 2];
                               int tmp_bytes_received = -1;
                               try
                               {
                                   tmp_bytes_received = _socket.Receive(tmp_buffer);
                               }
                               catch (Exception e)
                               {
                                   Disconnect();

                                   return;
                               }
                               buffer = buffer.Concat(tmp_buffer.Take(tmp_bytes_received)).ToArray();
                           }
                           // get rid of whole buffer away if nothing was found
                           else if (header_info.return_code == NetworkMessage.HeaderInfo.ReturnCode.HeaderNotFound)
                           {
                               buffer = new byte[0];
                           }
                       }
                       // receive data if downloading message is in progress
                       else
                       {
                           int skip_size = message_size - stack_buffer.Length;

                           stack_buffer = stack_buffer.Concat(buffer).ToArray();
                           buffer = buffer.Skip(skip_size).ToArray();
                       }

                       // trigger event if whole message was downloaded
                       if (stack_buffer.Length >= message_size && is_receiving_messege)
                       {
                           msg_recived_callback(new NetworkMessage(stack_buffer.Take(message_size).ToArray()), this);
                           message_size = -1;
                           stack_buffer = new byte[0];
                           is_receiving_messege = false;
                       }
                   }
               }
           });
        }

        /*
         * Stops processing requests
         */
        public void StopProcessingRequests()
        {
            _processing_messages = false;
        }

        /* 
         * Send messege to other endpoint
         */
        public void Send(NetworkMessage message)
        {
            _socket?.Send(message.GetSerialized());
        }

        /*
         * Terminate connection
         */
        public void Disconnect()
        {
            StopListeningForConnections();
            StopProcessingRequests();

            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();

            _socket = null;
        }

        /*
         * Ping other endpoint. Returns true if connection is still online, false otherwise
         */
        public bool Ping()
        {
            if (_socket == null)
                return false;

            byte[] ping_message = Encoding.ASCII.GetBytes("<PING>");

            try
            {
                _socket.Send(ping_message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning - Trying to ping resulted in {e.Message}!");

                return false;
            }

            return _socket.Connected;
        }
    }
}
