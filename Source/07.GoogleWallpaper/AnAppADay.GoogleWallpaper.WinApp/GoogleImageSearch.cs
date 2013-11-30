using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.GoogleWallpaper.WinApp
{

    public class GoogleImageSearch
    {

        private string _keyword;
        private bool _safe;

        public GoogleImageSearch()
        {
        }

        public string KeyWord
        {
            get { return _keyword; }
            set { _keyword = value; }
        }

        public bool Safe
        {
            get { return _safe; }
            set { _safe = value; }
        }

        public string[] DoSearch()
        {
            string url = @"http://images.google.com/images?lr=&imgsz=xxlarge&safe="+(_safe?"on":"off")+"&q=" + _keyword;
            WebClient wc = new WebClient();
            string html = wc.DownloadString(url);
            string[] pieces = html.Split(new string[] { "<a href=/imgres?imgurl=" }, StringSplitOptions.None);
            string[] images = new string[pieces.Length-1];
            for (int i = 1; i < pieces.Length; i++) //purposefully starting at 1
            {
                images[i - 1] = pieces[i].Split('&')[0];
            }
            return images;
        }

    }

}
