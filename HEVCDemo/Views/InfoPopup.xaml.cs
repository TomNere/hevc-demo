using HEVCDemo.Helpers;
using HEVCDemo.Models;
using System.Windows;
using System.Windows.Controls;

namespace HEVCDemo.Views
{
    /// <summary>
    /// Interaction logic for InfoPopup.xaml
    /// </summary>
    public partial class InfoPopup : UserControl
    {
        private readonly GridLength visibleRowHeight = new GridLength(40);
        private readonly GridLength hiddenRowHeight = new GridLength(0);

        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(nameof(Parameters), typeof(InfoPopupParameters), typeof(InfoPopup));
        public InfoPopupParameters Parameters
        {
            get => (InfoPopupParameters)GetValue(ParametersProperty);
            set => SetValue(ParametersProperty, value);
        }

        public static readonly DependencyProperty IntraPredictionRowHeightProperty = DependencyProperty.Register(nameof(IntraPredictionRowHeight), typeof(GridLength), typeof(InfoPopup));
        public GridLength IntraPredictionRowHeight
        {
            get => (GridLength)GetValue(IntraPredictionRowHeightProperty);
            set => SetValue(IntraPredictionRowHeightProperty, value);
        }

        public static readonly DependencyProperty InterPredictionRowHeightProperty = DependencyProperty.Register(nameof(InterPredictionRowHeight), typeof(GridLength), typeof(InfoPopup));
        public GridLength InterPredictionRowHeight
        {
            get => (GridLength)GetValue(InterPredictionRowHeightProperty);
            set => SetValue(InterPredictionRowHeightProperty, value);
        }


        public InfoPopup()
        {
            InitializeComponent();
            Loaded += InfoPopup_Loaded;
        }

        private void InfoPopup_Loaded(object sender, RoutedEventArgs e)
        {
            SetRowsVisibility();
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

        private void SetRowsVisibility()
        {
            IntraPredictionRowHeight = string.IsNullOrEmpty(Parameters?.IntraMode) ? hiddenRowHeight : visibleRowHeight;
            InterPredictionRowHeight = string.IsNullOrEmpty(Parameters?.InterMode) ? hiddenRowHeight : visibleRowHeight;
        }
    }
}
