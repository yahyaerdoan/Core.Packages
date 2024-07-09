using Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.HttpProblemDetails;

public class ValidationProblemDetails : ProblemDetails
{
    public IEnumerable<ValidationExceptionModel> Errors { get; init; }

    public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
    {
        Title = "Validation error(s)";
        Detail = "One or more validation errors occured";
        Errors = errors;
        Status = StatusCodes.Status400BadRequest;
        Type = "https://github.com/yahyaerdoan";
    }
}
