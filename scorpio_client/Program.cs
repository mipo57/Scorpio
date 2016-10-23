using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scorpio_lib;

namespace scorpio_client
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.Start();
        }
    }
}
