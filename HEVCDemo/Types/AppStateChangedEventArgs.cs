using System;

namespace HEVCDemo.Types
{
    public class AppStateChangedEventArgs : EventArgs
    {
        public string StateText { get; set; }
        public bool? IsViewerEnabled { get; set; }
    }
}
