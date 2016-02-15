using J.SharePoint.Lists.Expressions;
using Microsoft.SharePoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace J.SharePoint.Lists
{
    public static partial class Extension
    {
        public static IQueryable<TSource> ViewScope<TSource>(this IQueryable<TSource> source, SPViewScope scope)
        {
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new Type[] { typeof(TSource) }), new Expression[] { source.Expression, Expression.Constant(scope) }));
        }

        public static IQueryable<TSource> Folder<TSource>(this IQueryable<TSource> source, SPFolder folder)
        {
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new Type[] { typeof(TSource) }), new Expression[] { source.Expression, Expression.Constant(folder) }));
        }

        public static SPTypedListItemCollection<TSource> GetItems<TSource>(this IQueryable<TSource> source) where TSource : SPTypedListItem, new()
        {
            return (SPTypedListItemCollection<TSource>)source.Provider.Execute(source.Expression);
        }
    }

    public class SPTypedListItemCollection<T> : IOrderedQueryable<T> where T : SPTypedListItem, new()
    {
        private bool _throwFieldErrors;
        private SPListItemCollection _items;
        private bool _hasExpression;
        private SPList _list;
        private SPQuery _query;

        public SPListItemCollection Items
        {
            get
            {
                if (_items == null)
                    _items = _list.GetItems(Query);

                return _items;
            }
        }

        public SPQuery Query
        {
            get
            {
                if (_query == null)
                {
                    if (_hasExpression)
                        _query = ((SPTypedListItemCollectionQueryProvider)Provider).GetQuery(Expression);
                    else
                        _query = SharePoint.EmptyQuery;
                }

                return _query;
            }
        }

        public T this[int index]
        {
            get
            {
                return SPTypedListItem.CreateTypedItem<T>(Items[index], _throwFieldErrors);
            }
        }

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public SPTypedListItemCollection(SPList list, bool throwFieldErrors = false)
        {
            _throwFieldErrors = throwFieldErrors;
            _list = list;
            Expression = Expression.Constant(this);
        }

        public SPTypedListItemCollection(SPList list, SPQuery query, bool throwFieldErrors = false)
        {
            _throwFieldErrors = throwFieldErrors;
            _list = list;
            _query = query;
            Expression = Expression.Constant(this);
        }

        protected SPTypedListItemCollection(SPList list, Expression expression, IQueryProvider provider, bool throwFieldErrors = false)
        {
            _list = list;
            _throwFieldErrors = throwFieldErrors;
            Expression = expression;
            Provider = provider;
            _hasExpression = true;
        }

        public SPTypedListItemCollection<T> NextPage()
        {
            Query.ListItemCollectionPosition = Items.ListItemCollectionPosition;
            return new SPTypedListItemCollection<T>(_list, Query, _throwFieldErrors);
        }

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return new SPTypedListItemCollectionEnumerator<T>((SPTypedListItemCollection<T>)Provider.Execute<System.Collections.IEnumerable>(Expression));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SPTypedListItemCollectionEnumerator<T>((SPTypedListItemCollection<T>)Provider.Execute<System.Collections.IEnumerable>(Expression));
        }
        #endregion IEnumerable

        #region Enumerator
        protected class SPTypedListItemCollectionEnumerator<Y> : IEnumerator<Y> where Y : SPTypedListItem, new()
        {
            private IEnumerator _enumerator;
            private SPTypedListItemCollection<Y> _typedListItemCollection;
            private bool _throwFieldErrors;

            public Y Current
            {
                get { return SPTypedListItem.CreateTypedItem<Y>((SPListItem)Enumerator.Current, _throwFieldErrors); }
            }

            private IEnumerator Enumerator
            {
                get
                {
                    if (_enumerator == null)
                        _enumerator = _typedListItemCollection.Items.GetEnumerator();

                    return _enumerator;
                }
            }

            public SPTypedListItemCollectionEnumerator(SPTypedListItemCollection<Y> listItemCollection, bool throwFieldErrors = false)
            {
                _throwFieldErrors = throwFieldErrors;
                _typedListItemCollection = listItemCollection;
            }

            public void Dispose()
            {
                //throw new NotImplementedException();
            }

            object IEnumerator.Current
            {
                get { return Enumerator.Current; }
            }

            public bool MoveNext()
            {
                return Enumerator.MoveNext();
            }

            public void Reset()
            {
                Enumerator.Reset();
            }
        }
        #endregion Enumerator

        #region IQueryable
        public Type ElementType
        {
            get { return typeof(T); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get;
            private set;
        }

        private SPTypedListItemCollectionQueryProvider _queryProvider;
        public IQueryProvider Provider
        {
            get { return _queryProvider != null ? _queryProvider : new SPTypedListItemCollectionQueryProvider(_list, _throwFieldErrors); }
            private set { _queryProvider = (SPTypedListItemCollectionQueryProvider)value; }
        }
        #endregion IQueryable

        #region QueryProvider
        protected class SPTypedListItemCollectionQueryProvider : IQueryProvider
        {
            private SPList _list;
            private bool _throwFieldErrors;

            public SPTypedListItemCollectionQueryProvider(SPList list, bool throwFieldErrors)
            {
                _list = list;
                _throwFieldErrors = throwFieldErrors;
            }

            public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
            {
                return (IQueryable<TResult>)CreateQuery(expression);
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new SPTypedListItemCollection<T>(_list, expression, this, _throwFieldErrors);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return (TResult)Execute(expression);
            }

            public SPQuery GetQuery(Expression expression)
            {
                Expression evalExpression = Evaluator.PartialEval(expression);
                return (new SPQueryTranslator()).Translate(evalExpression);
            }

            public object Execute(Expression expression)
            {
                return new SPTypedListItemCollection<T>(_list, GetQuery(expression), _throwFieldErrors);
            }
        }
        #endregion QueryProvider
    }
}
