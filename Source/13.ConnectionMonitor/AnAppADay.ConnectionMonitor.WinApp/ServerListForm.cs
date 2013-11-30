using System;
using System.Threading;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    public partial class ServerListForm : Form
    {

        public ServerListForm()
        {
            InitializeComponent();
            //here we load the list of existing servers
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("AnAppADay.ConnectionMonitor.WinApp.jedi");
                lock (Program._mainThreadMutex)
                {
                    foreach (XmlElement e in doc.DocumentElement.ChildNodes)
                    {
                        ServerInfo info = new ServerInfo();
                        info._serverName = e.GetAttribute("Server");
                        info._attempts = Int32.Parse(e.GetAttribute("Attempts"));
                        info._type = (ServerInfoMonitorType)Enum.Parse(typeof(ServerInfoMonitorType), e.GetAttribute("Type"));
                        ServerInfoList._list.Add(info);
                    }
                    Monitor.PulseAll(Program._mainThreadMutex);
                }
            }
            catch (FileNotFoundException)
            {
                // ok
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error loading file: " + ex.Message);
            }
            RefreshListBox();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ServerInfo i = new ServerInfo();
            i._serverName = "<new>";
            i._attempts = 3;
            ServerDetailForm f = new ServerDetailForm();
            f.CurrentServerInfo = i;
            if (f.ShowDialog() == DialogResult.OK)
            {
                //we add it to our list
                lock (Program._mainThreadMutex)
                {
                    ServerInfoList._list.Add(f.CurrentServerInfo);
                    Monitor.PulseAll(Program._mainThreadMutex);
                }
            }
            RefreshListBox();
        }

        private void RefreshListBox()
        {
            listBox1.Items.Clear();
            foreach (ServerInfo i in ServerInfoList._list)
            {
                listBox1.Items.Add(i);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                ServerInfo i = (ServerInfo)listBox1.SelectedItem;
                ServerDetailForm f = new ServerDetailForm();
                f.CurrentServerInfo = i;
                if (f.ShowDialog() == DialogResult.OK)
                {
                    RefreshListBox();
                    lock (Program._mainThreadMutex)
                    {
                        Monitor.PulseAll(Program._mainThreadMutex);
                    }
                }
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                lock (Program._mainThreadMutex)
                {
                    ServerInfoList._list.Remove((ServerInfo)listBox1.SelectedItem);
                }
                RefreshListBox();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        internal void SaveServers()
        {
            //we save to file
            using (XmlWriter write = XmlWriter.Create("AnAppADay.ConnectionMonitor.WinApp.jedi"))
            {
                write.WriteStartDocument();
                write.WriteStartElement("Servers");
                foreach (ServerInfo i in ServerInfoList._list)
                {
                    write.WriteStartElement("Server");
                    write.WriteAttributeString("Server", i._serverName);
                    write.WriteAttributeString("Attempts", Convert.ToString(i._attempts));
                    write.WriteAttributeString("Type", Convert.ToString(i._type));
                    write.WriteEndElement();
                }
                write.WriteEndElement();
                write.WriteEndDocument();
                write.Close();
            }
        }

    }

}