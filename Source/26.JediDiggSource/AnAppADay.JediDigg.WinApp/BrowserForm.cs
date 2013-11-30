using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.JediDigg.WinApp
{

    public partial class BrowserForm : Form
    {

        public BrowserForm()
        {
            InitializeComponent();
            if (Width < 100) Width = 100;
            if (Height < 100) Height = 100;
            SetLocation();
            Opacity = (double)Properties.Settings.Default.BrowserAlpha / 100;
        }

        private void SetLocation()
        {
            Location = new Point((Screen.PrimaryScreen.WorkingArea.Right / 2) - (Width / 2), Screen.PrimaryScreen.WorkingArea.Top);
        }

        public void OpenUrl(string url) {
            webBrowser1.Navigate(url);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            Close();
        }

        private void BrowserForm_SizeChanged(object sender, EventArgs e)
        {
            SetLocation();
        }

    }

}