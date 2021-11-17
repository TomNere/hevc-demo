using System;

namespace HEVCDemo.CustomEventArgs
{
    public class VideoLoadedEventArgs : EventArgs
    {
        public string Resolution { get; set; }
        public string FileSize { get; set; }
    }
}
