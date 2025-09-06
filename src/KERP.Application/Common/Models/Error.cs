namespace KERP.Application.Common.Models;

/// <summary>
/// Reprezentuje pojedyńczy błąd w systemie.
/// </summary>
/// <param name="Code">Unikalny kod błędu, użyteczny dla klienta API.</param>
/// <param name="Description">Opis błędu zrozumiały dla człowieka.</param>
/// <param name="Type">Typ błędu (np. krytyczny, ostrzeżenie).</param>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Critical);