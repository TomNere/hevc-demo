namespace HEVCDemo.Hevc
{
    public enum PredictionMode
    {
        MODE_SKIP = 0,            ///< SKIP mode
        MODE_INTER = 1,           ///< inter-prediction mode
        MODE_INTRA = 2,           ///< intra-prediction mode
        MODE_NONE = 15
    }
}
