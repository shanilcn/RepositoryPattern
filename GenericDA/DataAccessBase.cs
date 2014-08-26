using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using LinqKit;
using PagedList;
using LAMSFinishingDA.Infrastructure;


namespace DataAccess
{
    /// <summary>
    /// This is an abstract class for data operations on ODATA services
    /// </summary>
    /// <typeparam name="TViewModel">This is the model Controller project would be operating against</typeparam>
    /// <typeparam name="TReadModel">Read Entity model of ODATA service</typeparam>
    /// <typeparam name="TEditModel">Edit Entity model of ODATA service</typeparam>
    public abstract class DataAccessBase<TViewModel, TReadModel, TEditModel>
    {
        protected Type _ReadEntityType;
        protected Type _EditEntityType;
        private Type GenericDataAccessType;
        private object GenericDataAccessObject;
        public string ContextConfigKey;

        /// <summary>
        /// Setting read & edit entity types
        /// </summary>
        public DataAccessBase()
        {
            _ReadEntityType = typeof(TReadModel);
            _EditEntityType = typeof(TEditModel);
        }

        /// <summary>
        /// Initializing objects to call Generic Data Access class which does all the actual data operations.
        /// </summary>
        /// <param name="EntityType">Either _ReadEntityType or _EditEntityType</param>
        /// <param name="ContextType">Either Read or Edit</param>
        private void GenericDataAccessInitialize(Type EntityType, string ContextType)
        {

            Type Generic = typeof(GenericDataAccess<>);
            GenericDataAccessType = Generic.MakeGenericType(EntityType);
            GenericDataAccessObject = Activator.CreateInstance(GenericDataAccessType, new object[] { ContextConfigKey + ContextType });
        }

        /// <summary>
        /// Function to Get All items from an entity set without any search filters. 
        /// 1. Query without any filter
        /// 2. Pagination if page meta data is passed
        /// 3. Sort the result based on the expression passed
        /// </summary>
        /// <param name="Page">Sets the Page number</param>
        /// <param name="PageSize">Sets total items in a page</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a paged list of result </returns>
        public IPagedList<TReturnModel> GetAll<TReturnModel>(int Page, int PageSize, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithoutFilter");
            method = method.MakeGenericMethod(typeof(TReturnModel));
            return (IPagedList<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { sortExpression, Page, PageSize });
        }

        /// <summary>
        /// Function to Get All items from an entity set without any search filters. 
        /// 1. Query without any filter
        /// 2. Sort the result based on the expression passed
        /// </summary>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a list of result </returns>
        public IEnumerable<TReturnModel> GetAll<TReturnModel>(string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithoutFilterWithoutPaging");
            method = method.MakeGenericMethod(typeof(TReturnModel));
            return (IEnumerable<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { sortExpression });
        }

        /// <summary>
        /// Function to Get All items from an entity set based on search values present in Object. 
        /// 1. Query by trying to match each non default values exactly with database
        /// 2. Pagination if page meta data is passed
        /// 3. Sort the result based on the expression passed
        /// </summary>
        /// <param name="SearchObject">An AND based query is generated for non default fields in SearchObject</param>
        /// <param name="Page">Sets the Page number</param>
        /// <param name="PageSize">Sets total items in a page</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a paged list of result </returns>
        public IPagedList<TReturnModel> GetAllExact<TReturnModel>(TViewModel SearchObject, int Page, int PageSize, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            bool SearchValuePresent = false;
            if (SearchObject != null)
            {
                PropertyInfo[] prop = SearchObject.GetType().GetProperties();
                foreach (PropertyInfo p in prop)
                {
                    if (p.GetValue(SearchObject, null) != null)
                    {
                        SearchValuePresent = true;
                        break;
                    }
                }
            }
            if (SearchValuePresent)
            {
                MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithFilter");
                method = method.MakeGenericMethod(typeof(TReturnModel));
                return (IPagedList<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { BuildSearchConditionFromObject(SearchObject, false), sortExpression, Page, PageSize });
            }
            else
            {
                MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithoutFilter");
                method = method.MakeGenericMethod(typeof(TReturnModel));
                return (IPagedList<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { sortExpression, Page, PageSize });
            }
        }

        /// <summary>
        /// Function to Get All items from an entity set based on search values present in Object. 
        /// 1. Query by trying to match each non default values exactly with database
        /// 2. Sort the result based on the expression passed
        /// </summary>
        /// <param name="SearchObject">An AND based query is generated for non default fields in SearchObject</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Returns a list of result </returns>
        public IEnumerable<TReturnModel> GetAllExact<TReturnModel>(TViewModel SearchObject, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            bool SearchValuePresent = false;
            if (SearchObject != null)
            {
                PropertyInfo[] prop = SearchObject.GetType().GetProperties();
                foreach (PropertyInfo p in prop)
                {
                    if (p.GetValue(SearchObject, null) != null)
                    {
                        SearchValuePresent = true;
                        break;
                    }
                }
            }
            if (SearchValuePresent)
            {
                MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithFilterWithoutPaging");
                method = method.MakeGenericMethod(typeof(TReturnModel));
                return (IEnumerable<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { BuildSearchConditionFromObject(SearchObject, false), sortExpression });
            }
            else
            {
                MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithoutFilterWithoutPaging");
                method = method.MakeGenericMethod(typeof(TReturnModel));
                return (IEnumerable<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { sortExpression });
            }
        }

        /// <summary>
        /// Function to Get All items from an entity set with custom search filters. 
        /// 1. Query with custom built search filter.
        /// 2. Pagination if page meta data is passed
        /// 3. Sort the result based on the expression passed
        /// </summary>
        /// <param name="SearchObject">An custom query has to be created for fields in SearchObject to use this function</param>
        /// <param name="Page">Sets the Page number</param>
        /// <param name="PageSize">Sets total items in a page</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a paged list of result </returns>
        public IPagedList<TReturnModel> GetAllWithCustomQuery<TReturnModel>(TViewModel SearchObject, int Page, int PageSize, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithFilter");
            method = method.MakeGenericMethod(typeof(TReturnModel));
            Mapper.CreateMap<TViewModel, TReadModel>();
            return (IPagedList<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { BuildGetAllCustomCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)), sortExpression, Page, PageSize });
        }

        /// <summary>
        /// Function to Get All items from an entity set with custom search filters. 
        /// 1. Query with custom built search filter.
        /// 2. Sort the result based on the expression passed
        /// </summary>
        /// <param name="SearchObject">An custom query has to be created for fields in SearchObject to use this function</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a list of result </returns>
        public IEnumerable<TReturnModel> GetAllWithCustomQuery<TReturnModel>(TViewModel SearchObject, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithFilterWithoutPaging");
            method = method.MakeGenericMethod(typeof(TReturnModel));
            Mapper.CreateMap<TViewModel, TReadModel>();
            return (IEnumerable<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { BuildGetAllCustomCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)), sortExpression });
        }

        /// <summary>
        /// Function to Get All items from an entity set based on search values present in Object. "StartsWith" will be applied for all string fields.
        /// 1. Query with search filter. "StartsWith" will be applied for all string fields.
        /// 2. Pagination if page meta data is passed
        /// 3. Sort the result based on the expression passed
        /// </summary>
        /// <param name="SearchObject">An custom query has to be created for fields in SearchObject to use this function</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a paged list of result </returns>
        public  IPagedList<TReturnModel> GetAllStartsWith<TReturnModel>(TViewModel SearchObject, int Page, int PageSize, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithFilter");
            method = method.MakeGenericMethod(typeof(TReturnModel));
            return (IPagedList<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { BuildSearchConditionFromObject(SearchObject, true), sortExpression, Page, PageSize });
        }

        /// <summary>
        /// Function to Get All items from an entity set based on search values present in Object. "StartsWith" will be applied for all string fields.
        /// 1. Query with search filter. "StartsWith" will be applied for all string fields.
        /// 2. Sort the result based on the expression passed
        /// </summary>
        /// <param name="SearchObject">An custom query has to be created for fields in SearchObject to use this function</param>
        /// <param name="sortExpression">If sort Expression is available, this function will return an ordered list</param>
        /// <returns>Return a list of result </returns>
        public IEnumerable<TReturnModel> GetAllStartsWith<TReturnModel>(TViewModel SearchObject, string sortExpression = "")
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetAllWithFilterWithoutPaging");
            method = method.MakeGenericMethod(typeof(TReturnModel));
            return (IEnumerable<TReturnModel>)method.Invoke(GenericDataAccessObject, new object[] { BuildSearchConditionFromObject(SearchObject, true), sortExpression });
        }

        /// <summary>
        /// Get a single record based on the primary key field value
        /// </summary>
        /// <param name="SearchObject">This object should have value in Primary Key field for correct result</param>
        /// <returns>Single record of the object</returns>
        public TViewModel GetSingle(TViewModel SearchObject)
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetSingle");
            method = method.MakeGenericMethod(typeof(TViewModel));
            Mapper.CreateMap<TViewModel, TReadModel>();
            return (TViewModel)method.Invoke(GenericDataAccessObject, new object[] { BuildPrimaryKeyWhereCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)) });
        }

        /// <summary>
        /// Get a single record based on the primary key field value
        /// </summary>
        /// <param name="SearchObject">This object should have value in Primary Key field for correct result</param>
        /// <returns>Single record of the object, Null if no such record exists in table</returns>
        public TViewModel GetSingleOrDefault(TViewModel SearchObject)
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetSingleOrDefault");
            method = method.MakeGenericMethod(typeof(TViewModel));
            Mapper.CreateMap<TViewModel, TReadModel>();
            return (TViewModel)method.Invoke(GenericDataAccessObject, new object[] { BuildPrimaryKeyWhereCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)) });
        }

        /// <summary>
        /// Get a single record based on the custom query. Custom Query has to be built for this function to perform correctly.
        /// </summary>
        /// <param name="SearchObject">This object should have search values</param>
        /// <returns>Single record of the object</returns>
        public TViewModel GetSingleWithCustomQuery(TViewModel SearchObject)
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetSingle");
            method = method.MakeGenericMethod(typeof(TViewModel));
            Mapper.CreateMap<TViewModel, TReadModel>();
            return (TViewModel)method.Invoke(GenericDataAccessObject, new object[] { BuildGetSingleCustomCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)) });
        }

        /// <summary>
        /// Get a single record based on the custom query. Custom Query has to be built for this function to perform correctly.
        /// </summary>
        /// <param name="SearchObject">This object should have search values</param>
        /// <returns>Single record of the object, Null if no such record exists in table</returns>
        public TViewModel GetSingleOrDefaultWithCustomQuery(TViewModel SearchObject)
        {
            GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
            MethodInfo method = GenericDataAccessType.GetMethod("GetSingleOrDefault");
            method = method.MakeGenericMethod(typeof(TViewModel));
            Mapper.CreateMap<TViewModel, TReadModel>();
            return (TViewModel)method.Invoke(GenericDataAccessObject, new object[] { BuildGetSingleCustomCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)) });
        }

        /// <summary>
        /// Add Entity to Database
        /// </summary>
        /// <param name="Entity">Entity to be added</param>
        /// <returns>Success or failure message</returns>
        public string Add(TViewModel Entity)
        {
            try
            {
                GenericDataAccessInitialize(_EditEntityType, GlobalConstants.CONTEXT_Edit_Context);
                MethodInfo method = GenericDataAccessType.GetMethod("Add");
                method = method.MakeGenericMethod(typeof(TViewModel));
                return (string)method.Invoke(GenericDataAccessObject, new object[] { Entity });
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Updates the entity passed in the database
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public string Update(TViewModel Entity)
        {
            try
            {
                GenericDataAccessInitialize(_EditEntityType, GlobalConstants.CONTEXT_Edit_Context);
                MethodInfo method = GenericDataAccessType.GetMethod("Update");
                method = method.MakeGenericMethod(typeof(TViewModel));
                Mapper.CreateMap<TViewModel, TEditModel>();
                return (string)method.Invoke(GenericDataAccessObject, new object[] { BuildPrimaryKeyWhereCondition(Mapper.Map<TViewModel, TEditModel>(Entity)), Entity });     //, BuildPrimaryKeyWhereCondition(Mapper.Map<T, U>(Entity))
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string Delete(TViewModel Entity)
        {
            try
            {
                GenericDataAccessInitialize(_EditEntityType, GlobalConstants.CONTEXT_Edit_Context);
                MethodInfo method = GenericDataAccessType.GetMethod("Delete");
                method = method.MakeGenericMethod(typeof(TViewModel));
                return (string)method.Invoke(GenericDataAccessObject, new object[] { Entity });
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public TViewModel GetUnique(TViewModel SearchObject)
        //{
        //    try
        //    {
        //        GenericDataAccessInitialize(_ReadEntityType, GlobalConstants.CONTEXT_Read_Context);
        //        MethodInfo method = GenericDataAccessType.GetMethod("GetUnique");
        //        method = method.MakeGenericMethod(typeof(TViewModel));
        //        Mapper.CreateMap<TViewModel, TReadModel>();
        //        return (TViewModel)method.Invoke(GenericDataAccessObject, new object[] { BuildUniqueKeyCondition(Mapper.Map<TViewModel, TReadModel>(SearchObject)) });
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        private dynamic BuildPrimaryKeyWhereCondition<TEntity>(TEntity SearchObject)
        {
            var predicate = PredicateBuilder.True<TEntity>();
            ICollection<string> PrimaryKeys = GetPrimaryKeys(typeof(TEntity));
            foreach (string keys in PrimaryKeys)
            {
                PropertyInfo prop = SearchObject.GetType().GetProperty(keys);
                ParameterExpression p = Expression.Parameter(typeof(TEntity), "p");
                MemberExpression body = LambdaExpression.Property(p, prop);
                LambdaExpression expr = LambdaExpression.Lambda(body, p);


                Expression<Func<TEntity, bool>> exp = Expression.Lambda<Func<TEntity, bool>>(
                                                                                            Expression.Equal(
                                                                                                                expr.Body,
                                                                                                                Expression.Constant(prop.GetValue(SearchObject, null))
                                                                                                            ),
                                                                                            expr.Parameters
                                                                                            );
                predicate = predicate.And<TEntity>(exp);
            }
            return predicate;
        }

        private dynamic BuildSearchConditionFromObject(TViewModel SearchObject, bool searchMode)
        {
            MethodInfo startsWithMethodInfo = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var predicate = PredicateBuilder.True<TReadModel>();

            foreach (PropertyInfo entityPropertyInfo in typeof(TReadModel).GetProperties())
            {
                if (!entityPropertyInfo.PropertyType.FullName.Contains("DataServiceCollection"))
                {
                    PropertyInfo viewModelPropertyInfo = typeof(TViewModel).GetProperty(entityPropertyInfo.Name);

                    System.Type tp = ("").GetType();
                    if (viewModelPropertyInfo != null)
                    {
                        tp = viewModelPropertyInfo.PropertyType;
                    }

                    if (viewModelPropertyInfo != null && viewModelPropertyInfo.GetValue(SearchObject, null) != null && !isDefaultValue(viewModelPropertyInfo.GetValue(SearchObject, null).ToString(), tp))
                    {
                        ParameterExpression p = Expression.Parameter(typeof(TReadModel), "p");
                        MemberExpression body = LambdaExpression.Property(p, entityPropertyInfo);
                        LambdaExpression expr = LambdaExpression.Lambda(body, p);
                        bool applyStartsWithIfString = false;
                        if (searchMode && viewModelPropertyInfo.PropertyType.FullName == typeof(string).FullName)
                        {
                            applyStartsWithIfString = true;
                        }
                        if (applyStartsWithIfString)
                        {
                            Expression<Func<TReadModel, bool>> exp = Expression.Lambda<Func<TReadModel, bool>>(
                                                                                                        Expression.Call(
                                                                                                                            expr.Body, startsWithMethodInfo,
                                                                                                                            Expression.Constant(viewModelPropertyInfo.GetValue(SearchObject, null))
                                                                                                                        ),
                                                                                                        expr.Parameters
                                                                                                        );
                            predicate = predicate.And<TReadModel>(exp);
                        }
                        else
                        {
                            Expression<Func<TReadModel, bool>> exp = Expression.Lambda<Func<TReadModel, bool>>(
                                                                                                        Expression.Equal(
                                                                                                                            expr.Body,
                                                                                                                            Expression.Convert(Expression.Constant(viewModelPropertyInfo.GetValue(SearchObject, null)), entityPropertyInfo.PropertyType)
                                                                                                                        ),
                                                                                                        expr.Parameters
                                                                                                        );
                            predicate = predicate.And<TReadModel>(exp);
                        }

                    }
                }
            }
            return predicate;
        }

        private bool isDefaultValue(string strVal, System.Type type)
        {
            if (type.IsValueType)
            {
                if (Activator.CreateInstance(type) != null)
                {
                    return strVal == Activator.CreateInstance(type).ToString();
                }
            }
            return false;
        }

        private ICollection<string> GetPrimaryKeys(Type typeOfEntity)
        {
            DataServiceKeyAttribute dataServiceKeyAttribute = (DataServiceKeyAttribute)typeOfEntity.GetCustomAttributes(typeof(DataServiceKeyAttribute), false).FirstOrDefault();
            var keyCollection = dataServiceKeyAttribute.KeyNames;
            return keyCollection;
        }

        protected virtual dynamic BuildGetAllCustomCondition(TReadModel SearchKey)
        {
            throw new NotImplementedException();
        }

        //protected virtual dynamic BuildUniqueKeyCondition(TReadModel SearchKey)
        //{
        //    throw new NotImplementedException();
        //}

        protected virtual dynamic BuildGetSingleCustomCondition(TReadModel SearchKey)
        {
            throw new NotImplementedException();
        }
    }
}
