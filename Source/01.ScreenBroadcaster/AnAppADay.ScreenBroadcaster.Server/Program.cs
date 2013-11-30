using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AnAppADay.ScreenBroadcaster.Common;
using System.Drawing;

namespace AnAppADay.ScreenBroadcaster.Server
{

    class Program
    {

        private static Form form;
        public static NotifyIcon ntfy;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //setup notify icon
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerMainForm));
            Icon ico = ((Icon)(resources.GetObject("$this.Icon")));
            ntfy = new NotifyIcon();
            ntfy.Icon = ico;
            ntfy.MouseDoubleClick += new MouseEventHandler(ntfy_MouseDoubleClick);
            MenuItem[] menuItems = new MenuItem[2];
            menuItems[0] = new MenuItem("About");
            menuItems[0].Click += new EventHandler(Program_Click);
            menuItems[1] = new MenuItem("Exit");
            menuItems[1].Click += new EventHandler(Program2_Click);
            ContextMenu menu = new ContextMenu(menuItems);
            ntfy.ContextMenu = menu;
            ntfy.Visible = true;
            //startup the form and app loop
            form = new ServerMainForm();
            form.Show();
            Application.Run();
        }

        static void Program_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Program2_Click(object sender, EventArgs e)
        {
            ntfy.Visible = false;
            ntfy.Dispose();
            System.Environment.Exit(0);
        }

        static void ntfy_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            form.Show();
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            CommonLib.HandleException(e.Exception);
        }

    }

}
