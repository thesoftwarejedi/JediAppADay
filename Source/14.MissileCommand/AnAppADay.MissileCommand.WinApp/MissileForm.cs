using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.MissileCommand.WinApp
{

    public partial class MissileForm : Form
    {

        private int _explode = 0;

        public MissileForm()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_explode == 0)
            {
                e.Graphics.FillEllipse(Brushes.Gray, 47, 47, 6, 6);
            }
            else
            {
                int loc = 50 - (_explode / 2);
                e.Graphics.FillEllipse(Brushes.Red, loc, loc, _explode, _explode);
            }
        }

        public void Explode(int size)
        {
            _explode = size;
            Invalidate();
        }

    }

}