using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Drawing;

namespace AnAppADay.GraffitiWallpaper.Server
{

    static class Program
    {
        public static NotifyIcon _icon = new NotifyIcon();
        static HttpRuntime _httpRuntime = new HttpRuntime();
        static Form1 _mainForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.GraffitiWallpaper.Server.Icon.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[2];
            items[1] = new MenuItem("Exit");
            items[1].Click += new EventHandler(Exit_Click);
            items[0] = new MenuItem("About");
            items[0].Click += new EventHandler(About_Click);
            _icon.DoubleClick += new EventHandler(_icon_DoubleClick);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;
            _mainForm = new Form1();
            Application.Run(_mainForm);
            Environment.Exit(0);
        }

        static void _icon_DoubleClick(object sender, EventArgs e)
        {
            _mainForm.Show();
            _mainForm.Activate();
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

    }

}