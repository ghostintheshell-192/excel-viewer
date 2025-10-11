using System.Text.RegularExpressions;
using SheetAtlas.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SheetAtlas.Core.Application.Services
{
    public interface ISearchService
    {
        List<SearchResult> Search(ExcelFile file, string query, SearchOptions? options = null);
        List<SearchResult> SearchInSheet(ExcelFile file, string sheetName, string query, SearchOptions? options = null);
    }

    //public class SearchOptions
    //{
    //    public bool CaseSensitive { get; set; }
    //    public bool ExactMatch { get; set; }
    //    public bool UseRegex { get; set; }
    //}

    public class SearchService : ISearchService
    {
        private readonly ILogger<SearchService> _logger;

        public SearchService(ILogger<SearchService> logger)
        {
            _logger = logger;
        }

        public List<SearchResult> Search(ExcelFile file, string query, SearchOptions? options = null)
        {
            var results = new List<SearchResult>();

            // Return empty results for whitespace-only queries
            if (string.IsNullOrWhiteSpace(query))
                return results;

            // Cerca nel nome del file
            if (IsMatch(file.FileName, query, options))
            {
                results.Add(new SearchResult(file, "", -1, -1, file.FileName)
                {
                    Context = { ["Type"] = "FileName" }
                });
            }

            // Cerca in tutti i fogli
            foreach (var sheetName in file.GetSheetNames())
            {
                // Cerca nel nome del foglio
                if (IsMatch(sheetName, query, options))
                {
                    results.Add(new SearchResult(file, sheetName, -1, -1, sheetName)
                    {
                        Context = { ["Type"] = "SheetName" }
                    });
                }

                // Cerca nel contenuto del foglio
                results.AddRange(SearchInSheet(file, sheetName, query, options));
            }

            return results;
        }

        public List<SearchResult> SearchInSheet(ExcelFile file, string sheetName, string query, SearchOptions? options = null)
        {
            var results = new List<SearchResult>();
            var sheet = file.GetSheet(sheetName);

            if (sheet == null) return results;

            for (int rowIndex = 0; rowIndex < sheet.RowCount; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                for (int colIndex = 0; colIndex < sheet.ColumnCount; colIndex++)
                {
                    var cellValue = row[colIndex].Value.ToString();
                    if (!string.IsNullOrEmpty(cellValue) && IsMatch(cellValue, query, options))
                    {
                        var result = new SearchResult(file, sheetName, rowIndex, colIndex, cellValue);

                        // Aggiungi header di colonna come contesto
                        result.Context["ColumnHeader"] = sheet.ColumnNames[colIndex];

                        // Aggiungi header di riga (prima colonna) come contesto
                        if (colIndex > 0)
                        {
                            result.Context["RowHeader"] = row[0].Value.ToString();
                        }

                        // Aggiungi coordinate della cella
                        result.Context["CellCoordinates"] = $"R{rowIndex + 1}C{colIndex + 1}";

                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private bool IsMatch(string text, string query, SearchOptions? options)
        {
            if (string.IsNullOrEmpty(text)) return false;

            options ??= new SearchOptions();

            try
            {
                // Ricerca con espressioni regolari
                if (options.UseRegex)
                {
                    var regexOptions = options.CaseSensitive ?
                        RegexOptions.None : RegexOptions.IgnoreCase;

                    return Regex.IsMatch(text, query, regexOptions);
                }

                // Ricerca con corrispondenza esatta
                if (options.ExactMatch)
                {
                    return options.CaseSensitive
                        ? text.Equals(query)
                        : text.Equals(query, StringComparison.OrdinalIgnoreCase);
                }

                // Ricerca standard
                return options.CaseSensitive
                    ? text.Contains(query)
                    : text.Contains(query, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in search matching");

                // Fallback alla ricerca semplice
                return text.Contains(query, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
