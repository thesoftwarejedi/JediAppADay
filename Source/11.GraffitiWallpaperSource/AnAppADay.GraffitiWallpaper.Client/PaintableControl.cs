using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.GraffitiWallpaper.Client
{
    public partial class PaintableControl : PictureBox
    {

        private Graphics _graphics;
        private Pen _pen;
        private Point _lastPoint = Point.Empty;
        private bool _allowDraw = false;

        public PaintableControl()
        {
            InitializeComponent();
            _pen = Pens.Black;
        }

        private void PaintableControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_allowDraw && e.Button == MouseButtons.Left)
            {
                Point newPoint = new Point(e.X, e.Y);
                if (_lastPoint != Point.Empty)
                {
                    _graphics.DrawLine(_pen, _lastPoint, newPoint);
                    Invalidate();
                }
                _lastPoint = newPoint;
            }
        }

        private void PaintableControl_MouseUp(object sender, MouseEventArgs e)
        {
            _lastPoint = Point.Empty;
        }

        public Pen CurrentPen
        {
            get { return _pen; }
            set { _pen = value; }
        }

        public bool AllowDraw
        {
            get { return _allowDraw; }
            set { 
                _allowDraw = value;
                if (value == true)
                {
                    _graphics = Graphics.FromImage(Image);
                }
            }
        }

    }

}
