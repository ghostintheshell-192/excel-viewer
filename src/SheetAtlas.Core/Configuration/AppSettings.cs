namespace SheetAtlas.Core.Configuration
{
    /// <summary>
    /// Application-wide configuration settings
    /// </summary>
    public class AppSettings
    {
        public PerformanceSettings Performance { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }

    /// <summary>
    /// Performance-related configuration
    /// </summary>
    public class PerformanceSettings
    {
        /// <summary>
        /// Maximum number of Excel files to load simultaneously.
        /// Higher values = faster batch loading but more memory usage.
        /// Recommended: 3-5 for typical systems.
        /// </summary>
        public int MaxConcurrentFileLoads { get; set; } = 5;
    }

    /// <summary>
    /// Logging-related configuration
    /// </summary>
    public class LoggingSettings
    {
        public bool EnableFileLogging { get; set; } = true;
        public bool EnableActivityLog { get; set; } = true;
    }
}
