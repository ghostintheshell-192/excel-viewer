using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ExcelViewer.UI.Avalonia.ViewModels
{
    public class RowComparisonViewModel : ViewModelBase
    {
        private readonly ILogger<RowComparisonViewModel> _logger;
        private RowComparison? _comparison;
        private ObservableCollection<RowComparisonColumnViewModel> _columns = new();

        public RowComparison? Comparison
        {
            get => _comparison;
            set
            {
                if (SetField(ref _comparison, value))
                {
                    RefreshColumns();
                }
            }
        }

        public ObservableCollection<RowComparisonColumnViewModel> Columns
        {
            get => _columns;
            set => SetField(ref _columns, value);
        }

        public string Title => Comparison?.Name ?? "Row Comparison";
        public int RowCount => Comparison?.Rows.Count ?? 0;
        public DateTime CreatedAt => Comparison?.CreatedAt ?? DateTime.MinValue;

        public ICommand CloseCommand { get; }

        public event EventHandler? CloseRequested;

        public RowComparisonViewModel(ILogger<RowComparisonViewModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            CloseCommand = new RelayCommand(() =>
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            });
        }

        public RowComparisonViewModel(RowComparison comparison, ILogger<RowComparisonViewModel> logger)
            : this(logger)
        {
            Comparison = comparison;
        }

        private void RefreshColumns()
        {
            Columns.Clear();

            if (Comparison == null)
                return;

            var allHeaders = Comparison.GetAllColumnHeaders();
            var maxColumns = Comparison.MaxColumns;

            // Ensure we have enough columns to display all data
            var columnCount = Math.Max(allHeaders.Count, maxColumns);

            for (int i = 0; i < columnCount; i++)
            {
                var header = i < allHeaders.Count ? allHeaders[i] : $"Column {i + 1}";
                var columnViewModel = new RowComparisonColumnViewModel(header, i, Comparison.Rows);
                Columns.Add(columnViewModel);
            }

            _logger.LogInformation("Refreshed comparison columns: {ColumnCount} columns for {RowCount} rows",
                columnCount, Comparison.Rows.Count);
        }
    }

    public class RowComparisonColumnViewModel : ViewModelBase
    {
        public string Header { get; }
        public int ColumnIndex { get; }
        public ObservableCollection<RowComparisonCellViewModel> Cells { get; }

        public RowComparisonColumnViewModel(string header, int columnIndex, IReadOnlyList<ExcelRow> rows)
        {
            Header = header;
            ColumnIndex = columnIndex;
            Cells = new ObservableCollection<RowComparisonCellViewModel>();

            foreach (var row in rows)
            {
                var cellValue = row.GetCellAsString(columnIndex);
                var cellViewModel = new RowComparisonCellViewModel(row, columnIndex, cellValue);
                Cells.Add(cellViewModel);
            }
        }
    }

    public class RowComparisonCellViewModel : ViewModelBase
    {
        public ExcelRow SourceRow { get; }
        public int ColumnIndex { get; }
        public string Value { get; }
        public string RowInfo { get; }
        public bool HasValue => !string.IsNullOrWhiteSpace(Value);

        public RowComparisonCellViewModel(ExcelRow sourceRow, int columnIndex, string value)
        {
            SourceRow = sourceRow ?? throw new ArgumentNullException(nameof(sourceRow));
            ColumnIndex = columnIndex;
            Value = value ?? string.Empty;
            RowInfo = $"{sourceRow.FileName} - {sourceRow.SheetName} - R{sourceRow.RowIndex + 1}";
        }
    }
}