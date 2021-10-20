using HEVCDemo.Types;
using System;
using System.Windows;

namespace HEVCDemo.Helpers
{
    public static class ViewOptionsHelper
    {
        public static event EventHandler<VisibilityChangedEventArgs> DecodedFramesVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> CodingUnitsVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> PredictionTypeVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> IntraPredictionVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> InterPredictionVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> VectorsStartVisibilityChanged;

        public static void OnDecodedFramesVisibilityChanged(VisibilityChangedEventArgs e)
        {
            DecodedFramesVisibilityChanged?.Invoke(null, e);
        }

        public static void OnCodingUnitsVisibilityChanged(VisibilityChangedEventArgs e)
        {
            CodingUnitsVisibilityChanged?.Invoke(null, e);
        }

        public static void OnPredictionTypeVisibilityChanged(VisibilityChangedEventArgs e)
        {
            PredictionTypeVisibilityChanged?.Invoke(null, e);
        }

        public static void OnIntraPredictionVisibilityChanged(VisibilityChangedEventArgs e)
        {
            IntraPredictionVisibilityChanged?.Invoke(null, e);
        }

        public static void OnInterPredictionVisibilityChanged(VisibilityChangedEventArgs e)
        {
            InterPredictionVisibilityChanged?.Invoke(null, e);
        }

        public static void OnVectorsStartVisibilityChanged(VisibilityChangedEventArgs e)
        {
            VectorsStartVisibilityChanged?.Invoke(null, e);
        }

        public static Visibility ConvertBoolToVisibility(bool isVisible)
        {
            return isVisible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
