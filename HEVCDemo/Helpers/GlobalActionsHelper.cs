using HEVCDemo.CustomEventArgs;
using System;

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
        public static event EventHandler<AppStateChangedEventArgs> AppStateChanged;
        public static event EventHandler MainWindowDeactivated;
        public static event EventHandler CacheCleared;
        public static event EventHandler<ShowTipsEventArgs> ShowTipsEnabledChanged;

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

        public static void OnAppStateChanged(string stateText, bool? isViewerEnabled, bool isBusy)
        {
            var e = new AppStateChangedEventArgs
            {
                StateText = stateText,
                IsViewerEnabled = isViewerEnabled,
                IsBusy = isBusy
            };

            AppStateChanged?.Invoke(new object(), e);
        }

        public static void OnMainWindowDeactivated(object sender, EventArgs e)
        {
            MainWindowDeactivated?.Invoke(sender, e);
        }

        public static void OnCacheCleared()
        {
            CacheCleared?.Invoke(new object(), new EventArgs());
        }

        public static void OnShowTipsEnabledChanged(bool isEnabled)
        {
            ShowTipsEnabledChanged?.Invoke(new object(), new ShowTipsEventArgs { IsEnabled = isEnabled });
        }
    }
}
