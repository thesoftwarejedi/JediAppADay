using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace AnAppADay.ScreenBroadcaster.Common
{

    public interface IBroadcastServer
    {

        byte[] GetScreen();

    }

}

