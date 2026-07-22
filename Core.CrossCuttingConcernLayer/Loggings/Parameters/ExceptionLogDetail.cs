namespace Core.CrossCuttingConcernLayer.Loggings.Parameters;

public class ExceptionLogDetail : LogDetail
{
    public string ExceptionMessage { get; set; }

    public ExceptionLogDetail()
    {
        ExceptionMessage = string.Empty;
    }

    public ExceptionLogDetail(string fullName, string methodName, string user,
        List<LogParameter> parameters, string exceptionMessage) : base(fullName, methodName, user, parameters)
    {
        ExceptionMessage = exceptionMessage;
    }
}
