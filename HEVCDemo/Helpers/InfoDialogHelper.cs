using HEVCDemo.Models;
using HEVCDemo.Views;
using Rasyidf.Localization;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HEVCDemo.Helpers
{
    public static class InfoDialogHelper
    {
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
                new InfoImage { Name = "CodingUnitsFig1,Text".Localize(), DrawingImage = (DrawingImage)Application.Current.FindResource("cupuStructureDrawingImage") }
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
                new InfoImage { Name = "IntraPredictionFig1,Text".Localize(), DrawingImage = (DrawingImage)Application.Current.FindResource("dcDrawingImage")},
                new InfoImage { Name = "IntraPredictionFig2,Text".Localize(), DrawingImage = (DrawingImage)Application.Current.FindResource("angularDrawingImage")},
            };

            var infoDialog = new InfoDialog("IntraPredictionMode,Content".Localize(), "IntraPrediction", images);
            infoDialog.Show();
        }

        public static void ShowInterPredictionInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "InterPredictionFig1,Text".Localize(), DrawingImage = (DrawingImage)Application.Current.FindResource("mvDrawingImage")},
            };

            var infoDialog = new InfoDialog("InterPredictionTitle,Text".Localize(), "InterPrediction", images);
            infoDialog.Show();
        }
    }
}
