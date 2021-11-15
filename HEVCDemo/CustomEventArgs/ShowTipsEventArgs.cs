using System;

namespace HEVCDemo.CustomEventArgs
{
    public class ShowTipsEventArgs : EventArgs
    {
        public bool IsEnabled { get; set; }
    }
}
