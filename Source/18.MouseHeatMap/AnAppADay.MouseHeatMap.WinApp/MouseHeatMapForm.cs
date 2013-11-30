using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.MouseHeatMap.WinApp
{

    public partial class MouseHeatMapForm : Form
    {

        const int CIRCLE_SIZE = 29;
        //really should be half - .5
        const int HALF_CIRCLE_SIZE = 14;

        private Dictionary<Point, int> _points;

        public MouseHeatMapForm()
        {
            InitializeComponent();
            Location = new Point(0, 0);
            Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        public Dictionary<Point, int> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_points != null)
            {
                //first get max count to even the spread.
                int max = 0;
                foreach(KeyValuePair<Point, int> p in _points) {
                    if (p.Value > max) max = p.Value;
                }
                //linear spread for colors - blue = cold, yellow = warm, red = hot
                Brush[] brushes = new Brush[max];
                for (int i = 1; i <= max; i++)
                {
                    brushes[i-1] = new SolidBrush(ComputeColor((float)i / max));
                }
                //paint the dots!
                using (Bitmap b = new Bitmap(Width, Height, e.Graphics))
                {
                    Graphics g = Graphics.FromImage(b);
                    g.Clear(Color.Blue);
                    //draw all, lowest first, then to highest
                    for (int i = 1; i <= max; i++)
                    {
                        foreach (KeyValuePair<Point, int> p in _points)
                        {
                            if (p.Value == i)
                            {
                                g.FillEllipse(brushes[p.Value - 1], p.Key.X - HALF_CIRCLE_SIZE, p.Key.Y - HALF_CIRCLE_SIZE, CIRCLE_SIZE, CIRCLE_SIZE);
                            }
                        }
                    }
                    //blur the hell outta it
                    BitmapFilter.GaussianBlur(b, 10);
                    BitmapFilter.GaussianBlur(b, 10);
                    BitmapFilter.GaussianBlur(b, 10);
                    BitmapFilter.GaussianBlur(b, 10);
                    BitmapFilter.GaussianBlur(b, 10);
                    BitmapFilter.GaussianBlur(b, 10);
                    //draw it on the form
                    e.Graphics.DrawImage(b, new Point(0, 0));
                }
            }
        }

        /**
         * Warning, complex code, not for the faint of heart
         * See ColorGradientDoc.bmp to get an idea of what I'm doing here
         */ 
        private Color ComputeColor(float p)
        {
            //p is a float between 0 and 1 where 0 is cold and 1 is hot
            //blue
            int b = 0;
            if (p < .33)
            {
                b = 255;
            }
            else if (p >= .33 && p < .66)
            {
                float temp = (p - .33f) / .33f;
                b = (int)((1-temp) * 255);
            }
            else
            {
                b = 0;
            }

            //green
            int g = 0;
            if (p < .33)
            {
                float temp = p / .33f;
                g = (int)(temp * 255);
            }
            else if (p >= .33 && p < .66)
            {
                g = 255;
            }
            else
            {
                float temp = (p - .66f) / .34f;
                g = (int)((1 - temp) * 255);
            }

            //red
            int r = 0;
            if (p < .33)
            {
                r = 0;
            }
            else if (p >= .33 && p < .66)
            {
                float temp = (p - .33f) / .33f;
                r = (int)(temp * 255);
            }
            else
            {
                r = 255;
            } 
            Color c = Color.FromArgb(115, r, g, b);
            return c;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        private void MouseHeatMapForm_MouseClick(object sender, MouseEventArgs e)
        {
            Close();
        }

    }

}