using Microsoft.Extensions.Logging;

namespace HospitalManagementApp.Infrastructure.Logging;

public sealed class FileLoggingOptions
{
    public const string SectionName = "FileLogging";

    public bool Enabled { get; set; } = true;

    public string DirectoryPath { get; set; } = "Logs";

    public string FileNamePrefix { get; set; } = "hospital-app";

    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}
