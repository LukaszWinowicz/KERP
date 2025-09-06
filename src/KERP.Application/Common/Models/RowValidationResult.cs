namespace KERP.Application.Common.Models;

public class RowValidationResult
{
    public int RowNumber { get; }
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; }

    private RowValidationResult(int rowNumber, List<string> errors)
    {
        RowNumber = rowNumber;
        Errors = errors;
    }

    public static RowValidationResult Success(int rowNumber) => new(rowNumber, new List<string>());
    public static RowValidationResult Failure(int rowNumber, string errorMessage) => new(rowNumber, new List<string> { errorMessage });
}