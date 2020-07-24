using System;
using System.Collections.Generic;
using System.Data;
using Autofac;
using Core.Data.Helper.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Data.Helper.Extensions
{
#pragma warning disable CS8603
    public static class DbContextExtensions
    {
        public static DbContext GetObjectContext(this IDbContext dbContext)
        {
            return dbContext as DbContext;
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext)
        {
            return dbContext.Database.BeginTransaction();
        }

        public static IDbContextTransaction BeginTransaction(this IDbContext dbContext, IsolationLevel isolationLevel)
        {
            return dbContext.Database.BeginTransaction(isolationLevel);
        }

        public static IServiceCollection RegisterContext<TContext>(this IServiceCollection services, string connectionStringName) where TContext : DbContext
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration   = serviceProvider.GetService<IConfiguration>();

            services.AddDbContext<TContext>(options =>
                                                options.UseSqlServer(configuration.GetConnectionString("DB"))
                                                       .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug))));
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

            return services;
        }

        public static void RegisterContext<TContext>(this ContainerBuilder builder, string connectionStringName) where TContext : DbContext
        {
            builder.Register(componentContext =>
                   {
                       var ServiceProvider  = componentContext.Resolve<IServiceProvider>();
                       var Configuration    = componentContext.Resolve<IConfiguration>();
                     
                       var DbContextOptions = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());
                       var OptionsBuilder = new DbContextOptionsBuilder<TContext>(DbContextOptions)
                                            .UseApplicationServiceProvider(ServiceProvider)
                                            .UseSqlServer(Configuration.GetConnectionString(connectionStringName))
                                            // .UseSqlServer(Configuration.GetConnectionString(connectionStringName),
                                            //               serverOptions => serverOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null))
                                            .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)));

                       return OptionsBuilder.Options;
                   }).As<DbContextOptions<TContext>>()
                   .InstancePerLifetimeScope();

            builder.Register(context => context.Resolve<DbContextOptions<TContext>>())
                   .As<DbContextOptions>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<TContext>()
                   .AsSelf()
                   .InstancePerLifetimeScope();

            builder.Register<IUnitOfWork>(x => new UnitOfWork<TContext>(x.Resolve<TContext>()));
            builder.Register<IUnitOfWork<TContext>>(x => new UnitOfWork<TContext>(x.Resolve<TContext>()));
        }
    }

    public static class ContextExtensions
    {
        //public static TContext GetContext<TContext>(this IDbContext dbContext) where TContext : class, new() => new TContext();

        //public static TContext GetContext<TContext>(this TContext context) where TContext : class, new() => new TContext();
    }
}