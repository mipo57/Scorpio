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
     * Example server application
     */
    public class Server
    {
        NetworkSocket _socket = null;

        /*
         * Start server on a given adress
         */
        public void Start()
        {
            _socket = NetworkSocket.InitializeProtocol();

            string ip = "0.0.0.0";
            int port = 0;

            while (true)
            {
                Console.WriteLine("Enter host ip:");
                ip = Console.ReadLine();
                Console.WriteLine("Enter host port:");
                string port_str = Console.ReadLine();

                if (int.TryParse(port_str, out port))
                {
                    break;
                }

                Console.WriteLine("Ports needs to be a number!");
            }

            try
            {
                _socket.ListenForConnections(ip, port, OnNewConnection);
            }
            catch
            {
                Console.WriteLine("Connection to {ip}:{port} failed, quitting");
                return;
            }
        }

        /* 
         * Event triggered on every new client connected to server
         */
        public void OnNewConnection(NetworkSocket socket)
        {
            socket.ProcessRequests(OnMessageReceived);
            Console.WriteLine($"New Connection: {socket.ConnectedIP}:{socket.ConnectedPort}!");
        }

        /*
         * Event triggered on every new message recived
         */
        public void OnMessageReceived(NetworkMessage message, NetworkSocket connection_socket)
        {
            string request = message.GetElementString("request");

            if (request != null)
            {
                Console.WriteLine($"{connection_socket.ConnectedIP}:{connection_socket.ConnectedPort} requests {request}");

                switch (request)
                {
                    case "add":
                        try
                        {
                            int x = message.GetElementInt32("x");
                            int y = message.GetElementInt32("y");
                            Console.WriteLine($"Result of adding {x} to {y} is {x + y}");

                            NetworkMessage return_message = new NetworkMessage();
                            return_message.AttachString((x + y).ToString(), "result");

                            connection_socket.Send(return_message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error processing request '{request}'");
                        }

                        break;
                    case "author":
                        try
                        {
                            NetworkMessage return_message = new NetworkMessage();
                            return_message.AttachString("Michał Pogoda", "result");

                            connection_socket.Send(return_message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error processing request '{request}'");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
