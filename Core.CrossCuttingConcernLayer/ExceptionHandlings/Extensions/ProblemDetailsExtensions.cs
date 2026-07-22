using Microsoft.AspNetCore.Mvc;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions;

public static class ProblemDetailsExtensions
{
    public static string AsJson<TProblemdetail>(this TProblemdetail problemDetails)
        where TProblemdetail : ProblemDetails => JsonSerializer.Serialize(problemDetails);
}
