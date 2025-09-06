namespace KERP.Domain.Exceptions;

/// <summary>
/// Wyjątek rzucany w przypadku naruszenia reguły biznesowej.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string message) : base(message)
    {
    }
}
