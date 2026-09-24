namespace Core.CrossCuttingConcernLayer.Loggings.Parameters;

public class LogDetail
{
    public string FullName { get; set; }
    public string MethodName { get; set; }
    public string User { get; set; }
    public List<LogParameter> Parameters { get; set; }

    /// <summary>Set only by outcome-logging behaviors (e.g. LogResultAddingBehavior); null for
    /// request-only logging, which runs before a result exists.</summary>
    public object? Result { get; set; }

    public LogDetail()
    {
        FullName = string.Empty;
        MethodName = string.Empty;
        User = string.Empty;
        Parameters = [];
    }

    public LogDetail(string fullName, string methodName, string user, List<LogParameter> parameters)
    {
        FullName = fullName;
        MethodName = methodName;
        User = user;
        Parameters = parameters;
    }
}
