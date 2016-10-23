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

            server.Start("127.0.0.1", 1024);

            client.Connect("127.0.0.1", 1024);
            client.SendMessage("Ala ma kota");
            client.SendMessage("A kot ma Ale");
            client.Disconnect();

            Console.ReadKey();
        }
    }
}