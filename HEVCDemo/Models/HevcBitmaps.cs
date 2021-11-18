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

        // Signalizes if all data are valid (default value)
        public bool IsValid { get; private set; }

        public HevcBitmaps(WriteableBitmap allOthers, WriteableBitmap predictionType, bool isValid)
        {
            AllOthers = allOthers;
            PredictionType = predictionType;
            IsValid = isValid;
        }
    }
}
