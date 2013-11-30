using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Torian.Magnifier
{
    class MagnifierNativeWindow : NativeWindow
    {
        private MagnifierForm f;

        public MagnifierNativeWindow(MagnifierForm f)
            : base()
        {
            this.f = f;
            AssignHandle(f.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)Stuff.WindowsMessages.WM_MOUSEWHEEL)
            {
                if (Stuff.IsControlHeld())
                {
                    if (m.WParam.ToInt32() > 0)
                        f.m_Zoom += MagnifierForm.ZOOM_AMT;
                    else
                        f.m_Zoom -= MagnifierForm.ZOOM_AMT;
                    if (f.m_Zoom < MagnifierForm.ZOOM_MIN)
                        f.m_Zoom = MagnifierForm.ZOOM_MIN;
                    f.Invalidate();
                }
                else if (Stuff.IsShiftHeld())
                {
                    double opacity = f.Opacity;
                    if (m.WParam.ToInt32() > 0)
                    {
                        opacity += .05;
                    }
                    else
                    {
                        opacity -= .05;
                    }
                    if (opacity > 1) opacity = 1;
                    if (opacity < 0) opacity = 0;
                    f.Opacity = opacity;
                }
                else
                {
                    int width = f.Width;
                    int height = f.Height;
                    int delta = 0;
                    if (m.WParam.ToInt32() > 0)
                    {
                        delta = 25;
                    }
                    else
                    {
                        delta = -25;
                    }
                    width += delta;
                    height += delta;
                    if (width < 20) width = 20;
                    if (height < 20) height = 20;
                    f.paint = false;
                    f.Width = width;
                    f.Height = height;
                    f.Recenter(Control.MousePosition.X, Control.MousePosition.Y);
                    f.paint = true;
                    f.Invalidate();
                }
            }/*
            else if (m.Msg == (int)Stuff.WindowsMessages.WM_ACTIVATE ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_ACTIVATEAPP ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_NCACTIVATE ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_WINDOWPOSCHANGING)
            {
                //nada
            }*/
            else if (m.Msg == (int)Stuff.WindowsMessages.WM_LBUTTONDOWN ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_LBUTTONUP ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_LBUTTONDBLCLK ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_RBUTTONDOWN ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_RBUTTONUP ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_RBUTTONDBLCLK ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_MBUTTONDOWN ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_MBUTTONUP ||
                     m.Msg == (int)Stuff.WindowsMessages.WM_MBUTTONDBLCLK)
            {
                SendEventBelow((int)m.Msg);
            }
            else if (m.Msg == (int)Stuff.WindowsMessages.WM_MOUSEMOVE ||
                m.Msg == (int)Stuff.WindowsMessages.WM_MOUSEHOVER)
            {
                SendEventBelow((int)m.Msg);
                base.WndProc(ref m);
            }
            else if (m.Msg == (int)Stuff.WindowsMessages.WM_NCHITTEST)
            {
                //m.LParam = new IntPtr(-1);
                base.WndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void SendEventBelow(int p)
        {
            IntPtr hwnd = GetWindowBehind();
            f.BringToFront();
            Stuff.RECT rect;
            Stuff.GetWindowRect(hwnd, out rect);
            IntPtr lparam = new IntPtr(Control.MousePosition.X - rect.Left | (Control.MousePosition.Y - rect.Top) << 16);
            Stuff.PostMessage(hwnd, p, new IntPtr(), lparam);
        }

        private IntPtr GetWindowBehind()
        {
            Point mousePoint = new Point(Control.MousePosition.X, Control.MousePosition.Y);
            double opacity = f.Opacity;
            f.paint = false;
            f.Opacity = 0;
            IntPtr hwnd = Stuff.WindowFromPoint(mousePoint);
            f.Opacity = opacity;
            f.paint = true;
            return hwnd;
        }
    }
}
