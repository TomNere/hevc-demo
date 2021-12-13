using HEVCDemo.Hevc;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Models
{
    public class InfoPopupParameters
    {
        public ComPU Pu { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public string PredictionType { get; set; }
        public string IntraMode { get; set; }
        public string InterMode { get; set; }
        public BitmapSource Image { get; set; }
    }
}
