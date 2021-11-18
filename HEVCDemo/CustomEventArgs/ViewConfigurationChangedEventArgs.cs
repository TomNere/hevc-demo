using HEVCDemo.Models;
using System;

namespace HEVCDemo.CustomEventArgs
{
    public class ViewConfigurationChangedEventArgs : EventArgs
    {
        public ViewConfiguration ViewConfiguration { get; set; }
    }
}
