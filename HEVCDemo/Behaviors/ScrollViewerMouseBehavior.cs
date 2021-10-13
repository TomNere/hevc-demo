using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HEVCDemo.Behaviors
{
    // Mouse behavior for getting mouse position on ScrollViewer and handling mouse wheel
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

        public static readonly DependencyProperty MouseWheelDirectionProperty = DependencyProperty.Register(
            nameof(MouseWheelDirection), typeof(bool), typeof(ScrollViewerMouseBehavior), new PropertyMetadata(default(bool)));

        public bool MouseWheelDirection
        {
            get => (bool)GetValue(MouseWheelDirectionProperty);
            set => SetValue(MouseWheelDirectionProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += AssociatedObjectOnMouseMove;
            AssociatedObject.PreviewMouseWheel += AssociatedObjectOnMouseWheel;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= AssociatedObjectOnMouseMove;
            AssociatedObject.PreviewMouseWheel -= AssociatedObjectOnMouseWheel;
        }

        private void AssociatedObjectOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!(AssociatedObject is ScrollViewer scrollViewer)) return;

            var pos = mouseEventArgs.GetPosition(AssociatedObject);
            MouseX = pos.X + scrollViewer.HorizontalOffset;
            MouseY = pos.Y + scrollViewer.VerticalOffset;
        }

        private void AssociatedObjectOnMouseWheel(object sender, MouseWheelEventArgs args)
        {
            MouseWheelDirection = args.Delta > 0;
        }
    }
}
