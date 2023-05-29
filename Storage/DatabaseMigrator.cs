using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ITTP23.Storage
{
    public static class DatabaseMigrator
    {
        public static void MigrateDatabase(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AutoDataContext>();
                dbContext.Database.Migrate();
                dbContext.Database.EnsureCreated();
            }
        }
    }
}
