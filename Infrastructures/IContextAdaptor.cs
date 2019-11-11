using System;

namespace core.data.helper.infrastructures
{
    public interface IContextAdaptor<out TContext> : IDisposable where TContext : IDisposable
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        TContext GetContext();
    }
}
