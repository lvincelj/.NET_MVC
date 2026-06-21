using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HospitalManagementApp.Infrastructure.Logging;

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(
        this ILoggingBuilder builder,
        IConfiguration configuration,
        string contentRootPath)
    {
        builder.Services.Configure<FileLoggingOptions>(configuration);
        builder.Services.PostConfigure<FileLoggingOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.DirectoryPath))
            {
                options.DirectoryPath = "Logs";
            }

            if (!Path.IsPathRooted(options.DirectoryPath))
            {
                options.DirectoryPath = Path.Combine(contentRootPath, options.DirectoryPath);
            }
        });

        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();

        return builder;
    }
}
