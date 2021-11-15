using System;
using System.Windows.Input;

namespace HEVCDemo.CustomEventArgs
{
    public class KeyDownEventArgs : EventArgs
    {
        public Key Key { get; set; }
    }
}
