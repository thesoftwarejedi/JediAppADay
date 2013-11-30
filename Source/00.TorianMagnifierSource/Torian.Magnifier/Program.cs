using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using Torian.Magnifier.Properties;

namespace Torian.Magnifier
{
    static class Program
    {
        static MagnifierForm f;
        static int sleepBringToFront = Settings.Default.BringToFrontRate;
        static int sleepRefresh = Settings.Default.RefreshRate;
        static bool refreshThreadOn = Settings.Default.RefreshThreadOn;
        static bool bringToFrontThreadOn = Settings.Default.BringToFrontThreadOn;
        private static bool exit = false;

        [STAThread]
        static void Main()
        {
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            f = new MagnifierForm();
            Thread t1 = new Thread(new ThreadStart(BringToFrontLoop));
            Thread t2 = new Thread(new ThreadStart(RefreshLoop));
            t1.Start();
            t2.Start();
            Stuff.StartThread();

            NotifyIcon icon = SetupSystray();

            //MAIN LOOP
            Application.Run(f);

            Settings.Default.Save();
            exit = true;
            Stuff.caretThreadStop = true;
            lock (Stuff.caretMutex)
            {
                Monitor.PulseAll(Stuff.caretMutex);
                Monitor.PulseAll(Stuff.caretMutex);
            }
            t1.Join();
            t2.Join();
            Stuff.caretThread.Join();
            icon.Dispose();
            
        }

        private static NotifyIcon SetupSystray()
        {
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("&About", new EventHandler(aboutClicked));
            menu.MenuItems.Add("&Options", new EventHandler(optionsClicked));
            menu.MenuItems[1].DefaultItem = true;
            menu.MenuItems.Add("E&xit", new EventHandler(exitClicked));
            NotifyIcon icon = new NotifyIcon();
            icon.ContextMenu = menu;
            icon.Icon = f.Icon;
            icon.DoubleClick += new EventHandler(icon_DoubleClick);
            icon.Visible = true;
            return icon;
        }

        static void icon_DoubleClick(object sender, EventArgs e)
        {
            optionsClicked(null, null);
        }

        private static void optionsClicked(object source, EventArgs args)
        {
            OptionsForm o = new OptionsForm();
            o.ShowDialog();
        }

        private static void aboutClicked(object source, EventArgs args)
        {
            AboutBox box = new AboutBox();
            box.ShowDialog();
        }

        private static void exitClicked(object source, EventArgs args)
        {
            f.Close();
        }

        static void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            sleepBringToFront = Settings.Default.BringToFrontRate;
            sleepRefresh = Settings.Default.RefreshRate;
            refreshThreadOn = Settings.Default.RefreshThreadOn;
            bringToFrontThreadOn = Settings.Default.BringToFrontThreadOn;
            f.refreshOnMouseMove = Settings.Default.RefreshOnMouseMove;
            f.m_Zoom = Settings.Default.ZoomAmount;
            f.refreshOnMouseMoveRate = Settings.Default.RefreshOnMouseMoveRate;
            f.refreshOverInvalidate = Settings.Default.RefreshOverInvalidate;
            f.followTyping = Settings.Default.FollowTyping;
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(f, e.Exception.Message + Environment.NewLine + e.Exception.StackTrace,
                "Unknown error has occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void BringToFrontLoop()
        {
            while (!exit)
            {
                Thread.Sleep(sleepBringToFront);
                if (bringToFrontThreadOn && f.Visible)
                {
                    f.Invoke(new MethodInvoker(f.BringToFront));
                }
            }
        }

        private static void RefreshLoop()
        {
            while (!exit)
            {
                Thread.Sleep(sleepRefresh);
                if (refreshThreadOn && f.Visible)
                {
                    f.Invoke(new MethodInvoker(f.RefreshOrInvalidate));
                }
            }
        }
    }
}
