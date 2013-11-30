using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.MissileCommand.WinApp
{

    static class Program
    {

        private static MouseHookManager _mouseHook;
        private static Random _random = new Random();
        private static int _screenBottom = Screen.PrimaryScreen.Bounds.Bottom;
        private static int _screenWidth = Screen.PrimaryScreen.Bounds.Width;
        private static MissileForm _dumbForm;
        private static NotifyIcon _icon;

        const int RANDOM_AMT = 8;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            _icon = new NotifyIcon();
            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.MissileCommand.WinApp.App.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[2];
            items[1] = new MenuItem("Exit");
            items[1].Click += new EventHandler(Exit_Click);
            items[0] = new MenuItem("About");
            items[0].Click += new EventHandler(About_Click);
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;

            _dumbForm = new MissileForm();
            _dumbForm.Location = new Point(-1000, -1000);
            _dumbForm.Show();
            _dumbForm.Hide();

            _mouseHook = new MouseHookManager();
            _mouseHook.OnMouseActivity += new MouseEventHandler(_mouseHook_OnMouseActivity);

            Application.Run();
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            _dumbForm.Close();
            Application.DoEvents();
            Application.Exit();
            Environment.Exit(0);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled Exception in Connection Monitor: " + e.Exception.Message + Environment.NewLine + Environment.NewLine + e.Exception.StackTrace);
        }

        static void _mouseHook_OnMouseActivity(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_random.Next(0, RANDOM_AMT) == 0)
                {
                    //FIRE!!!!!!!!!!
                    Thread t = new Thread(new ParameterizedThreadStart(FireMissile));
                    t.Start(e.Location);
                }
            }
        }

        private static object CreateMissileForm(Point p) {
            MissileForm mf = new MissileForm();
            mf.Location = p;
            mf.Visible = true;
            return mf;
        }

        private delegate object CreateMissileFormDelegate(Point p);

        private static void FireMissile(object objPoint)
        {
            //TEMP SO I CAN DEBUG
            //_mouseHook.Stop();

            Point point = (Point)objPoint;
            //get random position across bottom of screen
            int x = _random.Next(0, _screenWidth);
            Point currentLocation = new Point(x, _screenBottom);

            MissileForm mf = (MissileForm)_dumbForm.Invoke(new CreateMissileFormDelegate(CreateMissileForm), currentLocation);

            //now we have to slowly move the Missile to the point clicked
            //start: currentLocation
            //end: point
            int distance = (int)Math.Sqrt(Math.Pow(point.X - currentLocation.X, 2) + Math.Pow(point.Y - currentLocation.Y, 2));
            int frames = distance / 8; //pixels per frame
            float deltaX = (point.X - currentLocation.X) / (float)frames;
            float deltaY = (point.Y - currentLocation.Y) / (float)frames;
            //don't allow 0 delta.  Instead do 1 or -1 depending on position
            if (deltaX == 0) deltaX = point.X < currentLocation.X ? -1 : 1;
            if (deltaY == 0) deltaY = point.Y < currentLocation.Y ? -1 : 1;
            //precision
            float _curX = currentLocation.X;
            float _curY = currentLocation.Y;
            //more the required number of frames for the specified delta over each direction
            while (frames-- >= 0)
            {
                //adjust for image center
                SetMissileLocation(new Point((int)_curX - 50, (int)_curY - 50), mf);
                //move
                _curX += deltaX;
                _curY += deltaY;
                //check step over bounds
                if (deltaX < 0)
                {
                    //heading left
                    if (_curX < point.X)
                        _curX = point.X;
                }
                else
                {
                    //heading right
                    if (_curX > point.X)
                        _curX = point.X;
                }
                if (deltaY < 0)
                {
                    //heading up
                    if (_curY < point.Y)
                        _curY = point.Y;
                }
                else
                {
                    //heading down
                    if (_curY > point.Y)
                        _curY = point.Y;
                }
                Thread.Sleep(20);
            }
            //ok, we're there.  Now we need to explode
            mf.Invoke(new ExplodeDelegate(mf.Explode), 10);
            Thread.Sleep(200);
            mf.Invoke(new ExplodeDelegate(mf.Explode), 30);
            Thread.Sleep(200);
            mf.Invoke(new ExplodeDelegate(mf.Explode), 50);
            Thread.Sleep(200);
            mf.Invoke(new ExplodeDelegate(mf.Explode), 30);
            Thread.Sleep(200);
            mf.Invoke(new ExplodeDelegate(mf.Explode), 10);
            Thread.Sleep(300);
            //DIE FORM, DIE!
            _dumbForm.Invoke(new MethodInvoker(mf.Dispose));
        }

        delegate void ExplodeDelegate(int i);

        delegate void SinglePointDelegate(Point p, MissileForm mf);

        private static void SetMissileLocation(Point point, MissileForm mf)
        {
            //thread switch to message loop if required
            if (mf.InvokeRequired)
            {
                mf.Invoke(new SinglePointDelegate(InvokedSetMissileLocation), point, mf);
            }
            else
            {
                InvokedSetMissileLocation(point, mf);
            }
        }

        private static void InvokedSetMissileLocation(Point point, MissileForm mf)
        {
            mf.Location = point;
        }

    }

}