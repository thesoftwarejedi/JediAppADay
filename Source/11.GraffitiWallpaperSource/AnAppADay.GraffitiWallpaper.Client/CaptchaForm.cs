using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AnAppADay.GraffitiWallpaper.Shared;

namespace AnAppADay.GraffitiWallpaper.Client
{
    public partial class CaptchaForm : Form
    {
        private CaptchaInfo _info;

        public CaptchaForm()
        {
            InitializeComponent();
        }

        public CaptchaInfo CurrentCaptchaInfo
        {
            get { return _info; }
            set { _info = value; }
        }

        public string CaptchaGuess
        {
            get { return textBox1.Text; }
        }

        private void CaptchaForm_Load(object sender, EventArgs e)
        {
            using (MemoryStream ms = new MemoryStream(_info.image))
            {
                pictureBox1.Image = Image.FromStream(ms);
                pictureBox1.Size = pictureBox1.Image.Size;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}