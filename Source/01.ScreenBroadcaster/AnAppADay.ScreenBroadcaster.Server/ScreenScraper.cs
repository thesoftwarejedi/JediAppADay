using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace AnAppADay.ScreenBroadcaster.Server
{

    class ScreenScraper
    {

        private static ScreenScraper instance = new ScreenScraper();

        private Size _size;
        private Bitmap _bmp;
        private Graphics _graphics;
        private byte[] _image;
        private object _imageMutex = new object();
        private bool STOP = false;
        private Thread _thread;

        private ScreenScraper()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            _size = new Size(width, height);
            _bmp = new Bitmap(width, height);
            _graphics = Graphics.FromImage(_bmp);
            _thread = new Thread(ScreenLoop);
            _thread.Start();
        }

        public static ScreenScraper Instance
        {
            get { return instance; }
        }

        private void ScreenLoop()
        {
            while (!STOP)
            {
                try
                {
                    _graphics.CopyFromScreen(0, 0, 0, 0, _size, CopyPixelOperation.SourceCopy);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        _bmp.Save(ms, ImageFormat.Jpeg);
                        //not sure if byte arrays are volatile, so I'll just play safe
                        lock (_imageMutex)
                        {
                            _image = ms.ToArray();
                            Monitor.PulseAll(_imageMutex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //?????  screensaver?  locked pc???
                }
                Thread.Sleep(5000);
            }
        }

        public byte[] GetImage()
        {
            //not sure if byte arrays are volatile, so I'll just play safe
            lock (_imageMutex)
            {
                while (_image == null)
                {
                    Monitor.Wait(_imageMutex);
                }
                return _image;
            }
        }

    }

}
