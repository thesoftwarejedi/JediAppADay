using System;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.RSSAlerter.WinApp
{
    static class FeedManager
    {

        internal static LinkedList<string> feeds = new LinkedList<string>();

        internal static void Load() {
            lock (feeds)
            {
                feeds.Clear();
                try
                {
                    XmlDocument doc = null;
                    using (Stream read = new FileStream("AnAppADay.RSSAlerter.WinApp.jedi", FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XmlReader readXml = XmlReader.Create(read);
                        doc = new XmlDocument();
                        doc.Load(readXml);
                        readXml.Close();
                    }
                    foreach (XmlNode node in doc.SelectNodes("/Feeds/Feed"))
                    {
                        feeds.AddLast(node.InnerText);
                    }
                }
                catch (Exception ex)
                {
                    //oh well, no file
                }
            }
        }

        internal static void Save() {
            lock (feeds) {
                try {
                    using (Stream write = new FileStream("AnAppADay.RSSAlerter.WinApp.jedi", FileMode.Create, FileAccess.Write, FileShare.None)) {
                        XmlWriter writeXml = XmlWriter.Create(write);
                        writeXml.WriteStartDocument(true);
                        writeXml.WriteStartElement("Feeds");
                        foreach (string feed in feeds) {
                            writeXml.WriteElementString("Feed", feed);
                        }
                        writeXml.WriteEndElement();
                        writeXml.WriteEndDocument();
                        writeXml.Flush();
                        write.Flush();
                        write.Close();
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Error saving feed file: " + ex.Message);
                }
            }
        }
    }
}
