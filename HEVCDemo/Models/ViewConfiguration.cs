namespace HEVCDemo.Models
{
    /// <summary>
    /// Represents view configuration - what is shown in ImageViewer as HEVC features
    /// </summary>
    public class ViewConfiguration
    {
        public bool IsYuvFrameVisible { get; set; } = true;
        public bool IsCodingPredictionUnitsVisible { get; set; } = true;
        public bool IsPredictionTypeVisible { get; set; } = true;
        public bool IsIntraPredictionVisible { get; set; } = true;
        public bool IsInterPredictionVisible { get; set; } = true;
        public bool IsMotionVectorsStartEnabled { get; set; } = true;
    }
}
