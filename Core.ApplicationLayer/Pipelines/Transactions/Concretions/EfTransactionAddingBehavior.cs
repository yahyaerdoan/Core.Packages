using Core.ApplicationLayer.Pipelines.Transactions.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Transactions.Concretions;

/// <summary>
/// Wraps a MediatR handler in a single Entity Framework Core transaction, so that if the handler
/// performs more than one <c>SaveChangesAsync</c> call, they either all succeed or all get rolled
/// back together.
/// </summary>
/// <remarks>
/// <para>
/// Use this instead of <see cref="TransactionAddingBehavior{TRequest,TResponse}"/> whenever the
/// handler writes through Entity Framework Core. <see cref="TransactionAddingBehavior{TRequest,TResponse}"/>
/// relies on <see cref="System.Transactions.TransactionScope"/>, which escalates to a distributed
/// transaction (MSDTC) as soon as a second connection or <c>SaveChangesAsync</c> call joins the
/// same scope. Modern SQL Server clients refuse that escalation unless MSDTC is installed and
/// running, so <see cref="TransactionAddingBehavior{TRequest,TResponse}"/> throws in that case. This
/// type never has that problem: it opens one <see cref="DbContext"/> transaction directly
/// (<see cref="Microsoft.EntityFrameworkCore.Storage.RelationalDatabaseFacadeExtensions.BeginTransactionAsync(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Threading.CancellationToken)"/>)
/// and keeps every write on that same connection, so MSDTC is never involved.
/// </para>
/// <para>
/// <b>How to use it:</b> a request opts in by implementing <see cref="ITransactionAddRequest"/>.
/// Because <typeparamref name="TContext"/> has to be a concrete type, this class can't be
/// registered directly as an open generic (<c>typeof(IPipelineBehavior&lt;,&gt;)</c> requires every
/// type parameter to stay open). Instead, define one small subclass per module that closes
/// <typeparamref name="TContext"/> to that module's own <see cref="DbContext"/>, and register the
/// subclass:
/// <code>
/// public class OrdersTransactionAddingBehavior&lt;TRequest, TResponse&gt;(OrdersDbContext context)
///     : EfTransactionAddingBehavior&lt;TRequest, TResponse, OrdersDbContext&gt;(context)
///     where TRequest : IRequest&lt;TResponse&gt;, ITransactionAddRequest
///     where TResponse : IOperationResult;
///
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(OrdersTransactionAddingBehavior&lt;,&gt;));
/// </code>
/// </para>
/// <para>
/// Only use this on requests whose handler genuinely performs more than one write. A handler with
/// a single <c>SaveChangesAsync</c> call is already atomic on its own - wrapping it here just adds
/// overhead for no benefit.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The MediatR request being handled. Must implement <see cref="ITransactionAddRequest"/> to opt in.</typeparam>
/// <typeparam name="TResponse">The result type returned by the handler. Must implement <see cref="IOperationResult"/> so a "graceful" failure (a returned result with <see cref="IOperationResult.IsSuccessful"/> == <see langword="false"/>) can be rolled back too, not just a thrown exception.</typeparam>
/// <typeparam name="TContext">The <see cref="DbContext"/> the handler writes through. Closed to one concrete type per module - see the usage example above.</typeparam>
public class EfTransactionAddingBehavior<TRequest, TResponse, TContext>(TContext context) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionAddRequest
    where TResponse : IOperationResult
    where TContext : DbContext
{
    /// <summary>
    /// Runs the rest of the pipeline inside one database transaction, committing on success and
    /// rolling back on either a thrown exception or a returned result with
    /// <see cref="IOperationResult.IsSuccessful"/> == <see langword="false"/>.
    /// </summary>
    /// <param name="request">The incoming MediatR request.</param>
    /// <param name="next">The next step in the pipeline - usually the request handler itself.</param>
    /// <param name="cancellationToken">Propagated to every EF Core call this method makes.</param>
    /// <returns>Whatever <paramref name="next"/> returned, unchanged.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next(cancellationToken);

                if (response.IsSuccessful)
                    await transaction.CommitAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(cancellationToken);

                return response;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
