using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.JediImageShacker.WinApp
{

    public partial class MainForm : Form
    {

        private int files = 0;
        private object mutexLockObj = new object();
        private string[] fileNames = null;
        private string thumbSize = null;

        delegate void SingleStringMethodInvoker(string s);

        public MainForm()
        {
            ServicePointManager.Expect100Continue = false;
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Multiselect = true;
            d.Title = "Select the images to upload";
            if (d.ShowDialog() == DialogResult.OK)
            {
                button2.Enabled = false;
                textBox2.Enabled = false;
                textBox1.Clear();
                thumbSize = textBox2.Text + "x" + textBox2.Text;
                fileNames = d.FileNames;
                new Thread(GoFool).Start();
            }
        }

        private void GoFool()
        {
            lock (mutexLockObj)
            {
                ThreadPool.SetMaxThreads(5, 5);
                foreach (string fileName in fileNames)
                {
                    files++;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(UploadFileToImageShack), fileName);
                }
                while (files > 0)
                {
                    Monitor.Wait(mutexLockObj);
                }
            }
            button2.Invoke(new MethodInvoker(EnableButton));
        }

        private void EnableButton()
        {
            button2.Enabled = true;
            textBox2.Enabled = true;
        }

        private void UploadFileToImageShack(object oFileName)
        {
            try
            {
                string fileName = oFileName as string;
                string contentType = null;
                CookieContainer cookie = new CookieContainer();
                NameValueCollection col = new NameValueCollection();
                col["MAX_FILE_SIZE"] = "3145728";
                col["refer"] = "";
                col["brand"] = "";
                col["optimage"] = "1";
                col["rembar"] = "1";
                col["submit"] = "host it!";
                List<string> l = new List<string>();
                switch (fileName.Substring(fileName.Length - 3, 3))
                {
                    case "jpg":
                        contentType = "image/jpeg";
                        break;
                    case "peg": //courtesy of daemoncollector
                        //only friggin mac users do this crap, so
                        //in reality we don't have to worry about
                        //it but people on the chat are being picky
                        contentType = "image/jpeg";
                        break;
                    case "gif":
                        contentType = "image/gif";
                        break;
                    case "png":
                        contentType = "image/png";
                        break;
                    case "bmp":
                        contentType = "image/bmp";
                        break;
                    case "tif":
                        contentType = "image/tiff";
                        break;
                    //yet another for those mac wanna-be fools
                    case "iff":
                        contentType = "image/tiff";
                        break;
                    default:
                        contentType = "image/unknown";
                        break;
                }
                col["optsize"] = thumbSize;
                string resp = UploadFile.UploadFileEx(fileName,
                                                        "http://www.imageshack.us/index.php",
                                                        "fileupload",
                                                        contentType,
                                                        col,
                                                        cookie,
                                                        "http://www.imageshack.us");
                string thumb = GetLink(resp);
                col["optsize"] = "resample";
                resp = UploadFile.UploadFileEx(fileName,
                                               "http://www.imageshack.us/index.php",
                                               "fileupload",
                                               contentType,
                                               col,
                                               cookie,
                                               "http://www.imageshack.us");
                string full = GetLink(resp);
                StringBuilder sb = new StringBuilder();
                sb.Append("<a href=\"");
                sb.Append(full);
                sb.Append("\"><img src=\"");
                sb.Append(thumb);
                sb.Append("\" /></a>");
                lock (mutexLockObj)
                {
                    textBox1.Invoke(new SingleStringMethodInvoker(textBox1.AppendText), sb.ToString());
                    textBox1.Invoke(new SingleStringMethodInvoker(textBox1.AppendText), Environment.NewLine);
                    textBox1.Invoke(new SingleStringMethodInvoker(textBox1.AppendText), Environment.NewLine);
                    files--;
                    Monitor.PulseAll(mutexLockObj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading " + oFileName + Environment.NewLine + ex.Message);
            }
        }

        private string GetLink(string resp)
        {
            //no regex for the jedi
            string split = "\" /> Direct link to image";
            string[] resps = resp.Split(new string[] { split }, StringSplitOptions.None);
            if (resps.Length == 1) throw new Exception("No image link returned");
            return resps[0].Substring(resps[0].LastIndexOf('\"') + 1);
        }

    }

}