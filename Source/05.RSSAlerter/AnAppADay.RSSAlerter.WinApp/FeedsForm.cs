using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using My5U.RSSReader;

namespace AnAppADay.RSSAlerter.WinApp
{
    public partial class FeedsForm : Form
    {
        private static FeedsForm _instance = new FeedsForm();

        private FeedsForm()
        {
            InitializeComponent();
        }

        public static FeedsForm Instance
        {
            get { return _instance; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                RssChannel c = RssParses.ProcessRSS(textBox1.Text);
                if (c.Items.Length > 0)
                {
                    if (c.Items[0].guid == null)
                    {
                        throw new Exception("Feeds with no GUIDs are not supported.  Contact the webmaster and whine to them.");
                    }
                }
                lock (FeedManager.feeds)
                {
                    FeedManager.feeds.AddLast(textBox1.Text);
                }
                listBox1.Items.Add(textBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding feed: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            } catch (Exception) {
                //nothing selected?
            }
        }

        protected override void OnShown(EventArgs e)
        {
            listBox1.Items.Clear();
            lock (FeedManager.feeds)
            {
                foreach (string feed in FeedManager.feeds)
                {
                    listBox1.Items.Add(feed);
                }
            }
            base.OnShown(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            lock (FeedManager.feeds)
            {
                FeedManager.feeds.Clear();
                foreach (string feed in listBox1.Items)
                {
                    FeedManager.feeds.AddLast(feed);
                }
                FeedManager.Save();
            }
            Hide();
            e.Cancel = true;
        }
    }
}