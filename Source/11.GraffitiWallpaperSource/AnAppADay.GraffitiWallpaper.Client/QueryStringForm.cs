using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.GraffitiWallpaper.Client
{
    public partial class QueryStringForm : Form
    {
        public QueryStringForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public string Server
        {
            get { return textBox1.Text; }
        }

        public string Port
        {
            get { return textBox2.Text; }
        }
    }
}