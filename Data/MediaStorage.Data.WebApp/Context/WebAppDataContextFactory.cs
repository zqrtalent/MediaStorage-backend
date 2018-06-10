using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using MediaStorage.Data.Context;

/*
Add data migration: dotnet ef migrations add migration_name_here
Update database: dotnet ef database update
 */

namespace MediaStorage.Data.WebApp.Context
{
    public class WebAppDataContextFactory : IDesignTimeDbContextFactory<WebAppDataContext>
    {
        public WebAppDataContext CreateDbContext(string[] args)
        {
            var connectionString = "server=localhost;userid=root;pwd=mysqlpass;database=MediaStreamingAppDb;sslmode=none;";
            var optionsBuilder = new DbContextOptionsBuilder<WebAppDataContext>();
            optionsBuilder.UseMySql(connectionString);

            return new WebAppDataContext(optionsBuilder.Options);
        }
    }
}