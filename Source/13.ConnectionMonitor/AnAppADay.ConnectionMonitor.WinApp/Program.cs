using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Threading;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    static class Program
    {

        private static NotifyIcon _icon;
        private static ServerListForm _mainForm;
        private static OptionsForm _optionsForm;
        private static Thread _mainThread;
        internal static object _mainThreadMutex = new object();
        internal static int _retryRate = 300;

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            _mainThread = new Thread(MonitorThread);
            _mainThread.Start();

            _icon = new NotifyIcon();
            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.ConnectionMonitor.WinApp.App.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[4];
            items[3] = new MenuItem("Exit");
            items[3].Click += new EventHandler(Exit_Click);
            items[2] = new MenuItem("About");
            items[2].Click += new EventHandler(About_Click);
            items[1] = new MenuItem("Options");
            items[1].Click += new EventHandler(Options_Click);
            items[0] = new MenuItem("Server Config");
            items[0].Click += new EventHandler(Config_Click);
            _icon.DoubleClick += new EventHandler(_icon_DoubleClick);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;

            _optionsForm = new OptionsForm();
            _mainForm = new ServerListForm();

            Application.Run();
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled Exception in Connection Monitor: " + e.Exception.Message + Environment.NewLine + Environment.NewLine + e.Exception.StackTrace);
        }

        private static bool STOP = false;
        private static Hashtable retrys = new Hashtable();

        static void MonitorThread()
        {
            //here's where we do the money
            while (!STOP)
            {
                lock (_mainThreadMutex)
                {
                    //iterate through the loop and check all servers
                    foreach (ServerInfo i in ServerInfoList._list)
                    {
                        try
                        {
                            if (!retrys.ContainsKey(i))
                            {
                                retrys.Add(i, 0);
                            }
                            if (!TryServer(i))
                            {
                                int retrysInt = (int)retrys[i] + 1;
                                if (retrysInt >= i._attempts)
                                {
                                    _icon.ShowBalloonTip(10000, "Connection Failure", i._serverName + " has failed " + retrysInt + " times", ToolTipIcon.Warning);
                                }
                                retrys[i] = retrysInt;
                            }
                            else
                            {
                                retrys[i] = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _icon.ShowBalloonTip(10000, "Fatal Exception", i._serverName + ": " + ex.Message, ToolTipIcon.Warning);
                        }
                    }
                    //wait 5 mins, or for an update
                    Monitor.Wait(_mainThreadMutex, _retryRate * 1000);
                }
            }
        }

        private static bool TryServer(ServerInfo i)
        {
            IConnectable connectable = null;
            switch (i._type)
            {
                case ServerInfoMonitorType.Ping:
                    connectable = new SimplePing();
                    break;
                case ServerInfoMonitorType.Socket:
                    connectable = new SimpleSocket();
                    break;
                case ServerInfoMonitorType.HTTP:
                    connectable = new SimpleHttp();
                    break;
                default:
                    throw new Exception("Invalid server type");
            }
            return connectable.TryConnect(i._serverName);
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

        static void Options_Click(object sender, EventArgs e)
        {
            _optionsForm.Show();
        }

        static void Config_Click(object sender, EventArgs e)
        {
            _icon_DoubleClick(null, null);
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            lock (_mainThreadMutex)
            {
                STOP = true;
                Monitor.PulseAll(_mainThreadMutex);
            }
            _icon.Visible = false;
            _icon.Dispose();
            _mainForm.Close();
            _mainForm.SaveServers();
            Application.DoEvents();
            Application.Exit();
            Environment.Exit(0);
        }

    }

}