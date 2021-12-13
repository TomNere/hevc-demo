namespace HEVCDemo.Hevc
{
    public enum PredictionType
    {
        SKIP = 0,            ///< SKIP mode
        INTER = 1,           ///< inter-prediction mode
        INTRA = 2,           ///< intra-prediction mode
        NONE = 15
    }
}
