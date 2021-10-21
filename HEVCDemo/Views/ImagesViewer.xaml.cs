using HEVCDemo.Helpers;
using HEVCDemo.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for ImagesViewer
    /// </summary>
    public partial class ImagesViewer : UserControl
    {
        private readonly ScrollDragger scrollDragger;
        private readonly ImagesViewerViewModel viewModel;

        public ImagesViewer()
        {
            InitializeComponent();
            viewModel = new ImagesViewerViewModel();
            DataContext = viewModel;
            scrollDragger = new ScrollDragger(ScrollViewerContent, ImageScrollViewer);
        }

        public bool ProcessPreviewKeyDown(Key key)
        {
            if (key == Key.Left)
            {
                viewModel.StepBackwardCommand.Execute();
            }
            else if (key == Key.Right)
            {
                viewModel.StepForwardCommand.Execute();
            }
            else if (key == Key.Add)
            {
                viewModel.ZoomInCommand.Execute();
            }
            else if (key == Key.Subtract)
            {
                viewModel.ZoomOutCommand.Execute();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
