using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.GoogleWallpaper.WinApp
{
    public partial class OptionsForm : Form
    {
        public int refreshRate;
        public string keywords;
        public bool safeSearch;

        public OptionsForm()
        {
            InitializeComponent();
        }

        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshRate = (int)numericUpDown1.Value;
            keywords = textBox1.Text;
            safeSearch = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = refreshRate;
            textBox1.Text = keywords;
            checkBox1.Checked = safeSearch;
        }

    }
}