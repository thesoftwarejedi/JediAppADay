using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;

namespace AnAppADay.JediHoneyPot.WinApp
{

    class LongLat
    {
        public string lon;
        public string lat;
    }

    static class Program
    {

        private static Dictionary<string, LongLat> _locs;
        private static Dictionary<string, string> _ips;
        private static string _html;
        private static NotifyIcon _icon;
        private static HttpListener _listener;

        [STAThread]
        static void Main()
        {
            _html = File.ReadAllText("AnAppADay.JediHoneyPot.MainPage.htm");
            _locs = new Dictionary<string, LongLat>();
            _ips = new Dictionary<string, string>();
            _icon = new NotifyIcon();

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.JediHoneyPot.WinApp.Icon.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[2];
            items[1] = new MenuItem("Exit");
            items[1].Click += new EventHandler(Exit_Click);
            items[0] = new MenuItem("About");
            items[0].Click += new EventHandler(About_Click);

            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;

            ThreadPool.SetMinThreads(10, 10);
            ThreadPool.SetMaxThreads(20, 20);
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:15632/");
            _listener.Prefixes.Add("http://+:15632/data/");
            _listener.Start();

            new Thread(PretendYouAreIIS).Start();

            //needed for a message loop creation to drive notify icon
            Application.Run();
        }

        private static void PretendYouAreIIS()
        {
            while (true)
            {
                try
                {
                    HttpListenerContext ctx = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveRequest), ctx);
                }
                catch
                {
                    try { _listener.Close(); }
                    catch { }
                    bool stop = false;
                    while (!stop)
                    {
                        try
                        {
                            _listener = new HttpListener();
                            _listener.Prefixes.Add("http://+:15632/");
                            _listener.Prefixes.Add("http://+:15632/data/");
                            _listener.Start();
                            stop = true;
                        }
                        catch
                        {
                            Thread.Sleep(5000);
                        }
                    }
                }
            }
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            Environment.Exit(0);
        }

        private static void ReceiveRequest(object o)
        {
            HttpListenerContext ctx = o as HttpListenerContext;
            try
            {
                if (ctx != null)
                {
                    if (ctx.Request.Url.OriginalString.Contains("data"))
                    {
                        ProcessDataRequest(ctx);
                    }
                    else
                    {
                        //respond with map page
                        using (StreamWriter sw = new StreamWriter(ctx.Response.OutputStream))
                        {
                            sw.Write(_html);
                            sw.Close();
                        }
                        ctx.Response.Close();
                    }
                }
            }
            catch
            {
                //oh well...
            }
        }

        private static void ProcessDataRequest(HttpListenerContext ctx)
        {
            Exception exception = null;
            string ip = null;
            try
            {
                ip = ctx.Request.RemoteEndPoint.Address.ToString();
                //don't lookup again for same IP!
                if (!_ips.ContainsKey(ip))
                {
                    _ips.Add(ip, null);
                    //now lookup the IP in long/lat
                    HttpWebRequest req = WebRequest.Create("http://www.showmyip.com/geo/?ip=" + ip) as HttpWebRequest;
                    req.CookieContainer = new CookieContainer();
                    req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727";
                    req.Referer = "http://www.showmyip.com/";
                    HttpWebResponse httpResp = req.GetResponse() as HttpWebResponse;
                    string resp = null;
                    using (StreamReader sr = new StreamReader(httpResp.GetResponseStream()))
                    {
                        resp = sr.ReadToEnd();
                        sr.Close();
                    }
                    string lat = resp.Split(new string[] { "Latitude: </TD><TD CLASS=\"glossary_left\">" }, StringSplitOptions.None)[1];
                    lat = lat.Substring(0, lat.IndexOf("<"));
                    lat = lat.Trim();
                    string lon = resp.Split(new string[] { "Longitude: </TD><TD CLASS=\"glossary_left\">" }, StringSplitOptions.None)[1];
                    lon = lon.Substring(0, lon.IndexOf("<"));
                    lon = lon.Trim();
                    if (lon == "" || lat == "")
                    {
                        throw new Exception("blank long lat");
                    }
                    string loc = lon + "," + lat;
                    if (!_locs.ContainsKey(loc))
                    {
                        LongLat lonlat = new LongLat();
                        lonlat.lon = lon;
                        lonlat.lat = lat;
                        _locs.Add(loc, lonlat);
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                try
                {
                    //NOW, give back the XML of all previous requests (the honey tracks)
                    XmlWriter write = XmlWriter.Create(ctx.Response.OutputStream);
                    write.WriteStartElement("XMLPoints");
                    write.WriteElementString("IP", ip);
                    foreach (LongLat lonlat in _locs.Values)
                    {
                        write.WriteStartElement("XMLPoint");
                        write.WriteAttributeString("lon", lonlat.lon);
                        write.WriteAttributeString("lat", lonlat.lat);
                        write.WriteEndElement();
                    }
                    if (exception != null)
                    {
                        write.WriteElementString("Exception", exception.Message);
                    }
                    write.WriteEndElement();
                    write.Flush();
                    write.Close();
                    ctx.Response.Close();
                }
                catch
                {
                    //oh well
                }
            }
        }

    }

}
