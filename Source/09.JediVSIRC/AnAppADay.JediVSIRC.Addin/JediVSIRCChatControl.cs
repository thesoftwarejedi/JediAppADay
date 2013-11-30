using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Meebey.SmartIrc4net;
using Microsoft.Win32;

namespace AnAppADay.JediVSIRC.Addin
{
    public partial class JediVSIRCChatControl : UserControl
    {
        private IrcClient _irc;
        private Thread _thread;
        private string _channel;
        private string _nick;
        private string _realName;
        private string _server;
        private int _port;

        public static JediVSIRCChatControl _instance;

        delegate void SingleStringMethodInvoker(string s);

        public JediVSIRCChatControl()
        {
            InitializeComponent();

            _instance = this;
        }

        public static void LoadAndStart()
        {
            if (_instance == null) return;

            if (_instance._irc != null && _instance._irc.IsConnected)
            {
                _instance._irc.Disconnect();
                _instance._irc = null;
            }

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software", true);
            key = key.CreateSubKey(@"AnAppADay\JediVSIRC");
            try
            {
                _instance._server = key.GetValue("server", "irc.diboo.net") as string;
                try
                {
                    _instance._port = Int32.Parse(key.GetValue("port", "6667") as string);
                }
                catch
                {
                    _instance._port = 6667;
                }
                _instance._nick = key.GetValue("nick", "Anony") as string;
                _instance._realName = key.GetValue("realName", "Anony") as string;
                _instance._channel = key.GetValue("channel", "#softwarejedianappaday") as string;
                key.Close();
            }
            catch
            {
                //_server will be null, fall through
            }

            if (_instance._server == null || _instance._server.Trim() == "")
            {
                _instance.OutputMessage("CONFIGURE USING TOOLS->OPTIONS");
            }
            else
            {
                _instance._thread = new Thread(_instance.Listen);
                _instance._thread.Start();
            }
        }

        void Listen()
        {
            try
            {
                _irc = new IrcClient();
                _irc.AutoRejoin = true;
                _irc.SendDelay = 500;
                _irc.OnConnecting += new EventHandler(_irc_OnConnecting);
                _irc.OnConnected += new EventHandler(_irc_OnConnected);
                _irc.OnConnectionError += new EventHandler(_irc_OnConnectionError);
                _irc.OnDisconnected += new EventHandler(_irc_OnDisconnected);
                _irc.OnChannelMessage += new IrcEventHandler(_irc_OnChannelMessage);
                _irc.OnRawMessage += new IrcEventHandler(_irc_OnRawMessage);
                _irc.OnError += new ErrorEventHandler(_irc_OnError);
                _irc.OnErrorMessage += new IrcEventHandler(_irc_OnErrorMessage);
                _irc.Connect(_server, _port);
                _irc.Login(_nick, _realName);
                _irc.RfcJoin(_channel);
                _irc.Listen();
            }
            catch (Exception ex)
            {
                OutputMessage("Fatal Error: " + ex.Message);
            }
        }

        void _irc_OnRawMessage(object sender, IrcEventArgs e)
        {
            if (e.Data.Channel == null && e.Data.Nick != null)
            {
                OutputLine("PM: <");
                OutputLine(e.Data.Nick);
                OutputLine("> ");
                OutputMessage(e.Data.Message);
            }
        }

        private void OutputLine(string line)
        {
            try
            {
                if (richTextBox1.InvokeRequired)
                {
                    richTextBox1.Invoke(new SingleStringMethodInvoker(richTextBox1.AppendText), line);
                    richTextBox1.Invoke(new MethodInvoker(richTextBox1.ScrollToCaret));
                    Application.DoEvents();
                }
                else
                {
                    richTextBox1.AppendText(line);
                    richTextBox1.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void OutputMessage(string message)
        {
            OutputLine(message);
            OutputLine(Environment.NewLine);
        }

        void _irc_OnChannelMessage(object sender, IrcEventArgs e)
        {
            OutputLine("<");
            OutputLine(e.Data.Nick);
            OutputLine("> ");
            OutputLine(e.Data.Message);
            OutputLine(Environment.NewLine);
        }

        void  _irc_OnDisconnected(object sender, EventArgs e)
        {
            OutputMessage("<-- IRC Disconnected -->");
        }

        void _irc_OnConnectionError(object sender, EventArgs e)
        {
            OutputMessage("<-- IRC Error -->");
        }

        void _irc_OnErrorMessage(object sender, IrcEventArgs e)
        {
            OutputLine("<-- IRC Error: ");
            OutputLine(e.Data.Message);
            OutputLine(" -->");
            OutputLine(Environment.NewLine);
        }

        void _irc_OnError(object sender, ErrorEventArgs e)
        {
            OutputLine("<-- IRC Error: ");
            OutputLine(e.Data.Message);
            OutputLine(" -->");
            OutputLine(Environment.NewLine);
        }

        void  _irc_OnConnected(object sender, EventArgs e)
        {
            OutputMessage("<-- IRC Connected -->");
        }

        void  _irc_OnConnecting(object sender, EventArgs e)
        {
            OutputMessage("<-- IRC Connecting... -->");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string text = textBox1.Text;
                textBox1.Clear();
                if (_irc != null && _irc.IsConnected)
                {
                    if (text.StartsWith("/msg "))
                    {
                        string[] texts = text.Split(' ');
                        _irc.RfcPrivmsg(texts[1], text.Substring(texts[0].Length + texts[1].Length));
                    }
                    else if (text.StartsWith("/nick "))
                    {
                        string[] texts = text.Split(' ');
                        _irc.RfcNick(texts[1]);
                        _nick = texts[1];
                    }
                    else if (text.StartsWith("/msg "))
                    {
                        string[] texts = text.Split(' ');
                        _irc.RfcPrivmsg(texts[1], text.Substring(texts[0].Length + texts[1].Length));
                    }
                    else
                    {
                        _irc.SendMessage(SendType.Message, _channel, text);
                        OutputLine("<");
                        OutputLine(_nick);
                        OutputLine("> ");
                        OutputLine(text);
                        OutputLine(Environment.NewLine);
                    }
                }
                else
                {
                    OutputMessage("NOT CONNECTED");
                }
            }
            catch
            {
                OutputMessage("Invalid syntax");
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                button1_Click(textBox1, null);
            }
        }

        private void JediVSIRCChatControl_Load(object sender, EventArgs e)
        {
            LoadAndStart();
        }
    }
}
