using KERP.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace KERP.Infrastructure.Services;

/// <summary>
/// Serwis dostarczający informacje o aktualnie zalogowanym użytkowniku.
/// Odczytuje dane z Claims Principal (z authentication cookie).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Pobiera ID użytkownika z claims.
    /// Identity automatycznie dodaje claim NameIdentifier z User.Id
    /// </summary>
    public string? UserId => 
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Pobiera FactoryId z custom claims.
    /// </summary>
    public int? FactoryId
    {
        get
        {
            var factoryIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("FactoryId");
            if (int.TryParse(factoryIdClaim, out int factoryId))
            {
                return factoryId;
            }
            return null;
        }
    }

    /// <summary>
    /// Pobiera nazwę fabryki z claims.
    /// </summary>
    public string? FactoryName =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("FactoryName");

    /// <summary>
    /// Pobiera nazwę użytkownika z claims.
    /// </summary>
    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("GivenUsername");

    /// <summary>
    /// Pobiera email użytkownika z claims.
    /// </summary>
    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("GivenEmail");

    /// <summary>
    /// Sprawdza czy użytkownik jest zalogowany.
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
