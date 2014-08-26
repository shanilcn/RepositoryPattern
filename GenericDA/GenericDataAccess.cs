using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using LinqKit;
using PagedList;
using LAMSFinishingDA.Infrastructure;


namespace DataAccess
{
    public class GenericDataAccess<TEntity> : IGenericDataAccess<TEntity>
        where TEntity : class
    {
        dynamic context;
        string EntitySetName;
        public GenericDataAccess(string ContextConfigKey)
        {
            try
            {                
                context = ServiceContext.SetContext(typeof(TEntity), ContextConfigKey);
                EntitySetName = GetEntitySetName(context.GetType().GetProperties());
                context.IgnoreMissingProperties = true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetEntitySetName(PropertyInfo[] Properties)
        {
            try
            {
                EntitySetName = (from p in Properties
                                 where p.Name.StartsWith(typeof(TEntity).Name) && p.Name.Length < typeof(TEntity).Name.Length + 3
                                 select p.Name).FirstOrDefault();
                return EntitySetName;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
 
        public IPagedList<TOutput> GetAllWithoutFilter<TOutput>(string sortExpression, int Page, int PageSize)
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                IPagedList<TEntity> PagedList;

                if (sortExpression != "" && sortExpression != null)
                    PagedList = GetSortedResults(Query, sortExpression).ToPagedList(Page, PageSize);
                else
                    PagedList = Query.ToPagedList(Page, PageSize);

                Mapper.CreateMap<TEntity, TOutput>();
                var minList = Mapper.Map<IEnumerable<TEntity>, IEnumerable<TOutput>>(PagedList);
                var minPagedList = new StaticPagedList<TOutput>(minList, PagedList);
                return minPagedList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IPagedList<TOutput> GetAllWithFilter<TOutput>(Expression<Func<TEntity, bool>> whereCondition, string sortExpression, int Page, int PageSize)
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                IPagedList<TEntity> PagedList;

                if (sortExpression != "" && sortExpression != null)
                    PagedList = GetSortedResults(Query.Where(whereCondition.Expand()), sortExpression).ToPagedList(Page, PageSize);
                else
                    PagedList = Query.Where(whereCondition.Expand()).ToPagedList(Page, PageSize);

                Mapper.CreateMap<TEntity, TOutput>();
                var minList = Mapper.Map<IEnumerable<TEntity>, IEnumerable<TOutput>>(PagedList);
                var minPagedList = new StaticPagedList<TOutput>(minList, PagedList);
                return minPagedList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<TOutput> GetAllWithoutFilterWithoutPaging<TOutput>(string sortExpression)
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                IEnumerable<TEntity> PagedList;

                if (sortExpression != "" && sortExpression != null)
                    PagedList = GetSortedResults(Query, sortExpression);
                else
                    PagedList = Query;

                Mapper.CreateMap<TEntity, TOutput>();
                var minList = Mapper.Map<IEnumerable<TEntity>, IEnumerable<TOutput>>(PagedList);
                return minList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<TOutput> GetAllWithFilterWithoutPaging<TOutput>(Expression<Func<TEntity, bool>> whereCondition, string sortExpression)
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                IEnumerable<TEntity> PagedList;

                if (sortExpression != "" && sortExpression != null)
                    PagedList = GetSortedResults(Query.Where(whereCondition.Expand()), sortExpression);
                else
                    PagedList = Query.Where(whereCondition.Expand());

                Mapper.CreateMap<TEntity, TOutput>();
                var minList = Mapper.Map<IEnumerable<TEntity>, IEnumerable<TOutput>>(PagedList);
                return minList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IQueryable<TEntity> GetSortedResults(IQueryable<TEntity> Query, string sortExpression)
        {
            IQueryable<TEntity> entityList;
            IOrderedQueryable<TEntity> orderedEntityList;
            if (sortExpression != "" && sortExpression != null)
            {
                string[] sortParts = sortExpression.Split(',');
                var param = Expression.Parameter(typeof(TEntity), string.Empty);
                if (sortParts[0].ToLower().TrimEnd().EndsWith(" desc"))
                {
                    string[] sortParts1 = sortParts[0].Split(' ');
                    var property = Expression.Property(param, sortParts1[0].Trim());
                    var sortLambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), param);
                    orderedEntityList = Query.OrderByDescending<TEntity, object>(sortLambda);
                }
                else
                {
                    var property = Expression.Property(param, sortParts[0].Trim());
                    var sortLambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), param);
                    orderedEntityList = Query.OrderBy<TEntity, object>(sortLambda);
                }

                if (sortParts.Length > 1)
                {
                    for (int i = 1; i < sortParts.Length; i++)
                    {
                        var nextParam = Expression.Parameter(typeof(TEntity), string.Empty);
                        if (sortParts[i].ToLower().TrimEnd().EndsWith(" desc"))
                        {
                            string[] sortParts1 = sortParts[i].Split(' ');
                            var nextProperty = Expression.Property(nextParam, sortParts1[0].Trim());
                            var nextSortLambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(nextProperty, typeof(object)), nextParam);
                            orderedEntityList = orderedEntityList.ThenByDescending<TEntity, object>(nextSortLambda);
                        }
                        else
                        {
                            var nextProperty = Expression.Property(nextParam, sortParts[i].Trim());
                            var nextSortLambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(nextProperty, typeof(object)), nextParam);
                            orderedEntityList = orderedEntityList.ThenBy<TEntity, object>(nextSortLambda);
                        }
                    }
                }
                entityList = orderedEntityList;
            }
            else
                entityList = Query;

            return entityList;
        }

        public TOutput GetSingleOrDefault<TOutput>(Expression<Func<TEntity, bool>> whereCondition)
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                Mapper.CreateMap<TEntity, TOutput>();
                return Mapper.Map<TEntity, TOutput>(Query.Where(whereCondition.Expand()).SingleOrDefault());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public TOutput GetSingle<TOutput>(Expression<Func<TEntity, bool>> whereCondition)
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                Mapper.CreateMap<TEntity, TOutput>();
                return Mapper.Map<TEntity, TOutput>(Query.Where(whereCondition.Expand()).Single());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string Add<TInput>(TInput entity)
        {
            try
            {
                Mapper.CreateMap<TInput, TEntity>();
                context.AddObject(EntitySetName, Mapper.Map<TInput, TEntity>(entity));
                context.SaveChanges();
                return "SUCCESS";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string Update<TInput>(Expression<Func<TEntity, bool>> whereCondition, TInput entity) 
        {
            try
            {
                IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
                var MappedEntity = Query.Where(whereCondition.Expand()).FirstOrDefault();
                Mapper.CreateMap<TInput, TEntity>();
                Mapper.Map<TInput, TEntity>(entity, MappedEntity);

                context.UpdateObject(MappedEntity);
                context.SaveChanges();
                context.Detach(MappedEntity);
                context.SaveChanges();
                return "SUCCESS";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string Delete<TInput>(TInput entity)
        {
            try
            {
                Mapper.CreateMap<TInput, TEntity>();
                TEntity DeleteEntity = Mapper.Map<TInput, TEntity>(entity);
                context.AttachTo(EntitySetName, DeleteEntity);
                context.DeleteObject(DeleteEntity);
                context.SaveChanges();

                return "SUCCESS";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public TOutput GetUnique<TOutput>(Expression<Func<TEntity, bool>> whereCondition)
        {
            IQueryable<TEntity> Query = context.CreateQuery<TEntity>(EntitySetName);
            Mapper.CreateMap<TEntity, TOutput>();
            return Mapper.Map<TEntity, TOutput>(Query.Where(whereCondition.Expand()).FirstOrDefault());
        }
    }
}