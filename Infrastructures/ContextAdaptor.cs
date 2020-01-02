using System;

namespace core.data.helper.infrastructures
{

    #pragma warning disable CS8603
    public class ContextAdaptor<TContext> : IContextAdaptor<TContext> where TContext : class, IDbContext, IDisposable
    {
        private readonly TContext dataContext;

        public ContextAdaptor(TContext context)
        {
            dataContext = context;
        }

        public ContextAdaptor()
        {
        }

        public TContext GetContext()
        {
            return dataContext;
        }

        public void Dispose()
        {
            dataContext?.Dispose();
        }
    }
}