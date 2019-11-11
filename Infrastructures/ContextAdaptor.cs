using System;

namespace core.data.helper.infrastructures
{
#pragma warning disable CS8603 
    public class ContextAdaptor<TContext> : IContextAdaptor<TContext> where TContext : class, IDbContext, IDisposable
    {
        private readonly TContext DataContext;

        public ContextAdaptor(TContext context) { DataContext = context; }

        public ContextAdaptor() { }

        public TContext GetContext() { return DataContext; }

        public void Dispose() { DataContext?.Dispose(); }
    }
}
