using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.InputPrank.ConsoleApp
{

    static class Program
    {

        private static bool repeatKeys = false;
        private static bool consumeKeys = false;
        private static bool changeKeys = false;
        private static int repeatRate = 0;
        private static int consumeRate = 0;
        private static int changeRate = 0;
        private static KeyHookManager _keyMgr;
        private static Random _random;

        [STAThread]
        static void Main(string[] args)
        {
            ProcessArgs(args);
            _random = new Random();
            _keyMgr = new KeyHookManager();
            _keyMgr.KeyDown += new KeyEventHandler(_keyMgr_KeyDown);
            _keyMgr.KeyPress += new KeyPressEventHandler(_keyMgr_KeyPress);
            _keyMgr.Start();
            Application.Run();
            _keyMgr.Stop();
        }

        static void _keyMgr_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (repeatKeys)
                {
                    int num = _random.Next(1, repeatRate + 1);
                    if (num == 1)
                    {
                        //do the repeat here
                        SendKeys.Send(new string(e.KeyChar, 1));
                    }
                }
            }
            catch
            {
                //shhhhhhhhhhhhhhhh
            }
        }

        static void _keyMgr_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Back && KeyHookManager.IsKeyHeld(Keys.ControlKey)) {
                    _keyMgr.Stop();
                    Environment.Exit(0);
                }
                bool handled = false;
                if (consumeKeys)
                {
                    int num = _random.Next(1, consumeRate + 1);
                    if (num == 1)
                    {
                        //do the consume here
                        e.SuppressKeyPress = true;
                        handled = true;
                    }
                }
                if (!handled && changeKeys)
                {
                    int num = _random.Next(1, consumeRate + 1);
                    if (num == 1)
                    {
                        //do the change here
                        int rnd = _random.Next(-3, 4);
                        if (rnd == 0) //0 is no fun
                        {
                            rnd = 1;
                        }
                        char newKey = (char)((int)e.KeyCode + rnd);
                        //would be better to check state of shift and capslock, but alas, always do lower
                        string newString = new string(newKey, 1).ToLower();
                        SendKeys.Send(newString);
                        e.SuppressKeyPress = true;
                        handled = true;
                    }
                }
            }
            catch
            {
                //shhhhhhhhhhh
            }
        }

        private static void ProcessArgs(string[] args)
        {
            try
            {
                repeatRate = int.Parse(args[0]);
                if (args.Length > 1)
                {
                    consumeRate = int.Parse(args[1]);
                    if (args.Length > 2)
                    {
                        changeRate = int.Parse(args[2]);
                    }
                }
                if (repeatRate > 0)
                {
                    repeatKeys = true;
                }
                if (consumeRate > 0)
                {
                    consumeKeys = true;
                }
                if (changeRate > 0)
                {
                    changeKeys = true;
                }
            }
            catch
            {
                ShowHelp();
                Environment.Exit(-1);
            }
        }

        private static void ShowHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("usage: repeat_rate consume_rate change_rate");
            sb.Append(Environment.NewLine);
            sb.Append("");
            sb.Append(Environment.NewLine);
            sb.Append("All values are numbers indicating probablity of that prank occuring");
            sb.Append(Environment.NewLine);
            sb.Append("where higher is less frequest, 0 is never, and 1 is always");
            sb.Append(Environment.NewLine);
            sb.Append("");
            sb.Append(Environment.NewLine);
            sb.Append("For example, a 5 would make that prank occur randomly one in 5 times");
            sb.Append(Environment.NewLine);
            sb.Append("");
            sb.Append(Environment.NewLine);
            sb.Append("Best prank usage: ankey.exe 500 500 500");
            sb.Append(Environment.NewLine);
            sb.Append("Would make keys repeat, change, or consume 1 in 500 (not too much to suspect)");
            sb.Append(Environment.NewLine);
            sb.Append("");
            sb.Append(Environment.NewLine);
            sb.Append("**** Pressing Ctrl-Backspace will exit the application at anytime ****");
            MessageBox.Show(sb.ToString());
        }

    }

}