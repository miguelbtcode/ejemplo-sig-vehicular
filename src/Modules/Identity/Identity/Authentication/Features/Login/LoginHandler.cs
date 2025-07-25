using Identity.Authentication.Dtos;
using Identity.Authentication.Models;
using Identity.Authentication.Services;

namespace Identity.Authentication.Features.Login;

public record LoginCommand(LoginRequestDto Request) : ICommand<Result<LoginResult>>;

public record LoginResult(LoginResponseDto Response);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

internal class LoginHandler(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IHttpContextAccessor httpContextAccessor
) : ICommandHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Validar usuario (igual para web y mobile)
        var userResult = await ValidateUserAsync(
            command.Request.Email,
            command.Request.Password,
            cancellationToken
        );

        var user = userResult.Value;

        // 2. Detectar tipo de cliente
        var clientInfo = DetectClientType(command);

        // 3. Logica especifica por tipo
        RefreshToken? refreshToken = null;

        if (clientInfo.IsMobile)
        {
            // MOBILE: Refresh token persistente con device info
            refreshToken = await HandleMobileLogin(user, clientInfo, cancellationToken);
        }
        else
        {
            // WEB: Refresh token persistente SIN device info específico
            refreshToken = await HandleWebLogin(user, clientInfo, cancellationToken);
        }

        // 4. Generar access token (igual para ambos)
        var roles = GetUserRoles(user);
        var permissions = GetUserPermissions(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles, permissions);

        // 5. Guardar refresh token
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 6. Obtener sesiones activas
        var activeSessions = await GetActiveSessionsAsync(user.Id, clientInfo, cancellationToken);

        return new LoginResult(
            new LoginResponseDto(
                accessToken.Token,
                refreshToken.Token,
                accessToken.Expiry,
                refreshToken.ExpiresAt,
                new UserLoginDto(user.Id, user.Name, user.Email, user.Enabled, roles, permissions),
                activeSessions,
                clientInfo.IsMobile ? "mobile" : "web"
            )
        );
    }

    private async Task<Result<User>> ValidateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        // Buscar usuario con todas las relaciones necesarias
        var user = await dbContext
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.Module)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.PermissionType)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

        if (user == null)
            return UserErrors.InvalidCredentials;

        if (!user.Enabled)
            return UserErrors.InactiveUser;

        // Verificar contraseña
        if (!passwordHasher.VerifyPassword(password, user.Password))
            return UserErrors.InvalidCredentials;

        return user;
    }

    private ClientInfo DetectClientType(LoginCommand command)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "";

        // ESTRATEGIA DE DETECCIÓN
        var isMobile =
            !string.IsNullOrEmpty(command.Request.DeviceId)
            || // Explicit mobile
            !string.IsNullOrEmpty(command.Request.Platform)
            || // Platform specified
            userAgent.Contains("Mobile")
            || // Mobile user agent
            userAgent.Contains("Android")
            || // Android
            userAgent.Contains("iPhone")
            || // iOS
            command.Request.Platform?.ToLower() is "ios" or "android"; // Explicit platform

        return new ClientInfo
        {
            IsMobile = isMobile,
            DeviceId = command.Request.DeviceId ?? GenerateWebDeviceId(httpContext),
            DeviceName = command.Request.DeviceName ?? ExtractDeviceNameFromUserAgent(userAgent),
            Platform = command.Request.Platform ?? (isMobile ? "mobile" : "web"),
            AppVersion = command.Request.AppVersion ?? "web",
            UserAgent = userAgent,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
        };
    }

    private async Task<RefreshToken> HandleMobileLogin(
        User user,
        ClientInfo clientInfo,
        CancellationToken cancellationToken
    )
    {
        // Revocar token existente del MISMO dispositivo
        var existingToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == user.Id && rt.DeviceId == clientInfo.DeviceId && !rt.IsRevoked,
            cancellationToken
        );

        if (existingToken != null)
        {
            existingToken.Revoke("New login on same device");
        }

        // Crear nuevo token mobile
        return RefreshToken.CreateMobile(
            user.Id,
            clientInfo.DeviceId,
            clientInfo.DeviceName,
            clientInfo.Platform,
            clientInfo.AppVersion ?? "1.0.0",
            DateTime.UtcNow.AddDays(60) // Mobile: 60 días
        );
    }

    private async Task<RefreshToken> HandleWebLogin(
        User user,
        ClientInfo clientInfo,
        CancellationToken cancellationToken
    )
    {
        // Web: Revocar tokens antiguos del mismo browser (opcional)
        var existingWebTokens = await dbContext
            .RefreshTokens.Where(rt =>
                rt.UserId == user.Id
                && rt.Platform == "web"
                && rt.DeviceId == clientInfo.DeviceId
                && !rt.IsRevoked
            )
            .ToListAsync(cancellationToken);

        foreach (var token in existingWebTokens)
        {
            token.Revoke("New web login");
        }

        // Crear nuevo token web
        return RefreshToken.CreateWeb(
            user.Id,
            clientInfo.DeviceId,
            clientInfo.DeviceName,
            clientInfo.UserAgent ?? "",
            clientInfo.IpAddress,
            DateTime.UtcNow.AddDays(30) // Web: 30 días
        );
    }

    private List<string> GetUserRoles(User user)
    {
        return user.UserRoles.Where(ur => ur.Role.Enabled).Select(ur => ur.Role.Name).ToList();
    }

    private List<string> GetUserPermissions(User user)
    {
        return user
            .UserRoles.Where(ur => ur.Role.Enabled)
            .SelectMany(ur => ur.Role.Permissions)
            .Where(p => p.Module.Enabled)
            .Select(p => $"{p.Module.Name}:{p.PermissionType.Name}")
            .Distinct()
            .ToList();
    }

    private async Task<List<DeviceSessionDto>> GetActiveSessionsAsync(
        Guid userId,
        ClientInfo currentClient,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var sessions = await dbContext
                .RefreshTokens.Where(rt =>
                    rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow
                )
                .OrderByDescending(rt => rt.LastUsed)
                .Select(rt => new DeviceSessionDto(
                    rt.DeviceId,
                    rt.DeviceName,
                    rt.Platform,
                    rt.LastUsed,
                    rt.DeviceId == currentClient.DeviceId // Es la sesión actual
                ))
                .ToListAsync(cancellationToken);

            return sessions;
        }
        catch (Exception ex)
        {
            // Manejo de errores específico si es necesario
            throw new Exception("Error al obtener sesiones activas", ex);
        }
    }

    private string GenerateWebDeviceId(HttpContext? context)
    {
        if (context == null)
            return Guid.NewGuid().ToString();

        // Generar ID consistente para el browser
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
        var fingerprint = $"{userAgent}:{ip}";

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToBase64String(hashBytes)[..16]; // Primeros 16 chars
    }

    private string ExtractDeviceNameFromUserAgent(string userAgent)
    {
        // Extraer info útil del User-Agent
        if (userAgent.Contains("Chrome"))
            return "Chrome Browser";
        if (userAgent.Contains("Firefox"))
            return "Firefox Browser";
        if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
            return "Safari Browser";
        if (userAgent.Contains("Edge"))
            return "Edge Browser";
        return "Web Browser";
    }
}
