using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

using System.Windows.Forms;


namespace Torian.Magnifier
{

    public class UserActivityHook : object
    {

        public UserActivityHook()
        {
            Start();
        }

        ~UserActivityHook()
        {
            Stop();
        }

        public event MouseEventHandler OnMouseActivity;
        public event KeyEventHandler KeyDown;
        public event KeyPressEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        static int hMouseHook = 0; 
        static int hKeyboardHook = 0;

        public const int WH_MOUSE_LL = 14;	
        public const int WH_KEYBOARD_LL = 13;	

        HookProc MouseHookProcedure;
        HookProc KeyboardHookProcedure;

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

        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode;	//Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
            public int scanCode; // Specifies a hardware scan code for the key. 
            public int flags;  // Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            public int time; // Specifies the time stamp for this message.
            public int dwExtraInfo; // Specifies extra information associated with the message. 
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

            if (hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                    KeyboardHookProcedure,
                    Marshal.GetHINSTANCE(
                    Assembly.GetExecutingAssembly().GetModules()[0]),
                    0);

                if (hKeyboardHook == 0)
                {
                    Stop();
                    throw new Exception("SetWindowsHookEx ist failed.");
                }
            }
        }

        public void Stop()
        {
            bool retMouse = true;
            bool retKeyboard = true;
            if (hMouseHook != 0)
            {
                retMouse = UnhookWindowsHookEx(hMouseHook);
                hMouseHook = 0;
            }

            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            //if (!(retMouse && retKeyboard)) throw new Exception("UnhookWindowsHookEx failed.");
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

        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, //[in] Specifies the virtual-key code to be translated. 
                                         int uScanCode, // [in] Specifies the hardware scan code of the key to be translated. The high-order bit of this value is set if the key is up (not pressed). 
                                         byte[] lpbKeyState, // [in] Pointer to a 256-byte array that contains the current keyboard state. Each element (byte) in the array contains the state of one key. If the high-order bit of a byte is set, the key is down (pressed). The low bit, if set, indicates that the key is toggled on. In this function, only the toggle bit of the CAPS LOCK key is relevant. The toggle state of the NUM LOCK and SCROLL LOCK keys is ignored.
                                         byte[] lpwTransKey, // [out] Pointer to the buffer that receives the translated character or characters. 
                                         int fuState); // [in] Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise. 

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (KeyDown != null || KeyUp != null || KeyPress != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                if (KeyDown != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyDown(this, e);
                }
                if (KeyPress != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.vkCode,
                            MyKeyboardHookStruct.scanCode,
                            keyState,
                            inBuffer,
                            MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPress(this, e);
                    }
                }
                if (KeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyUp(this, e);
                }

            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
    }
}
