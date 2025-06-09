using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Moq.Language.Flow;

namespace GestMantIA.Application.UnitTests.TestHelpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> BuildMock<T>(this IQueryable<T> source)
        {
            var mock = new Mock<IQueryable<T>>();
            var enumerable = new TestAsyncEnumerable<T>(source);
            
            mock.As<IAsyncEnumerable<T>>()
                .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(enumerable.GetAsyncEnumerator());

            mock.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(source.Provider));

            mock.As<IQueryable<T>>()
                .Setup(m => m.Expression)
                .Returns(source.Expression);
                
            mock.As<IQueryable<T>>()
                .Setup(m => m.ElementType)
                .Returns(source.ElementType);
                
            mock.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(source.GetEnumerator());
                
            return mock.Object;
        }
    }

    public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            
            var resultType = typeof(TResult).GetGenericArguments()?[0];
            if (resultType == null) 
                throw new InvalidOperationException("TResult must be a generic Task or ValueTask type.");

            var executeMethod = typeof(IQueryProvider)
                .GetMethods()
                .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                .MakeGenericMethod(resultType);

            var result = executeMethod.Invoke(_inner, new object[] { expression });
            
            return (TResult)Activator.CreateInstance(
                typeof(Task<>).MakeGenericType(resultType),
                new object[] { result })!;
        }
    }

    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }
    }
}
