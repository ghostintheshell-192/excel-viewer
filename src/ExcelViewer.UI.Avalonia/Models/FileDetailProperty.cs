namespace ExcelViewer.UI.Avalonia.Models;

public class FileDetailProperty
{
    public string Key { get; set; }
    public string Value { get; set; }

    public FileDetailProperty(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public bool IsSeparator => string.IsNullOrEmpty(Key) && string.IsNullOrEmpty(Value);
    public bool IsHeader => !string.IsNullOrEmpty(Key) && string.IsNullOrEmpty(Value);
    public bool IsRegularRow => !IsSeparator && !IsHeader;
}
