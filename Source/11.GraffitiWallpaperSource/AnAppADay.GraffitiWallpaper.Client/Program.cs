using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Web;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using AnAppADay.GraffitiWallpaper.Shared;

namespace AnAppADay.GraffitiWallpaper.Client
{

    static class Program
    {

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.Run(new PaintForm());
            Environment.Exit(0);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled Exception: " + e.Exception.Message);
        }

    }

}