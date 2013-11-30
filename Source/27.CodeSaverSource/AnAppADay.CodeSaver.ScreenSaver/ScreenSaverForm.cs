using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.CodeSaver.ScreenSaver
{

    public partial class ScreenSaverForm : Form
    {
        private Point MouseXY;
        private int ScreenNumber;
        private Thread _thread;
        private Random _random = new Random();
        private string _code;
        private object _mutex = new object();

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
            label1.Text = "";
            Bounds = Screen.AllScreens[ScreenNumber].Bounds;
            Cursor.Hide();
            _thread = new Thread(Go);
            _thread.Start();
        }

        private void Go()
        {
            lock (_mutex)
            {
                while (!IsDisposed)
                {
                    string[] files = Directory.GetFiles("AnAppADay.CodeSaver.ScreenSaver.Code", "*.cs");
                    int i = _random.Next(0, files.Length);
                    string code = File.ReadAllText(files[i]);
                    _code = code;
                    Invoke(new MethodInvoker(SetCode));
                    Monitor.Wait(_mutex, 30000);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            lock (_mutex)
            {
                Dispose();
                Monitor.PulseAll(_mutex);
            }
        }

        private void SetCode()
        {
            label1.Text = _code;
        }

    }

}