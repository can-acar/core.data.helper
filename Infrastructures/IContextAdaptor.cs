using System;

namespace Core.Data.Helper.Infrastructures
{
    public interface IContextAdaptor<out TContext> : IDisposable where TContext : IDisposable
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        TContext GetContext();
    }
}