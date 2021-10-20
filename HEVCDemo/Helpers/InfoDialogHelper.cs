using HEVCDemo.Types;
using HEVCDemo.Views;
using Rasyidf.Localization;
using System.Collections.Generic;

namespace HEVCDemo.Helpers
{
    public static class InfoDialogHelper
    {
        public static void ShowResolutionInfoDialog()
        {
            var infoDialog = new InfoDialog("VideoResolutionTitle,Title".Localize(), "VideoResolution", null);
            infoDialog.Show();
        }

        public static void ShowFileSizeInfoDialog()
        {
            var infoDialog = new InfoDialog("FileSizeTitle,Title".Localize(), "FileSize", null);
            infoDialog.Show();
        }

        public static void ShowDecodedFramesInfoDialog()
        {
            var infoDialog = new InfoDialog("DecodedFramesLabel,Content".Localize(), "DecodedFrames", null);
            infoDialog.Show();
        }

        public static void ShowCodingUnitsInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "CodingUnitsFig1,Content".Localize(), ImagePath = "../Assets/Images/cupuStructure.png" }
            };

            var infoDialog = new InfoDialog("CodingUnitsLabel,Content".Localize(), "CodingUnits", images);
            infoDialog.Show();
        }

        public static void ShowPredictionTypeInfoDialog()
        {
            var infoDialog = new InfoDialog("PredictionLabel,Content".Localize(), "PredictionType", null);
            infoDialog.Show();
        }

        public static void ShowIntraPredictionInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "IntraPredictionFig1,Content".Localize(), ImagePath = "../Assets/Images/referencePixels.png"},
                new InfoImage { Name = "IntraPredictionFig2,Content".Localize(), ImagePath = "../Assets/Images/intraNotation.png"},
                new InfoImage { Name = "IntraPredictionFig3,Content".Localize(), ImagePath = "../Assets/Images/intraInterpolation.png"},
                new InfoImage { Name = "IntraPredictionFig4,Content".Localize(), ImagePath = "../Assets/Images/intraModes.png"},
            };

            var infoDialog = new InfoDialog("IntraLabel,Content".Localize(), "IntraPrediction", images);
            infoDialog.Show();
        }

        public static void ShowInterPredictionInfoDialog()
        {
            var images = new List<InfoImage>
            {
                new InfoImage { Name = "InterPredictionFig1,Content".Localize(), ImagePath = "../Assets/Images/vectorsCandidateBlocks.png"},
            };

            var infoDialog = new InfoDialog("InterPrediction,Content".Localize(), "InterPrediction", images);
            infoDialog.Show();
        }
    }
}
