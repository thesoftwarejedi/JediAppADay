using System;
using System.Management;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//using System.Diagnostics;

namespace AnAppADay.MooPrank.SendMoo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string copyLocation = @"\\"+txtRemoteMachine1.Text+@"\c$\";
            if (MessageBox.Show("This will create files " + copyLocation + "AnAppADay.MooPrank.Moo.exe" + copyLocation + "AnAppADay.MooPrank.WavLibMixer.dll; ", "Copy confirm", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                try
                {
                    File.Copy("AnAppADay.MooPrank.Moo.exe", copyLocation + "AnAppADay.MooPrank.Moo.exe");
                    File.Copy("AnAppADay.MooPrank.WaveLibMixer.dll", copyLocation + "AnAppADay.MooPrank.WaveLibMixer.dll");
                } catch (Exception ex) {
                    MessageBox.Show("Copy failed: "+ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string path = null;
                string exe = null;
                string exeAndPath = null;
                if (txtNetworkLocation.Text.Trim() == "")
                {
                    sb.Append(@"c:\");
                }
                else
                {
                    sb.Append(txtNetworkLocation.Text);
                    sb.Append(@"\");
                }
                path = sb.ToString();
                sb = new StringBuilder();
                sb.Append("AnAppADay.MooPrank.Moo.exe");
                if (txtMin.Text.Trim() != "" && txtMax.Text.Trim() != "")
                {
                    sb.Append(" ");
                    sb.Append(txtMin.Text);
                    sb.Append(" ");
                    sb.Append(txtMax.Text);
                }
                exe = sb.ToString();
                exeAndPath = path + exe;
                ManagementClass po = new ManagementClass(@"\\" + txtRemoteMachine2.Text + @"\root\cimv2:Win32_Process");
                po.InvokeMethod("Create", new object[] { exeAndPath, null, null, null });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unknown Error: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}