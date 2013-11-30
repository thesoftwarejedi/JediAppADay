using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AnAppADay.Utils
{

    public class MouseHookManager : object
    {

        public MouseHookManager()
        {
            Start();
        }

        ~MouseHookManager()
        {
            Stop();
        }

        public event MouseEventHandler OnMouseActivity;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        static int hMouseHook = 0; 

        public const int WH_MOUSE_LL = 14;	

        HookProc MouseHookProcedure;

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
            IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,
            Int32 wParam, IntPtr lParam);

        public void Start()
        {
            if (hMouseHook == 0)
            {
                MouseHookProcedure = new HookProc(MouseHookProc);

                hMouseHook = SetWindowsHookEx(WH_MOUSE_LL,
                    MouseHookProcedure,
                    Marshal.GetHINSTANCE(
                        Assembly.GetExecutingAssembly().GetModules()[0]),
                    0);
                if (hMouseHook == 0)
                {
                    Stop();
                    throw new Exception("SetWindowsHookEx failed.");
                }
            }
        }

        public void Stop()
        {
            bool retMouse = true;
            if (hMouseHook != 0)
            {
                retMouse = UnhookWindowsHookEx(hMouseHook);
                hMouseHook = 0;
            }
        }

        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_MOUSEWHEEL = 0x20A;


        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (OnMouseActivity != null))
            {

                MouseButtons button = MouseButtons.None;
                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        break;
                    case WM_MOUSEWHEEL:
                        button = MouseButtons.Middle;
                        break;
                }
                int clickCount = 0;
                if (button != MouseButtons.None)
                    if (wParam == WM_LBUTTONDBLCLK || wParam == WM_RBUTTONDBLCLK) clickCount = 2;
                    else clickCount = 1;

                MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                //dumb up mouse up and down scrolling
                int delta = 0;
                if (button == MouseButtons.Middle)
                {
                    if (MyMouseHookStruct.hwnd > 0)
                    {
                        delta = 1;
                    }
                    else
                    {
                        delta = 2;
                    }
                }
                MouseEventArgs e = new MouseEventArgs(
                                                    button,
                                                    clickCount,
                                                    MyMouseHookStruct.pt.x,
                                                    MyMouseHookStruct.pt.y,
                                                    delta);
                OnMouseActivity(this, e);
            }
            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }

    }

}
