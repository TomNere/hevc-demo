﻿using HEVCDemo.Models;
using HEVCDemo.Views;
using Rasyidf.Localization;
using System.Collections.Generic;

namespace HEVCDemo.Helpers
{
    public static class InfoDialogHelper
    {
        private const string subPath = "../Assets/Images/InfoDialogs/";

        public static void ShowResolutionInfoDialog()
        {
            var infoDialog = new InfoDialog("VideoResolutionTitle,Text".Localize(), "VideoResolution", null);
            infoDialog.Show();
        }

        public static void ShowFileSizeInfoDialog()
        {
            var infoDialog = new InfoDialog("FileSizeTitle,Text".Localize(), "FileSize", null);
            infoDialog.Show();
        }

        public static void ShowDecodedFramesInfoDialog()
        {
            var infoDialog = new InfoDialog("DecodedFrames,Content".Localize(), "DecodedFrames", null);
            infoDialog.Show();
        }

        public static void ShowCodingUnitsInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "CodingUnitsFig1,Text".Localize(), ImagePath = $"{subPath}cupuStructure.png"}
            };

            var infoDialog = new InfoDialog("CodingPredictionUnits,Content".Localize(), "CodingUnits", images);
            infoDialog.Show();
        }

        public static void ShowPredictionTypeInfoDialog()
        {
            var infoDialog = new InfoDialog("PredictionType,Content".Localize(), "PredictionType", null);
            infoDialog.Show();
        }

        public static void ShowIntraPredictionInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "IntraPredictionFig1,Text".Localize(), ImagePath = $"{subPath}dc.png"},
                new InfoImage { Name = "IntraPredictionFig2,Text".Localize(), ImagePath = $"{subPath}angular.png"},
            };

            var infoDialog = new InfoDialog("IntraPredictionTitle,Text".Localize(), "IntraPrediction", images);
            infoDialog.Show();
        }

        public static void ShowInterPredictionInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "InterPredictionFig1,Text".Localize(), ImagePath = $"{subPath}mv.png"}
            };

            var infoDialog = new InfoDialog("InterPredictionTitle,Text".Localize(), "InterPrediction", images);
            infoDialog.Show();
        }
    }
}
