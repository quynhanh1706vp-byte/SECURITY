using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using System.Reflection;
using System.ComponentModel;

namespace DeMasterProCloud.Repository
{
    /// <inheritdoc />
    /// <summary>
    /// Generic repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        #region Properties
        private readonly Type _type;
        private readonly HttpContext _httpContext;
        private readonly AppDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        #endregion

        /// <summary>
        /// GenericRepository constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="contextAccessor"></param>
        protected GenericRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
            _httpContext = contextAccessor?.HttpContext;
            _type = typeof(T);
        }

        #region Implementation
        /// <summary>
        /// Add T entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Add(T entity)
        {
            try
            {
                SetCreated(entity);
                SetUpdated(entity);
                _dbSet.Add(entity);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Update T entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(T entity)
        {
            try
            {
                SetUpdated(entity);
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            catch (Exception)
            {

            }
        }

        public virtual void UpdateSpecific(T entity, List<string> columns)
        {
            try
            {
                SetUpdated(entity);
                _dbSet.Attach(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;

                foreach (var property in entity.GetType().GetProperties())
                {
                    try
                    {
                        if(!property.Name.Equals(Constants.CommonFields.UpdatedBy)
                            && !property.Name.Equals(Constants.CommonFields.UpdatedOn))
                        {
                            var result = columns.Contains(property.Name);

                            //if (!result)
                            //{
                            //    Console.WriteLine($"PROPERTY NAME : {property.Name}");
                            //    var t1 = _dbContext.Entry(entity).Property(property.Name).OriginalValue;
                            //    Console.WriteLine($"PROPERTY t1 : {t1}");
                            //    var t2 = _dbContext.Entry(entity).Property(property.Name).CurrentValue;
                            //    Console.WriteLine($"PROPERTY t2 : {t2}");
                            //}

                            _dbContext.Entry(entity).Property(property.Name).IsModified = result;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Delete entity from system.
        /// If there is "IsDeleted" attribute in entity Model, set the "IsDeleted" to TRUE instead of deleting entity from DB.
        /// If there is not "IsDeleted" attribute, Remove the entity from DB.
        /// </summary>
        /// <param name="entity"> entity to be deleted from system. </param>
        public virtual void DeleteFromSystem(T entity)
        {
            // Check "IsDeleted" in entity model
            if (HasProperty(Constants.CommonFields.IsDeleted))
            {
                // Set "IsDeleted" to TRUE.
                SetProperty(entity, Constants.CommonFields.IsDeleted, true);
                // Update entity.
                Update(entity);
            }
            else
            {
                // Remove entity from DB.
                Delete(entity);
            }
        }

        /// <summary>
        /// Delete entities from system.
        /// If there is "IsDeleted" attribute in entity Model, set the "IsDeleted" to TRUE instead of deleting entity from DB.
        /// If there is not "IsDeleted" attribute, Remove the entities from DB.
        /// </summary>
        /// <param name="entity"> list of entity to be deleted from system. </param>
        public virtual void DeleteRangeFromSystem(IEnumerable<T> entities)
        {
            // Check "IsDeleted" in entity model
            if (HasProperty(Constants.CommonFields.IsDeleted))
            {
                // Set "IsDeleted" to TRUE.
                foreach (var entity in entities)
                {
                    // Set "IsDeleted" to TRUE.
                    SetProperty(entity, Constants.CommonFields.IsDeleted, true);
                    // Update entity.
                    Update(entity);
                }
            }
            else
            {
                // Remove entity from DB.
                DeleteRange(entities);
            }
        }

        /// <summary>
        /// Delete entities from system.
        /// The entities is filtered by "where" conditions.
        /// </summary>
        /// <param name="where"> a condition to filter entities to be deleted. </param>
        public virtual void DeleteFromSystem(Expression<Func<T, bool>> where)
        {
            // Filter entities by "where" conditions.
            IEnumerable<T> objects = _dbSet.Where(where).AsEnumerable();
            // Delete from system.
            DeleteRangeFromSystem(objects);
        }

        /// <summary>
        /// Delete T entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Delete a list entities
        /// </summary>
        /// <param name="entities"></param>
        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        ///// <summary>
        ///// Delete a list entities
        ///// </summary>
        ///// <param name="entities"></param>
        //public virtual void DeleteRange(params T[] entities)
        //{
        //    _dbSet.RemoveRange(entities);
        //}

        /// <summary>
        /// Delete T entity by condition
        /// </summary>
        /// <param name="where"></param>
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = _dbSet.Where(where).AsEnumerable();
            foreach (T obj in objects)
            {
                _dbSet.Remove(obj);
            }
        }

        /// <summary>
        /// Get T entity by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        /// <summary>
        /// Get all entities by some condition
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet.AsNoTracking().ToList();
        }

        /// <summary>
        /// Get many T entity by condition
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetMany(Func<T, bool> where)
        {
            return _dbSet.AsNoTracking().AsEnumerable().Where(where);
        }

        /// <summary>
        /// Get T entity by condition
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual T Get(Expression<Func<T, bool>> where)
        {
            return _dbSet.AsNoTracking().FirstOrDefault(where);
        }

        public virtual int Count(Expression<Func<T, bool>> where)
        {
            return _dbSet.AsNoTracking().Count(where);
        }
        
        public virtual int Count()
        {
            return _dbSet.AsNoTracking().Count();
        }

        public virtual IQueryable<T> Gets(Expression<Func<T, bool>> where)
        {
            return _dbSet.Where(where);
        }

        #endregion

        #region Method to check common fields existed
        /// <summary>
        /// Check model have a specific property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected bool HasProperty(string property)
        {
            return _type.GetProperty(property) != null;
        }
        protected void SetProperty(T entity, string property, object value)
        {
            if(value != null)
            {
                entity.GetType().GetProperty(property).SetValue(entity,
                    int.TryParse(value.ToString(), out var number) ? number : value);
            }
        }
        protected void SetCreated(T entity)
        {
            if (HasProperty(Constants.CommonFields.CreatedBy))
            {
                var accountId = _httpContext?.User?.Claims?
                    .FirstOrDefault(m => m.Type == Constants.ClaimName.AccountId)?.Value;

                if(!string.IsNullOrEmpty(accountId))
                {
                    SetProperty(entity, Constants.CommonFields.CreatedBy, accountId);
                }
            }
            if (HasProperty(Constants.CommonFields.CreatedOn))
            {
                SetProperty(entity, Constants.CommonFields.CreatedOn, DateTime.UtcNow);
            }
        }
        protected void SetUpdated(T entity) 
        {
            if (HasProperty(Constants.CommonFields.UpdatedBy))
            {
                var accountId = _httpContext?.User?.Claims?
                    .FirstOrDefault(m => m.Type == Constants.ClaimName.AccountId)?.Value;

                if (!string.IsNullOrEmpty(accountId))
                {
                    SetProperty(entity, Constants.CommonFields.UpdatedBy, accountId);
                }
            }
            if (HasProperty(Constants.CommonFields.UpdatedOn))
            {
                SetProperty(entity, Constants.CommonFields.UpdatedOn, DateTime.UtcNow);
            }
        }
        #endregion
    }
}