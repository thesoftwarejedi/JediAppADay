using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.JediConcentrate.WinApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetSize();
        }

        private void SetSize()
        {
            int xMin = int.MaxValue;
            int yMin = int.MaxValue;
            int xMax = int.MinValue;
            int yMax = int.MinValue;
            //here we are figuring out the farthest reaches in
            //all directions for a multi monitor system
            foreach (Screen s in Screen.AllScreens)
            {
                if (s.Bounds.X < xMin)
                    xMin = s.Bounds.X;
                if (s.Bounds.Y < yMin)
                    yMin = s.Bounds.Y;
                if (s.Bounds.X + s.Bounds.Width > xMax)
                    xMax = s.Bounds.X + s.Bounds.Width;
                if (s.Bounds.Y + s.Bounds.Height > yMax)
                    yMax = s.Bounds.Y + s.Bounds.Height;
            }
            Location = new Point(xMin, yMin);
            Size = new Size(xMax, yMax);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg >= 0x201 && m.Msg <= 0x209)
            {
                Program.UnConcentrate();
                m.Result = new IntPtr(1);
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}