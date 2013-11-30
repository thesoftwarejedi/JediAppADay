/*
Copyright 2006 Dana Hanna
This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace AnAppADay.GraffitiWallpaper.Shared
{

    public interface IGraffitiController
    {

        byte[] GetCurrent();

        CaptchaInfo GetCaptcha();

        void SaveImage(byte[] image, int id, string captcha);

    }

    [Serializable]
    public class CaptchaInfo
    {
        public int id;
        public byte[] image;
    }

}
