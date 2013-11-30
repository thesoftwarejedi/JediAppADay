using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using My5U.RSSReader;

namespace AnAppADay.RSSAlerter.WinApp
{
    static class Program
    {

        private static NotifyIcon _icon;
        private static Thread _thread;
        private static Dictionary<string, Dictionary<string, DateTime>> _feeds;
        static string _lastUrl;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _feeds = new Dictionary<string, Dictionary<string, DateTime>>();

            _icon = new NotifyIcon();
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.RSSAlerter.WinApp.Jedi.ico"))
            {
                _icon.Icon = new Icon(s);
            }
            _icon.BalloonTipClicked += new EventHandler(_icon_BalloonTipClicked);
            _icon.Visible = true;

            MenuItem[] items = new MenuItem[3];
            items[0] = new MenuItem("Feeds");
            items[0].Click += new EventHandler(Feeds_Click);
            items[1] = new MenuItem("About");
            items[1].Click += new EventHandler(About_Click);
            items[2] = new MenuItem("Exit");
            items[2].Click += new EventHandler(Exit_Click);
            ContextMenu menu = new ContextMenu(items);
            _icon.ContextMenu = menu;

            FeedManager.Load();

            //start the balloon thread!
            _thread = new Thread(GoBizatch);
            _thread.Start();

            Application.Run();
        }

        static void GoBizatch()
        {
            while (true)
            {
                try
                {
                    lock (FeedManager.feeds)
                    {
                        foreach (string feed in FeedManager.feeds)
                        {
                            //load the feed
                            RssChannel channel = RssParses.ProcessRSS(feed);
                            //if we've never read, mark all as read
                            if (!_feeds.ContainsKey(channel.link))
                            {
                                Dictionary<string, DateTime> curDictionary = _feeds[channel.link] = new Dictionary<string, DateTime>();
                                foreach (RssItem item in channel.Items)
                                {
                                    //just use timestamp as value.  no purpose, but dictionary is best method
                                    curDictionary[item.guid] = DateTime.Now;
                                }
                            }
                            else
                            {
                                Dictionary<string, DateTime> curDictionary = _feeds[channel.link];
                                foreach (RssItem item in channel.Items)
                                {
                                    //if item is not seen before, balloon
                                    if (!curDictionary.ContainsKey(item.guid))
                                    {
                                        _lastUrl = item.link;
                                        string title = StripHTML(item.title);
                                        string description = StripHTML(item.description);
                                        _icon.ShowBalloonTip(20000, title, description, ToolTipIcon.Info);
                                        curDictionary[item.guid] = DateTime.Now;
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    _icon.ShowBalloonTip(90000, "AnAppADay.com RSS Alerter Error", ex.Message, ToolTipIcon.Error);
                }
            }
        }

        private static string StripHTML(string p)
        {
            string ret = Regex.Replace(p, @"<(.|\n)*?>", "");
            return ret;
        }

        static void _icon_BalloonTipClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_lastUrl);
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            Environment.Exit(0);
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.AnAppADay.com");
        }

        static void Feeds_Click(object sender, EventArgs e)
        {
            FeedsForm.Instance.Show();
        }
    }
}