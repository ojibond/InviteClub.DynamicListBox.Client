using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportsClub.Server.Data;

namespace SportsClub.Tests;

/// <summary>
/// Boots the real API in-process but swaps the SQL Server database for an
/// in-memory one, so integration tests exercise the full HTTP pipeline
/// (routing, model validation, the global exception handler) without a DB.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ListBoxDbContext>)
                         || d.ServiceType == typeof(ListBoxDbContext))
                .ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<ListBoxDbContext>(opt =>
                opt.UseInMemoryDatabase("IntegrationTestsDb"));
        });
    }
}
