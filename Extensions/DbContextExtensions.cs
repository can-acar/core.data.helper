// Copyright (c) Can ACAR  All rights reserved.


using Autofac;
using CoreEntityHelper.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreEntityHelper.Extensions;

/// <summary>
/// 
/// </summary>
public static class DbContextExtensions
{
    private static DbContextOptions<TContext> RegisterContext<TContext>(this IComponentContext componentContext,
        string connectionStringName,
        string databaseName = null) where TContext : DbContext
    {
        var serviceProvider = componentContext.Resolve<IServiceProvider>();
        var configuration = componentContext.Resolve<IConfiguration>();
        var dbContextOptions = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());
        var optionsBuilder = new DbContextOptionsBuilder<TContext>(dbContextOptions)
            .UseApplicationServiceProvider(serviceProvider)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .EnableServiceProviderCaching()
            .UseSqlServer(configuration.GetConnectionString(connectionStringName)!,
                options =>
                {
                    options.UseRelationalNulls();
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
            // .UseSqlServer(Configuration.GetConnectionString(connectionStringName),
            //               serverOptions => serverOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null))
            .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)));

        if (databaseName != null)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: databaseName);
        }

        return optionsBuilder.Options;
    }

    public static void RegisterContext<TContext>(this ContainerBuilder builder, string connectionStringName) where TContext : DbContext
    {
        builder.Register(componentContext => componentContext.RegisterContext<TContext>(connectionStringName))
            .As<DbContextOptions<TContext>>()
            .InstancePerLifetimeScope();

        builder.Register(context => context.Resolve<DbContextOptions<TContext>>())
            .As<DbContextOptions>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork<TContext>>().As<IUnitOfWork<TContext>>().As<IUnitOfWork>().InstancePerDependency();
    }


    private static DbContextOptions<TContext> MariaDbRegisterContext<TContext>(this IComponentContext componentContext,
        string connectionStringName,
        string databaseName = null) where TContext : DbContext
    {
        // var serverVersion = new MariaDbServerVersion(new Version(10, 8, 3));
        var serviceProvider = componentContext.Resolve<IServiceProvider>();
        var configuration = componentContext.Resolve<IConfiguration>();
        var dbContextOptions = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());
        var optionsBuilder = new DbContextOptionsBuilder<TContext>(dbContextOptions)
            // .UseApplicationServiceProvider(serviceProvider)
            // .EnableDetailedErrors()
            // .EnableSensitiveDataLogging()
            // .EnableServiceProviderCaching()
            //.UseMySql(configuration.GetConnectionString(connectionStringName), serverVersion)
            // .UseSqlServer(Configuration.GetConnectionString
            //               serverOptions => serverOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null))
            .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)));

        if (databaseName != null)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: databaseName);
        }

        return optionsBuilder.Options;
    }

    public static void RegisterMariaDbContext<TContext>(this ContainerBuilder builder, string connectionStringName)
        where TContext : DbContext
    {
        builder.Register(componentContext => componentContext.MariaDbRegisterContext<TContext>(connectionStringName))
            .As<DbContextOptions<TContext>>()
            .InstancePerLifetimeScope();

        builder.Register(context => context.Resolve<DbContextOptions<TContext>>())
            .As<DbContextOptions>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork<TContext>>().As<IUnitOfWork<TContext>>().As<IUnitOfWork>().InstancePerDependency();
    }

    public static IServiceCollection RegisterDbContext<TContext>(this IServiceCollection services, string connectionStringName)
        where TContext : DbContext
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();

        services.AddDbContext<TContext>(options =>
            options.EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .EnableServiceProviderCaching()
                .UseSqlServer(configuration.GetConnectionString(connectionStringName)!,
                    options =>
                    {
                        options.UseRelationalNulls();
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                    })
                .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug))));

        //foreach (var serviceDescriptor in )

        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

        return services;
    }

    public static void RegisterDbContext<TContext>(this ContainerBuilder builder, string connectionStringName, string databaseName = null)
        where TContext : DbContext
    {
        builder.Register(componentContext => componentContext.RegisterContext<TContext>(connectionStringName, databaseName))
            .As<DbContextOptions<TContext>>()
            .InstancePerLifetimeScope();

        builder.Register(context => context.Resolve<DbContextOptions<TContext>>())
            .As<DbContextOptions>()
            .InstancePerLifetimeScope();

        builder.RegisterType<TContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork<TContext>>()
            .As<IUnitOfWork<TContext>>()
            .As<IUnitOfWork>()
            .InstancePerDependency();
    }
}