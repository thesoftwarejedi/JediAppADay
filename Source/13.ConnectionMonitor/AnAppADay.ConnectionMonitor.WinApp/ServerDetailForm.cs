using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    public partial class ServerDetailForm : Form
    {

        private ServerInfo _info;

        public ServerDetailForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            if (_info == null)
                throw new Exception("Must set current server info first");
            textBox1.Text = _info._serverName;
            numericUpDown1.Value = _info._attempts;
            switch (_info._type)
            {
                case ServerInfoMonitorType.Ping:
                    radioButton1.Select();
                    break;
                case ServerInfoMonitorType.Socket:
                    radioButton2.Select();
                    break;
                case ServerInfoMonitorType.HTTP:
                    radioButton3.Select();
                    break;
                default:
                    break;
            }
        }

        internal ServerInfo CurrentServerInfo
        {
            get { return _info; }
            set { _info = value; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked == false &&
                    radioButton2.Checked == false &&
                    radioButton3.Checked == false)
            {
                MessageBox.Show(this, "You must select a monitor type");
            }
            else
            {
                _info._serverName = textBox1.Text;
                _info._attempts = (int)numericUpDown1.Value;
                if (radioButton1.Checked)
                    _info._type = ServerInfoMonitorType.Ping;
                if (radioButton2.Checked)
                    _info._type = ServerInfoMonitorType.Socket;
                if (radioButton3.Checked)
                    _info._type = ServerInfoMonitorType.HTTP;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            if (textBox1.Text.Trim().Length < 1)
            {
                e.Cancel = true;
            }
        }

    }

}