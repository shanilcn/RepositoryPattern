using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PagedList;
using System.Linq.Expressions;

namespace DataAccess
{
    interface IGenericDataAccess<TEntity>
        where TEntity : class
    {
        TOutput GetSingleOrDefault<TOutput>(Expression<Func<TEntity, bool>> whereCondition);
        TOutput GetSingle<TOutput>(Expression<Func<TEntity, bool>> whereCondition);
        TOutput GetUnique<TOutput>(Expression<Func<TEntity, bool>> whereCondition);
        IPagedList<TOutput> GetAllWithoutFilter<TOutput>(string sortExpression, int Page, int PageSize);
        IPagedList<TOutput> GetAllWithFilter<TOutput>(Expression<Func<TEntity, bool>> whereCondition, string sortExpression, int Page, int PageSize);
        IEnumerable<TOutput> GetAllWithoutFilterWithoutPaging<TOutput>(string sortExpression);
        IEnumerable<TOutput> GetAllWithFilterWithoutPaging<TOutput>(Expression<Func<TEntity, bool>> whereCondition, string sortExpression);
        string Add<TInput>(TInput entity);
        string Update<TInput>(Expression<Func<TEntity, bool>> whereCondition, TInput entity);   
        string Delete<TInput>(TInput entity);
    }
}
