using Microsoft.EntityFrameworkCore;
using MediaStorage.Data.Streaming.Entities;
using System.Linq;
using System;
using System.Collections.Generic;
using MediaStorage.Data.Context;
using Microsoft.EntityFrameworkCore;
using MediaStorage.Data.Streaming.Entities;

namespace MediaStorage.Data.ConsoleApp
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

    public static class UnitTest11
    {
        public static void TestMethod1()
        {
            var options = new DbContextOptionsBuilder<TestDataContext>()
                .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                .Options;

            using(var context = new TestDataContext(options))
            {
                var streamingUser = new StreamingUser()
                {
                    UserName = "testUser",
                    PasswordSalt = "testUser" + Guid.NewGuid().ToString(),
                };

                var streamingUser1 = new StreamingUser()
                {
                    UserName = "testUser1",
                    PasswordSalt = "testUser1" + Guid.NewGuid().ToString(),
                };

                var streamingUser2 = new StreamingUser()
                {
                    UserName = "testUser2",
                    PasswordSalt = "testUser2" + Guid.NewGuid().ToString(),
                };

                // Add new entity.
                context.Add(streamingUser);
                context.AddRange(new[] {streamingUser1, streamingUser2}.ToList<StreamingUser>());
                context.SaveChanges();

                var q = from su in context.Get<StreamingUser>()
                        where su.Id == streamingUser.Id ||
                                su.Id == streamingUser1.Id ||
                                su.Id == streamingUser2.Id
                        select su;
                
                Console.WriteLine("IDataContext addnew test");
                int ct = q.Count();
                //Assert.IsTrue(q.Count() == 3);
 
                // Update entity.
                streamingUser.UserName = "1"; 
                context.Update(streamingUser);

                streamingUser1.UserName = "2"; 
                context.UpdateRange(new[] {streamingUser1}.ToList<StreamingUser>());
                context.Update<StreamingUser>(x => x.Id == streamingUser2.Id, x => { x.UserName = "3"; } );
                context.SaveChanges();

                var qUpdateCheck = from su in context.Get<StreamingUser>()
                        where   su.UserName == "1" ||
                                su.UserName == "2" ||
                                su.UserName == "3"
                        select su;
                Console.WriteLine("IDataContext update test");
                ct = qUpdateCheck.Count();
                //Assert.IsTrue(qUpdateCheck.Count() == 3);

                // Delete entry test.
                context.Delete(streamingUser);
                context.DeleteRange(new[] {streamingUser1}.ToList<StreamingUser>());
                context.Delete<StreamingUser>(x => x.UserName == "3");
                context.SaveChanges();

                Console.WriteLine("IDataContext delete test");
                bool b = context.Get<StreamingUser>().Any();
                //Assert.IsTrue(!context.Get<StreamingUser>().Any());
            }
        }
    }
}
