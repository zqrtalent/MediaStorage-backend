using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;
using MediaStorage.Data.Context;

/*
Add data migration: dotnet ef migrations add migration_name_here
Update database: dotnet ef database update
 */

namespace MediaStorage.Data.Streaming.Context
{
    public class StreamingDataContextFactory : IDesignTimeDbContextFactory<StreamingDataContext>
    {
        public StreamingDataContext CreateDbContext(string[] args)
        {
            var connectionString = "server=localhost;userid=root;pwd=mysqlpass;database=StreamingDb;sslmode=none;";
            var optionsBuilder = new DbContextOptionsBuilder<StreamingDataContext>();
            optionsBuilder.UseMySql(connectionString);

            return new StreamingDataContext(optionsBuilder.Options);
        }
    }
}