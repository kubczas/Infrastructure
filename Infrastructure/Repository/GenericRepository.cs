using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using Infrastructure.Helpers;
using Infrastructure.Specification;
using Infrastructure.UnitOfWork;

namespace Infrastructure.Repository
{
    public class GenericRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly DbContext _context;
        private readonly IDbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null)
        {
            return predicate != null ? _dbSet.Where(predicate) : _dbSet.AsEnumerable();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public T Delete(Expression<Func<T, bool>> predicate)
        {
            T entity = _dbSet.First(predicate);
            _dbSet.Remove(entity);
            return entity;
        }

        public IQueryable<T> Query(ISpecification<T> specification, IEnumerable<SortSpecification<T>> sortSpecifications, int pageNumber, int pageSize)
        {
            var data = GetAll() as IQueryable<T>;
            data = ApplyFilterSpecification(data, specification);
            data = ApplySortSpecification(data, sortSpecifications);
            data = ApplyPaging(data, pageNumber, pageSize);
            return data;
        }

        public IQueryable<T> Query(ISpecification<T> specification)
        {
            var data = GetAll() as IQueryable<T>;
            data = ApplyFilterSpecification(data, specification);
            return data;
        }

        public IQueryable<T> QueryLastPage(ISpecification<T> specification, IEnumerable<SortSpecification<T>> sortSpecifications, int pageSize)
        {
            var data = GetAll() as IQueryable<T>;
            data = ApplyFilterSpecification(data, specification);
            data = ApplySortSpecification(data, sortSpecifications);
            data = ApplyPaging(data, data.Count()/pageSize+1, pageSize);
            return data;
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().AddOrUpdate(entity);
        }

        public void Update(T entity,
            IEnumerable<object> updatedSet, string propertyName)
        {
            Type type = updatedSet.GetType().GetGenericArguments()[0];
            T previous = _dbSet.Include(propertyName).First(e => e.Id == entity.Id);
            var values = ListHelper.CreateList(type);
            foreach (var entry in updatedSet
                .Select(obj => (int)obj
                 .GetType()
                 .GetProperty("ID")
                 .GetValue(obj, null))
                .Select(value => _context.Set(type).Find(value)))
            {
                values.Add(entry);
            }
            _context.Entry(previous).Collection(propertyName).CurrentValue = values;
            _context.Entry(previous).CurrentValues.SetValues(entity);
        }

        public void Create(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Comit()
        {
            _context.SaveChanges();
        }

        private static IQueryable<T> ApplyPaging(IQueryable<T> data, int pageNumber, int pageSize)
        {
            if (pageNumber >= 0 && pageSize > 0)
                data = data.Skip((pageNumber-1) * pageSize).Take(pageSize);
            return data;
        }

        private static IQueryable<T> ApplySortSpecification(IQueryable<T> data, IEnumerable<SortSpecification<T>> sortSpecifications)
        {
            if (sortSpecifications == null) return data;

            foreach (var sortSpecification in sortSpecifications)
            {
                switch (sortSpecification.Direction)
                {
                    case ListSortDirection.Ascending:
                        data = data.OrderBy(sortSpecification.Predicate);
                        break;
                    case ListSortDirection.Descending:
                        data = data.OrderByDescending(sortSpecification.Predicate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return data;
        }

        private static IQueryable<T> ApplyFilterSpecification(IQueryable<T> data, ISpecification<T> specification)
        {
            if (specification != null)
                data = data.Where(specification.IsSatisfiedBy());
            return data;
        }
    }
}
