namespace KERP.Domain.Exceptions;

/// <summary>
/// Bazowa klasa dla wszystkich wyjątków zdefiniowanych w warstwie domeny.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
