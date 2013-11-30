using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.IO;
using System.Drawing;

namespace AnAppADay.GoogleAnyText.WinApp
{

    static class Program
    {

        private static KeyHookManager _keyHook;
        private static NotifyIcon _icon;

        static void Main()
        {
            _keyHook = new KeyHookManager();
            _keyHook.KeyDown += new KeyEventHandler(_keyHook_KeyDown);
            
            _icon = new NotifyIcon();

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.GoogleAnyText.WinApp.Icon.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[2];
            items[1] = new MenuItem("Exit");
            items[1].Click += new EventHandler(Exit);
            items[0] = new MenuItem("About");
            items[0].Click += new EventHandler(About);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;
            
            Application.Run();
        }

        static void About(object sender, EventArgs e)
        {
            MethodInvoker i = delegate()
            {
                System.Diagnostics.Process.Start("http://www.anappaday.com");
            };
            new Thread(new ThreadStart(i)).Start();
        }

        static void Exit(object sender, EventArgs e)
        {
            //one of the rare instances I'd like an old sk00l ON ERROR RESUME NEXT
            try { _icon.Visible = false; }
            catch { }
            try { _icon.Dispose(); }
            catch { }
            try { _keyHook.Stop(); }
            catch { }
            try { Application.Exit(); }
            catch { }
            Environment.Exit(0);
        }
        
        static void _keyHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11 && KeyHookManager.IsKeyHeld(Keys.ControlKey))
            {
                DoYourThang();
            }
        }

        private static void DoYourThang()
        {
            //get the focused control
            IntPtr forgroundWindow = WinApi.GetForegroundWindow();
            if (forgroundWindow != IntPtr.Zero)
            {
                string selText = null;
                uint myThreadId = WinApi.GetCurrentThreadId();
                uint otherProcessId = 0;
                uint otherThreadId = WinApi.GetWindowThreadProcessId(forgroundWindow, out otherProcessId);
                //attach ourselves
                if (WinApi.AttachThreadInput(myThreadId, otherThreadId, true))
                {
                    //get the focused control
                    IntPtr ptr = WinApi.GetFocus();
                    //what kinda control is it
                    StringBuilder className = new StringBuilder(4096);
                    WinApi.GetClassName(ptr, className, 4096);
                    //get the text of the control
                    HandleRef r = new HandleRef(_keyHook, ptr);
                    StringBuilder sb = new StringBuilder(1048576);
                    WinApi.SendMessageGetText(r, (uint)WinApi.WindowsMessages.WM_GETTEXT, 1048576, sb);
                    string textContents = sb.ToString();
                    if (textContents.Length > 0)
                    {
                        //get the start and end of the control
                        int start = 0;
                        int end = 0;
                        WinApi.SendMessageGetSel(r, (uint)WinApi.WindowsMessages.EM_GETSEL, ref start, ref end);
                        if (end <= textContents.Length)
                        {
                            selText = textContents.Substring(start, end - start);
                            if (selText != null && selText.Trim().Length == 0)
                            {
                                selText = null;
                            }
                        }
                    }
                    //detach
                    WinApi.AttachThreadInput(myThreadId, otherThreadId, false);
                }
                else
                {
                    int i = Marshal.GetLastWin32Error();
                }
                if (selText != null)
                {
                    //We got highlighted text...
                    MethodInvoker i = delegate()
                    {
                        Process.Start("http://www.google.com/search?q=" + HttpUtility.UrlEncode(selText));
                    };
                    new Thread(new ThreadStart(i)).Start();
                }
            }
         
        }
        
    }

}