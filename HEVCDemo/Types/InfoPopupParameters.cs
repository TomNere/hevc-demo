using System.Windows.Media.Imaging;

namespace HEVCDemo.Types
{
    public class InfoPopupParameters
    {
        public ComPU Pu { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public string PredictionMode { get; set; }
        public string IntraMode { get; set; }
        public string InterMode { get; set; }
        public string MotionVectors { get; set; }
        public BitmapSource Image { get; set; }
    }
}
