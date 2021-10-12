using HEVCDemo.Helpers;
using System.Windows.Controls;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for ImagesViewer
    /// </summary>
    public partial class ImagesViewer : UserControl
    {
        private readonly ScrollDragger scrollDragger;

        public ImagesViewer()
        {
            InitializeComponent();
            scrollDragger = new ScrollDragger(ScrollViewerContent, ImageScrollViewer);
        }
    }
}
