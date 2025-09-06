using KERP.Application.Common.Abstractions;

namespace KERP.Application.Validation;

/// <summary>
/// Definiuje walidatora dla komendy.
/// </summary>
/// <typeparam name="TCommand">Typ komendy do walidacji.</typeparam>
public interface IValidator<in TCommand> where TCommand : ICommand
{
    Task<ValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken);
}
