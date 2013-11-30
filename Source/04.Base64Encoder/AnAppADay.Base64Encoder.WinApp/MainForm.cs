using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.Base64Encoder.WinApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fs = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    MemoryStream ms = new MemoryStream();
                    byte[] buffer = new byte[4096];
                    int len = fs.Read(buffer, 0, 4096);
                    while (len > 0)
                    {
                        ms.Write(buffer, 0, len);
                        len = fs.Read(buffer, 0, 4096);
                    }
                    string base64 = Convert.ToBase64String(ms.ToArray());
                    textBox2.Text = base64;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error encoding: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = @"<img src=""data:image/png;base64," + textBox2.Text + @""" />";

        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(1)) {
                textBox2.SelectAll();
                e.Handled = true;
            }
        }
    }
}