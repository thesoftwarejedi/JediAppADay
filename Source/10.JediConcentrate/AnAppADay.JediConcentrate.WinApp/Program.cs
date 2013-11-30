using System;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;
using System.IO;

namespace AnAppADay.JediConcentrate.WinApp
{
    static class Program
    {
        static Form _mainForm;
        static KeyHookManager _keyMgr;
        static NotifyIcon _icon;

        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

                _mainForm = new Form1();
                //creates the hWnd
                _mainForm.Show();
                _mainForm.Hide();

                _keyMgr = new KeyHookManager();
                _keyMgr.KeyDown += new KeyEventHandler(_keyMgr_KeyDown);
                _keyMgr.KeyPress += new KeyPressEventHandler(_keyMgr_KeyPress);
                _keyMgr.KeyUp += new KeyEventHandler(_keyMgr_KeyUp);

                _icon = new NotifyIcon();

                //load the icon
                using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.JediConcentrate.WinApp.Icon.ico"))
                {
                    _icon.Icon = new System.Drawing.Icon(s);
                }

                MenuItem[] items = new MenuItem[2];
                items[1] = new MenuItem("Exit");
                items[1].Click += new EventHandler(Exit_Click);
                items[0] = new MenuItem("About");
                items[0].Click += new EventHandler(About_Click);
                _icon.ContextMenu = new ContextMenu(items);
                _icon.Visible = true;

                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("JediConcentrate unhandled error: " + ex.Message);
            }
        }

        static void _keyMgr_KeyUp(object sender, KeyEventArgs e)
        {
            if (ShouldHandle(e))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        static void _keyMgr_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((KeyHookManager.IsKeyHeld(Keys.LWin) || KeyHookManager.IsKeyHeld(Keys.RWin)) && e.KeyChar == 'j') ||
               (e.KeyChar == (char)88) ||
               (KeyHookManager.IsKeyHeld(Keys.ControlKey) && e.KeyChar == '/'))
            {
                e.Handled = true;
            }
        }

        static void _keyMgr_KeyDown(object sender, KeyEventArgs e)
        {
            if (ShouldHandle(e))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (_curWin == IntPtr.Zero)
                {
                    if (_mainForm.Visible)
                    {
                        UnConcentrate();
                    }
                    else
                    {
                        Concentrate();
                    }
                }
            }
        }

        static bool ShouldHandle(KeyEventArgs e)
        {
            if (((KeyHookManager.IsKeyHeld(Keys.LWin) || KeyHookManager.IsKeyHeld(Keys.RWin)) && e.KeyCode == Keys.J) ||
               (e.KeyCode == Keys.F12) ||
               (KeyHookManager.IsKeyHeld(Keys.ControlKey) && e.KeyCode == Keys.Oem2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("JediConcentrate unhandled error: " + e.Exception.Message);
        }

        static void About_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            _keyMgr.Stop();
            Application.Exit();
            Environment.Exit(0);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        static IntPtr _curWin = IntPtr.Zero;
        static IntPtr _lastWin = IntPtr.Zero;

        private static void Concentrate()
        {
            _curWin = GetForegroundWindow();
            _lastWin = _curWin;
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new MethodInvoker(ConcentrateThread));
            }
            else
            {
                ConcentrateThread();
            }
        }

        public static void UnConcentrate()
        {
            _curWin = _lastWin;
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new MethodInvoker(UnConcentrateThread));
            }
            else
            {
                UnConcentrateThread();
            }
        }

        private static void ConcentrateThread()
        {
            _mainForm.Opacity = 0;
            _mainForm.Show();
            BringWindowToTop(_curWin);
            while (_mainForm.Opacity < .70)
            {
                Application.DoEvents();
                Thread.Sleep(5);
                _mainForm.Opacity += .04;
            }
            _curWin = IntPtr.Zero;
        }

        private static void UnConcentrateThread()
        {
            _mainForm.Opacity = .70;
            _mainForm.Show();
            BringWindowToTop(_curWin);
            while (_mainForm.Opacity > 0)
            {
                Application.DoEvents();
                Thread.Sleep(5);
                _mainForm.Opacity -= .04;
            }
            _mainForm.Hide();
            _curWin = IntPtr.Zero;
        }
    }
}