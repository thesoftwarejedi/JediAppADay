using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.WPMTray.WinApp
{

    public partial class GraphControl : UserControl
    {

        const int SAMPLE_WIDTH = 180;

        //linked list will behave better then a arraylist
        //because we're popping out the first item whenever the size gets big.
        //in an array list this would do a new array alloc and copy
        //EVERY time.
        private LinkedList<int> _points = new LinkedList<int>();
        private Font _font = new Font("Times New Roman", 8);

        public GraphControl()
        {
            InitializeComponent();
        }

        public void AddPoint(int point)
        {
            _points.AddLast(point);
            if (_points.Count > SAMPLE_WIDTH)
            {
                _points.RemoveFirst();
            }
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            Invalidate();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_points.Count > 0)
            {
                int[] _pointsCopy = new int[_points.Count];
                _points.CopyTo(_pointsCopy, 0);

                Array.Reverse(_pointsCopy);
                Graphics g = e.Graphics;
                //first calculate the max
                int max = 0;
                foreach (int i in _pointsCopy)
                {
                    if (i > max) max = i;
                }
                if (max > 0)
                {
                    //draw a line for max
                    g.DrawLine(Pens.DarkGray, 0, 10, Width, 10);
                    g.DrawString(max.ToString(), _font, Brushes.DarkGray, 0, 10);
                    int height = Height - 10;
                    if (height > 100)
                    {
                        //draw a line for 25%, 50%, and 75% too
                        //50
                        g.DrawLine(Pens.DarkGray, 0, (height / 2) + 10, Width, (height / 2) + 10);
                        g.DrawString((max / 2).ToString(), _font, Brushes.DarkGray, 0, (height / 2) + 10);
                        //25
                        g.DrawLine(Pens.DarkGray, 0, ((float)(height * .75)) + 10, Width, ((float)(height * .75)) + 10);
                        g.DrawString((max * .75).ToString(), _font, Brushes.DarkGray, 0, (height / 4) + 10);
                        //75
                        g.DrawLine(Pens.DarkGray, 0, (height / 4) + 10, Width, (height / 4) + 10);
                        g.DrawString((max / 4).ToString(), _font, Brushes.DarkGray, 0, ((float)(height * .75)) + 10);
                    }
                    //build a point array
                    float unit = ((float)Width / SAMPLE_WIDTH);
                    int cnt = 1;
                    List<Point> _pts = new List<Point>();
                    foreach (int p in _pointsCopy)
                    {
                        float pctOfMax = p / (float)max;
                        float location = height * pctOfMax;
                        Point pp = new Point(Width - (int)(cnt * unit), Height - (int)location);
                        _pts.Add(pp);
                        cnt++;
                    }
                    Point[] ptArray = _pts.ToArray();
                    g.DrawLines(Pens.LightGreen, ptArray);
                }
            }
        }

    }

}
