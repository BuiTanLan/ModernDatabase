using System;
using System.Linq;
using System.Linq.Expressions;
using Core.Entities;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
namespace Infrastructure.Data
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
    {
 
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            var query = inputQuery;
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }
            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }
            if(spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }
            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
            return query;
        }    
    }

    public class SpecificationEvaluatorMongo<TEntity> where TEntity : BaseEntityMongo
    {
        public static IFindFluent<TEntity, TEntity> GetQuery(IMongoCollection<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            IFindFluent<TEntity, TEntity> query = null;
            if (spec.Criteria != null)
            {
                query = inputQuery.Find(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = query.SortBy(spec.OrderBy);
            }
            if (spec.OrderByDescending != null)
            {
                query = query.SortByDescending(spec.OrderByDescending);
            }
            if (!spec.IsPagingEnabled) return query;
            query = query?.Skip(spec.Skip).Limit(spec.Take);
            //query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            return query;
        }
    }
}