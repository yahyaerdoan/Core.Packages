namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;

/// <summary>A client-supplied dynamic filter or sort that can't be applied; answered with 400 Bad Request.</summary>
public class DynamicQueryException(string message, Exception? innerException = null) : Exception(message, innerException);
