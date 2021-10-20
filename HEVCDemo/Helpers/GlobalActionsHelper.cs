using HEVCDemo.Types;
using System;
using System.Windows;

namespace HEVCDemo.Helpers
{
    public static class GlobalActionsHelper
    {
        public static event EventHandler<VisibilityChangedEventArgs> DecodedFramesVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> CodingUnitsVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> PredictionTypeVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> IntraPredictionVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> InterPredictionVisibilityChanged;
        public static event EventHandler<VisibilityChangedEventArgs> VectorsStartVisibilityChanged;
        public static event EventHandler SelectVideoClicked;

        public static void OnDecodedFramesVisibilityChanged(VisibilityChangedEventArgs e)
        {
            DecodedFramesVisibilityChanged?.Invoke(new object(), e);
        }

        public static void OnCodingUnitsVisibilityChanged(VisibilityChangedEventArgs e)
        {
            CodingUnitsVisibilityChanged?.Invoke(new object(), e);
        }

        public static void OnPredictionTypeVisibilityChanged(VisibilityChangedEventArgs e)
        {
            PredictionTypeVisibilityChanged?.Invoke(new object(), e);
        }

        public static void OnIntraPredictionVisibilityChanged(VisibilityChangedEventArgs e)
        {
            IntraPredictionVisibilityChanged?.Invoke(new object(), e);
        }

        public static void OnInterPredictionVisibilityChanged(VisibilityChangedEventArgs e)
        {
            InterPredictionVisibilityChanged?.Invoke(new object(), e);
        }

        public static void OnVectorsStartVisibilityChanged(VisibilityChangedEventArgs e)
        {
            VectorsStartVisibilityChanged?.Invoke(new object(), e);
        }

        public static void OnSelectVideoClicked()
        {
            SelectVideoClicked?.Invoke(new object(), new EventArgs());
        }
    }
}
