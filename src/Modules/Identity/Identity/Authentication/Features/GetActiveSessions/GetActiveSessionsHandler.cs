using Identity.Authentication.Dtos.Common;

namespace Identity.Authentication.Features.GetActiveSessions;

public record GetActiveSessionsQuery(Guid UserId) : IQuery<GetActiveSessionsResult>;

public record GetActiveSessionsResult(List<DeviceSessionDto> Sessions);

internal class GetActiveSessionsHandler(IdentityDbContext dbContext)
    : IQueryHandler<GetActiveSessionsQuery, GetActiveSessionsResult>
{
    public async Task<GetActiveSessionsResult> HandleAsync(
        GetActiveSessionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
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
                false // Current session flag se establece externamente
            ))
            .ToListAsync(cancellationToken);

        return new GetActiveSessionsResult(sessions);
    }
}
