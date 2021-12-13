using HEVCDemo.Hevc;
using HEVCDemo.Models;
using Rasyidf.Localization;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.Helpers
{
    public static class InfoPopupHelper
    {
        public static InfoPopupParameters GetInfo(VideoSequence videoSequence, int frameIndex, Point mouseLocation, double horizontalOffset, double verticalOffset, Grid grid, double zoom)
        {
            var locationWithZoom = new Point((int)((mouseLocation.X - horizontalOffset) / zoom), (int)((mouseLocation.Y - verticalOffset) / zoom));
            var predictionUnit = GetPredictionUnit(videoSequence.GetFrameByPoc(frameIndex).CodingUnits, locationWithZoom);

            if (predictionUnit == null) return null;

            var parameters = new InfoPopupParameters
            {
                Pu = predictionUnit,
                Location = $"{predictionUnit.X}x{predictionUnit.Y}",
                Size = $"{predictionUnit.Width}x{predictionUnit.Height}"
            };

            if (predictionUnit.PredictionType == PredictionType.INTRA)
            {
                parameters.PredictionType = "IntraPredictionTitle,Text".Localize();
                GetIntraParameters(parameters, predictionUnit);
            }
            else if (predictionUnit.PredictionType == PredictionType.INTER)
            {
                parameters.PredictionType = "InterPredictionTitle,Text".Localize();
                GetInterParameters(parameters, predictionUnit);
            }
            else if (predictionUnit.PredictionType == PredictionType.SKIP)
            {
                parameters.PredictionType = "SkipType,Text".Localize();
            }
            else
            {
                parameters.PredictionType = "NoneType,Text".Localize();
            }

            try
            {
                GetUnitImage(parameters, predictionUnit, horizontalOffset, verticalOffset, grid, zoom);
            }
            catch
            {
                return null;
            }

            return parameters;
        }

        private static ComPU GetPredictionUnit(List<ComCU> codingUnits, Point mouseLocation)
        {
            foreach (var unit in codingUnits)
            {
                if (unit.SCUs.Count > 0)
                {
                    var subUnit = GetPredictionUnit(unit.SCUs, mouseLocation);
                    if (subUnit != null)
                    {
                        return subUnit;
                    }
                }

                foreach (var punit in unit.PUs)
                {
                    var rectangle = new Rect(punit.X, punit.Y, punit.Width, punit.Height);
                    if (rectangle.Contains(mouseLocation))
                    {
                        return punit;
                    }
                }
            }

            return null;
        }

        private static void GetIntraParameters(InfoPopupParameters parameters, ComPU predictionUnit)
        {
            parameters.InterMode = string.Empty;

            string intraMode;
            if (predictionUnit.IntraDirLuma == 0)
            {
                intraMode = "PredictionPlanar,Text".Localize();

            }
            else if (predictionUnit.IntraDirLuma == 1)
            {
                intraMode = "PredictionDC,Text".Localize();
            }
            else
            {
                intraMode = $"{"PredictionAngular,Text".Localize()} ({predictionUnit.IntraDirLuma})";
            }

            parameters.IntraMode = $"{intraMode}";
        }

        private static void GetInterParameters(InfoPopupParameters parameters, ComPU predictionUnit)
        {
            parameters.IntraMode = string.Empty;

            switch (predictionUnit.InterDir)
            {
                case 1:
                    parameters.InterMode = "UniPrediction1,Content".Localize();
                    break;
                case 2:
                    parameters.InterMode = "UniPrediction2,Content".Localize();
                    break;
                case 3:
                    parameters.InterMode = "BiPrediction,Content".Localize();
                    break;
            }
        }

        private static void GetUnitImage(InfoPopupParameters parameters, ComPU predictionUnit, double horizontalOffset, double verticalOffset, Grid grid, double zoom)
        {
            double actualHeight = grid.ActualHeight - (verticalOffset * 2);
            double actualWidth = grid.ActualWidth - (horizontalOffset * 2);

            var renderTarget = new RenderTargetBitmap((int)actualWidth, (int)actualHeight, 96, 96, PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(grid);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            parameters.Image = new CroppedBitmap(renderTarget, new Int32Rect((int)(predictionUnit.X * zoom), (int)(predictionUnit.Y * zoom), (int)(predictionUnit.Width * zoom), (int)(predictionUnit.Height * zoom)));
            parameters.Image.Freeze();
        }
    }
}
