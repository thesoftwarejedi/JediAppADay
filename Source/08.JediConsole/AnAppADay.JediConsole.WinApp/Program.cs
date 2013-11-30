using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.JediConsole.KeyHook;

namespace AnAppADay.JediConsole.WinApp
{

    static class Program
    {

        private static Form1 _cmd;
        private static KeyHookManager _hookMgr;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _hookMgr = new KeyHookManager();

            _hookMgr.KeyDown += new KeyEventHandler(_hookMgr_KeyDown);

            _cmd = new Form1("cmd.exe", "", 80);
            _cmd.Exiting += new EventHandler(_cmd_Exiting);
            //start the win loop on nothing.  lazy....
            Application.Run(_cmd);
        }

        static void _cmd_Exiting(object sender, EventArgs e)
        {
            _hookMgr.Stop();
        }

        static void _hookMgr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Oemtilde && IsControlHeld())
            {
                if (_cmd.Visible == true)
                {
                    _cmd.Hide();
                }
                else
                {
                    _cmd.Show();
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static bool IsControlHeld()
        {
            short s = GetAsyncKeyState((int)Keys.ControlKey);
            if (s == -32767 || s == -32768) return true;
            return false;
        }

    }

}