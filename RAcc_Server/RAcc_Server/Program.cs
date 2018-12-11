using System;
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
            Console.WriteLine("    RAccThing Server -- Written by ScottBeebiWan");
            Console.WriteLine("    Configuring socket...");
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            int chosenip = 0;
            if (ipHostInfo.AddressList.Length > 0)
            {
                char i = '0';
                List<char> validans = new List<char>();
                Console.WriteLine("[?] Which IP do you want to listen on?");
                foreach (var ip in ipHostInfo.AddressList)
                {
                    i++;
                    validans.Add(i);
                    Console.WriteLine(" "+i.ToString()+": "+ip.ToString());
                }
                char ans = Choose(validans);
                chosenip = validans.FindIndex(item => item == ans);
            }
            IPAddress ipAddress = ipHostInfo.AddressList[chosenip];
        }
        static List<int> Range(int start, int stop)
        {
            int i;
            List<int> returnme = new List<int>();
            for (i=start; i==stop; i++)
            {
                returnme.Add(i);
            }
            return returnme;
        }
        static char Choose(List<char> validans)
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
