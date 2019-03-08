using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RAcc_Client
{
    public partial class Connected : Form
    {
        private string ip2Connect;
        private string usrname;
        private NetworkStream cnnctn;

        public Connected(string ip1Connect, string uname)
        {
            ip2Connect = ip1Connect;
            usrname = uname;
            InitializeComponent();
        }

        private void Connected_Load(object sender, EventArgs e)
        {
            TcpClient tcpc = new TcpClient(ip2Connect, 4000);
            cnnctn = tcpc.GetStream();
            Draw();
            while (GStatus() != "Ready")
            {
                string msg = GetMsg(cnnctn);
                string op = ProcessMsg(msg);
                if (op == "OK") { return; }
                if (op != "") { NSStringWrite(cnnctn, op); }
                Draw();
            }
        }

        private void Connected_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private string GetMsg(NetworkStream ns)
        {
            string outp = "";
            bool reading = true;
            int rdbyte = 0;
            while (reading)
            {
                try { rdbyte = cnnctn.ReadByte(); }
                catch (IOException) { return "MDISCONNECT"; }
                reading = ((char)rdbyte != ';');
                if (reading) { outp += (char)rdbyte; }
            }
            return outp;
        }

        private string ProcessMsg(string msg)
        {
            switch (msg)
            {
                case "MGV_NAME":
                    return $"{usrname};";
                case "MWT_FUSR":
                    Status("Waiting for server confirmation");
                    Draw();
                    break;
                case "MOK":
                    Status("Ready");
                    Draw();
                    return "OK";
                case "MWHAT":
                    Status("Server confused");
                    Draw();
                    MessageBox.Show("This client and server are probably incompatible. Server confused");
                    Application.Exit();
                    break;
                default:
                    if (msg.StartsWith("MGF_PLS"))
                    {
                        Status("File transfer started"); Draw();
                        NSStringWrite(cnnctn, "ROK;");
                        string[] splt = msg.Split(',');
                        int port = Convert.ToInt16(splt[1]);
                        int length = Convert.ToInt32(splt[3]);
                        string filename = splt[2];
                        // try until accepted
                        Status("Recieving file: Trying to connect"); Draw();
                        TcpClient rtcpc;
                        try { rtcpc = new TcpClient(ip2Connect, port); }
                        catch (SocketException)
                        {
                            Status("Recieving file: Trying again"); Draw();
                            System.Threading.Thread.Sleep(500); rtcpc = new TcpClient(ip2Connect, port);
                        }
                        NetworkStream ftns = rtcpc.GetStream();
                        Status("Recieving data"); Draw();
                        if (filename == "screenshot.png")
                        {
                            File.WriteAllBytes(filename, NSRcv(ftns, length));
                            System.Diagnostics.Process.Start(filename);
                        } else
                        {
                            List<byte> read = new List<byte>();
                            bool looping = true;
                            while (looping)
                            {
                                try { byte a = (byte)ftns.ReadByte(); if ((int)a > -1) { read.Add(a); } else { looping = false; } }
                                catch (IOException) { looping = false; }
                            }
                            File.WriteAllBytes(filename, read.ToArray());
                        }
                        ftns.Close();
                        return "";
                    }
                    MessageBox.Show($"Unexpected message: \n{msg}");
                    Application.Exit();
                    break;
            }
            return "";
        }

        private void Status(string status) => labelStatus.Text = $"Status: {status}";
        private string GStatus() { return labelStatus.Text.Substring(7); }
        private void Draw() { Application.DoEvents(); Application.DoEvents(); Application.DoEvents(); }

        private void button1_Click(object sender, EventArgs e)
        {
            NSStringWrite(cnnctn,"ISS;");
            while (GStatus() != "Ready")
            {
                string msg = GetMsg(cnnctn);
                string op = ProcessMsg(msg);
                if (op == "OK") { return; }
                if (op != "") { NSStringWrite(cnnctn, op); }
                Draw();
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

        static byte[] NSRcv(NetworkStream ns, int length) //NetworkStream File Transfer Recieve
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i > length; i++) {
                bytes.Add((byte)ns.ReadByte());
            }
            return bytes.ToArray();
        }
    }
}
