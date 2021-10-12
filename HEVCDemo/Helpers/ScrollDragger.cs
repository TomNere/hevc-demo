using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HEVCDemo.Helpers
{
    public class ScrollDragger
    {
        private readonly ScrollViewer scrollViewer;
        private readonly UIElement content;
        private Point scrollMousePoint;
        private double verticalOffset = 1;
        private double horizontalOffset = 1;

        public ScrollDragger(UIElement content, ScrollViewer scrollViewer)
        {
            this.scrollViewer = scrollViewer;
            this.content = content;
            content.MouseLeftButtonDown += MouseLeftButtonDown;
            content.PreviewMouseMove += PreviewMouseMove;
            content.PreviewMouseLeftButtonUp += PreviewMouseLeftButtonUp;
        }

        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            content.CaptureMouse();
            scrollMousePoint = e.GetPosition(scrollViewer);
            verticalOffset = scrollViewer.VerticalOffset;
            horizontalOffset = scrollViewer.HorizontalOffset;
            Mouse.OverrideCursor = Cursors.SizeAll;
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (content.IsMouseCaptured)
            {
                var newVerticalOffset = verticalOffset + (scrollMousePoint.Y - e.GetPosition(scrollViewer).Y);
                scrollViewer.ScrollToVerticalOffset(newVerticalOffset);

                var newHorizontalOffset = horizontalOffset + (scrollMousePoint.X - e.GetPosition(scrollViewer).X);
                scrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            }
        }

        private void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            content.ReleaseMouseCapture();
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
