using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Torian.Magnifier
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            settingsBindingSource.DataSource = Properties.Settings.Default;
        }

        private void OptionsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            settingsBindingSource.EndEdit();
            Properties.Settings.Default.Save();
        }
    }
}