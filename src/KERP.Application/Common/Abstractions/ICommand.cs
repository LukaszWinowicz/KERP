namespace KERP.Application.Common.Abstractions;

/// <summary>
/// Interfejs-znacznik dla komendy, która nie zwraca żadnej wartości.
/// </summary>
public interface ICommand { }

/// <summary>
/// Interfejs-znacznik dla komendy, która zwraca wartość.
/// </summary>
/// <typeparam name="TResult">Typ rezultatu.</typeparam>
public interface ICommand<out TResult> : ICommand { }