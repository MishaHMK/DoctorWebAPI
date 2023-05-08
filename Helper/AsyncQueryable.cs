using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DoctorWebApi.Helper
{
    public static class AsyncQueryable
    {
        public static IQueryable<TEntity> AsAsyncQueryable<TEntity>(this IEnumerable<TEntity> source)
            => new AsyncQueryable<TEntity>(source ?? throw new ArgumentNullException(nameof(source)));
    }

    public class AsyncQueryable<TEntity> : EnumerableQuery<TEntity>, IAsyncEnumerable<TEntity>, IQueryable<TEntity>
    {
        public AsyncQueryable(IEnumerable<TEntity> enumerable) : base(enumerable) { }
        public AsyncQueryable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<TEntity> GetEnumerator() => new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new AsyncQueryProvider(this);

        class AsyncEnumerator : IAsyncEnumerator<TEntity>
        {
            private readonly IEnumerator<TEntity> inner;
            public AsyncEnumerator(IEnumerator<TEntity> inner) => this.inner = inner;
            public void Dispose() => inner.Dispose();
            public TEntity Current => inner.Current;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(inner.MoveNext());
            public async ValueTask DisposeAsync() => inner.Dispose();
        }

        class AsyncQueryProvider : IAsyncQueryProvider
        {
            private readonly IQueryProvider inner;
            internal AsyncQueryProvider(IQueryProvider inner) => this.inner = inner;
            public IQueryable CreateQuery(Expression expression) => new AsyncQueryable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncQueryable<TElement>(expression);
            public object Execute(Expression expression) => inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) => new AsyncQueryable<TResult>(expression);
            TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) => Execute<TResult>(expression);
        }
    }

}

