namespace SheetAtlas.Logging.Models
{
    /// <summary>
    /// Represents an action that can be performed on a notification
    /// </summary>
    public class LogAction
    {
        public LogAction(string label, Func<Task> action)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Display text for the action button
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Action to perform when button is clicked
        /// </summary>
        public Func<Task> Action { get; }
    }
}
