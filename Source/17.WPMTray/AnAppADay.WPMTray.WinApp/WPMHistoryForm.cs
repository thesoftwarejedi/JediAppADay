using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.WPMTray.WinApp
{

    public partial class WPMHistoryForm : Form
    {

        public WPMHistoryForm()
        {
            InitializeComponent();
            SetLocation();
        }

        private void SetLocation()
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(wa.Right - Width, wa.Bottom - Height);
        }

        protected override void OnResize(EventArgs e)
        {
            SetLocation();
            base.OnResize(e);
        }

        public void AddPoint(int p)
        {
            graphControl1.AddPoint(p);
        }

        private void graphControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Hide();
        }

        private void WPMHistoryForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.X <= 20)
            {
                // Cursor left side
                if (e.Y <= 20)
                {
                    // Cursor top left
                    StartResize((int)HitTestValues.HTTOPLEFT, 0);
                }
                else if (e.Y >= 20)
                {
                    // Cursor bottom left
                    StartResize((int)HitTestValues.HTBOTTOMLEFT, 0);
                }
            }
            else
            {
                // Cursor right side
                if (e.Y <= 20)
                {
                    // Cursor top right
                    StartResize((int)HitTestValues.HTTOPRIGHT, 0);
                }
                else if (e.Y >= 20)
                {
                    // Cursor bottom right
                    StartResize((int)HitTestValues.HTBOTTOMRIGHT, 0);
                }
            }
        }

        private const int WM_NCLBUTTONDOWN = 0x00A1;

        private void StartResize(int ht, int lparam)
        {
           ReleaseCapture();
           SendMessage(Handle, WM_NCLBUTTONDOWN, ht, lparam);
        }

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        enum HitTestValues
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21
        }

    }

}