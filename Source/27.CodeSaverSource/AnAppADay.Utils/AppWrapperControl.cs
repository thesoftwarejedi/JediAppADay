using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace AnAppADay.Utils
{

    public partial class AppWrapperControl : UserControl
    {

        private IntPtr _child;
        private bool _closeApp = true;
        private IntPtr _prevState;

        public AppWrapperControl()
        {
            InitializeComponent();
        }

        public IntPtr Child
        {
            set
            {
                _child = value;
                WinApi.SetParent(_child, Handle);
                RemoveBorder();
                SetSizeForOverlay();
            }
            get { return _child; }
        }

        public bool CloseApp
        {
            set { _closeApp = value; }
            get { return _closeApp; }
        }

        private void RemoveBorder()
        {
            _prevState = WinApi.GetWindowLongPtr(_child, -16);
            WinApi.SetWindowLong(_child, -16, 0x10000000);
        }

        private void SetSizeForOverlay()
        {
            WinApi.MoveWindow(_child, 0, 0, Width, Height, true);
        }

        protected override void OnResize(EventArgs e)
        {
            SetSizeForOverlay();
        }

        public void DieDieDie()
        {
            if (_closeApp)
            {
                //post close message to the window
                WinApi.PostMessage(_child, 0x0010, IntPtr.Zero, IntPtr.Zero);
            }
            else
            {
                //put back the state
                WinApi.SetWindowLong(_child, -16, _prevState.ToInt32());
                //set back to the desktop
                WinApi.SetParent(_child, WinApi.GetDesktopWindow());
            }
        }

    }

}

