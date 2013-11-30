using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AnAppADay.Utils;

namespace AnAppADay.JediWindowDock.WinApp
{

    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
            button1_Click(null, null);
        }

        public void AddAppTab(IntPtr child)
        {
            if (tabControl1.TabPages[child.ToString()] != null)
            {
                throw new Exception("That window is already on a tab");
            }
            StringBuilder sb = new StringBuilder();
            WinApi.GetWindowText(child, sb, 30);
            string title = sb.ToString();
            if (title != null && title.Trim().Length > 0)
            {
                TabPage tp = new TabPage(sb.ToString());
                tp.Name = child.ToString();
                tabControl1.TabPages.Add(tp);
                AppWrapperControl a = new AppWrapperControl();
                a.Dock = DockStyle.Fill;
                tp.Controls.Add(a);
                a.Child = child;
            }
        }

        private class WinInfo
        {
            public string title;
            public IntPtr win;
            public override string ToString()
            {
                return title;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            //kewl - anony methods in c#  - almost as good as java now!
            WinApi.EnumDesktopWindows(IntPtr.Zero, 
                delegate(IntPtr win, int i)
                {
                    //filter the windows:
                    if (win == Handle) {
                        //not ourselves
                    } else if ((WinApi.GetWindowLongPtr(win, -20).ToInt32() & 0x00000080) > 0) {
                        //tool window
                    }
                    else if (!WinApi.IsWindowVisible(win))
                    {
                        //window not visible
                    }
                    else
                    {
                        //maybe....
                        StringBuilder sb = new StringBuilder();
                        WinApi.GetWindowText(win, sb, 100);
                        if (sb.Length > 0)
                        {
                            //THIS is a good window!
                            WinInfo wi = new WinInfo();
                            wi.title = sb.ToString();
                            wi.win = win;
                            listBox1.Items.Add(wi);
                        }
                    }
                    return true;
                }, 
                IntPtr.Zero);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WinInfo wi = listBox1.SelectedItem as WinInfo;
            if (wi != null)
            {
                AddAppTab(wi.win);
                listBox1.Items.Remove(wi);
            }
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseTab(true);
        }

        private void popoutAppToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CloseTab(false);
        }

        private void CloseTab(bool closeApp)
        {
            TabPage tp = tabControl1.SelectedTab;
            CloseTab(closeApp, tp);
        }

        private void CloseTab(bool closeApp, TabPage tp)
        {
            if (tp != tabControl1.TabPages[0]) //not the first tab!
            {
                AppWrapperControl aw = tp.Controls[0] as AppWrapperControl;
                if (aw != null)
                {
                    aw.CloseApp = closeApp;
                    aw.DieDieDie();
                    tabControl1.TabPages.Remove(tp);
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (TabPage tp in tabControl1.TabPages)
            {
                CloseTab(false, tp);
            }
            //base.OnClosing(e);
            //stop the message loop
            Application.Exit();
            //wait before forcing suicide
            System.Threading.Thread.Sleep(1000);
            //kill ourselves
            Environment.Exit(0);
        }

    }

}