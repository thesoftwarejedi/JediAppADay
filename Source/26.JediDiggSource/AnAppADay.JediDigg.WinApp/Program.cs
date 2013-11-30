using System;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Drawing;

namespace AnAppADay.JediDigg.WinApp
{

    internal class DiggStory
    {
        public string title;
        public string description;
        public string url;
        public string diggUrl;
        public string id;
        public string row;
        public string diggCheck;
        public string diggs;

        public override string ToString()
        {
            return title;
        }
    }

    static class Program
    {

        internal static CookieContainer _cookies = new CookieContainer();
        internal static NotifyIcon _icon;
        private static OptionsForm _optionsForm;
        private static bool _loggedIn = false;
        private static TickerForm _tickerForm;
        private static Thread _thread;

        [STAThread]
        static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                _icon = new NotifyIcon();
                _optionsForm = new OptionsForm();
                _tickerForm = new TickerForm();

                //load the icon
                using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.JediDigg.WinApp.Icon.ico"))
                {
                    _icon.Icon = new Icon(s);
                }

                _loggedIn = LoginToDigg(Properties.Settings.Default.username, Properties.Settings.Default.password);

                MenuItem[] items = new MenuItem[3];
                items[2] = new MenuItem("Exit");
                items[2].Click += new EventHandler(Exit_Click);
                items[1] = new MenuItem("About");
                items[1].Click += new EventHandler(About_Click);
                items[0] = new MenuItem("Options");
                items[0].Click += new EventHandler(Options_Click);
                _icon.DoubleClick += new EventHandler(Options_Click);
                _icon.ContextMenu = new ContextMenu(items);
                _icon.Visible = true;

                _thread = new Thread(Go);
                _thread.Start();

                _tickerForm.Show();

                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Jedi Digg Unhandled Error: " + ex.Message);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Jedi Digg Unhandled Domain Error: " + e.ExceptionObject);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Jedi Digg Unhandled Error: " + e.Exception.Message);
        }

        static void Options_Click(object sender, EventArgs e)
        {
            _optionsForm.Show();
            _optionsForm.Activate();
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            try
            {
                _tickerForm.Close();
                _tickerForm.Dispose();
            }
            catch { }
            try
            {
                _icon.Visible = false;
                _icon.Dispose();
            }
            catch { }
            try
            {
                Properties.Settings.Default.Save();
            }
            catch { }
            Environment.Exit(0);
        }

        private static void Go()
        {
            while (true)
            {
                try
                {
                    if (_loggedIn == true)
                    {
                        foreach (string s in Properties.Settings.Default.categories)
                        {
                            DiggStory[] stories = GetStories(s);
                            foreach (DiggStory story in stories)
                            {
                                _tickerForm.SetStory(story);
                                Thread.Sleep(20000);
                                while (_tickerForm._pause)
                                {
                                    Thread.Sleep(20000);
                                }
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
                catch (Exception ex)
                {
                    _icon.ShowBalloonTip(5000, "Jedi Digg Error", ex.Message + Environment.NewLine + "Perhaps digg changed the page, which breaks the parsing..." + Environment.NewLine + "Will retry in 30 seconds", ToolTipIcon.Error);
                    Thread.Sleep(30000);
                }
            }
        }

        internal static DiggStory[] GetStories(string page)
        {
            return GetStories(Properties.Settings.Default.username, Properties.Settings.Default.password, page);
        }

        internal static DiggStory[] GetStories(string username, string password, string page) //page for future use
        {
            string url = "";
            switch (page) {
                case "Technology":
                    url = "http://www.digg.com/view/technology";
                    break;
                case "Science":
                    url = "http://www.digg.com/view/science";
                    break;
                case "World & Business":
                    url = "http://www.digg.com/view/world_business";
                    break;
                case "Videos":
                    url = "http://www.digg.com/view/videos";
                    break;
                case "Entertainment":
                    url = "http://www.digg.com/view/entertainment";
                    break;
                case "Gaming":
                    url = "http://www.digg.com/view/gaming";
                    break;
                default:
                    url = "http://www.digg.com/";
                    break;
            }

            //first login
            if (!_loggedIn)
            {
                LoginToDigg(username, password);
            }

            HttpWebRequest req = CreateRequest(url, _cookies);
            string responseString = GetResponseString(req);
            //parse out the stories...  I know, I know, the jedi should learn regex
            string[] stories = responseString.Split(new string[] { " target=\"_blank\">" }, StringSplitOptions.None);
            List<DiggStory> diggStoryList = new List<DiggStory>();
            int d = 0;
            for (int i = 1; i < stories.Length; i += 2)
            {
                DiggStory ds = new DiggStory();
                do
                {
                    ds.title = stories[i].Split('<')[0];
                    if (ds.title == "") i++;
                } while (ds.title == "");
                string temp = stories[i + 1].Split(new string[] { "<p>" }, StringSplitOptions.None)[1];
                ds.description = temp.Substring(0, temp.IndexOf('&'));
                ds.diggUrl = temp.Substring(temp.IndexOf("href=") + 6);
                ds.diggUrl = ds.diggUrl.Substring(0, ds.diggUrl.IndexOf('"'));
                ds.url = stories[i - 1].Substring(stories[i - 1].LastIndexOf("href=") + 6);
                ds.url = ds.url.Substring(0, ds.url.Length - 1);
                string diggs = temp.Substring(0, temp.IndexOf("</strong>"));
                ds.diggs = diggs.Substring(diggs.LastIndexOf('>') + 1);
                string[] idInfoTemp = stories[i + 1].Split(new string[] { "javascript:wrapper_full(" }, StringSplitOptions.None);
                if (idInfoTemp.Length > 1)
                {
                    string idInfo = idInfoTemp[1];
                    idInfo = idInfo.Substring(0, idInfo.IndexOf(')'));
                    string[] infoDetails = idInfo.Split(',');
                    ds.id = infoDetails[2];
                    ds.row = infoDetails[0];
                    ds.diggCheck = infoDetails[3].Substring(1, infoDetails[3].Length - 2);
                }
                diggStoryList.Add(ds);
            }
            return diggStoryList.ToArray();
        }

        internal static bool LoginToDigg(string username, string password)
        {
            if (username == null || username.Length < 1) return false;
            //fake little request to get any digg cookies
            HttpWebRequest req = CreateRequest("http://www.digg.com", _cookies);
            req.GetResponse().Close();
            //login
            req = CreateRequest("http://www.digg.com/login", _cookies);
            PostData(req, String.Format("side-username={0}&side-password={1}&processlogin=1&side-persistent=1", username, password));
            string responseString = GetResponseString(req);
            bool success = !responseString.Contains("<legend>Lost Your Username or Password?</legend>");
            _loggedIn = success;
            return success;
        }

        internal static string DiggStory(DiggStory diggStory)
        {
            HttpWebRequest req = CreateRequest("http://www.digg.com/diginfull", _cookies);
            PostData(req, "id=" + diggStory.id + "&row=" + diggStory.row + "&digcheck=" + diggStory.diggCheck);
            string responseString = GetResponseString(req);
            return responseString;
        }

        private static string GetResponseString(HttpWebRequest req)
        {
            WebResponse res = req.GetResponse();
            string responseString = null;
            using (StreamReader read = new StreamReader(res.GetResponseStream()))
            {
                responseString = read.ReadToEnd();
            }
            res.Close();
            return responseString;
        }

        private static void PostData(HttpWebRequest req, string p)
        {
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            req.Referer = "http://www.digg.com/";
            byte[] b = Encoding.ASCII.GetBytes(p);
            req.ContentLength = b.Length;
            using (Stream s = req.GetRequestStream())
            {
                s.Write(b, 0, b.Length);
                s.Close();
            }
        }

        private static HttpWebRequest CreateRequest(string url, CookieContainer c)
        {
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1) Gecko/20061003 Firefox/2.0";
            req.CookieContainer = c;
            return req;
        }

    }

}