using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AnAppADay.ScreenBroadcaster.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.Run(new ClientMainForm());
            System.Environment.Exit(0);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Error:" + Environment.NewLine + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);
        }
    }
}