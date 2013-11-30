using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using AnAppADay.JediSpeak.Shared;

namespace AnAppADay.JediSpeak.Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HttpClientChannel cnl = new HttpClientChannel();
            try
            {
                //register channel
                ChannelServices.RegisterChannel(cnl, false);

                //lookup graffiti object
                string url = "http://" + textBox2.Text + ":8911/AnAppADay.JediSpeak.Server/JediSpeak";
                IJediSpeak speak = (IJediSpeak)Activator.GetObject(typeof(IJediSpeak), url);
                speak.Speak(textBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                ChannelServices.UnregisterChannel(cnl);
            }
        }
    }
}