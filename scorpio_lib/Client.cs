using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace scorpio_lib
{
    /*
     * Example client application
     */ 
    public class Client
    {
        public NetworkSocket socket = null;

        public void Start()
        {
            string ip = "0.0.0.0";
            int port = 0;

            while (true)
            {
                Console.WriteLine("Enter destination ip:");
                ip = Console.ReadLine();
                Console.WriteLine("Enter destination port:");
                string port_str = Console.ReadLine();

                if (int.TryParse(port_str, out port))
                {
                    break;
                }

                Console.WriteLine("Ports needs to be a number!");
            }
            


            if ( !Connect(ip, port) )
            {
                Console.WriteLine("Connection to {ip}:{port} failed, quitting");
                return;
            }

            while (true)
            {
                string request = Console.ReadLine();
                string[] tokens = request.Split(' ');

                if (tokens.Length < 1)
                    continue;

                switch (tokens[0])
                {
                    case "add":
                        if (tokens.Length != 3)
                            Console.WriteLine("Wrong usage of add command. Two parameters of type int requied (example: add 1 2)");

                        try
                        {
                            int x = int.Parse(tokens[1]);
                            int y = int.Parse(tokens[2]);

                            NetworkMessage msg = new NetworkMessage();
                            msg.AttachString("add", "request");
                            msg.AttachInt32(x, "x");
                            msg.AttachInt32(y, "y");

                            socket.Send(msg);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error occurred when trying to use add command!");
                        }
                        break;
                    case "author":
                        if (tokens.Length != 1)
                            Console.WriteLine("Wrong usage of author command. No parameters requied");

                        try
                        {
                            NetworkMessage msg = new NetworkMessage();
                            msg.AttachString("author", "request");

                            socket.Send(msg);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error occurred when trying to use author command!");
                        }
                        break;
                    case "quit":
                        goto End;
                        break;
                    default:
                        Console.WriteLine("Unrecognized command!");
                        break;
                }
            }

        End:;
        }

        public bool Connect(string ip, int port)
        {
            try
            {
                socket = NetworkSocket.InitializeProtocol();

                if (!socket.ConnectToServer(ip, port))
                    return false;                

                socket.ProcessRequests(OnReceivedMessage);
            }
            catch
            {
                Console.Write($"Problem while trying to connect to {ip}:{port}");
            }

            return true;
        }

        public void SendMessage(string message)
        {
            NetworkMessage msg = new NetworkMessage();
            msg.AttachString(message, "msg");

            socket.Send(msg);
        }

        public void OnReceivedMessage(NetworkMessage msg, NetworkSocket connection_socket)
        {
            string result = msg.GetElementString("result");

            if (result != null)
            {
                Console.WriteLine($"Result: {result}");
            }
        }

        public void Disconnect()
        {
            socket.Disconnect();
        }

    }
}
