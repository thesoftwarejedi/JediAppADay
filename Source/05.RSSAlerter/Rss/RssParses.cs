/*
 * Title  : My5U RSS Reader Parses Class
 * Author : Zhen.Liang [MS-MVP]
 * Email  : zhen.liang@gmail.com
 */
using System;
using System.Text;
using System.Xml;

namespace My5U
{
    namespace RSSReader
    {
        #region Rss Item Struct
        /// <summary>
        /// Rss Item
        /// </summary>
        public struct RssItem
        {
            /// <summary>
            /// The title of the item.
            /// </summary>
            public string title;
            /// <summary>
            /// The URL of the item.
            /// </summary>
            public string link;
            /// <summary>
            /// The item synopsis.
            /// </summary>
            public string description;
            /// <summary>
            /// Email address of the author of the item
            /// </summary>
            public string author;
            /// <summary>
            /// Includes the item in one or more categories
            /// </summary>
            public string category;
            /// <summary>
            /// URL of a page for comments relating to the item
            /// </summary>
            public string comments;
            /// <summary>
            /// Describes a media object that is attached to the item
            /// </summary>
            public string enclosure;
            /// <summary>
            /// A string that uniquely identifies the item
            /// </summary>
            public string guid;
            /// <summary>
            /// Indicates when the item was published
            /// </summary>
            public string pubDate;
            /// <summary>
            /// The RSS channel that the item came from
            /// </summary>
            public string source;

        }
        #endregion

        #region Rss Channel Struct
        /// <summary>
        /// RssChannel
        /// </summary>
        public struct RssChannel
        {
            /// <summary>
            /// The name of the channel. It's how people refer to your service.
            /// If you have an HTML website that contains the same information as your RSS file
            /// the title of your channel should be the same as the title of your website. 
            /// </summary>
            public string title;
            /// <summary>
            /// The URL to the HTML website corresponding to the channel
            /// </summary>
            public string link;
            /// <summary>
            /// Phrase or sentence describing the channel.
            /// </summary>
            public string description;

            /// <summary>
            /// Rss Channel Items
            /// </summary>
            public RssItem[] Items;
        }
        #endregion

        /// <summary>
        /// RSS Parses
        /// </summary>
        public class RssParses
        {
            /// <summary>
            /// RSS Parses Process
            /// </summary>
            /// <param name="rssURL">Rss URL</param>
            /// <example>
            /// RssParses.ProcessRSS("c:\\rss.xml");
            /// </example>
            /// <returns>RssChannel</returns>
            public static RssChannel ProcessRSS(string file)
            {
                RssChannel Rss = new RssChannel();

                XmlDocument rssDoc = new XmlDocument();
                rssDoc.Load(file);

                #region Rss Channel Parses
                XmlNode element = rssDoc.DocumentElement;
                int j = 0;
                XmlNode rssNode = null;
                while (rssNode == null || !(rssNode is XmlElement) || rssNode is XmlComment)
                {
                    rssNode = element.ChildNodes[j];
                }
                foreach (XmlElement temp in rssNode.ChildNodes)
                {
                    if (temp.Name == "title")
                    {    
                        Rss.title = temp.InnerText;
                    }
                    else if (temp.Name == "link")
                    {
                        Rss.link = temp.InnerText;
                    }
                    else if (temp.Name == "description")
                    {
                        Rss.description = temp.InnerText;
                    }
                }
                #endregion

                #region RSS Items Parses
                //use XPath get items list.
                XmlNodeList itemlist = rssNode.SelectNodes("item");
                XmlElement tElementItem;
                RssItem[] tRssItem = new RssItem[itemlist.Count];
                for (int i = 0; i < itemlist.Count; i++)
                {
                    tElementItem = itemlist.Item(i) as XmlElement;
                    foreach (XmlElement temp in tElementItem.ChildNodes)
                    {
                        if (temp.Name == "title")
                        {
                            tRssItem[i].title = temp.InnerText;
                        }
                        else if (temp.Name == "link")
                        {
                            tRssItem[i].link = temp.InnerText;
                        }
                        else if (temp.Name == "description")
                        {
                            tRssItem[i].description = temp.InnerText;
                        }
                        else if (temp.Name == "author")
                        {
                            tRssItem[i].author = temp.InnerText;
                        }
                        else if (temp.Name == "category")
                        {
                            tRssItem[i].category = temp.InnerText;
                        }
                        else if (temp.Name == "comments")
                        {
                            tRssItem[i].comments = temp.InnerText;
                        }
                        else if (temp.Name == "enclosure")
                        {
                            tRssItem[i].enclosure = temp.InnerText;
                        }
                        else if (temp.Name == "guid")
                        {
                            tRssItem[i].guid = temp.InnerText;
                        }
                        else if (temp.Name == "pubDate")
                        {
                            tRssItem[i].pubDate = temp.InnerText;
                        }
                        else if (temp.Name == "source")
                        {
                            tRssItem[i].source = temp.InnerText;
                        }
                    }
                }
                Rss.Items = tRssItem;
                #endregion

                return Rss;
            }
        }
    }
}
