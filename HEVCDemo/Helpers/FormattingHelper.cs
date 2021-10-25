namespace HEVCDemo.Helpers
{
    public static class FormattingHelper
    {
        public static string GetFileSize(long lengthInBytes)
        {
            double kiloBytes = lengthInBytes / 1024d;
            return kiloBytes < 1000 ? $"{kiloBytes:0.000} KB" : $"{kiloBytes / 1024:0.000} MB";
        }
    }
}
