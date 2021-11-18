using HEVCDemo.CustomEventArgs;
using HEVCDemo.Models;
using System;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public static class GlobalActionsHelper
    {
        public static event EventHandler<ViewConfigurationChangedEventArgs> ViewConfigurationChanged;
        public static event EventHandler SelectVideoClicked;
        public static event EventHandler<AppStateChangedEventArgs> AppStateChanged;
        public static event EventHandler MainWindowDeactivated;
        public static event EventHandler CacheCleared;
        public static event EventHandler<ShowTipsEventArgs> ShowTipsEnabledChanged;
        public static event EventHandler<KeyDownEventArgs> KeyDown;
        public static event EventHandler<VideoLoadedEventArgs> VideoLoaded;

        public static void OnViewConfigurationChanged(ViewConfiguration configuration)
        {
            ViewConfigurationChanged?.Invoke(new object(), new ViewConfigurationChangedEventArgs { ViewConfiguration = configuration });
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

        public static void OnKeyDown(Key key)
        {
            KeyDown?.Invoke(new object(), new KeyDownEventArgs { Key = key });
        }

        public static void OnVideoLoaded(string resolution, string fileSize)
        {
            var e = new VideoLoadedEventArgs
            {
                Resolution = resolution,
                FileSize = fileSize
            };

            VideoLoaded?.Invoke(new object(), e);
        }
    }
}
