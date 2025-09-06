using KERP.Application.Services;
using Microsoft.AspNetCore.Http;

namespace KERP.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => "dev-user-001";
    public int? FactoryId => 241;

    /*public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

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
    }*/
}
