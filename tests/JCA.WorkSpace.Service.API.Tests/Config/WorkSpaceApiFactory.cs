using JCA.WorkSpace.Infrastructure.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace JCA.WorkSpace.Service.API.Tests.Config;

public class WorkSpaceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .WithImage("postgres:15-alpine")
        .WithDatabase("workspace_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<WorkSpaceContext>));
            services.RemoveAll(typeof(DbContextOptions<WorkSpaceContextRead>));

            var connectionString = _dbContainer.GetConnectionString();

            services.AddDbContext<WorkSpaceContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddDbContext<WorkSpaceContextRead>(options =>
                options.UseNpgsql(connectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkSpaceContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}