using Identity.Authentication.Dtos.Common;
using Identity.Authentication.Services;

namespace Identity.Authentication.Features.GetActiveSessions;

public record GetActiveSessionsQuery(Guid UserId) : IQuery<GetActiveSessionsResult>;

public record GetActiveSessionsResult(List<DeviceSessionDto> Sessions);

internal class GetActiveSessionsHandler(
    IdentityDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    IDeviceService deviceService
) : IQueryHandler<GetActiveSessionsQuery, GetActiveSessionsResult>
{
    public async Task<GetActiveSessionsResult> HandleAsync(
        GetActiveSessionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var currentDevice = deviceService.DetectDevice(httpContextAccessor.HttpContext!);

        var sessions = await dbContext
            .RefreshTokens.Where(rt =>
                rt.UserId == query.UserId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow
            )
            .OrderByDescending(rt => rt.LastUsed)
            .Select(rt => new DeviceSessionDto(
                rt.DeviceId,
                rt.DeviceName,
                rt.Platform,
                rt.LastUsed,
                rt.DeviceId == currentDevice.DeviceId
            ))
            .ToListAsync(cancellationToken);

        return new GetActiveSessionsResult(sessions);
    }
}
