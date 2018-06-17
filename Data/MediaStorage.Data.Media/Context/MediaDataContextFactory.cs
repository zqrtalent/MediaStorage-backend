using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using MediaStorage.Data.Context;

/*
Add data migration: dotnet ef migrations add migration_name_here
Update database: dotnet ef database update
 */

namespace MediaStorage.Data.Media.Context
{
    public class MediaDataContextFactory : IDesignTimeDbContextFactory<MediaDataContext>
    {
        public MediaDataContext CreateDbContext(string[] args)
        {
            var connectionString = "server=localhost;userid=root;pwd=mysqlpass;database=MediaDb;sslmode=none;";    
            var optionsBuilder = new DbContextOptionsBuilder<MediaDataContext>();
            optionsBuilder.UseMySql(connectionString);

            return new MediaDataContext(optionsBuilder.Options);
        }
    }
}