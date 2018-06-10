using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStorage.Data.Context
{
    public interface IDataContext : IDisposable
    {
        IQueryable<T> Get<T>() where T : class;
        void Add<T>(T entity) where T : class;
        void AddRange<T>(IEnumerable<T> entities) where T : class;
        void Update<T>(T entity) where T : class;
        void UpdateRange<T>(IEnumerable<T> entities) where T : class;
        int Update<T>(Func<T, bool> predicate, Action<T> actionUpdate) where T : class;
        void Delete<T>(T entity) where T : class;
        void DeleteRange<T>(IEnumerable<T> entities) where T : class;
        int Delete<T>(Func<T, bool> predicate) where T : class;
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
