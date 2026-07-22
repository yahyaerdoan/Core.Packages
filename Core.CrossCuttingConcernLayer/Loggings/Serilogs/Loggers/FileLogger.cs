using Core.CrossCuttingConcernLayer.Loggings.Serilogs.ConfigurationModels;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Messages;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Core.CrossCuttingConcernLayer.Loggings.Serilogs.Loggers;

public class FileLogger : BaseLoggerService
{
    //private readonly IConfiguration _configuration;
    public FileLogger(IConfiguration configuration)
    {
        // _configuration = configuration;
        FileLogConfiguration logConfiguration = configuration.GetSection("SeriLogConfigurations:FileLogConfiguration").Get<FileLogConfiguration>() ?? throw new Exception(SerilogMessage.NullOptionsMessage);

        string logFilePath = string.Format(format: "{0}{1}", arg0: Directory.GetCurrentDirectory() + logConfiguration.FolderPath, arg1: ".txt");

        Logger = new LoggerConfiguration().WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: null, fileSizeLimitBytes: 5000000, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();
    }
}
