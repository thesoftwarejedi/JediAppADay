using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using AnAppADay.ScreenBroadcaster.Common;

namespace AnAppADay.ScreenBroadcaster.Server
{
    public partial class ServerMainForm : Form
    {
        HttpServerChannel _chnl;
        WellKnownServiceTypeEntry _entry;

        public ServerMainForm()
        {
            InitializeComponent();
            //register the broadcast server
            _entry = new WellKnownServiceTypeEntry(typeof(BroadcastServer), "AnAppADay.ScreenBroadcaster.Server", WellKnownObjectMode.SingleCall);
            RemotingConfiguration.RegisterWellKnownServiceType(_entry);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                Application.DoEvents();
                //create and start the channel
                _chnl = new HttpServerChannel(Int32.Parse(textBox1.Text));
                ChannelServices.RegisterChannel(_chnl, false);
                _chnl.StartListening(null);
                button2.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonLib.HandleException(ex);
                button1.Enabled = true;
                button2.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                button2.Enabled = false;
                Application.DoEvents();
                //stop the channel
                _chnl.StopListening(null);
                ChannelServices.UnregisterChannel(_chnl);
                _chnl = null;
                button1.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonLib.HandleException(ex);
                button1.Enabled = false;
                button2.Enabled = true;
            }
        }

        private bool announced = false;

        private void ServerMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            if (!announced)
            {
                announced = true;
                Program.ntfy.ShowBalloonTip(5000, "AnAppADay Screen Broadcaster", "I'm down here! To exit right click me and choose exit.", ToolTipIcon.Info);
            }
        }
    }
}