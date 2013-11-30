using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Drawing;
using AnAppADay.ScreenBroadcaster.Common;

namespace AnAppADay.ScreenBroadcaster.Server
{

    public class BroadcastServer : MarshalByRefObject, IBroadcastServer
    {

        public BroadcastServer()
        {
        }

        public byte[] GetScreen()
        {
            byte[] image = ScreenScraper.Instance.GetImage();
            return image;
        }

    }

}
