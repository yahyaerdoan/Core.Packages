using Core.CrossCuttingConcernLayer.ExceptionHandlings.Middlewares;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}
