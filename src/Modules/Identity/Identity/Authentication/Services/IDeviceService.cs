using Identity.Authentication.Dtos.Common;
using Identity.Authentication.Dtos.Requests;
using Identity.Authentication.Models;
using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Services;

public interface IDeviceService
{
    DeviceInfo DetectDevice(HttpContext context, LoginRequestDto request);
    Task<RefreshToken> CreateSessionAsync(
        User user,
        DeviceInfo device,
        string refreshToken,
        DateTime expiry
    );
    Task RevokeExistingSessionsAsync(Guid userId, DeviceInfo device);
    Task<List<DeviceSessionDto>> GetActiveSessionsAsync(Guid userId, DeviceInfo currentDevice);
}
