using System;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    internal class ServerInfoList : List<ServerInfo>
    {

        internal static List<ServerInfo> _list = new List<ServerInfo>();

    }

    class ServerInfo
    {

        public string _serverName;
        public int _attempts;
        public ServerInfoMonitorType _type;

        public override string ToString()
        {
            return _type.ToString() + " - " + _serverName;
        }

    }

    internal enum ServerInfoMonitorType
    {
        Ping,
        Socket,
        HTTP
    }

}
