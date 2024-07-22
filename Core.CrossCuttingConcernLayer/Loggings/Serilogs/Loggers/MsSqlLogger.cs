using Core.CrossCuttingConcernLayer.Loggings.Serilogs.ConfigurationModels;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Messages;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcernLayer.Loggings.Serilogs.Loggers;

public class MsSqlLogger : BaseLoggerService
{
    //private readonly IConfiguration _configuration;
    public MsSqlLogger(IConfiguration configuration)
    {
        //_configuration = configuration;
        MsSqlConfiguration msSqlConfiguration  = configuration.GetSection("SeriLogConfigurations:MsSqlLogConfiguration").Get<MsSqlConfiguration>() ?? throw new Exception(SerilogMessage.NullOptionsMessage);

        MSSqlServerSinkOptions mSSqlServerSinkOptions = new()
        {
            TableName = msSqlConfiguration.TableName,
            AutoCreateSqlDatabase = msSqlConfiguration.AutoCreateSqlTable,
        };
        ColumnOptions columnOptions = new();
        Logger logger = new LoggerConfiguration().WriteTo.MSSqlServer(msSqlConfiguration.ConnectionString, mSSqlServerSinkOptions, columnOptions: columnOptions).CreateLogger();
        Logger = logger;
    }
}
