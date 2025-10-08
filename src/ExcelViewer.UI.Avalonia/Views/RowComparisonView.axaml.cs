using Avalonia.Controls;
using ExcelViewer.UI.Avalonia.ViewModels;

namespace ExcelViewer.UI.Avalonia.Views
{
    public partial class RowComparisonView : UserControl
    {
        public RowComparisonView()
        {
            InitializeComponent();
        }

        public RowComparisonView(RowComparisonViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
