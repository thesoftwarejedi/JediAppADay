using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.ScreenBroadcaster.Common
{
    public static class CommonLib
    {

        public static void HandleException(Exception ex)
        {
            MessageBox.Show("Error:" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
        }

    }
}
