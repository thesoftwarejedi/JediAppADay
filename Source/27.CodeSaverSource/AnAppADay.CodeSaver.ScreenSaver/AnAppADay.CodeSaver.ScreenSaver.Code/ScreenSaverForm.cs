using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.CodeSaver.ScreenSaver
{

    public partial class ScreenSaverForm : Form
    {
        private Point MouseXY;
        private int ScreenNumber;

        public ScreenSaverForm()
        {
            InitializeComponent();
        }

        public ScreenSaverForm(int scrn)
        {
            InitializeComponent();
            ScreenNumber = scrn;
        }

        private void ScreenSaverForm_Load(object sender, System.EventArgs e)
        {
            Bounds = Screen.AllScreens[ScreenNumber].Bounds;
            Cursor.Hide();
            TopMost = true;
        }

        private void OnMouseEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!MouseXY.IsEmpty)
            {
                if (MouseXY != new Point(e.X, e.Y))
                    Close();
                if (e.Clicks > 0)
                    Close();
            }
            MouseXY = new Point(e.X, e.Y);
        }

        private void ScreenSaverForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Close();
        }

    }

}