using System.Windows.Media.Imaging;

namespace HEVCDemo.Models
{
    /// <summary>
    /// 2 bitmaps representing HEVC data - prediction type and all other data
    /// </summary>
    public class HevcBitmaps
    {
        // YUV frame, coding and prediction units, intra prediction and inter prediction
        public WriteableBitmap AllOthers { get; private set; }

        // Prediction type needs to be shown separately because of transparency
        public WriteableBitmap PredictionType { get; private set; }


        public HevcBitmaps(WriteableBitmap allOthers, WriteableBitmap predictionType)
        {
            AllOthers = allOthers;
            PredictionType = predictionType;
        }
    }
}
