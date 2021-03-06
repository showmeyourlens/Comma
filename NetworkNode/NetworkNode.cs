﻿using NetworkNode;
using System;
using System.Collections.Generic;
using CableCloud;

namespace CableCloud
{
    class NetworkNode
    {
             
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            NodeCloudCommunication cloudCommunicator = new NodeCloudCommunication(args[0], args[1], args[2]);

            Console.WriteLine("Starting network node with following parameters:");
            Console.WriteLine("Address on device: {0}:{1}", cloudCommunicator.instanceAddress, cloudCommunicator.instancePort);
            Console.WriteLine("Address in emulated network: {0}", cloudCommunicator.emulationNodeAddress);
            Console.WriteLine("Node identificator: {0}", cloudCommunicator.emulationNodeId);
            Console.WriteLine();
            Console.WriteLine("Press 's' to turn off/on router, 'c' to close");
            cloudCommunicator.Start();
            char key = 'a';
            key = Console.ReadKey().KeyChar;
            while(key != 'c')
            {
                cloudCommunicator.isRouterUp = !cloudCommunicator.isRouterUp;
                string message = cloudCommunicator.isRouterUp ? "Router turned on" : "Router turned off";
                Console.WriteLine(message);
                key = Console.ReadKey().KeyChar;
            }

            cloudCommunicator.Stop();
            Console.WriteLine("Closing");
            Console.ReadKey();
        }
    }
}
