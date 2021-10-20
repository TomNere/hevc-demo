using HEVCDemo.Helpers;
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

        private void ShowCodingUnitsInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialogHelper.ShowCodingUnitsInfoDialog();
        }

        private void ShowPredictionTypeInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialogHelper.ShowPredictionTypeInfoDialog();
        }

        private void ShowIntraPredictionInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialogHelper.ShowIntraPredictionInfoDialog();
        }

        private void ShowInterPredictionInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoDialogHelper.ShowInterPredictionInfoDialog();
        }
    }
}
