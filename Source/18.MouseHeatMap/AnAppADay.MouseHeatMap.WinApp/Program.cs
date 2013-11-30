using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using AnAppADay.Utils;

namespace AnAppADay.MouseHeatMap.WinApp
{

    static class Program
    {

        private static MouseHookManager _mgr;
        private static Dictionary<Point, int> _dictionary;
        private static NotifyIcon _icon;
        private static MouseHeatMapForm _form;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            _icon = new NotifyIcon();
            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.MouseHeatMap.WinApp.App.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[3];
            items[0] = new MenuItem("View Heat Map");
            items[0].Click += new EventHandler(ViewMap_Click);
            items[1] = new MenuItem("About");
            items[1].Click += new EventHandler(About_Click);
            items[2] = new MenuItem("Exit");
            items[2].Click += new EventHandler(Exit_Click);
            ContextMenu menu = new ContextMenu(items);
            _icon.ContextMenu = menu;
            _icon.Visible = true;

            _dictionary = new Dictionary<Point, int>();

            _mgr = new MouseHookManager();
            _mgr.OnMouseActivity += new MouseEventHandler(_mgr_OnMouseActivity);

            Application.Run();
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Mouse Heat Map - Unhandled Exception: " + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        }

        static void _mgr_OnMouseActivity(object sender, MouseEventArgs e)
        {
            //just got a GREAT idea - to make the resolution not so
            //fine, round the point to the nearest multiple of 7.
            int x = e.Location.X / 7 * 7;
            int y = e.Location.Y / 7 * 7;
            Point p = new Point(x, y);
            //on any mouse message, add the point to the dictionary
            if (_dictionary.ContainsKey(p)) {
                _dictionary[p] += 1;
            } else {
                _dictionary.Add(p, 1);
            }
            if (_form != null && e.Button != MouseButtons.None)
            {
                _form.Close();
            }
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            _mgr.Stop();
            Environment.Exit(0);
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.AnAppADay.com");
        }

        static void ViewMap_Click(object sender, EventArgs e)
        {
            if (_form != null)
            {
                _form.Close();
            }
            _form = new MouseHeatMapForm();
            _form.Points = _dictionary;
            _form.Show();
            _form.FormClosed += new FormClosedEventHandler(_form_FormClosed);
        }

        static void _form_FormClosed(object sender, FormClosedEventArgs e)
        {
            _form.Dispose();
            _form = null;
        }

    }

}