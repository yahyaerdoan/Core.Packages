namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;

public class BusinessRuleException(string message) : Exception(message);
