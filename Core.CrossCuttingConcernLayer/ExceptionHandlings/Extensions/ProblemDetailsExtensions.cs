using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions;

public static class ProblemDetailsExtensions
{
    public static string AsJson<TProblemdetail>(this TProblemdetail problemDetails)
        where TProblemdetail : ProblemDetails => JsonSerializer.Serialize(problemDetails);
}
