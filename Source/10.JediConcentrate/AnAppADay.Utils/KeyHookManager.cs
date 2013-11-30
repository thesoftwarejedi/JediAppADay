using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;


namespace AnAppADay.Utils
{

    public class KeyHookManager
    {

        public KeyHookManager()
        {
            Start();
        }

        ~KeyHookManager()
        {
            Stop();
        }

        public event KeyEventHandler KeyDown;
        public event KeyPressEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        static int hKeyboardHook = 0;

        public const int WH_KEYBOARD_LL = 13;	

        HookProc KeyboardHookProcedure;

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
            bool retKeyboard = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
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

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public static bool IsKeyHeld(Keys key)
        {
            short s = GetAsyncKeyState((int)key);
            if (s == -32767 || s == -32768) return true;
            return false;
        }

    }
}
