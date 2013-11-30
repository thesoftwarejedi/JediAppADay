using System;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.WPMTray.WinApp
{

    static class Program
    {

        const int POLL_RATE = 3000;
        const int RUNNING_AVERAGE = 15000;

        private static Font _font;
        private static Bitmap _bmp;
        private static NotifyIcon _icon;
        private static KeyHookManager _mgr;
        private static List<DateTime> _wpmCounter;
        private static Thread _thread;
        private static object _exitMonitor = new object();
        private static bool _stop;
        private static bool _lastWasSeperator;
        private static Brush _slowBrush = Brushes.DarkGreen;
        private static Brush _fastBrush = Brushes.DarkGoldenrod;
        private static Brush _insaneBrush = Brushes.DarkRed;
        private static Brush _wtfBrush = Brushes.White;
        private static WPMHistoryForm _form;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MenuItem[] items = new MenuItem[2];
            items[1] = new MenuItem("Exit");
            items[1].Click += new EventHandler(Exit_Click);
            items[0] = new MenuItem("About");
            items[0].Click += new EventHandler(About_Click);

            _stop = false;
            _lastWasSeperator = false;
            _wpmCounter = new List<DateTime>();
            _font = new Font("Times New Roman", 8);
            _bmp = new Bitmap(16, 16);
            _icon = new NotifyIcon();
            _form = new WPMHistoryForm();
            UpdateIconText(0);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;
            _icon.DoubleClick += new EventHandler(_icon_DoubleClick);

            _mgr = new KeyHookManager();
            _mgr.KeyDown += new KeyEventHandler(_mgr_KeyDown);

            _thread = new Thread(ThreadGo);
            _thread.Start();

            Application.Run();
        }

        static void _icon_DoubleClick(object sender, EventArgs e)
        {
            _form.Show();
        }

        static void _mgr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space || e.KeyData == Keys.Return || e.KeyData == Keys.Tab || e.KeyData == Keys.OemPeriod || e.KeyData == Keys.OemQuestion)
            {
                if (!_lastWasSeperator)
                {
                    _lastWasSeperator = true;
                    lock (_wpmCounter)
                    {
                        _wpmCounter.Add(DateTime.Now);
                    }
                }
            }
            else
            {
                _lastWasSeperator = false;
            }
        }


        private static void ThreadGo()
        {
            float pollsPerMin = 60000 / RUNNING_AVERAGE;
            while (!_stop)
            {
                int cnt = 0;
                lock (_wpmCounter)
                {
                    DateTime now = DateTime.Now;
                    DateTime then = now - new TimeSpan(0, 0, 0, 0, RUNNING_AVERAGE);
                    //list up the old keypress records
                    LinkedList<DateTime> toRemove = new LinkedList<DateTime>();
                    foreach (DateTime d in _wpmCounter)
                    {
                        if (d < then) toRemove.AddLast(d);
                    }
                    //remove old keypresses
                    foreach (DateTime d in toRemove)
                    {
                        _wpmCounter.Remove(d);
                    }
                    cnt = _wpmCounter.Count;
                }
                int wpm = (int)(cnt * pollsPerMin);
                UpdateIconText(wpm);
                _form.AddPoint(wpm);
                lock (_exitMonitor)
                {
                    Monitor.Wait(_exitMonitor, POLL_RATE);
                }
            }
        }

        private static void UpdateIconText(int val)
        {
            Brush backBrush = _slowBrush;
            if (val > 99)
            {
                val -= 100;
                backBrush = _fastBrush;
            }
            if (val > 99)
            {
                val -= 100;
                backBrush = _insaneBrush;
            }
            if (val > 99)
            {
                val = 0;
                backBrush = _wtfBrush;
            }
            using (Graphics g = Graphics.FromImage(_bmp))
            {
                g.FillRectangle(backBrush, 0, 0, 16, 16);
                g.DrawString(val.ToString(), _font, Brushes.White, 1, 1);
            }
            //I bet this throws a IllegalCrossThreadOperationException
            _icon.Icon = Icon.FromHandle(_bmp.GetHicon());
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            lock (_exitMonitor)
            {
                _stop = true;
                Monitor.PulseAll(_exitMonitor);
            }
            _mgr.Stop();
            _icon.Visible = false;
            _icon.Dispose();
            Application.Exit();
            Thread.Sleep(1000);
            Environment.Exit(0);
        }

    }

}
