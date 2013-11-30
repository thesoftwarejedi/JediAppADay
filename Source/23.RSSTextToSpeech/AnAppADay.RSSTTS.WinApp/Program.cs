using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace AnAppADay.RSSTTS.WinApp
{

    static class Program
    {

        internal static NotifyIcon _icon;
        private static MainForm _mainForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _icon = new NotifyIcon();
            _mainForm = new MainForm();

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.RSSTTS.WinApp.Icon.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[5];
            items[4] = new MenuItem("Exit");
            items[4].Click += new EventHandler(Exit_Click);
            items[3] = new MenuItem("About");
            items[3].Click += new EventHandler(About_Click);
            items[2] = new MenuItem("Options");
            items[2].Click += new EventHandler(Options_Click);
            items[1] = new MenuItem("-");
            items[0] = new MenuItem("Last Item Details");
            items[0].Click += new EventHandler(Details_Click);
            _icon.DoubleClick += new EventHandler(_icon_DoubleClick);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;

            Application.Run();
        }

        static void _icon_DoubleClick(object sender, EventArgs e)
        {
            Options_Click(sender, e);
        }

        static void Details_Click(object sender, EventArgs e)
        {
            string link = _mainForm._lastUrl;
            if (link != null && link.Trim().Length > 0 && (link.StartsWith("http://") || link.StartsWith("https://")))
            {
                Process.Start(_mainForm._lastUrl);
            }
        }

        static void Options_Click(object sender, EventArgs e)
        {
            _mainForm.Show();
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            Environment.Exit(0);
        }

        static void About_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.anappaday.com");
        }

    }

}
