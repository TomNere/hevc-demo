using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HEVCDemo.Behaviors
{
    // Mouse behavior for getting mouse position on ScrollViewer
    public class ScrollViewerMouseBehavior : System.Windows.Interactivity.Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty MouseXProperty = DependencyProperty.Register(
            nameof(MouseX), typeof(double), typeof(ScrollViewerMouseBehavior), new PropertyMetadata(default(double)));
        
        public double MouseX
        {
            get => (double)GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }

        public static readonly DependencyProperty MouseYProperty = DependencyProperty.Register(
            nameof(MouseY), typeof(double), typeof(ScrollViewerMouseBehavior), new PropertyMetadata(default(double)));

        public double MouseY
        {
            get => (double)GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!(AssociatedObject is ScrollViewer scrollViewer)) return;

            var pos = mouseEventArgs.GetPosition(AssociatedObject);
            MouseX = pos.X + scrollViewer.HorizontalOffset;
            MouseY = pos.Y + scrollViewer.VerticalOffset;
        }
    }
}
