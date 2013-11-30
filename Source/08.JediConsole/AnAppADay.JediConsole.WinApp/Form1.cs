using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AnAppADay.JediConsole.KeyHook;

namespace AnAppADay.JediConsole.WinApp
{
    public partial class Form1 : Form
    {

        private string _cmd;
        private string _args;
        private StringBuilder _curLine = new StringBuilder();
        private ArrayList _commandBuffer = new ArrayList();
        private int _commandBufferPosition = 1;
        private int _inputEliminateChars = 0;
        Process _process;
        Thread _readOutThread;
        Thread _readErrThread;

        delegate void SingleStringInvoker(string arg);

        public event EventHandler Exiting;

        public Form1(string cmd, string args, double opacity)
        {
            InitializeComponent();
            
            SetLocation();

            _cmd = cmd;
            _args = args;

            Opacity = opacity;
        }

        private void SetLocation()
        {
            Point p = new Point();
            p.X = Screen.PrimaryScreen.WorkingArea.Width / 2 - Width / 2;
            p.Y = 0;
            Location = p;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProcessStartInfo si = new ProcessStartInfo(_cmd, _args);
            si.CreateNoWindow = true;
            si.RedirectStandardError = true;
            si.RedirectStandardInput = true;
            si.RedirectStandardOutput = true;
            si.UseShellExecute = false;
            si.WindowStyle = ProcessWindowStyle.Hidden;

            _process = new Process();
            _process.StartInfo = si;
            _process.Start();

            _readOutThread = new Thread(new ParameterizedThreadStart(ReadOutStream));
            _readOutThread.Start(_process.StandardOutput);
            _readErrThread = new Thread(new ParameterizedThreadStart(ReadErrStream));
            _readErrThread.Start(_process.StandardError);
        }

        void ReadOutStream(object oStream)
        {
            StreamReader read = oStream as StreamReader;
            char[] buffer = new char[1024];
            while (true)
            {
                int cnt = read.Read(buffer, 0, buffer.Length);
                lock (this)
                {
                    int charsToRemove = 0;
                    if (_inputEliminateChars > 0)
                    {
                        if (_inputEliminateChars > cnt)
                        {
                            _inputEliminateChars -= cnt;
                            return;
                        }
                        else
                        {
                            charsToRemove = _inputEliminateChars;
                            cnt -= charsToRemove;
                            _inputEliminateChars = 0;
                            char[] buffer2 = new char[buffer.Length];
                            Array.Copy(buffer, charsToRemove, buffer2, 0, cnt);
                            buffer = buffer2;
                        }
                    }
                    textBox1.Invoke(new SingleStringInvoker(textBox1.AppendText),
                                    new string(buffer, 0, cnt));
                }
            }
        }

        void ReadErrStream(object oStream)
        {
            StreamReader read = oStream as StreamReader;
            char[] buffer = new char[1024];
            while (true)
            {
                int cnt = read.Read(buffer, 0, buffer.Length);
                textBox1.Invoke(new SingleStringInvoker(textBox1.AppendText),
                                new string(buffer, 0, cnt));
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;

                //damn linux fanboys
                if (_curLine.ToString().ToLower() == "ls")
                {
                    _curLine = new StringBuilder("dir");
                }

                if (_curLine.ToString().ToLower() == "exit")
                {
                    Exiting(this, null);
                    Environment.Exit(0);
                }
                else if (_curLine.ToString().StartsWith("jediopacity "))
                {
                    try {
                        int o = Int32.Parse(_curLine.ToString().Split(' ')[1]);
                        if (o > 35 && o <= 100)
                        {
                            Opacity = (double)o / 100;
                        }
                    } catch (Exception) { }
                }
                if (_commandBufferPosition < 1)
                    _commandBufferPosition = 1;
                _commandBuffer.Insert(_commandBufferPosition-1, _curLine.ToString());
                _commandBufferPosition++;

                _inputEliminateChars = _curLine.Length;

                _curLine.Append(Environment.NewLine);
                _process.StandardInput.Write(_curLine.ToString());

                _curLine = new StringBuilder();
            }
            else if (e.KeyChar == '\b')
            {
                MoveToEnd();
                if (_curLine.Length == 0)
                {
                    e.Handled = true;
                }
                else
                {
                    _curLine.Remove(_curLine.Length - 1, 1);
                }
            }
            else if (char.IsLetterOrDigit(e.KeyChar) ||
              char.IsPunctuation(e.KeyChar) ||
              char.IsSeparator(e.KeyChar) ||
              char.IsSymbol(e.KeyChar) ||
              char.IsWhiteSpace(e.KeyChar))
            {
                MoveToEnd();
                _curLine.Append(e.KeyChar);
            }
            else if (e.KeyChar == 22)
            {
                //paste
                MoveToEnd();
                _curLine.Append(Clipboard.GetText());
            }
        }

        private void MoveToEnd()
        {
            textBox1.SelectionStart = textBox1.Text.Length;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            SetLocation();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            Hide();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //38 == up
            //40 == down
            try
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    if (e.KeyCode == Keys.Up)
                        _commandBufferPosition--;
                    else if (e.KeyCode == Keys.Down)
                        _commandBufferPosition++;
                    if (_commandBufferPosition < 0)
                        _commandBufferPosition = 0;
                    else if (_commandBufferPosition > _commandBuffer.Count)
                        _commandBufferPosition = _commandBuffer.Count;
                    MoveToEnd();
                    string curline = _curLine.ToString();
                    _curLine = new StringBuilder();
                    //I hate this shit.  I wish it was a builder or stream
                    textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - curline.Length);
                    string newText = _commandBuffer[_commandBufferPosition-1] as string;
                    textBox1.AppendText(newText);
                    _curLine.Append(newText);
                }
                e.Handled = true;
            }
            catch (Exception)
            {
                //out of bounds, tough shit
            }
        }

    }

}