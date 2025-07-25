using Identity.Authentication.Services;
using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Features.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<Result<RefreshTokenResult>>;

public record RefreshTokenResult(string AccessToken, DateTime ExpiresAt);

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

internal class RefreshTokenHandler(IdentityDbContext dbContext, ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>>
{
    public async Task<Result<RefreshTokenResult>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Find and validate refresh token
        var refreshToken = await dbContext
            .RefreshTokens.Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.Module)
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.PermissionType)
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsValid)
            return new Error("Auth.InvalidRefreshToken", "Refresh token inválido o expirado");

        // 2. Update last used
        refreshToken.UpdateLastUsed();

        // 3. Generate new access token
        var user = refreshToken.User;
        var roles = GetUserRoles(user);
        var permissions = GetUserPermissions(user);
        var device = new DeviceInfo(
            refreshToken.DeviceId,
            refreshToken.DeviceName,
            refreshToken.IsMobile ? DeviceType.Mobile : DeviceType.Web,
            refreshToken.Platform
        );

        var tokenPair = tokenService.GenerateTokenPair(user, roles, permissions, device);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(tokenPair.AccessToken, tokenPair.AccessTokenExpiry);
    }

    private static List<string> GetUserRoles(User user) =>
        user.UserRoles.Where(ur => ur.Role.Enabled).Select(ur => ur.Role.Name).ToList();

    private static List<string> GetUserPermissions(User user) =>
        user
            .UserRoles.Where(ur => ur.Role.Enabled)
            .SelectMany(ur => ur.Role.Permissions)
            .Where(p => p.Module.Enabled)
            .Select(p => $"{p.Module.Name}:{p.PermissionType.Name}")
            .Distinct()
            .ToList();
}
