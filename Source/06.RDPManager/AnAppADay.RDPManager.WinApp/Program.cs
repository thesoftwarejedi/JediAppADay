using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace AnAppADay.RDPManager.WinApp
{
    static class Program
    {
        private static NotifyIcon _icon = new NotifyIcon();

        static string screenMode = null;
        static string desktopWidth = null;
        static string desktopHeight = null;
        static string compression = null;
        static string keyboardHook = null;
        static string audioMode = null;
        static string redirectDrives = null;
        static string redirectPrinters = null;
        static string redirectComPorts = null;
        static string redirectSmartCards = null;
        static string username = null;
        static string domain = null;
        static string disableWallpaper = null;
        static string disableWindowDrag = null;
        static string disableAnims = null;
        static string disableThemes = null;
        static string disableCursor = null;
        static string password = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.RDPManager.WinApp.Jedi.ico"))
            {
                _icon.Icon = new Icon(s);
            }

            _icon.ContextMenu = new ContextMenu();

            LoadFile();

            _icon.Visible = true;

            PasswordForm form = new PasswordForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                password = form.password;
            }

            Application.Run();
        }

        static void exitItem_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            Environment.Exit(0);
        }

        static void reloadItem_Click(object sender, EventArgs e)
        {
            _icon.ContextMenu.MenuItems.Clear();
            LoadFile();
        }

        static void aboutItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.AnAppADay.com");
        }

        static void LoadFile()
        {
            XmlDocument doc = new XmlDocument();
            using (Stream fileIn = new FileStream("AnAppADay.RDPManager.WinApp.xml", FileMode.Open, FileAccess.Read, FileShare.Read)) {
                doc.Load(fileIn);
            }
            screenMode = doc.SelectSingleNode("RDPConnections/@ScreenMode").InnerText;
            desktopWidth = doc.SelectSingleNode("RDPConnections/@DesktopWidth").InnerText;
            desktopHeight = doc.SelectSingleNode("RDPConnections/@DesktopHeight").InnerText;
            compression = doc.SelectSingleNode("RDPConnections/@Compression").InnerText;
            keyboardHook = doc.SelectSingleNode("RDPConnections/@KeyboardHook").InnerText;
            audioMode = doc.SelectSingleNode("RDPConnections/@AudioMode").InnerText;
            redirectDrives = doc.SelectSingleNode("RDPConnections/@RedirectDrives").InnerText;
            redirectPrinters = doc.SelectSingleNode("RDPConnections/@RedirectPrinters").InnerText;
            redirectComPorts = doc.SelectSingleNode("RDPConnections/@RedirectComPorts").InnerText;
            redirectSmartCards = doc.SelectSingleNode("RDPConnections/@RedirectSmartCards").InnerText;
            username = doc.SelectSingleNode("RDPConnections/@Username").InnerText;
            domain = doc.SelectSingleNode("RDPConnections/@Domain").InnerText;
            disableWallpaper = doc.SelectSingleNode("RDPConnections/@DisableWallpaper").InnerText;
            disableWindowDrag = doc.SelectSingleNode("RDPConnections/@DisableWindowDrag").InnerText;
            disableAnims = doc.SelectSingleNode("RDPConnections/@DisableAnims").InnerText;
            disableThemes = doc.SelectSingleNode("RDPConnections/@DisableThemes").InnerText;
            disableCursor = doc.SelectSingleNode("RDPConnections/@DisableCursor").InnerText;
            ProcessElement(doc.DocumentElement, _icon.ContextMenu.MenuItems);

            _icon.ContextMenu.MenuItems.Add(new MenuItem("-"));

            MenuItem item = new MenuItem("Reload XML");
            item.Click += new EventHandler(reloadItem_Click);
            _icon.ContextMenu.MenuItems.Add(item);

            _icon.ContextMenu.MenuItems.Add(new MenuItem("-"));

            item = new MenuItem("About");
            item.Click += new EventHandler(aboutItem_Click);
            _icon.ContextMenu.MenuItems.Add(item);

            item = new MenuItem("Exit");
            item.Click += new EventHandler(exitItem_Click);
            _icon.ContextMenu.MenuItems.Add(item);
        }

        private static void ProcessElement(XmlElement xmlElement, Menu.MenuItemCollection menuItems)
        {
            foreach (XmlElement subElement in xmlElement) {
                MenuItem m = new MenuItem(subElement.GetAttribute("DisplayName"));
                menuItems.Add(m);
                if (subElement.Name == "RDPConnection")
                {
                    m.Tag = subElement.GetAttribute("MachineName");
                    m.Click += new EventHandler(m_Click);
                }
                else
                {
                    //is a folder node
                    ProcessElement(subElement, m.MenuItems);
                }
            }
        }

        static void m_Click(object sender, EventArgs e)
        {
            //we create and use an RDP file
            using (FileStream fileStream = new FileStream("temp.rdp", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    string machineName = (sender as MenuItem).Tag as string;
                    sw.WriteLine("screen mode id:i:{0}", screenMode);
                    sw.WriteLine("desktopwidth:i:{0}", desktopWidth);
                    sw.WriteLine("desktopheight:i:{0}", desktopHeight);
                    sw.WriteLine("session bpp:i:16");
                    sw.WriteLine("full address:s:{0}", machineName);
                    sw.WriteLine("compression:i:{0}", compression);
                    sw.WriteLine("keyboardhook:i:{0}", keyboardHook);
                    sw.WriteLine("audiomode:i:{0}", audioMode);
                    sw.WriteLine("redirectdrives:i:{0}", redirectDrives);
                    sw.WriteLine("redirectprinters:i:{0}", redirectPrinters);
                    sw.WriteLine("redirectcomports:i:{0}", redirectComPorts);
                    sw.WriteLine("redirectsmartcards:i:{0}", redirectSmartCards);
                    sw.WriteLine("displayconnectionbar:i:1");
                    sw.WriteLine("autoreconnectionenabled:i:1");
                    sw.WriteLine("username:s:{0}", username);
                    sw.WriteLine("domain:s:{0}", domain);
                    sw.WriteLine("disable wallpaper:i:{0}", disableWallpaper);
                    sw.WriteLine("disable full window drage:i:{0}", disableWindowDrag);
                    sw.WriteLine("disable menu anims:i:{0}", disableAnims);
                    sw.WriteLine("disable themes:i:{0}", disableThemes);
                    sw.WriteLine("disable cursor setting:i:{0}", disableCursor);
                    sw.WriteLine("bitmapcachepersistenable:i:1");
                    sw.WriteLine("password 51:b:{0}", GetEncodedPassword());
                    sw.Flush();
                }
            }
            Process.Start("temp.rdp");
        }

        private static string GetEncodedPassword()
        {
            byte[] encoded = DPAPI.Encrypt(DPAPI.KeyType.UserKey, Encoding.Unicode.GetBytes(password), null, null);
            string temp = BitConverter.ToString(encoded).Replace("-", "");
            return temp;
        }
    }
}