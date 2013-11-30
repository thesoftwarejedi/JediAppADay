using System;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    internal interface IConnectable
    {

        bool TryConnect(string address);

    }

}
