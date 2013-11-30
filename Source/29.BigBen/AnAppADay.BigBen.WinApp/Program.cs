using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Reflection;
using System.Drawing;

namespace AnAppADay.BigBen.WinApp
{

    static class Program
    {

        private static NotifyIcon _icon;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _icon = new NotifyIcon();

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.BigBen.WinApp.Icon.ico"))
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

            new Thread(Go).Start();

            Application.Run();
        }

        private static void Go()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                DateTime chimeTime = GetChimeTime(now);
                Thread.Sleep(chimeTime - now);
                using (Stream wav = Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.BigBen.WinApp.bigben-3.wav"))
                {
                    SoundPlayer sp = new SoundPlayer(wav);
                    sp.PlaySync();
                }
                using (Stream wav = Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.BigBen.WinApp.bigbenchime.wav"))
                {
                    SoundPlayer sp = new SoundPlayer(wav);
                    int chimes = chimeTime.Hour;
                    if (chimes > 12)
                    {
                        chimes -= 12;
                    }
                    if (chimes == 0)
                    {
                        chimes = 12;
                    }
                    for (int i = 0; i < chimes; i++)
                    {
                        sp.PlaySync();
                    }
                }
            }
        }

        private static DateTime GetChimeTime(DateTime now)
        {
            DateTime anHourFromNow = now.AddHours(1);
            DateTime chimeTime = new DateTime(anHourFromNow.Year, anHourFromNow.Month, anHourFromNow.Day, anHourFromNow.Hour, 0, 0);
            return chimeTime;
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            //one of the rare instances I'd like an old sk00l ON ERROR RESUME NEXT
            try { _icon.Visible = false; }
            catch { }
            try { _icon.Dispose(); }
            catch { }
            try { Application.Exit(); }
            catch { }
            Environment.Exit(0);
        }

    }

}