using Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Businesses;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;

public abstract class ExceptionHandler
{
    public Task HandleExceptionsAsync(Exception exception) =>
        exception switch
        {
            BusinessException businessException => HandleException(businessException),
            ValidationException validationException => HandleException(validationException),    
            _ => HandleExceptionsAsync(exception)
        };

    protected abstract Task HandleException(BusinessException businessException);
    protected abstract Task HandleException(ValidationException validationException);
    protected abstract Task HandleException(Exception exception);
}
