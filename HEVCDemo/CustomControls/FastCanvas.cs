using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HEVCDemo.CustomControls
{
    class FastCanvas: Canvas
    {
        public List<Rectangle> Rectangles = new List<Rectangle>();

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            Bitmap bitmap = new Bitmap(1920, 1080);
            Graphics g = Graphics.FromImage(bitmap);

            //Image img = Bitmap.FromFile(LoadPath);
            //Image img2 = Bitmap.FromFile(TempPath);

            //g.DrawImage(img, 0, 0);
            //g.DrawImage(img2, 250, 250);

            var pen = new System.Drawing.Pen(System.Drawing.Brushes.Red, 1);
            for (int i = 0; i < Rectangles.Count; i++)
            {
                var rect = Rectangles[i];
                g.DrawRectangle(pen, rect);
            }


            bitmap.Save("tst.bmp");

            //using (FileStream stream = new FileStream("ColorSamples.png", FileMode.Create))
            //{
            //    PngBitmapEncoder encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(bitmap));
            //    encoder.Save(stream);
            //}
        }
    }
}
