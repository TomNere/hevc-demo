using HEVCDemo.Models;
using HEVCDemo.Views;
using Rasyidf.Localization;
using System.Collections.Generic;

namespace HEVCDemo.Helpers
{
    public static class InfoDialogHelper
    {
        private enum DialogType
        {
            Resolution,
            FileSize,
            DecodedFrames,
            CodingUnits,
            PredictionType,
            IntraPrediction,
            InterPrediction,
            WhatIsHevc
        }

        private const string subPath = "../Assets/Images/InfoDialogs/";

        // List of opened dialogs
        private static readonly List<DialogType> openedDialogs = new List<DialogType>();

        public static void ShowResolutionInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.Resolution)) return;

            var infoDialog = new InfoDialog("VideoResolutionTitle,Text".Localize(), "VideoResolution", null);
            ShowExclusiveDialog(infoDialog, DialogType.Resolution);
        }

        public static void ShowFileSizeInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.FileSize)) return;

            var infoDialog = new InfoDialog("FileSizeTitle,Text".Localize(), "FileSize", null);
            ShowExclusiveDialog(infoDialog, DialogType.FileSize);
        }

        public static void ShowDecodedFramesInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.DecodedFrames)) return;

            var infoDialog = new InfoDialog("DecodedFrames,Content".Localize(), "DecodedFrames", null);
            ShowExclusiveDialog(infoDialog, DialogType.DecodedFrames);
        }

        public static void ShowCodingUnitsInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.CodingUnits)) return;

            var images = new List<InfoImage>
            {
                new InfoImage { Name = "CodingUnitsFig1,Text".Localize(), ImagePath = $"{subPath}cupuStructure.png"}
            };

            var infoDialog = new InfoDialog("CodingPredictionUnits,Content".Localize(), "CodingUnits", images);
            ShowExclusiveDialog(infoDialog, DialogType.CodingUnits);
        }

        public static void ShowPredictionTypeInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.PredictionType)) return;

            var infoDialog = new InfoDialog("PredictionType,Content".Localize(), "PredictionType", null);
            ShowExclusiveDialog(infoDialog, DialogType.PredictionType);
        }

        public static void ShowIntraPredictionInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.IntraPrediction)) return;

            var images = new List<InfoImage>
            {
                new InfoImage { Name = "IntraPredictionFig1,Text".Localize(), ImagePath = $"{subPath}dc.png"},
                new InfoImage { Name = "IntraPredictionFig2,Text".Localize(), ImagePath = $"{subPath}angular.png"},
            };

            var infoDialog = new InfoDialog("IntraPredictionTitle,Text".Localize(), "IntraPrediction", images);
            ShowExclusiveDialog(infoDialog, DialogType.IntraPrediction);
        }

        public static void ShowInterPredictionInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.InterPrediction)) return;

            var images = new List<InfoImage>
            {
                new InfoImage { Name = "InterPredictionFig1,Text".Localize(), ImagePath = $"{subPath}mv.png"}
            };

            var infoDialog = new InfoDialog("InterPredictionTitle,Text".Localize(), "InterPrediction", images);
            ShowExclusiveDialog(infoDialog, DialogType.InterPrediction);
        }

        public static void ShowWhatIsHevcInfoDialog()
        {
            // Don't show the same dialog multiple times
            if (openedDialogs.Contains(DialogType.WhatIsHevc)) return;

            var infoDialog = new InfoDialog("WhatIsHevc,Content".Localize(), "WhatIsHevc", null);
            ShowExclusiveDialog(infoDialog, DialogType.WhatIsHevc);
        }

        private static void ShowExclusiveDialog(InfoDialog infoDialog, DialogType type)
        {
            openedDialogs.Add(type);
            infoDialog.Show();
            infoDialog.Closed += (s, e) => openedDialogs.Remove(type);
        }
    }
}
