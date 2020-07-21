using System;
using Microsoft.EntityFrameworkCore;

namespace Core.Data.Helper.Infrastructures
{
#pragma warning disable CS8603
    public class ContextAdaptor<TContext> : IContextAdaptor<TContext> where TContext : DbContext, IDisposable
    {
        public ContextAdaptor(TContext context)
        {
            DbContext = context;
        }

        public ContextAdaptor()
        {
        }

        public TContext GetContext()
        {
            return DbContext;
        }

        public TContext DbContext { get; }

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}