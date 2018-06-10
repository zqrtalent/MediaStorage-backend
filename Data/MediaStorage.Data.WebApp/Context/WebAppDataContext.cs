using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaStorage.Data.Context;
using MediaStorage.Data.WebApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MediaStorage.Data.WebApp.Context
{
    public class WebAppDataContext : IdentityDbContext<ApplicationUser>, IWebAppDataContext
    {
        private readonly IConfiguration _configuration;

        public WebAppDataContext(IConfiguration configuration) : base()
        {
            _configuration = configuration;
        }

        public WebAppDataContext(DbContextOptions options = null) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //builder.Entity<IdentityUser>().ToTable("", "");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_configuration.GetConnectionString("WebAppDataConnectionString"));
            }
        }

        public IQueryable<T> Get<T>() where T : class
        {
            return base.Set<T>();
        }

        void IDataContext.Add<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            throw new NotImplementedException();
        }

        void IDataContext.Update<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateRange<T>(IEnumerable<T> entities) where T : class
        {
            throw new NotImplementedException();
        }

        public int Update<T>(Func<T, bool> predicate, Action<T> actionUpdate) where T : class
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public void DeleteRange<T>(IEnumerable<T> entities) where T : class
        {
            throw new NotImplementedException();
        }

        public int Delete<T>(Func<T, bool> predicate) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}
