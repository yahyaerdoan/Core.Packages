using System.Globalization;

using Core.CrossCuttingConcernLayer.Loggings.Serilogs.ConfigurationModels;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Messages;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;

using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Core;
using Serilog.Sinks.MSSqlServer;

namespace Core.CrossCuttingConcernLayer.Loggings.Serilogs.Loggers;

public class MsSqlLogger : BaseLoggerService
{
    //private readonly IConfiguration _configuration;
    public MsSqlLogger(IConfiguration configuration)
    {
        //_configuration = configuration;
        MsSqlConfiguration msSqlConfiguration = configuration.GetSection("SeriLogConfigurations:MsSqlLogConfiguration").Get<MsSqlConfiguration>() ?? throw new InvalidOperationException(SerilogMessage.NullOptionsMessage);

        MSSqlServerSinkOptions mSSqlServerSinkOptions = new()
        {
            TableName = msSqlConfiguration.TableName,
            AutoCreateSqlDatabase = msSqlConfiguration.AutoCreateSqlTable,
        };
        ColumnOptions columnOptions = new();
        Logger logger = new LoggerConfiguration()
            .WriteTo.MSSqlServer(msSqlConfiguration.ConnectionString, mSSqlServerSinkOptions, formatProvider: CultureInfo.InvariantCulture, columnOptions: columnOptions)
            .CreateLogger();
        Logger = logger;
    }
}
