using Core.CrossCuttingConcernLayer.ExceptionHandlings.Types;
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
            _ => HandleExceptionsAsync(exception)
        };

    protected abstract Task HandleException(BusinessException businessException);
    protected abstract Task HandleException(Exception exception);
}
