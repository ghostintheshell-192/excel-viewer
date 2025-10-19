using System.Collections.ObjectModel;
using System.Windows.Input;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.UI.Avalonia.Commands;
using SheetAtlas.UI.Avalonia.Models;
using SheetAtlas.UI.Avalonia.Managers;
using SheetAtlas.Logging.Services;

namespace SheetAtlas.UI.Avalonia.ViewModels
{
    public class RowComparisonViewModel : ViewModelBase, IDisposable
    {
        private readonly ILogService _logger;
        private readonly IThemeManager? _themeManager;
        private RowComparison? _comparison;

        private bool _disposed = false;
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
        public bool HasRows => RowCount > 0;
        public DateTime CreatedAt => Comparison?.CreatedAt ?? DateTime.MinValue;

        public ICommand CloseCommand { get; }

        public event EventHandler? CloseRequested;

        public RowComparisonViewModel(ILogService logger, IThemeManager? themeManager = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _themeManager = themeManager;

            CloseCommand = new RelayCommand(() =>
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            });

            // Subscribe to theme changes to refresh cell colors
            if (_themeManager != null)
            {
                _themeManager.ThemeChanged += OnThemeChanged;
            }
        }

        public RowComparisonViewModel(RowComparison comparison, ILogService logger, IThemeManager? themeManager = null)
            : this(logger, themeManager)
        {
            Comparison = comparison;
        }

        private void OnThemeChanged(object? sender, Theme newTheme)
        {
            // Force re-evaluation of all cell background bindings
            RefreshCellColors();
            _logger.LogInfo($"Refreshed cell colors for theme: {newTheme}", "RowComparisonViewModel");
        }

        private void RefreshCellColors()
        {
            foreach (var column in Columns)
            {
                foreach (var cell in column.Cells)
                {
                    cell.RefreshColors();
                }
            }
        }

        private void RefreshColumns()
        {
            Columns.Clear();

            if (Comparison == null)
            {
                OnPropertyChanged(nameof(RowCount));
                OnPropertyChanged(nameof(HasRows));
                return;
            }

            var allHeaders = Comparison.GetAllColumnHeaders();

            // Log warnings if any structural issues were detected
            if (Comparison.Warnings.Any())
            {
                _logger.LogWarning($"Row comparison detected {Comparison.Warnings.Count} structural inconsistencies in column headers", "RowComparisonViewModel");
                foreach (var warning in Comparison.Warnings)
                {
                    _logger.LogWarning($"Column '{warning.ColumnName}': {warning.Message} (Files: {string.Join(", ", warning.AffectedFiles)})", "RowComparisonViewModel");
                }
            }

            // Create columns using header-based mapping
            for (int i = 0; i < allHeaders.Count; i++)
            {
                var header = allHeaders[i];
                var columnViewModel = new RowComparisonColumnViewModel(header, i, Comparison.Rows);
                Columns.Add(columnViewModel);
            }

            _logger.LogInfo($"Created row comparison with {allHeaders.Count} columns for {Comparison.Rows.Count} rows using intelligent header mapping", "RowComparisonViewModel");

            // Notify that RowCount and HasRows changed
            OnPropertyChanged(nameof(RowCount));
            OnPropertyChanged(nameof(HasRows));
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Unsubscribe from theme changes
                if (_themeManager != null)
                {
                    _themeManager.ThemeChanged -= OnThemeChanged;
                }

                // Dispose all column ViewModels
                if (Columns != null)
                {
                    foreach (var column in Columns.OfType<IDisposable>())
                    {
                        column.Dispose();
                    }
                    Columns.Clear();
                }

                _comparison = null;
            }

            _disposed = true;
        }
    }

    public class RowComparisonColumnViewModel : ViewModelBase, IDisposable
    {
        private bool _disposed = false;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Cells?.Clear();
            }
            _disposed = true;
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

        /// <summary>
        /// Forces re-evaluation of the ComparisonResult binding to update cell background colors
        /// Called when theme changes to refresh colors without recreating the entire comparison
        /// </summary>
        public void RefreshColors()
        {
            OnPropertyChanged(nameof(ComparisonResult));
        }
    }
}
