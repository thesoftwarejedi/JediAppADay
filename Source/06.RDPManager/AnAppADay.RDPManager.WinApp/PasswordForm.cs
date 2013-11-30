using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.RDPManager.WinApp
{
    public partial class PasswordForm : Form
    {

        internal string password;

        public PasswordForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            password = maskedTextBox1.Text;
            Close();
        }
    }
}