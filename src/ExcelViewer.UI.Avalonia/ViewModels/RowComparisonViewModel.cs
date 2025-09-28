using System.Collections.ObjectModel;
using System.Windows.Input;
using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.UI.Avalonia.Models;
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

            // Log warnings if any structural issues were detected
            if (Comparison.Warnings.Any())
            {
                _logger.LogWarning("Row comparison detected {WarningCount} structural inconsistencies in column headers", Comparison.Warnings.Count);
                foreach (var warning in Comparison.Warnings)
                {
                    _logger.LogWarning("Column '{ColumnName}': {Message} (Files: {Files})",
                        warning.ColumnName, warning.Message, string.Join(", ", warning.AffectedFiles));
                }
            }

            // Create columns using header-based mapping
            for (int i = 0; i < allHeaders.Count; i++)
            {
                var header = allHeaders[i];
                var columnViewModel = new RowComparisonColumnViewModel(header, i, Comparison.Rows);
                Columns.Add(columnViewModel);
            }

            _logger.LogInformation("Created row comparison with {ColumnCount} columns for {RowCount} rows using intelligent header mapping",
                allHeaders.Count, Comparison.Rows.Count);
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

            // Use intelligent header-based mapping instead of positional mapping
            var allValues = rows.Select(row => row.GetCellAsStringByHeader(header) ?? string.Empty).ToList();

            foreach (var row in rows)
            {
                var cellValue = row.GetCellAsStringByHeader(header) ?? string.Empty;
                var comparisonResult = DetermineComparisonResult(cellValue, allValues);
                var cellViewModel = new RowComparisonCellViewModel(row, columnIndex, cellValue, comparisonResult);

                Cells.Add(cellViewModel);
            }
        }

        private static CellComparisonResult DetermineComparisonResult(string currentValue, IList<string> allValues)
        {
            // Normalize values first
            var normalizedCurrentValue = (currentValue ?? "").Trim();
            var normalizedAllValues = allValues.Select(v => (v ?? "").Trim()).ToList();

            var hasValue = !string.IsNullOrWhiteSpace(normalizedCurrentValue);
            var allNonEmptyValues = normalizedAllValues.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
            var distinctNonEmptyValues = allNonEmptyValues.Distinct().ToList();

            // Handle empty values
            if (!hasValue)
            {
                return allNonEmptyValues.Any()
                    ? CellComparisonResult.CreateMissing(allNonEmptyValues.Count)
                    : CellComparisonResult.CreateMatch(allValues.Count, allValues.Count);
            }

            // Handle case where all non-empty values are the same
            if (distinctNonEmptyValues.Count <= 1)
            {
                return CellComparisonResult.CreateMatch(allNonEmptyValues.Count, allNonEmptyValues.Count);
            }

            // Advanced logarithmic distribution algorithm for optimal visual separation
            var valueGroups = allNonEmptyValues
                .GroupBy(v => v)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)      // Primary: Most frequent first (rank 0)
                .ThenBy(g => g.Value)                 // Secondary: Alphabetical for determinism
                .ToList();

            // Find current value's rank in the sorted groups
            var currentRank = valueGroups.FindIndex(g => g.Value == normalizedCurrentValue);
            var currentFrequency = valueGroups[currentRank].Count;
            var totalGroups = valueGroups.Count;


            return CellComparisonResult.CreateDifferent(currentFrequency, currentRank, totalGroups, allNonEmptyValues.Count);
        }
    }

    public class RowComparisonCellViewModel : ViewModelBase
    {
        public ExcelRow SourceRow { get; }
        public int ColumnIndex { get; }
        public string Value { get; }
        public string RowInfo { get; }
        public bool HasValue => !string.IsNullOrWhiteSpace(Value);
        public CellComparisonResult ComparisonResult { get; }

        // Backward compatibility - expose the type for existing bindings
        public ComparisonType ComparisonType => ComparisonResult.Type;

        public RowComparisonCellViewModel(ExcelRow sourceRow, int columnIndex, string value, CellComparisonResult comparisonResult)
        {
            SourceRow = sourceRow ?? throw new ArgumentNullException(nameof(sourceRow));
            ColumnIndex = columnIndex;
            Value = value ?? string.Empty;
            ComparisonResult = comparisonResult ?? CellComparisonResult.CreateMatch();
            RowInfo = $"{sourceRow.FileName} - {sourceRow.SheetName} - R{sourceRow.RowIndex + 1}";
        }

    }
}