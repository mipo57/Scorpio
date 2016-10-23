using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using scorpio_lib;

namespace scorpio_server
{
    class Program
    {
        static void Main()
        {
            Server server = new Server();

            server.Start();
        }
    }
}
