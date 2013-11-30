using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SpeechLib;
using AnAppADay.Utils;

namespace AnAppADay.RSSTTS.WinApp
{

    public partial class MainForm : Form
    {

        Thread _mainThread;
        SpVoice _speech;
        internal string _lastUrl;
        private static Dictionary<string, Dictionary<string, DateTime>> _feeds;

        public MainForm()
        {
            InitializeComponent();
            _speech = new SpVoice();
            _feeds = new Dictionary<string, Dictionary<string, DateTime>>();
            _mainThread = new Thread(GoYouFool);
            _mainThread.Start();
        }

        private void GoYouFool()
        {
            bool shouldRead = true;
            while (true)
            {
                try
                {
                    shouldRead = true;
                    if (Properties.Settings.Default.ActiveDuringTimes)
                    {
                        DateTime now = DateTime.Now;
                        string timeString = Properties.Settings.Default.TimeFrom;
                        int hours = int.Parse(timeString.Substring(0, 2));
                        int mins = int.Parse(timeString.Substring(3, 2));
                        DateTime from = new DateTime(now.Year, now.Month, now.Day, hours, mins, 0);
                        timeString = Properties.Settings.Default.TimeTo;
                        hours = int.Parse(timeString.Substring(0, 2));
                        mins = int.Parse(timeString.Substring(3, 2));
                        DateTime to = new DateTime(now.Year, now.Month, now.Day, hours, mins, 0);
                        if (now < from || now > to)
                        {
                            shouldRead = false;
                        }
                    }
                    if (shouldRead)
                    {
                        //here we get the RSS feeds
                        string[] feeds = Properties.Settings.Default.Feeds.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string feed in feeds)
                        {
                            if (feed.Trim() == "") continue;
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
                                    //if item is not seen before, SPEAK!
                                    if (!curDictionary.ContainsKey(item.guid))
                                    {
                                        string title = Utility.StripHTML(item.title);
                                        string description = Utility.StripHTML(item.description);
                                        _lastUrl = item.link;
                                        _speech.Speak(channel.title, SpeechVoiceSpeakFlags.SVSFDefault);
                                        Thread.Sleep(1000);
                                        _speech.Speak(title, SpeechVoiceSpeakFlags.SVSFDefault);
                                        Thread.Sleep(1000);
                                        _speech.Speak(description, SpeechVoiceSpeakFlags.SVSFDefault);
                                        curDictionary[item.guid] = DateTime.Now;
                                        //give 5 seconds for a hotkey press....
                                        Thread.Sleep(5000);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program._icon.ShowBalloonTip(5000, "RSS TTS Error", ex.Message, ToolTipIcon.Error);
                }
                Thread.Sleep(((int)Properties.Settings.Default.PollRate) * 60000);
            }
        }

        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            if (!CheckTime(textBox1.Text)) e.Cancel = true;
        }

        private void textBox2_Validating(object sender, CancelEventArgs e)
        {
            if (!CheckTime(textBox2.Text)) e.Cancel = true;
        }

        private bool CheckTime(string p)
        {
            try
            {
                int hour = int.Parse(p.Substring(0, 2));
                int min = int.Parse(p.Substring(3, 2));
                if (hour < 0 || hour > 23) throw new Exception();
                if (min < 0 || min > 59) throw new Exception();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

    }

}