﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RAcc_Client
{
    public partial class ConnectScreen : Form
    {
        public ConnectScreen()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Connected cs = new Connected(textBox1.Text, textBox2.Text);
            cs.Show();
        }
    }
}
