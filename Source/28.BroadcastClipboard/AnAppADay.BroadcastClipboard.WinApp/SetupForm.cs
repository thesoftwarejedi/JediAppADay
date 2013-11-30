using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AnAppADay.Utils;

namespace AnAppADay.BroadcastClipboard.WinApp
{

    public partial class SetupForm : Form
    {

        private IntPtr _nextCBHandler = IntPtr.Zero;
        private HandleRef _nextCBHandlerHRef;

        public SetupForm()
        {
            InitializeComponent();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            _nextCBHandler = WinApi.SetClipboardViewer(Handle);
            _nextCBHandlerHRef = new HandleRef(this, _nextCBHandler);
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WinApi.WindowsMessages)m.Msg)
            {
                case WinApi.WindowsMessages.WM_DRAWCLIPBOARD:
                    //clipboard contents changed
                    Program.SendClipboardData();
                    HandleRef r = new HandleRef(this, _nextCBHandler);
                    WinApi.SendMessage(_nextCBHandlerHRef, (uint)m.Msg, m.WParam, m.LParam);
                    break;
                case WinApi.WindowsMessages.WM_CHANGECBCHAIN:
                    //clipboard next handler changed
                    if (m.WParam == _nextCBHandler)
                    {
                        //process change
                        _nextCBHandler = m.LParam;
                    }
                    else
                    {
                        //forward it on
                        WinApi.SendMessage(_nextCBHandlerHRef, (uint)m.Msg, m.WParam, m.LParam);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("You must restart the application for these settings to take effect");
            Properties.Settings.Default.Save();
            Close();
        }

        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                IPAddress.Parse(textBox1.Text);
            }
            catch (Exception)
            {
                e.Cancel = true;
            }
        }

        private void textBox2_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                int.Parse(textBox2.Text);
            }
            catch (Exception)
            {
                e.Cancel = true;
            }
        }

        private void textBox3_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                short.Parse(textBox3.Text);
            }
            catch (Exception)
            {
                e.Cancel = true;
            }
        }

        internal void RemoveClipboardHandler()
        {
            WinApi.ChangeClipboardChain(Handle, _nextCBHandler);
        }
    }

}