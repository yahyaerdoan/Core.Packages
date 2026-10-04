using Core.ApplicationLayer.Pipelines.Transactions.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Transactions.Concretions;

/// <summary>
/// Opens a database transaction only on the DbContexts whose marker the request implements (see AddTransactionalDbContext), commits them
/// when the result is successful and rolls them back otherwise. A context already inside a transaction (a nested send) is left to its owner.
/// </summary>
public class EfTransactionAddingBehavior<TRequest, TResponse>(IEnumerable<TransactionalDbContextRegistration> registrations, IServiceProvider serviceProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionAddRequest
    where TResponse : IOperationResult
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<DbContext> contexts = [.. registrations
            .Where(registration => registration.MarkerType.IsAssignableFrom(typeof(TRequest)))
            .Select(registration => (DbContext)serviceProvider.GetRequiredService(registration.ContextType))
            .Where(context => context.Database.CurrentTransaction is null)
            .Distinct()];

        return contexts.Count == 0 ? next(cancellationToken) : RunInTransactionsAsync(contexts, 0, next, cancellationToken);
    }

    private static async Task<TResponse> RunInTransactionsAsync(List<DbContext> contexts, int index, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (index == contexts.Count)
        {
            return await next(cancellationToken);
        }

        DbContext context = contexts[index];
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            TResponse response = await RunInTransactionsAsync(contexts, index + 1, next, cancellationToken);

            if (response.IsSuccessful)
            {
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            return response;
        });
    }
}
