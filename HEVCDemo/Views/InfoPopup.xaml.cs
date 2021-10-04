using HEVCDemo.Types;
using System.Windows;
using System.Windows.Controls;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for InfoPopup.xaml
    /// </summary>
    public partial class InfoPopup : UserControl
    {
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(nameof(Parameters), typeof(InfoPopupParameters), typeof(InfoPopup));
        public InfoPopupParameters Parameters
        {
            get => (InfoPopupParameters)GetValue(ParametersProperty);
            set => SetValue(ParametersProperty, value);
        }

        public InfoPopup()
        {
            InitializeComponent();
        }
    }
}
