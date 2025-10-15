using KERP.Domain.Aggregates.User;

namespace KERP.Application.Services;

/// <summary>
/// Definiuje serwis odpowiedzialny za zarządzanie claims'ami użytkownika.
/// </summary>
public interface IUserClaimsService
{
    /// <summary>
    /// Aktualizuje i odświeża claims'y dla danego użytkownika na podstawie jego danych.
    /// </summary>
    /// <param name="user">Obiekt użytkownika do zaktualizowania.</param>
    /// <param name="fullName">Pełna nazwa użytkownika.</param>
    /// <param name="email">Adres email użytkownika.</param>
    Task UpdateUserClaimsAsync(ApplicationUser user, string fullName, string email);
}