using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.JediWindowDock.WinApp
{

    static class Program
    {

        private static MainForm _mainForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            _mainForm = new MainForm();
            _mainForm.Show();
            Application.Run();
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Utility.ShowMessage("Error: " + e.Exception.Message);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                Utility.ShowMessage("Error: " + ex.Message);
            }
            else
            {
                Utility.ShowMessage("Unknown Error: " + e.ExceptionObject);
            }
            if (e.IsTerminating)
            {
                Utility.ShowMessage("Application will exit...");
            }
        }

    }

}
