using System.Windows.Media.Imaging;

namespace HEVCDemo.Models
{
    /// <summary>
    /// Represents collection of bitmaps shown in ImageViewer as HEVC features
    /// </summary>
    public class HevcBitmaps
    {
        public BitmapImage YuvFrame { get; set; }
        public WriteableBitmap CodingPredictionUnits { get; set; }
        public WriteableBitmap PredictionType { get; set; }
        public WriteableBitmap IntraPrediction { get; set; }
        public WriteableBitmap InterPrediction { get; set; }
    }
}
