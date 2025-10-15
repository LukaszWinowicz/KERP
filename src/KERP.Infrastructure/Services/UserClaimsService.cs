using KERP.Application.Common.Abstractions.Repositories;
using KERP.Application.Services;
using KERP.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace KERP.Infrastructure.Services;

public class UserClaimsService : IUserClaimsService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IFactoryRepository _factoryRepository;
    private readonly ILogger<UserClaimsService> _logger;

    public UserClaimsService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFactoryRepository factoryRepository,
        ILogger<UserClaimsService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _factoryRepository = factoryRepository;
        _logger = logger;
    }

    public async Task UpdateUserClaimsAsync(ApplicationUser user, string fullName, string email)
    {
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var claimTypesToUpdate = new[] { "GivenUsername", "GivenEmail", "FactoryId", "FactoryName" };
        var claimsToRemove = existingClaims.Where(c => claimTypesToUpdate.Contains(c.Type)).ToList();

        if (claimsToRemove.Any())
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, claimsToRemove);
            if (!removeResult.Succeeded)
            {
                _logger.LogWarning("Failed to remove old claims for user {UserId}", user.Id);
            }
        }

        var newClaims = new List<Claim>
        {
            new Claim("GivenUsername", fullName),
            new Claim("GivenEmail", email)
        };

        if (user.FactoryId.HasValue)
        {
            newClaims.Add(new Claim("FactoryId", user.FactoryId.Value.ToString()));
            var factory = await _factoryRepository.GetByIdAsync(user.FactoryId.Value);
            if (factory != null)
            {
                newClaims.Add(new Claim("FactoryName", factory.Name));
                if (!factory.IsActive)
                {
                    _logger.LogWarning("User {UserId} has assigned inactive factory {FactoryId}", user.Id, factory.Id);
                }
            }
            else
            {
                _logger.LogError("Factory {FactoryId} not found for user {UserId}", user.FactoryId.Value, user.Id);
                newClaims.Add(new Claim("FactoryName", $"Factory {user.FactoryId.Value}"));
            }
        }

        var addResult = await _userManager.AddClaimsAsync(user, newClaims);
        if (addResult.Succeeded)
        {
            _logger.LogInformation("Successfully updated claims for user {UserId}", user.Id);
            await _signInManager.RefreshSignInAsync(user);
        }
        else
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to add claims for user {UserId}: {Errors}", user.Id, errors);
        }
    }
}
