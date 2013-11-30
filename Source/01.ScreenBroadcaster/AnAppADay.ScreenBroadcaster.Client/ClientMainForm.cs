using System;
using System.Threading;
using System.IO;
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

namespace AnAppADay.ScreenBroadcaster.Client
{
    public partial class ClientMainForm : Form
    {

        HttpClientChannel _cnl;
        IBroadcastServer _server;
        Thread _thread;

        delegate void SoftwareJediIsCool(Image o);
        SoftwareJediIsCool _mi;

        public ClientMainForm()
        {
            InitializeComponent();
            _mi = new SoftwareJediIsCool(SetImage);
        }

        private void DrawImage()
        {
            try
            {
                while (_server != null)
                {
                    byte[] bytes = _server.GetScreen();
                    Image i = null;
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        i = Bitmap.FromStream(ms);
                    }
                    Invoke(_mi, i);
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                CommonLib.HandleException(ex);
            }
        }

        private void SetImage(Image i)
        {
            pictureBox2.Image = i;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                button3.Enabled = false;
                Application.DoEvents();
                //register channel
                _cnl = new HttpClientChannel();
                ChannelServices.RegisterChannel(_cnl, false);
                //lookup remote
                string url = "http://" + textBox3.Text + ":" + textBox4.Text + "/AnAppADay.ScreenBroadcaster.Server";
                _server = (IBroadcastServer)Activator.GetObject(typeof(IBroadcastServer), url);
                _thread = new Thread(DrawImage);
                _thread.Start();
                button4.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonLib.HandleException(ex);
                button3.Enabled = true;
                button4.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                button4.Enabled = false;
                Application.DoEvents();
                _server = null;
                ChannelServices.UnregisterChannel(_cnl);
                _cnl = null;
                button3.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonLib.HandleException(ex);
                button3.Enabled = false;
                button4.Enabled = true;
            }
        }
    }
}