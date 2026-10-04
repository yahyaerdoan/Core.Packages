namespace Core.ApplicationLayer.Pipelines.Transactions.Abstractions;

/// <summary>Links a transaction marker to the DbContext its requests write to; added by AddTransactionalDbContext.</summary>
public sealed record TransactionalDbContextRegistration(Type MarkerType, Type ContextType);
