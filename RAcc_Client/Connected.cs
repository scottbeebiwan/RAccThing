using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private IPAddress ip2Connect;

        public Connected(IPAddress ip1Connect)
        {
            ip2Connect = ip1Connect;
            InitializeComponent();
        }

        private void Connected_Load(object sender, EventArgs e)
        {
            IPEndPoint endPoint = new IPEndPoint(ip2Connect, 3693);
            Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(endPoint);
            }
            catch (ArgumentNullException ae)
            {
                MessageBox.Show("ArgumentNullException : " + ae.ToString());
            }
            catch (SocketException se)
            {
                MessageBox.Show("SocketException : " + se.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected exception : "+ ex.ToString());
            }
        }

        private void Connected_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
