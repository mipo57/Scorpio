using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;

namespace scorpio_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            Client client = new Client();

            Task.Run(() => { server.Listen("127.0.0.1", 1024); });
            client.Connect("127.0.0.1", 1024);
            client.SendMessage("Ala ma kota");
            client.SendMessage("A kot ma Ale");
            client.Disconnect();

            Console.ReadKey();
        }
    }

    public class NetworkMessage
    {
        byte[] msg = new byte[1024];
        int stack_pos = 0;

        public bool AppendStringUnicode(string str)
        {
            byte[] msg_encoded = Encoding.Unicode.GetBytes(str);

            if (stack_pos + msg_encoded.Length <= 1024)
            {
                msg_encoded.CopyTo(msg, stack_pos);
                stack_pos += msg_encoded.Length;


                return true;
            }

            return false;
        }

        public bool AppendStringASCII(string str)
        {
            byte[] msg_encoded = Encoding.ASCII.GetBytes(str);

            if (stack_pos + msg_encoded.Length <= 1024)
            {
                msg_encoded.CopyTo(msg, stack_pos);
                stack_pos += msg_encoded.Length;

                return true;
            }

            return false;
        }

        public bool AppendUINT32(UInt32 value)
        {
            if (stack_pos + 4 <= 1024)
            {
                BitConverter.GetBytes(value).CopyTo(msg, stack_pos);
                stack_pos += 4;

                return true;
            }

            return false;
        }
    }

    public class Client
    {
        public Socket socket = null;

        public void Connect(string ip, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void SendMessage(string message)
        {
            string msg = message + "<EOM>";
            byte[] message_encoded = Encoding.ASCII.GetBytes(msg);

            socket.Send(message_encoded);
        }

        public void Disconnect()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(true);
        }
    }

    public class Connection
    {
        static void ProcessRequests(Socket socket)
        {
            byte[] buffer = new byte[1024];
            string message_queue = "";

            while (true)
            {
                int num_bytes_recived = socket.Receive(buffer);

                if (num_bytes_recived > 0)
                {
                    string msg = Encoding.ASCII.GetString(buffer, 0, num_bytes_recived);

                    string[] messages = msg.Split(new string[] { "<EOM>" }, StringSplitOptions.None);
                    messages[0] = message_queue + messages[0];
                    message_queue = messages.Last();
                    
                    foreach(string str in messages.Take(messages.Length - 1))
                    {
                        Console.WriteLine("Message from client {0}:{1} - {2}", (socket.RemoteEndPoint as IPEndPoint).Address, (socket.RemoteEndPoint as IPEndPoint).Port, str);
                    }

                    string return_msg = msg.ToUpper();
                    byte[] return_msg_en = Encoding.ASCII.GetBytes(return_msg);
                    socket.Send(return_msg_en);
                }
            }
        }

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
    }

    public class Server
    {
        public Socket socket = null;
        public List<Connection> active_connections = null;

        public void Listen(string adress, int port, int max_connections = 10)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(adress), port));

            socket.Listen(max_connections);

            while (true)
            {
                Socket new_connection = socket.Accept();
                Task.Run(() => { Connection.Run(new_connection); });
            }
        }
    }
}