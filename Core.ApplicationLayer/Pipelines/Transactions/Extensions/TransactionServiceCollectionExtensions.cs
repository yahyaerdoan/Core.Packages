using Core.ApplicationLayer.Pipelines.Transactions.Abstractions;
using Core.ApplicationLayer.Pipelines.Transactions.Concretions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.ApplicationLayer.Pipelines.Transactions.Extensions;

public static class TransactionServiceCollectionExtensions
{
    /// <summary>
    /// Requests implementing <typeparamref name="TMarker"/> run inside a transaction on <typeparamref name="TContext"/>. Call once per module
    /// with that module's marker; a request that writes to several modules implements each of their markers.
    /// </summary>
    public static IServiceCollection AddTransactionalDbContext<TMarker, TContext>(this IServiceCollection services)
        where TMarker : ITransactionAddRequest
        where TContext : DbContext
    {
        TransactionalDbContextRegistration registration = new(typeof(TMarker), typeof(TContext));
        if (!services.Any(descriptor => Equals(descriptor.ImplementationInstance, registration)))
        {
            _ = services.AddSingleton(registration);
        }

        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(EfTransactionAddingBehavior<,>)));
        return services;
    }
}
