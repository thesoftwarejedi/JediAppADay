using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.CodeSaver.ScreenSaver
{

    static class Program
    {
        private static KeyHookManager _keyHook;
        private static MouseHookManager _mouseHook;

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToLower().Trim().Substring(0, 2) == "/c")
                {
                    MessageBox.Show("This Screen Saver has no options you can set.", "AnAppADay.com Code Saver Screen Saver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (args[0].ToLower() == "/s")
                {
                    StartScreenSaver();
                }
            }
            else
            {
                StartScreenSaver();
            }
        }

        private static void StartScreenSaver()
        {
            _keyHook = new KeyHookManager();
            _keyHook.KeyDown += new KeyEventHandler(_keyHook_KeyDown);
            _mouseHook = new MouseHookManager();
            _mouseHook.OnMouseActivity += new MouseEventHandler(_mouseHook_OnMouseActivity);
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                new Thread(new ParameterizedThreadStart(delegate(object o)
                {
                    Go((int)o);
                })).Start(i);
            }
        }

        static void Go(int i)
        {
            System.Windows.Forms.Application.Run(new ScreenSaverForm(i));
        }

        static void _mouseHook_OnMouseActivity(object sender, MouseEventArgs e)
        {
            _mouseHook.Stop();
            _keyHook.Stop();
            Environment.Exit(0);
        }

        static void _keyHook_KeyDown(object sender, KeyEventArgs e)
        {
            _mouseHook.Stop();
            _keyHook.Stop();
            Environment.Exit(0);
        }

    }

}