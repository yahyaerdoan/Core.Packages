using Core.CrossCuttingConcernLayer.ExceptionHandlings.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void UseConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}
