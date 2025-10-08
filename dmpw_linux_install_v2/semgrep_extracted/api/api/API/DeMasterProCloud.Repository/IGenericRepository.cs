using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Generic repository interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericRepository<T> where T : class
    {
        // Marks an entity as new
        void Add(T entity);
        // Marks an entity as modified
        void Update(T entity);
        // Marks an entity to be removed

        void DeleteFromSystem(T entity);
        void DeleteRangeFromSystem(IEnumerable<T> entities);
        void DeleteFromSystem(Expression<Func<T, bool>> where);

        void Delete(T entity);
        void Delete(Expression<Func<T, bool>> where);
        void DeleteRange(IEnumerable<T> entities);
        //void DeleteRange(params T[] entities);
        // Get an entity by int id
        T GetById(int id);
        // Get an entity using delegate
        T Get(Expression<Func<T, bool>> where);
        // Gets all entities of type T
        IEnumerable<T> GetAll();
        // Gets entities using delegate
        IEnumerable<T> GetMany(Func<T, bool> where);
        int Count(Expression<Func<T, bool>> where);
        int Count();
        IQueryable<T> Gets(Expression<Func<T, bool>> where);

        void UpdateSpecific(T entity, List<string> properties);
    }
}