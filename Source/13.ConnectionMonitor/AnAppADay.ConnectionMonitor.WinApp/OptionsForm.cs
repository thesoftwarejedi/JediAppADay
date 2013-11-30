using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    public partial class OptionsForm : Form
    {

        public OptionsForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            numericUpDown1.Value = Program._retryRate;
            base.OnShown(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Program._retryRate = (int)numericUpDown1.Value;
            lock (Program._mainThreadMutex)
            {
                Monitor.PulseAll(Program._mainThreadMutex);
            }
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

    }

}