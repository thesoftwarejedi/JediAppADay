using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using AnAppADay.JediSpeak.Shared;

namespace AnAppADay.JediSpeak.Server
{

    static class Program
    {

        public static NotifyIcon _icon = new NotifyIcon();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //load the icon
            using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AnAppADay.JediSpeak.Server.Icon.ico")) {
                _icon.Icon = new Icon(s);
            }

            MenuItem[] items = new MenuItem[2];
            items[1] = new MenuItem("Exit");
            items[1].Click += new EventHandler(Exit_Click);
            items[0] = new MenuItem("About");
            items[0].Click += new EventHandler(About_Click);
            
            _icon.ContextMenu = new ContextMenu(items);
            _icon.Visible = true;

            HttpServerChannel channel;
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(JediSpeak), "AnAppADay.JediSpeak.Server/JediSpeak", WellKnownObjectMode.Singleton);
            channel = new HttpServerChannel(8911);
            channel.StartListening(null);

            Application.Run();
        }

        static void About_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.anappaday.com");
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _icon.Visible = false;
            _icon.Dispose();
            Environment.Exit(0);
        }

    }

}