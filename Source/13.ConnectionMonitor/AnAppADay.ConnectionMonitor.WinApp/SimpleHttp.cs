using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    class SimpleHttp : IConnectable
    {

        public bool TryConnect(string address)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
                req.GetResponse().Close();
            }
            catch (WebException)
            {
                return false;
            }
            return true;
        }

    }

}
