using System;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Drawing;
using AnAppADay.Utils;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace AnAppADay.BroadcastClipboard.WinApp
{

    static class Program
    {

        static NotifyIcon _icon;
        static SetupForm _setupForm;
        static UdpClient _sendClient;
        static UdpClient _rcvClient;
        static BinaryFormatter _formatter;
        static IPEndPoint _endPoint;
        static IPAddress _multicastIP;
        static bool _bounceBack = false;

        delegate void SingleObjectDelegate(object o);

        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                _icon = new NotifyIcon();

                _formatter = new BinaryFormatter();

                //load the icon
                using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.BroadcastClipboard.WinApp.Icon.ico"))
                {
                    _icon.Icon = new Icon(s);
                }

                MenuItem[] items = new MenuItem[3];
                items[2] = new MenuItem("Exit");
                items[2].Click += new EventHandler(Exit_Click);
                items[1] = new MenuItem("About");
                items[1].Click += new EventHandler(About_Click);
                items[0] = new MenuItem("Setup");
                items[0].Click += new EventHandler(Setup_Click);
                _icon.DoubleClick += new EventHandler(Setup_Click);
                _icon.ContextMenu = new ContextMenu(items);
                _icon.Visible = true;

                _multicastIP = IPAddress.Parse(Properties.Settings.Default.MulticastAddress);
                _endPoint = new IPEndPoint(_multicastIP, int.Parse(Properties.Settings.Default.MulticastPort));

                new Thread(SetupListener).Start();
                try
                {
                    SetupSender();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting up sender socket.  Adjust settings?!" + Environment.NewLine + ex.Message, "Broadcast Clipboard");
                }

                _setupForm = new SetupForm();
                //hack to make it create a handle
                _setupForm.Show();
                _setupForm.Hide();

                _icon.Visible = true;
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing" + Environment.NewLine + ex.Message, "Broadcast Clipboard");
                Exit_Click(null, null);
            }
        }

        private static void SetupListener()
        {
            try
            {
                while (true)
                {
                    _rcvClient = new UdpClient(int.Parse(Properties.Settings.Default.MulticastPort), AddressFamily.InterNetwork);
                    _rcvClient.MulticastLoopback = false;
                    _rcvClient.JoinMulticastGroup(_multicastIP, 1);
                    while (true)
                    {
                        try
                        {
                            IPEndPoint remoteHost = null;
                            byte[] data = _rcvClient.Receive(ref remoteHost);
                            if (data.Length > 0)
                            {
                                MemoryStream ms = new MemoryStream(data);
                                ms.Position = 0;
                                object o = _formatter.Deserialize(ms);
                                _setupForm.Invoke(new SingleObjectDelegate(SetClipboardData), o);
                            }
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    try { _rcvClient.Close(); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting up receiving thread, adjust settings and restart" + Environment.NewLine + ex.Message, "Broadcast Clipboard");
            }
        }

        private static void SetClipboardData(object o)
        {
            try
            {
                _bounceBack = true;
                Clipboard.SetData(DataFormats.Text, o);
                //make sure the WM_CLIPBOARD message hits here!  We want _bounceback to be true when it hits
                Thread.Sleep(300);
                Application.DoEvents();
                Thread.Sleep(300);
                Application.DoEvents();
                _bounceBack = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting clipboard data" + Environment.NewLine + ex.Message, "Broadcast Clipboard");
            }
        }

        private static void SetupSender()
        {
            _sendClient = new UdpClient();
            _sendClient.MulticastLoopback = false;
            _sendClient.EnableBroadcast = true;
            _sendClient.Ttl = short.Parse(Properties.Settings.Default.MulticastTTL);
            _sendClient.Client.SendBufferSize = 1048576;
        }

        public static void SendClipboardData()
        {
            try
            {
                if (!_bounceBack && Clipboard.GetDataObject() != null)
                {
                    object o = Clipboard.GetDataObject().GetData(DataFormats.Text, true);
                    if (o != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        _formatter.Serialize(ms, o);
                        byte[] data = ms.ToArray();
                        _sendClient.Send(data, data.Length, _endPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending clipboard, adjust settings and restart" + Environment.NewLine + ex.Message, "Broadcast Clipboard");
            }
        }

        static void Setup_Click(object sender, EventArgs e)
        {
            _setupForm.Show();
            _setupForm.Activate();
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            //one of the rare instances I'd like an old sk00l ON ERROR RESUME NEXT
            try { _setupForm.RemoveClipboardHandler(); }
            catch { }
            try { _icon.Visible = false; }
            catch { }
            try { _icon.Dispose(); }
            catch { }
            try { Application.Exit(); }
            catch { }
            Environment.Exit(0);
        }

    }

}