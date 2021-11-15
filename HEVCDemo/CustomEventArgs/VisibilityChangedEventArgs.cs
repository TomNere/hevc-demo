using System;

namespace HEVCDemo.CustomEventArgs
{
    public class VisibilityChangedEventArgs : EventArgs
    {
        public bool IsVisible { get; set; }
    }
}
