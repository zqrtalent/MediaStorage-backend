using System;
using MediaStorage.Data.Context;
using Microsoft.EntityFrameworkCore;
using MediaStorage.Data.Streaming.Entities;

namespace MediaStorage.Data.UnitTests.Context
{
    internal class TestDataContext : EfDataContext
    {
        public TestDataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<StreamingSession> StreamingSessions { get; set; }
        public DbSet<StreamingUser> StreamingUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StreamingUser>().HasIndex(e=> e.UserName).IsUnique(true);
            //builder.Entity<StreamingSession>().HasIndex(e=> new { e.UserId, e.IsActive }).HasName(@"IDX_StreamingSession_User").IsUnique(true);
            builder.Entity<StreamingSession>().HasIndex(e=> e.UserId).HasName(@"IDX_StreamingSession_User").IsUnique(true);
        }
    }

}