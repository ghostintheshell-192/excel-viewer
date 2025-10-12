using Avalonia.Controls;
using SheetAtlas.UI.Avalonia.ViewModels;

namespace SheetAtlas.UI.Avalonia.Views
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
