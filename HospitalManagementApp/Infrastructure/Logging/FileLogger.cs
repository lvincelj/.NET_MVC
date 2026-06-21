using System.Globalization;
using Microsoft.Extensions.Logging;

namespace HospitalManagementApp.Infrastructure.Logging;

public sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly FileLoggerProvider _provider;

    public FileLogger(string categoryName, FileLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _provider.IsEnabled(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null)
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture);
        var eventText = eventId.Id == 0 ? string.Empty : $" EventId={eventId.Id}";
        var line = $"[{timestamp}] {logLevel,-11} {_categoryName}{eventText}: {message}";

        if (exception is not null)
        {
            line = $"{line}{Environment.NewLine}{exception}";
        }

        _provider.WriteLine(line);
    }
}
