using System;
using System.Xml;
using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace AnAppADay.GoogleWallpaper.WinApp
{
    static class Program
    {

        static NotifyIcon _icon = new NotifyIcon();
        static bool safeSearch = true;
        static string keywords = null;
        static int refreshRate = 30;
        static Thread _thread;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoadOptions();

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.GoogleWallpaper.WinApp.Icon.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[4];
            items[3] = new MenuItem("Exit");
            items[3].Click += new EventHandler(Exit_Click);
            items[2] = new MenuItem("About");
            items[2].Click += new EventHandler(About_Click);
            items[1] = new MenuItem("Update Now");
            items[1].Click += new EventHandler(Update_Click);
            items[0] = new MenuItem("Options");
            items[0].Click += new EventHandler(Options_Click);
            _icon.DoubleClick += new EventHandler(_icon_DoubleClick);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;

            _thread = new Thread(Badabing);
            _thread.Start();

            Application.Run();
        }

        static void _icon_DoubleClick(object sender, EventArgs e)
        {
            Options_Click(sender, e);
        }

        static void Badabing()
        {
            GoogleImageSearch search = new GoogleImageSearch();
            Random r = new Random();
            while (true)
            {
                if (keywords != null && keywords.Length > 0)
                {
                    string[] keywordsSplit = keywords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (keywordsSplit.Length == 0) break;
                    int i = r.Next(0, keywordsSplit.Length);
                    search.KeyWord = keywordsSplit[i];
                    search.Safe = safeSearch;
                    string[] images = search.DoSearch();
                    if (images.Length > 0)
                    {
                        int attempt = 0;
                        Exception lastException;
                        do
                        {
                            try
                            {
                                i = r.Next(0, images.Length);
                                string imageUrl = images[i];
                                WebClient wc = new WebClient();
                                byte[] bytes = wc.DownloadData(imageUrl);
                                MemoryStream stream = new MemoryStream(bytes);
                                Image img = Image.FromStream(stream);
                                string bmpFilename = Directory.GetCurrentDirectory() + "\\temp.bmp";
                                img.Save(bmpFilename, System.Drawing.Imaging.ImageFormat.Bmp);
                                //ok, so we have the file now.  We just need to set it on the wallpaper.
                                bool result = WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, bmpFilename, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);
                                if (!result)
                                {
                                    throw new Exception("Unable to set wallpaper");
                                }
                                lastException = null;
                            }
                            catch (Exception ex)
                            {
                                lastException = ex;
                            }
                        } while (lastException != null && attempt++ < 3);
                        if (lastException != null)
                        {
                            _icon.ShowBalloonTip(5000, "Couldn't update desktop", lastException.Message, ToolTipIcon.Error);
                        }
                    }
                }
                lock (_icon) {
                    Monitor.Wait(_icon, refreshRate * 60000);
                }
            }
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            SaveOptions();
            Environment.Exit(0);
        }

        private static void LoadOptions()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("AnAppADay.GoogleWallpaper.WinApp.jedi");
                refreshRate = Int32.Parse(doc.DocumentElement.SelectSingleNode("RefreshRate").InnerText);
                keywords = doc.DocumentElement.SelectSingleNode("Keywords").InnerText;
                safeSearch = Boolean.Parse(doc.DocumentElement.SelectSingleNode("SafeSearch").InnerText);
            }
            catch (Exception)
            {
                //not there
            }
        }

        private static void SaveOptions()
        {
            try
            {
                using (XmlWriter write = XmlWriter.Create("AnAppADay.GoogleWallpaper.WinApp.jedi"))
                {
                    write.WriteStartDocument();
                    write.WriteStartElement("GoogleWallpaper");
                    write.WriteElementString("RefreshRate", refreshRate.ToString());
                    write.WriteElementString("Keywords", keywords.ToString());
                    write.WriteElementString("SafeSearch", safeSearch.ToString());
                    write.WriteEndElement();
                    write.WriteEndDocument();
                    write.Close();
                }
            }
            catch (Exception)
            {
                //not there
            }
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Update_Click(object sender, EventArgs e)
        {
            lock (_icon)
            {
                Monitor.PulseAll(_icon);
            }
        }

        static void Options_Click(object sender, EventArgs e)
        {
            OptionsForm f = new OptionsForm();
            f.safeSearch = safeSearch;
            f.keywords = keywords;
            f.refreshRate = refreshRate;
            if (f.ShowDialog() == DialogResult.OK)
            {
                safeSearch = f.safeSearch;
                refreshRate = f.refreshRate;
                keywords = f.keywords;
                lock (_icon)
                {
                    Monitor.PulseAll(_icon);
                }
            }
        }

    }

    public class WinAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        public const int SPI_SETDESKWALLPAPER = 20; 
        public const int SPIF_UPDATEINIFILE = 0x01;
        public const int SPIF_SENDWININICHANGE = 0x02;
    }  

}