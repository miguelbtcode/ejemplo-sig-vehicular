using Identity.Authentication.Dtos.Common;
using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Services.Implementation;

public class AuthenticationService(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDeviceService deviceService
) : IAuthenticationService
{
    public async Task<Result<AuthenticationResult>> AuthenticateAsync(LoginContext context)
    {
        // 1. Validate user credentials
        var userResult = await ValidateUserAsync(context.Email, context.Password);
        if (userResult.IsFailure)
            return userResult.Error;

        var user = userResult.Value;

        // 2. Revoke existing sessions if needed
        await deviceService.RevokeExistingSessionsAsync(user.Id, context.Device);

        // 3. Generate tokens
        var roles = GetUserRoles(user);
        var permissions = GetUserPermissions(user);
        var tokenPair = tokenService.GenerateTokenPair(user, roles, permissions, context.Device);

        // 4. Create and persist refresh token
        var refreshToken = await deviceService.CreateSessionAsync(
            user,
            context.Device,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiry
        );
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync();

        // 5. Get active sessions
        var activeSessions = await deviceService.GetActiveSessionsAsync(user.Id, context.Device);

        // 6. Build result
        var userDto = new UserLoginDto(
            user.Id,
            user.Name,
            user.Email,
            user.Enabled,
            roles,
            permissions
        );
        var result = AuthenticationResult.Success(tokenPair, userDto, activeSessions);

        return result;
    }

    private async Task<Result<User>> ValidateUserAsync(string email, string password)
    {
        var user = await dbContext
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.Module)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.PermissionType)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
            return UserErrors.InvalidCredentials;

        if (!user.Enabled)
            return UserErrors.InactiveUser;

        if (!passwordHasher.VerifyPassword(password, user.Password))
            return UserErrors.InvalidCredentials;

        return user;
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
