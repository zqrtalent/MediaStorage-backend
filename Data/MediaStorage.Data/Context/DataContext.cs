using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MediaStorage.Data.Context
{
    public class EfDataContext : DbContext, IDataContext
    {
        // public EfDataContext(string connectionString) : base()
        // {
        // }

#if DEBUG
        private readonly long _id;
#endif
        public EfDataContext() : base()
        {
        #if DEBUG
            _id = DateTime.UtcNow.Ticks;
            Console.WriteLine($"{nameof(EfDataContext)} -> new {_id}");
        #endif
        }

        public EfDataContext(DbContextOptions options) : base(options)
        {
        #if DEBUG
            _id = DateTime.UtcNow.Ticks;
            Console.WriteLine($"{nameof(EfDataContext)} -> new {_id}");
        #endif
        }

        ~EfDataContext()
        {
        #if DEBUG
            Console.WriteLine($"{nameof(EfDataContext)} -> ~ {_id}");
        #endif
            //Dispose();
        }

        public void Add<T>(T entity) where T : class
        {
            //Set<T>().Add(entity);
            base.Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            base.AddRange(entities);
        }

        public void Delete<T>(T entity) where T : class
        {
            //base.Entry(entity).State = EntityState.Deleted;
            base.Remove(entity);
        }

        public void DeleteRange<T>(IEnumerable<T> entities) where T : class
        {
            base.RemoveRange(entities);
            // foreach(var entity in entities)
            // {
            //     base.Entry(entity).State = EntityState.Deleted;
            // }
        }

        public int Delete<T>(Func<T, bool> predicate) where T : class
        {
            int numDeletedEntities = base.Set<T>()
            .Where(predicate)
            .Select(x => 
            {
                base.Entry(x).State = EntityState.Deleted;
                return x;
            })
            .Count();
            return numDeletedEntities;
        }

        public IQueryable<T> Get<T>() where T : class
        {
            return base.Set<T>();
        }

        public void Update<T>(T entity) where T : class
        {
            base.Update(entity);
        }

        public void UpdateRange<T>(IEnumerable<T> entities) where T : class
        {
            base.UpdateRange(entities);
        }

        public int Update<T>(Func<T, bool> predicate, Action<T> actionUpdate) where T : class
        {
            int numUpdatedEntities = base.Set<T>()
            .Where(predicate)
            .Select(x => 
            {
                actionUpdate.Invoke(x);
                base.Entry(x).State = EntityState.Modified;
                return x;
            })
            .Count();
            return numUpdatedEntities;
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }

        #region Dispose managed/unmanaged resources.

        public override void Dispose()
        { 
        #if DEBUG
            Console.WriteLine($"{nameof(EfDataContext)} -> Dispose");
        #endif
        
            base.Dispose();
        }
        
        #endregion
    }
}
