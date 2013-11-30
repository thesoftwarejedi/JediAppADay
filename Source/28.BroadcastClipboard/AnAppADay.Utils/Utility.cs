using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AnAppADay.Utils
{

    public static class Utility
    {

        public static void ShowMessage(string message)
        {
            System.Windows.Forms.MessageBox.Show(message);
        }

        public static string StripHTML(string p)
        {
            string ret = Regex.Replace(p, @"<(.|\n)*?>", "");
            return ret;
        }

    }

}
