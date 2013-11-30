using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using AnAppADay.GraffitiWallpaper.Shared;
using System.Web;
using System.Drawing;
using System.Web.Caching;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace AnAppADay.GraffitiWallpaper.Server
{

    public class GraffitiController : MarshalByRefObject, IGraffitiController
    {

        Cache _cache = HttpRuntime.Cache;
        Thread _captureThread;
        private Size _size;
        private Font _font;
        private Brush _brush;
        private Random _random;
        private TimeSpan _timeSpan;
        public static bool STOP = false;
        MemoryStream _curImage;
        object _curImageMonitor = new object();

        public GraffitiController()
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            _size = new Size(width, height);
            _font = new Font("Times New Roman", 12);
            _brush = Brushes.Yellow;
            _random = new Random();
            _timeSpan = new TimeSpan(0, 0, 30);
            _captureThread = new Thread(GetWallPaperThreadStart);
            _captureThread.Start();
        }

        void GetWallPaperThreadStart()
        {
            while (!STOP)
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop");
                    string bmpFile = key.GetValue("Wallpaper").ToString();
                    byte[] bytes = File.ReadAllBytes(bmpFile);
                    Image i = null;
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        i = Image.FromStream(ms);
                        lock (_curImageMonitor)
                        {
                            if (_curImage != null)
                            {
                                _curImage.Dispose();
                                _curImage = null;
                            }
                            _curImage = new MemoryStream();
                            i.Save(_curImage, System.Drawing.Imaging.ImageFormat.Jpeg);
                            Monitor.PulseAll(_curImageMonitor);
                        }
                        ms.Close();
                    }
                }
                catch (Exception ex)
                {
                    Program._icon.ShowBalloonTip(10000, "AnAppADay Graffiti Error", ex.Message, ToolTipIcon.Error);
                }
                //wait 60 seconds, then reload the wallpaper 
                //in case it's been reassigned manually
                Thread.Sleep(60000);
            }
        }

        byte[] IGraffitiController.GetCurrent()
        {
            lock (_curImageMonitor)
            {
                while (_curImage == null)
                {
                    Monitor.Wait(_curImageMonitor);
                }
                return _curImage.ToArray();
            }
        }

        CaptchaInfo IGraffitiController.GetCaptcha()
        {
            return CreateCaptcha();
        }

        void IGraffitiController.SaveImage(byte[] image, int id, string captchaString)
        {
            //validate captcha
            if (captchaString == _cache[id.ToString()] as string)
            {
                //valid
                Image newBitmap = null;
                lock (_curImageMonitor)
                {
                    if (_curImage != null)
                    {
                        _curImage.Dispose();
                        _curImage = null;
                    }
                    _curImage = new MemoryStream(image);
                    newBitmap = Image.FromStream(_curImage);
                    string bmpFilename = Directory.GetCurrentDirectory() + "\\AnAppADay.Graffiti.bmp";
                    newBitmap.Save(bmpFilename, System.Drawing.Imaging.ImageFormat.Bmp);
                    WinAPI.SystemParametersInfo(WinAPI.SPI_SETDESKWALLPAPER, 0, bmpFilename, WinAPI.SPIF_UPDATEINIFILE | WinAPI.SPIF_SENDWININICHANGE);
                }
            }
            else
            {
                throw new Exception("The text entered was incorrect");
            }
        }

        private CaptchaInfo CreateCaptcha()
        {
            char[] cText = new char[4];
            cText[0] = (char)_random.Next(97, 123);
            cText[1] = (char)_random.Next(97, 123);
            cText[2] = (char)_random.Next(97, 123);
            cText[3] = (char)_random.Next(97, 123);
            string text = new string(cText);
            Image img = new Bitmap(50, 30);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.DrawString(text, _font, _brush, 4, 4);
            }
            byte[] imgBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgBytes = ms.ToArray();
            }
            CaptchaInfo ci = new CaptchaInfo();
            ci.image = imgBytes;
            ci.id = _random.Next(int.MinValue, int.MaxValue);
            //we add the captcha generated to a cache to lookup upon submission
            //will expire in 30 seconds
            _cache.Add(ci.id.ToString(), text, null, Cache.NoAbsoluteExpiration, _timeSpan, CacheItemPriority.Default, null);
            return ci;
        }

    }

    public class WinAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPIF_UPDATEINIFILE = 0x01;
        public const int SPIF_SENDWININICHANGE = 0x02;
    }  

}

