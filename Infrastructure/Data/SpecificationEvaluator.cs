using System.Linq;
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
            var query = inputQuery;
            var temp = spec.Criteria;
            if (temp == null)
            {
                temp = (_ => true);
            }
            var ret = inputQuery.Find(temp);

            if (spec.OrderBy != null)
            {
                ret = ret.SortBy(spec.OrderBy);
            }
            if (spec.OrderByDescending != null)
            {
                ret = ret.SortByDescending(spec.OrderByDescending);
            }
            if (spec.IsPagingEnabled)
            {
                ret = ret.Skip(spec.Skip).Limit(spec.Take);
            }
            //query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            return ret;
        }
    }
}