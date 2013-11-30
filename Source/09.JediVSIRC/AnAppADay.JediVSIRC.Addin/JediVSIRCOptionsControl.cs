using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.Win32;

namespace AnAppADay.JediVSIRC.Addin
{
    
    public partial class JediVSIRCOptionsControl : UserControl, IDTToolsOptionsPage
    {

        public JediVSIRCOptionsControl()
        {
            InitializeComponent();
        }

        public void GetProperties(ref object PropertiesObject)
        {
        }

        public void OnAfterCreated(DTE DTEObject)
        {
            //read registry and set UI
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            key = key.CreateSubKey(@"AnAppADay\JediVSIRC");
            textBox1.Text = key.GetValue("server", "irc.diboo.net").ToString();
            try
            {
                textBox2.Text = key.GetValue("port", "6667") as string;
            }
            catch
            {
                textBox2.Text = "6667";
            }
            textBox3.Text = key.GetValue("nick", "Anony") as string;
            textBox4.Text = key.GetValue("realName", "Anony") as string;
            textBox5.Text = key.GetValue("channel", "#softwarejedianappaday") as string;
            key.Close();
        }

        public void OnCancel()
        {
        }

        public void OnHelp()
        {
        }

        public void OnOK()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software", true);
                key = key.CreateSubKey(@"AnAppADay\JediVSIRC");
                key.SetValue("server", textBox1.Text, RegistryValueKind.String);
                key.SetValue("port", textBox2.Text, RegistryValueKind.String);
                key.SetValue("nick", textBox3.Text, RegistryValueKind.String);
                key.SetValue("realName", textBox4.Text, RegistryValueKind.String);
                key.SetValue("channel", textBox5.Text, RegistryValueKind.String);
                key.Close();

                JediVSIRCChatControl.LoadAndStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Jedi VS IRC Error: " + ex.Message);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

    }

}
