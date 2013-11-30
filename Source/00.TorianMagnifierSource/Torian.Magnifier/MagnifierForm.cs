using System;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Torian.Magnifier.Properties;

namespace Torian.Magnifier
{
    public partial class MagnifierForm : Form
    {
        public const double ZOOM_AMT = .5;
        public const double ZOOM_MIN = 1.5;
        public const double ZOOM_MAX = 10;
        public const int SIZE_MIN = 100;
        public const int SIZE_MAX = 1000;

        public bool refreshOnMouseMove = Settings.Default.RefreshOnMouseMove;
        public double m_Zoom = Settings.Default.ZoomAmount;
        public bool refreshOverInvalidate = Settings.Default.RefreshOverInvalidate;
        public bool followTyping = Settings.Default.FollowTyping;
        public int refreshOnMouseMoveRate = Settings.Default.RefreshOnMouseMoveRate;

        private int destWidth;
        private int destHeight;
        private int srcWidth;
        private int srcHeight;
        private float halfWidth;
        private float halfHeight;
        private int offsetX;
        private int offsetY;
        private int mouseMoveRefreshCount = 1;

        private static readonly Color transColor = Color.Fuchsia;
        private static readonly Brush transBrush = new SolidBrush(transColor);
        private static readonly Point zeroPoint = new Point(0, 0);
        private static Size sizeSrc = new Size();
        private static Rectangle mouseArea = new Rectangle();
        private static Rectangle rectDest = new Rectangle(0, 0, 0, 0);
        private static Rectangle rectSrc = new Rectangle(0, 0, 0, 0);
        private static Region regionMask = new Region();
        private static GraphicsPath path = new GraphicsPath();
        private static Bitmap offscreenBitmap = new Bitmap(10, 10);
        private static Graphics offscreenGraphics = Graphics.FromImage(offscreenBitmap);

        private UserActivityHook hook;

        public enum RecenterType
        {
            Mouse,
            Keyboard
        }

        public MagnifierForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            mouseArea.Width = 5;
            mouseArea.Height = 5;

            hook = new UserActivityHook();
            hook.OnMouseActivity += new MouseEventHandler(hook_OnMouseActivity);
            hook.KeyUp += new KeyEventHandler(hook_KeyUp);
            hook.Start();

            Size = Settings.Default.Size;
            Location = Cursor.Position;
        }

        void hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && Stuff.IsShiftHeld())
            {
                Close();
            }
            else if (e.KeyCode == Keys.Scroll)
            {
                if (this.Visible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                }
            }
            else if (followTyping && (e.KeyValue < 160 || e.KeyValue > 165)) //eliminate modifiers
            {
                Point p;
                if (Stuff.GetCaretPosition(out p))
                {
                    Recenter(p.X, p.Y, RecenterType.Keyboard);
                }
            }
        }

        public void RefreshOrInvalidate()
        {
            if (refreshOverInvalidate)
            {
                Refresh();
            }
            else
            {
                Invalidate();
            }
        }

        void hook_OnMouseActivity(object sender, MouseEventArgs e)
        {
            if (Visible)
            {
                if (e.Button == MouseButtons.Middle)
                {
                    bool ctrl = Stuff.IsControlHeld();
                    bool shift = Stuff.IsShiftHeld();
                    if (ctrl && shift)
                    {
                        double opacity = Opacity;
                        if (e.Delta == 1)
                        {
                            opacity += .05;
                        }
                        else
                        {
                            opacity -= .05;
                        }
                        if (opacity > 1) opacity = 1;
                        if (opacity < 0) opacity = 0;
                        Opacity = opacity;
                    }
                    else if (ctrl)
                    {
                        if (e.Delta == 1)
                            m_Zoom += MagnifierForm.ZOOM_AMT;
                        else
                            m_Zoom -= MagnifierForm.ZOOM_AMT;
                        if (m_Zoom < MagnifierForm.ZOOM_MIN)
                            m_Zoom = MagnifierForm.ZOOM_MIN;
                        if (m_Zoom > MagnifierForm.ZOOM_MAX)
                            m_Zoom = MagnifierForm.ZOOM_MAX;
                        DoMath();
                        RefreshOrInvalidate();
                    }
                    else if (shift)
                    {
                        int width = Width;
                        int height = Height;
                        int delta = 0;
                        if (e.Delta == 1)
                        {
                            delta = 25;
                        }
                        else
                        {
                            delta = -25;
                        }
                        width += delta;
                        height += delta;
                        if (width < SIZE_MIN) width = SIZE_MIN;
                        if (height < SIZE_MIN) height = SIZE_MIN;
                        if (width > SIZE_MAX) width = SIZE_MAX;
                        if (height > SIZE_MAX) height = SIZE_MAX;
                        Width = width;
                        Height = height;
                        if (Width != Height)
                        {
                            if (Width > Height)
                            {
                                Width = Height;
                            }
                            else
                            {
                                Height = Width;
                            }
                        }
                        Recenter(e.X, e.Y, RecenterType.Mouse);
                    }
                    else
                    {
                        RefreshOrInvalidate();
                    }
                }
                else
                {
                    Recenter(e.X, e.Y, RecenterType.Mouse);
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            DoMath();
            base.OnResize(e);
        }

        private void DoMath()
        {
            destWidth = Width + 5; //the 5 extra eliminates the pink on the edges... ?!?!
            destHeight = Height + 5;
            srcWidth = (int)(destWidth / m_Zoom);
            srcHeight = (int)(destHeight / m_Zoom);
            halfWidth = destWidth / 2;
            halfHeight = destHeight / 2;
            offsetX = (int)(halfWidth - halfWidth / m_Zoom);
            offsetY = (int)(halfHeight - halfHeight / m_Zoom);

            offscreenBitmap = new Bitmap(srcWidth, srcHeight);
            offscreenGraphics = Graphics.FromImage(offscreenBitmap);

            rectDest.Width = destWidth;
            rectDest.Height = destHeight;
            rectSrc.Width = srcWidth;
            rectSrc.Height = srcHeight;

            sizeSrc.Width = srcWidth;
            sizeSrc.Height = srcHeight;

            mouseArea.X = (int)halfWidth - (int)(mouseArea.Width / 2);
            mouseArea.Y = (int)halfHeight - (int)(mouseArea.Height / 2);

            path = new GraphicsPath();
            path.AddEllipse(0, 0, destWidth, destHeight);
            regionMask.MakeInfinite();
            regionMask.Exclude(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Visible)
            {
                //where are we
                Point pt = PointToScreen(zeroPoint);
                //vars
                int srcX = (int)(pt.X + offsetX);
                int srcY = (int)(pt.Y + offsetY);
                offscreenGraphics.CopyFromScreen(srcX, srcY, 0, 0, sizeSrc, CopyPixelOperation.SourceCopy);
                //exclude our mouse click area
                e.Graphics.ExcludeClip(mouseArea);
                //exclude our mask
                e.Graphics.ExcludeClip(regionMask);
                //resize and draw from our temp space
                e.Graphics.DrawImage(offscreenBitmap, rectDest, rectSrc, GraphicsUnit.Pixel);
            }
        }

        public void Recenter(int x, int y, RecenterType t)
        {
            if (Visible)
            {
                Location = new Point((int)(x - halfWidth), (int)(y - halfHeight));
                if (refreshOnMouseMove && t == RecenterType.Mouse)
                {
                    if (mouseMoveRefreshCount++ >= refreshOnMouseMoveRate)
                    {
                        mouseMoveRefreshCount = 1;
                        RefreshOrInvalidate();
                    }
                }
                else if (t == RecenterType.Keyboard)
                {
                    RefreshOrInvalidate();
                }
            }
        }

        private void MagnifierForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Size = Size;
            Settings.Default.ZoomAmount = m_Zoom;
        }
        
    }
}