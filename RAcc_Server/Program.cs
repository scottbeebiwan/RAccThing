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
            NSStringWrite(ns, "MGV_NAME;"); //GiVe NAME
            string name = NSReadUntil(ns, ';');
            NSStringWrite(ns, "MWT_FUSR;"); //WaiT_For USeR
            if (ConsoleYN(name+" wants to connect. Let them? (Y/N) "))
            {
                NSStringWrite(ns, "MOK;"); //OK
            } else {
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
                    string instruction = NSReadUntil(ns, ';');
                    string[] sp_instruction = instruction.Split(',');
                    RunInstruction(sp_instruction, ns, addresses[chosenip]);
                }
                else
                {
                    char got = (char)rdbyte; //turn this int into a char
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
                Console.Write("Screenshot Requested...");
                var screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height,
                    PixelFormat.Format32bppArgb); // New screen
                var screenshot_obj = Graphics.FromImage(screenshot); //New gfx object
                screenshot_obj.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0,
                    Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy); //Copy from screen
                byte[] png;
                MemoryStream pngstream = new MemoryStream();
                screenshot.Save(pngstream, ImageFormat.Png); //save to stream
                png = pngstream.ToArray();
                MemoryStream r_pngstream = new MemoryStream(png); //Reverse_pngstream
                // initate file transfer on 4001
                FileTransfer(ns, 4001, ip, "screenshot.png", r_pngstream);
            }
            if (instruction[0]=="DIR")
            {

            }
        }
        static void FileTransfer(NetworkStream ns, int port, IPAddress ip, string filename, MemoryStream filecontent)
        {
            TcpListener sft = new TcpListener(ip, port);
            NSStringWrite(ns, $"MGF_PLS,{port},{filename},{filecontent.Length};");
            Console.Write("Waiting for acceptance...");
            string resp = NSReadUntil(ns, ';');
            switch (resp)
            {
                case "ROK":
                    break;
                case "RNO":
                    Console.Write("\nClient won't accept file transfer.\n");
                    return;
                default:
                    Console.Write("Server confused!\n");
                    NSStringWrite(ns, "MWHAT;");
                    return;
            }
            sft.Start();
            Console.Write("Waiting for connection...");
            TcpClient sftc = sft.AcceptTcpClient();
            NetworkStream sfts = sftc.GetStream();
            Console.Write("Sending bytes\n");
            while (filecontent.Position < filecontent.Length)
            {
                Console.Write($"{filecontent.Position} / {filecontent.Length}\r");
                sfts.WriteByte((byte)filecontent.ReadByte());
            }
            Console.Write("\n");
            Console.Write("Closing transfer connection");
            sfts.Close();
            sftc.Close();
            sft.Stop();
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
        static string NSReadUntil(NetworkStream ns, char until)
        {
            int rdbyte;
            string read = "";
            bool escaping = false; bool looping = true;
            while (looping)
            {
                try { rdbyte = ns.ReadByte(); } // read one byte
                catch (IOException) { return read; } // forced disconnect
                if ((char)rdbyte == '\\') // Escape
                {
                    escaping = true;
                    try { rdbyte = ns.ReadByte(); } // read one byte
                    catch (IOException) { return read; } // forced disconnect
                }
                if ((char)rdbyte == ';' && !escaping) //; ends name
                {
                    looping = false;
                }
                else
                {
                    read += (char)rdbyte;
                }
                escaping = false;
            }
            return read;
        }
    }
}
