using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HireHub.ApiCommon;

public static class DatabaseBootstrapExtensions
{
    /// <summary>
    /// Ensures the database exists. Retries while SQL Server is starting in Docker.
    /// Ignores SQL error 1801 when multiple services create the same database concurrently.
    /// </summary>
    public static async Task EnsureCreatedSafeAsync(this DatabaseFacade database, CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 30;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (SqlException ex) when (ex.Number == 1801)
            {
                // Database already exists.
                return;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
