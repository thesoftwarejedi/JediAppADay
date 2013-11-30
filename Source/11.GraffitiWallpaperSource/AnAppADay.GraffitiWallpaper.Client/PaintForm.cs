using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Deployment.Application;
using System.Collections.Specialized;
using System.Web;
using AnAppADay.GraffitiWallpaper.Shared;
using System.IO;

namespace AnAppADay.GraffitiWallpaper.Client
{
    public partial class PaintForm : Form
    {

        private string _server;
        private int _port;
        private HttpClientChannel _cnl;
        private IGraffitiController _controller;
        private MemoryStream _imageStream;

        public PaintForm()
        {
            InitializeComponent();
        }

        private void PaintForm_Load(object sender, EventArgs e)
        {
            //get the server and port
            NameValueCollection nameValueTable = new NameValueCollection();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                nameValueTable = HttpUtility.ParseQueryString(queryString);
            }

            if (!ApplicationDeployment.IsNetworkDeployed || nameValueTable["server"] == null)
            {
                QueryStringForm f = new QueryStringForm();
                f.ShowDialog();
                nameValueTable = new NameValueCollection();
                nameValueTable.Add("server", f.Server);
                nameValueTable.Add("port", f.Port);
            }

            _server = nameValueTable["server"];
            int.TryParse(nameValueTable["port"], out _port);

            //register channel
            _cnl = new HttpClientChannel();
            ChannelServices.RegisterChannel(_cnl, false);

            //lookup graffiti object
            string url = "http://" + _server + ":" + _port + "/AnAppADay.GraffitiWallpaper.Server/GraffitiController";
            _controller = (IGraffitiController)Activator.GetObject(typeof(IGraffitiController), url);

            byte[] bImage = _controller.GetCurrent();
            Image image = null;

            _imageStream = new MemoryStream(bImage);
            image = Image.FromStream(_imageStream);

            paintableControl1.Image = image;
            paintableControl1.Size = image.Size;
            paintableControl1.AllowDraw = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            paintableControl1.AllowDraw = false;
            Application.DoEvents();
            try
            {
                CaptchaInfo captcha = _controller.GetCaptcha();
                CaptchaForm f = new CaptchaForm();
                f.CurrentCaptchaInfo = captcha;
                f.ShowDialog();
                string guess = f.CaptchaGuess;
                byte[] bytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    paintableControl1.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    bytes = ms.ToArray();
                    ms.Close();
                }
                _controller.SaveImage(bytes, captcha.id, guess);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return;
            }
            MessageBox.Show("Success, this application will now exit");
            _imageStream.Close();
            Application.DoEvents();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.Red;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.Green;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.Blue;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.Yellow;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.White;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.Black;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            paintableControl1.CurrentPen = Pens.Fuchsia;
        }

        private bool warned = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!warned)
            {
                MessageBox.Show("You must finish within 60 seconds.  Keeping this open too long results in old images replacing newer ones!");
            }
            else
            {
                //I bet this is the first line of code hacked.  
                //I should have made server ticket based
                MessageBox.Show("Sorry, your time has expired");
                Environment.Exit(0);
            }
        }
    }
}