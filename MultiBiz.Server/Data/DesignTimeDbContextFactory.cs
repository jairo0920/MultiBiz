using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MultiBiz.Server.Tenants;
using System.IO;

namespace MultiBiz.Server.Data
{
    /// <summary>
    /// Permite a 'dotnet ef' crear AppDbContext en tiempo de diseño,
    /// sin depender del pipeline de ASP.NET ni del ITenantProvider real.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private class DummyTenantProvider : ITenantProvider
        {
            public Guid TenantId { get; private set; } = Guid.Empty;
            public string? Subdomain { get; private set; } = null;
            public void SetTenant(Guid id, string? subdomain) { TenantId = id; Subdomain = subdomain; }
        }

        public AppDbContext CreateDbContext(string[] args)
        {
            // Busca appsettings.* en el proyecto Server
            var basePath = Directory.GetCurrentDirectory();
            var cfg = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = cfg.GetConnectionString("DefaultConnection")
                     ?? "Server=localhost,1433;Database=MultiBizDb;User Id=sa;Password=Cruz.1011;TrustServerCertificate=True;";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs)
                .Options;

            return new AppDbContext(options, new DummyTenantProvider());
        }
    }
}