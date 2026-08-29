using Core.ApplicationLayer.Pipelines.Transactions.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Transactions.Concretions;

/// <summary>EF-native alternative to TransactionAddingBehavior: uses the DbContext's execution strategy
/// and a real database transaction instead of TransactionScope (no MSDTC needed). Also rolls back on a
/// non-throwing failed result (IsSuccessful == false), not just a thrown exception.</summary>
public class EfTransactionAddingBehavior<TRequest, TResponse, TContext>(TContext context) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionAddRequest
    where TResponse : IOperationResult
    where TContext : DbContext
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            TResponse response = await next(cancellationToken);

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
