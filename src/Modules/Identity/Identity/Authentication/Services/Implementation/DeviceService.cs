using System.Security.Cryptography;
using System.Text;
using Identity.Authentication.Dtos.Common;
using Identity.Authentication.Dtos.Requests;
using Identity.Authentication.Models;
using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Services.Implementation;

public class DeviceService(IdentityDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    : IDeviceService
{
    public DeviceInfo DetectDevice(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var isMobile = IsUserAgentMobile(userAgent);

        if (isMobile)
        {
            return DeviceInfo.CreateMobile(
                GenerateDeviceId(context),
                ExtractMobileDeviceName(userAgent),
                "mobile",
                "1.0.0"
            );
        }

        return DeviceInfo.CreateWeb(
            GenerateWebDeviceId(context),
            ExtractWebDeviceName(userAgent),
            userAgent
        );
    }

    public DeviceInfo DetectDevice(HttpContext context, LoginRequestDto request)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var isMobile = IsExplicitMobile(request) || IsUserAgentMobile(userAgent);

        if (isMobile)
        {
            return DeviceInfo.CreateMobile(
                request.DeviceId ?? GenerateDeviceId(context),
                request.DeviceName ?? ExtractMobileDeviceName(userAgent),
                request.Platform ?? "mobile",
                request.AppVersion ?? "1.0.0"
            );
        }

        return DeviceInfo.CreateWeb(
            GenerateWebDeviceId(context),
            ExtractWebDeviceName(userAgent),
            userAgent
        );
    }

    public async Task<RefreshToken> CreateSessionAsync(
        User user,
        DeviceInfo device,
        string refreshToken,
        DateTime expiry
    )
    {
        return device.IsMobile
            ? RefreshToken.CreateMobile(
                user.Id,
                device.DeviceId,
                device.DeviceName,
                device.Platform,
                device.AppVersion!,
                expiry,
                refreshToken
            )
            : RefreshToken.CreateWeb(
                user.Id,
                device.DeviceId,
                device.DeviceName,
                httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "",
                GetIpAddress(),
                expiry,
                refreshToken
            );
    }

    public async Task RevokeExistingSessionsAsync(Guid userId, DeviceInfo device)
    {
        var sessionsToRevoke = await dbContext
            .RefreshTokens.Where(rt =>
                rt.UserId == userId && rt.DeviceId == device.DeviceId && !rt.IsRevoked
            )
            .ToListAsync();

        foreach (var session in sessionsToRevoke)
        {
            session.Revoke("New login on same device");
        }
    }

    public async Task<List<DeviceSessionDto>> GetActiveSessionsAsync(
        Guid userId,
        DeviceInfo currentDevice
    )
    {
        return await dbContext
            .RefreshTokens.Where(rt =>
                rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow
            )
            .OrderByDescending(rt => rt.LastUsed)
            .Select(rt => new DeviceSessionDto(
                rt.DeviceId,
                rt.DeviceName,
                rt.Platform,
                rt.LastUsed,
                rt.DeviceId == currentDevice.DeviceId
            ))
            .ToListAsync();
    }

    private static bool IsExplicitMobile(LoginRequestDto request) =>
        !string.IsNullOrEmpty(request.DeviceId)
        || !string.IsNullOrEmpty(request.Platform)
        || request.Platform?.ToLower() is "ios" or "android";

    private static bool IsUserAgentMobile(string userAgent) =>
        userAgent.Contains("Mobile")
        || userAgent.Contains("Android")
        || userAgent.Contains("iPhone");

    private static string GenerateDeviceId(HttpContext context) => Guid.NewGuid().ToString();

    private string GenerateWebDeviceId(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ip = GetIpAddress();
        var fingerprint = $"{userAgent}:{ip}";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToBase64String(hashBytes)[..16];
    }

    private static string ExtractMobileDeviceName(string userAgent) =>
        userAgent.Contains("iPhone") ? "iPhone"
        : userAgent.Contains("Android") ? "Android Device"
        : "Mobile Device";

    private static string ExtractWebDeviceName(string userAgent) =>
        userAgent.Contains("Chrome") ? "Chrome Browser"
        : userAgent.Contains("Firefox") ? "Firefox Browser"
        : userAgent.Contains("Safari") && !userAgent.Contains("Chrome") ? "Safari Browser"
        : userAgent.Contains("Edge") ? "Edge Browser"
        : "Web Browser";

    private string? GetIpAddress() =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
