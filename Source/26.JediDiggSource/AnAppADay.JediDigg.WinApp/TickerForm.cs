using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ShellLib;

namespace AnAppADay.JediDigg.WinApp
{

    public partial class TickerForm : ApplicationDesktopToolbar
    {

        private DiggStory _diggStory;
        internal bool _pause;

        public TickerForm()
        {
            InitializeComponent();
            Edge = AppBarEdges.Top;
        }

        internal void SetStory(DiggStory story)
        {
            _diggStory = story;
            Invoke(new MethodInvoker(InternalSetStory));
        }

        private void InternalSetStory()
        {
            label1.Text = _diggStory.title;
            if (label1.Visible == false)
            {
                label1.Visible = true;
                label2.Visible = true;
                label3.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
            }
            if (_diggStory.diggCheck != null)
            {
                pictureBox1.Visible = true;
                label4.Visible = false;
            }
            else
            {
                pictureBox1.Visible = false;
                label4.Visible = true;
            }
            label3.Text = _diggStory.diggs;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                Program.DiggStory(_diggStory);
                pictureBox1.Visible = false;
                label4.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + Environment.NewLine + "Perhaps digg changed the page, which breaks the parsing..." + Environment.NewLine + "Will retry in 30 seconds", "Jedi Digg Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void bf_FormClosed(object sender, FormClosedEventArgs e)
        {
            _pause = false;
        }

        private void appaday_click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        private void opendigg_Click(object sender, EventArgs e)
        {
            try
            {
                _pause = true;
                BrowserForm bf = new BrowserForm();
                bf.OpenUrl(_diggStory.diggUrl);
                bf.Show();
                bf.FormClosed += new FormClosedEventHandler(bf_FormClosed);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unhandled error opening a browser: " + ex.Message, "Jedi Digg Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                _pause = true;
                BrowserForm bf = new BrowserForm();
                if (e.Button == MouseButtons.Left)
                {
                    bf.OpenUrl(_diggStory.url);
                }
                else
                {
                    bf.OpenUrl(_diggStory.diggUrl.Replace("digg.com", "duggmirror.com"));
                }
                bf.Show();
                bf.FormClosed += new FormClosedEventHandler(bf_FormClosed);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unhandled error opening a browser: "+ex.Message, "Jedi Digg Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label6_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Clipboard.SetText(_diggStory.url);
            }
            else
            {
                Clipboard.SetText(_diggStory.diggUrl);
            }
        }

    }

}