using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.ConnectionMonitor.WinApp
{

    class SimpleSocket : IConnectable
    {

        public bool TryConnect(string address)
        {
            string[] socketDetail = address.Split(':');
            if (socketDetail == null || socketDetail.Length != 2)
                throw new Exception("For socket connections, DNS / IP must be <server>:<port>");
            TcpClient c = new TcpClient();
            try
            {
                c.Connect(socketDetail[0], int.Parse(socketDetail[1]));
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                c.Close();
            }
            return true;
        }

    }

}
