using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Torian.Magnifier
{
    public class Stuff
    {

        #region Dll Imports

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern bool GetCaretPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        static extern void SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern void SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll")]
        static extern void FreeConsole();

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern void UnhookWinEvent(IntPtr hook);

        const uint EVENT_CONSOLE_CARET = 0x00004001;
        const uint EVENT_CONSOLE_END_APPLICATION = 0x00004007;

        private delegate int WinEventProc(int hWinEventHook, int idEvent, int hwnd, 
                                            int idObject, int idChild, int dwEventThread, 
                                            int dwmsEventTime);

        private static WinEventProc winEventProc = new WinEventProc(DoWinEvent);

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        #endregion

        public static Thread caretThread;
        public static Object caretMutex = new Object();
        private static Point caretPoint;
        public static bool caretThreadStop = false;
        private static bool createWinEventProxyFlag = false;
        private static IntPtr caretEventHook = IntPtr.Zero;
        private static uint consoleProcessId = 0;
        private static int consoleCharWidth = 8;
        private static int consoleCharHeight = 13;

        public static int consoleCaretX;
        public static int consoleCaretY;

        private Stuff() { }

        public static void StartThread() {
            if (caretThread != null) throw new Exception("Only one at a time fool");
            caretThread = new Thread(CaretThreadRun);
            caretThread.Start();
        }

        public static void CaretThreadRun()
        {
            uint processId;
            bool attachedToConsole = false;
            while (!caretThreadStop)
            {
                lock (caretMutex)
                {
                    Monitor.Wait(caretMutex); //wait for the pulse
                    try
                    {
                        if (caretThreadStop) break;
                        caretPoint = Point.Empty;
                        IntPtr activeWindow = GetForegroundWindow();
                        uint otherThreadId = GetWindowThreadProcessId(activeWindow, out processId);
                        if (attachedToConsole && processId == consoleProcessId)
                        {
                            //give caret position from winhook
                            caretPoint.X = consoleCaretX * consoleCharWidth;
                            caretPoint.Y = consoleCaretY * consoleCharHeight;
                            POINT p = caretPoint;
                            ClientToScreen(activeWindow, ref p);
                            caretPoint = p;
                        }
                        else
                        {
                            if (attachedToConsole)
                            {
                                try
                                {
                                    //detach and unhook
                                    UnhookWinEvent(caretEventHook);
                                    //FreeConsole();
                                }
                                finally
                                {
                                    caretEventHook = IntPtr.Zero;
                                    consoleProcessId = 0;
                                    attachedToConsole = false;
                                }
                            }
                            //begin processing expecting standard window
                            uint myThreadId = GetCurrentThreadId();
                            if (otherThreadId != myThreadId)
                            {
                                if (AttachThreadInput(otherThreadId, myThreadId, true)) //if attach success
                                {
                                    try
                                    {
                                        POINT p;
                                        if (GetCaretPos(out p)) //if got caret
                                        {
                                            if (p.X > 0 || p.Y > 0) //if valid point
                                            {
                                                IntPtr focused = GetFocus();
                                                if (focused != IntPtr.Zero) //if focus control found
                                                {
                                                    if (ClientToScreen(focused, ref p)) //if convert success
                                                    {
                                                        if (p.X > 0 || p.Y > 0) //if valid point
                                                        {
                                                            caretPoint = p;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //attempt attach to console
                                                //if (AttachConsole(processId))
                                                {
                                                    //sweet, we got a console, lets hook into it
                                                    //and we'll get events next time...
                                                    consoleProcessId = processId;
                                                    createWinEventProxyFlag = true;
                                                    //make the message loop thread call this...  
                                                    //yucky code!
                                                    Monitor.Pulse(caretMutex);
                                                    Monitor.Wait(caretMutex);
                                                    attachedToConsole = true;
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        AttachThreadInput(otherThreadId, myThreadId, false);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if(DEBUG)
                        Debugger.Break();
                        //MessageBox.Show("ERROR: " + ex.Message + Environment.NewLine + ex.StackTrace);
#endif
                    }
                    finally
                    {
                        Monitor.Pulse(caretMutex); //pulse back
                    }
                }
            }
        }

        public static int DoWinEvent(int hWinEventHook, int idEvent, int hwnd,
                                            int idObject, int idChild, int dwEventThread,
                                            int dwmsEventTime)
        {
            //fired by console window caret location change
            consoleCaretX = GetLowWord(idChild);
            consoleCaretY = GetHighWord(idChild);
            return 1;
        }

        public static int GetLowWord(int pintValue)
        {
            return (pintValue & 0xFFFF);
        }

        public static int GetHighWord(int pintValue)
        {
            if ((pintValue & 0x80000000) == 0x80000000)
            {
                return ((pintValue & 0x7FFF0000) / 0x10000) | 0x8000;
            }
            else
            {
                return (int)((pintValue & 0xFFFF0000) / 0x10000);
            }
        }

        public static bool IsControlHeld()
        {
            short s = GetAsyncKeyState((int)Keys.ControlKey);
            System.Console.WriteLine("Control " + s);
            if (s == -32767 || s == -32768) return true;
            return false;
        }

        public static bool IsShiftHeld()
        {
            short s = GetAsyncKeyState((int)Keys.ShiftKey);
            System.Console.WriteLine("Shift " + s);
            if (s == -32767 || s == -32768) return true;
            return false;
        }

        public static bool GetCaretPosition(out Point p)
        {
            lock (caretMutex)
            {
                Monitor.Pulse(caretMutex);
                Monitor.Wait(caretMutex);
                if (createWinEventProxyFlag)
                {

                    caretEventHook = SetWinEventHook(EVENT_CONSOLE_CARET,
                                                    EVENT_CONSOLE_CARET,
                                                    IntPtr.Zero,
                                                    winEventProc,
                                                    0, 0, //UM?  ONLY WORKS GLOBALLY!  ODD!  FOR NOW, LEAVE
                                                    0 /*out of process*/);
                    createWinEventProxyFlag = false;
                    Monitor.Pulse(caretMutex);
                    Monitor.Wait(caretMutex);
                }
                if (caretPoint != Point.Empty)
                {
                    p = caretPoint;
                    return true;
                }
                else
                {
                    p = Point.Empty;
                    return false;
                }
            }
        }
    }
}
