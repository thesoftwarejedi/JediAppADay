using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;

namespace AnAppADay.JediDigg.WinApp
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            if (Properties.Settings.Default.categories != null)
            {
                checkedListBox1.SuspendLayout();
                foreach (string s in Properties.Settings.Default.categories)
                {
                    int i = checkedListBox1.FindString(s);
                    if (i >= 0)
                    {
                        checkedListBox1.SetItemChecked(i, true);
                    }
                }
                checkedListBox1.ResumeLayout();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Program.LoginToDigg(textBox1.Text, textBox2.Text))
            {
                MessageBox.Show(this, "Your Username or Password was Incorrect", "Login error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                PutCheckedItemsInProps();
                Properties.Settings.Default.Save();
                Close();
            }
        }

        private void PutCheckedItemsInProps()
        {
            if (Properties.Settings.Default.categories == null)
            {
                Properties.Settings.Default.categories = new StringCollection();
            }
            else
            {
                Properties.Settings.Default.categories.Clear();
            }
            foreach (string s in checkedListBox1.CheckedItems)
            {
                Properties.Settings.Default.categories.Add(s);
            }
        }
    }
}