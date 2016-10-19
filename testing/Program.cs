using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace testing
{
    class Program
    {
        static public void LongLoop()
        {
            for (int i = 0; i < 10000; i++)
            {
                Console.WriteLine($"Line number {i}");
            }
        }

        static void Main(string[] args)
        {
            CancellationTokenSource ct = new CancellationTokenSource();

            Task.Run(() => { if (!ct.Token.IsCancellationRequested) { LongLoop(); } Console.WriteLine("Task ended"); }, ct.Token);
            ct.Cancel(false);

            Console.Read();
        }
    }
}
