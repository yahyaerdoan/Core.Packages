using System.Transactions;
using Core.ApplicationLayer.Pipelines.Transactions.Abstractions;
using MediatR;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Transactions.Concretions;

public class TransactionAddingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ITransactionAddRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

        TResponse response = await next(cancellationToken);

        if (response is not IOperationResult { IsSuccessful: false })
        {
            transactionScope.Complete();
        }

        return response;
    }
}
