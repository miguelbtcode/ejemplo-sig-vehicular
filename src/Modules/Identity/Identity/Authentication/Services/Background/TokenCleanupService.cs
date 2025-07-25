using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Authentication.Services.Background;

public class TokenCleanupService(
    IServiceProvider serviceProvider,
    ILogger<TokenCleanupService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token cleanup service started - scheduled for 1:00 AM daily");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextCleanup = GetNext1AM();
                var delay = nextCleanup - DateTime.Now;

                if (delay > TimeSpan.Zero)
                {
                    logger.LogDebug("Next cleanup scheduled for {NextCleanup}", nextCleanup);
                    await Task.Delay(delay, stoppingToken);
                }

                await CleanupExpiredTokensAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Token cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during token cleanup");
                //! Wait 5 minutes before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private static DateTime GetNext1AM()
    {
        var now = DateTime.Now;
        var next1AM = now.Date.AddHours(1); //* Today at 1:00 AM

        //? If it's already past 1:00 AM today, schedule for tomorrow
        if (now >= next1AM)
        {
            next1AM = next1AM.AddDays(1);
        }

        return next1AM;
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        try
        {
            logger.LogDebug("Starting immediate cleanup of expired and revoked tokens");

            //* Delete expired or revoked tokens immediately (no grace period)
            var tokensToDelete = await dbContext
                .RefreshTokens.Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
                .ToListAsync(cancellationToken);

            if (tokensToDelete.Count > 0)
            {
                logger.LogInformation(
                    "Cleaning up {Count} expired/revoked refresh tokens",
                    tokensToDelete.Count
                );

                dbContext.RefreshTokens.RemoveRange(tokensToDelete);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Successfully cleaned up {Count} refresh tokens",
                    tokensToDelete.Count
                );
            }
            else
            {
                logger.LogDebug("No tokens found for cleanup");
            }

            // Log active token count only
            var activeTokens = await dbContext.RefreshTokens.CountAsync(cancellationToken);
            logger.LogInformation("Active tokens remaining: {Count}", activeTokens);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cleanup expired tokens");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Token cleanup service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
