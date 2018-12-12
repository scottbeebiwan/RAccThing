using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                Console.WriteLine(" " + i.ToString() + ": " + ip.ToString()); //show that choice
            }
            char ans = Choose(validans); //choose from list 
            chosenip = validans.FindIndex(item => item == ans); //get the index of what was chosen
            TcpListener listener = new TcpListener(addresses[chosenip], 4000); // set up a listener waiting on the chosen ip
            listener.Start(); // start
            Console.WriteLine("\nServer started on " + addresses[chosenip] + ", port 4000");
            TcpClient tcpc = listener.AcceptTcpClient(); // wait for a client
            Console.WriteLine("Client connected.\n");
            NetworkStream ns = tcpc.GetStream(); // make stream
            bool connected = true; // this will be set false and connection will close if auth fails
            int rdbyte = 0; // initialize byte reading variable
            bool authing = true; // setting this to false will exit the auth loop
            NSStringWrite(ns, "MGV_NAME;"); //GiVe NAME
            string name = "";
            while (authing)
            {
                try { rdbyte = ns.ReadByte(); } // read one byte
                catch (IOException) { rdbyte = -2; } // forced disconnect
                if ((char)rdbyte == ';') //; ends name
                {
                    authing = false;
                }
                else
                {
                    name += (char)rdbyte;
                }
            }
            NSStringWrite(ns, "MWT_FUSR;"); //WaiT_For USeR
            if (ConsoleYN(name+" wants to connect. Let them? (Y/N) "))
            {
                NSStringWrite(ns, "MOK;"); //OK
            } else {
                NSStringWrite(ns, "MGTFO;"); //guess. just guess. what that means.
                connected = false;
            }
            while (connected)
            {
                try { rdbyte = ns.ReadByte(); } // read one byte
                catch (IOException) { rdbyte = -2; } // forced disconnect
                if (rdbyte < 0) // -1 means disconnected
                {
                    connected = false; //while will exit after this loop
                    listener.Stop();
                }
                else if ((char)rdbyte == 'I') // instruction: I[???][,arg];
                {
                    string instruction = "";
                    bool reading_instruction = true;
                    while (reading_instruction)
                    {
                        rdbyte = ns.ReadByte();
                        if (rdbyte == -1) { connected = false; reading_instruction = false; }
                        else if ((char)rdbyte == ';') { reading_instruction = false; }
                        if (reading_instruction) { instruction += (char)rdbyte; }
                    }
                    string[] sp_instruction = instruction.Split(',');
                    RunInstruction(sp_instruction, ns, addresses[chosenip]);
                }
                else
                {
                    char got = (char)rdbyte; //turn this byte into a char so it isnt gibberish
                    Console.Write(got); // and just write it to the screen
                }
            }
            ns.Close(500); // clean up your room, mister!
            tcpc.Close();
            listener.Stop();
            if (rdbyte == -1) {
                Console.WriteLine("\nDisconnected.\nPress any key to exit...");
            } else if (rdbyte == -2)
            {
                Console.WriteLine("\nForcefully disconnected.\nPress any key to exit...");
            }
            Console.ReadKey(); // wait for a keypress
        }
        static List<int> Range(int start, int stop) // basically a copy of python range()
        {
            int i;
            List<int> returnme = new List<int>();
            for (i = start; i == stop; i++)
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
                Console.WriteLine(string.Join(", ", validans.ToArray()));
                return Choose(validans);
            } else
            {
                return red;
            }
        }
        static bool ConsoleYN(string usrask) //Console Yes No
        {
            Console.WriteLine(usrask);
            List<char> yesno = new List<char>(); //yes or no char list
            yesno.Add('y'); yesno.Add('n');
            return (Choose(yesno) == 'y');
        }
        static void RunInstruction(string[] huh, NetworkStream ns, IPAddress ip)
        {
            List<string> instruction = huh.ToList();
            if (instruction[0]=="SS") //ScreenShot
            {
                Console.WriteLine("Screenshot Requested");
                var screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height,
                    PixelFormat.Format32bppArgb); // New screen
                var screenshot_obj = Graphics.FromImage(screenshot); //New gfx object
                screenshot_obj.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0,
                    Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy); //Copy from screen
                // initate file transfer on 4001
                TcpListener sft = new TcpListener(ip, 4001);
                sft.Start();
                NSStringWrite(ns, "MGF_PLS,4001;"); //Get File_PLeaSe, port 4001
                TcpClient sftc = sft.AcceptTcpClient();
                var sftstream = sftc.GetStream();
                screenshot.Save(sftstream, ImageFormat.Png); //save to string
                sftstream.Close(500); //clean up your mess young man
                sftc.Close();
                sft.Stop();
            }
        }
        static void NSStringWrite(NetworkStream ns, string wts) //write a string to the network stream without fuss
        {
            List<byte> bytes = new List<byte>();
            foreach (var a_char in wts.ToCharArray())
            {
                bytes.Add((byte)a_char);
            }
            byte[] sendme = bytes.ToArray();
            ns.Write(sendme, 0, sendme.Length);
        }
    }
}
