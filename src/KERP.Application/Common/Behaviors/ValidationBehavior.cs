using KERP.Application.Common.Abstractions;
using KERP.Application.Common.Models;
using KERP.Application.Validation;

namespace KERP.Application.Common.Behaviors;

/// <summary>
/// Behavior (dekorator) odpowiedzialny za uruchamianie walidacji dla komend.
/// Zatrzymuje przetwarzanie, jeśli walidacja wejściowa zakończyla się niepowodzeniem.
/// </summary>
public class ValidationBehavior<TCommand, TResult> : ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
    where TResult : Result
{
    private readonly ICommandHandler<TCommand, TResult> _decorated;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationBehavior(
        ICommandHandler<TCommand, TResult> decorated,
        IEnumerable<IValidator<TCommand>> validators)
    {
        _decorated = decorated;
        _validators = validators;
    }

    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await _decorated.Handle(command, cancellationToken);
        }

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(command, cancellationToken))
        );

        var errors = validationResults.SelectMany(r => r.Errors).ToList();

        if (errors.Any())
        {
            var resultErrors = errors
                .Select(e => new Error("ValidationError", e.ErrorMessage, ErrorType.Critical)).ToList();

            return Result.CreateFailure<TResult>(resultErrors);
        }

        return await _decorated.Handle(command, cancellationToken);
    }
}
