using System.Windows.Input;

namespace ExcelViewer.UI.Avalonia.Models;

public class FileDetailAction
{
    public string Name { get; set; }
    public ICommand Command { get; set; }
    public string Description { get; set; }

    public FileDetailAction(string name, ICommand command, string description)
    {
        Name = name;
        Command = command;
        Description = description;
    }
}
