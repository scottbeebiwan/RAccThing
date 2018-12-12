﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RAcc_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console Config
            Console.Title = "RAccThing Server";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            //
            Console.WriteLine("    RAccThing Server -- Written by ScottBeebiWan");
            Console.WriteLine("    Configuring TCP Listener...");
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); // Get host names
            Console.WriteLine("[?] Which IP do you want to listen on?");
            int chosenip = 0; // setup for creating a list of options
            char i = '0'; List<char> validans = new List<char>(); 
            List<IPAddress> addresses = ipHostInfo.AddressList.ToList(); // make a list of host names from thyself
            addresses.Add(IPAddress.Parse("127.0.0.1")); // add localhost for testing as it isn't included
            foreach (var ip in addresses)
            {
                i++; //increase char (this works because the char gets turned into an int, added, then turned back into a char)
                validans.Add(i); //add that to the list
                Console.WriteLine(" "+i.ToString()+": "+ip.ToString()); //show that choice
            }
            char ans = Choose(validans); //choose from list 
            chosenip = validans.FindIndex(item => item == ans); //get the index of what was chosen
            TcpListener listener = new TcpListener(addresses[chosenip], 3693); // set up a listener waiting on the chosen ip
            listener.Start(); // start
            Console.WriteLine("\nWaiting for client...");
            TcpClient tcpc = listener.AcceptTcpClient(); // wait for a client
            Console.WriteLine("Client connected!\n");
            NetworkStream ns = tcpc.GetStream(); // make stream
            bool connected = true;
            while (connected)
            {
                int rdbyte = ns.ReadByte(); // read one byte
                if (rdbyte == -1) // -1 means disconnected
                {
                    connected = false; //while will exit after this loop
                    listener.Stop();
                }
                else
                {
                    char got = (char)rdbyte; //turn this byte into a char so it isnt gibberish
                    Console.Write(got); // and just write it to the screen
                }
            }
            Console.WriteLine("\nDisconnected.\nPress any key to exit...");  
            Console.ReadKey(); // wait for a keypress
        }
        static List<int> Range(int start, int stop) // basically a copy of python range()
        {
            int i;
            List<int> returnme = new List<int>();
            for (i=start; i==stop; i++)
            {
                returnme.Add(i);
            }
            return returnme;
        }
        static char Choose(List<char> validans) // epic chooser, pretty selfexplanatory
        {
            Console.Write(" ?> ");
            char red = Console.ReadKey().KeyChar;
            Console.Write("\n");
            bool acceptable = validans.Any(item => item == red);
            if (!acceptable)
            {
                Console.WriteLine("Unacceptable input! Valid inputs:");
                Console.WriteLine(string.Join(", ",validans.ToArray()));
                return Choose(validans);
            } else
            {
                return red;
            }
        }
    }
}
