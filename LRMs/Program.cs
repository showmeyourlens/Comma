using System;
using ToolsLibrary;

namespace LRMs
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LRMCommunicator communicator = new LRMCommunicator(args[0]);
                communicator.Start();
                TimeStamp.WriteLine("LRMs working.");
                Console.WriteLine(String.Format("{0} choose link from 1 to {1} to be DEVASTATED by MONSTROUS DIGGER", TimeStamp.TAB, communicator.LRMs.Count));
                char c = Console.ReadKey().KeyChar;
                NetworkPackage networkPackage = new NetworkPackage(
                    "LRMs",
                    "Cloud",
                    Command.Break_The_Link,
                    c.ToString()
                    );
                communicator.Send(networkPackage);
                Console.WriteLine();
                TimeStamp.WriteLine("Link {0} destroyed. MWAHAHAAHAHA!", c.ToString());
                Console.WriteLine("{0} CC is not informed yet. Press anything to acknowledge", TimeStamp.TAB);
                Console.ReadKey().KeyChar.ToString();
                communicator.AlarmCC(Int32.Parse(c.ToString()));
                Console.WriteLine();
                Console.WriteLine("Press anything to close");
                Console.ReadKey();
                communicator.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();

        }
    }
}
