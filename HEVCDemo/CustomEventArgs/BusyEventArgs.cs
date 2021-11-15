using System;

namespace HEVCDemo.CustomEventArgs
{
    public class BusyEventArgs : EventArgs
    {
        public bool IsBusy { get; set; }
    }
}
