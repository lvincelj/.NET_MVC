using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HospitalManagementApp.Infrastructure.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly object _lock = new();
    private readonly FileLoggingOptions _options;

    public FileLoggerProvider(IOptions<FileLoggingOptions> options)
    {
        _options = options.Value;
        Directory.CreateDirectory(_options.DirectoryPath);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, this);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _options.Enabled
            && logLevel != LogLevel.None
            && logLevel >= _options.MinimumLevel;
    }

    public void WriteLine(string line)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var path = GetCurrentLogPath();

        lock (_lock)
        {
            File.AppendAllText(path, line + Environment.NewLine);
        }
    }

    public void Dispose()
    {
    }

    private string GetCurrentLogPath()
    {
        var date = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var safePrefix = string.IsNullOrWhiteSpace(_options.FileNamePrefix)
            ? "application"
            : _options.FileNamePrefix.Trim();

        return Path.Combine(_options.DirectoryPath, $"{safePrefix}-{date}.log");
    }
}
