using System;

namespace HEVCDemo.CustomEventArgs
{
    public class AppStateChangedEventArgs : EventArgs
    {
        public string StateText { get; set; }
        public bool? IsViewerEnabled { get; set; }
    }
}
