using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AnAppADay.GraffitiWallpaper.Shared;

namespace AnAppADay.GraffitiWallpaper.Server
{

    public partial class Form1 : Form
    {

        HttpServerChannel _channel;

        public Form1()
        {
            InitializeComponent();
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(GraffitiController), "AnAppADay.GraffitiWallpaper.Server/GraffitiController", WellKnownObjectMode.Singleton);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Application.DoEvents();
            CreateAndStartChannel();
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            Application.DoEvents();
            StopChannel();
            button1.Enabled = true;
        }

        private void CreateAndStartChannel()
        {
            if (_channel != null)
            {
                throw new Exception("How did you make another channel when one is active?!");
            }
            _channel = new HttpServerChannel(Int32.Parse(textBox1.Text));
            _channel.StartListening(null);
        }

        private void StopChannel()
        {
            if (_channel == null)
            {
                throw new Exception("There is no active channel?!");
            }
            _channel.StopListening(null);
            _channel = null;
        }

    }

}