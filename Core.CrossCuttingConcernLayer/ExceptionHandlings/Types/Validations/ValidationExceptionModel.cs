namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Validations;

public class ValidationExceptionModel
{
    public string? Property { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
