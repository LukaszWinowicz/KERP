using Microsoft.AspNetCore.Identity;

namespace KERP.Domain.Aggregates.User;

// Dziedziczymy po IdentityUser, aby uzyskać wszystkie standardowe pola
// takie jak Email, PasswordHash, UserName itp.
public class ApplicationUser : IdentityUser
{
    // Tutaj możemy dodawać własne właściwości do użytkownika.
    // To jest idealne miejsce na Twoje FactoryId.
    public int? FactoryId { get; set; }
}
