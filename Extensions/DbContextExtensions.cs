using Autofac;
using CoreEntityHelper.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreEntityHelper.Extensions;
#pragma warning disable CS8603
public static class DbContextExtensions
{
    private static DbContextOptions<TContext> RegisterContext<TContext>(this IComponentContext componentContext, string connectionStringName, string databaseName = null) where TContext : DbContext
    {
        var serviceProvider = componentContext.Resolve<IServiceProvider>();
        var configuration = componentContext.Resolve<IConfiguration>();
        var dbContextOptions = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());
        var optionsBuilder = new DbContextOptionsBuilder<TContext>(dbContextOptions)
            .UseApplicationServiceProvider(serviceProvider)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .EnableServiceProviderCaching()
            .UseSqlServer(configuration.GetConnectionString(connectionStringName),
                options =>
                {
                    options.UseRelationalNulls();
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
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

        builder.RegisterType<UnitOfWork<TContext>>().As<IUnitOfWork<TContext>>().As<IUnitOfWork>().InstancePerLifetimeScope();
    }

    public static IServiceCollection RegisterContext<TContext>(this IServiceCollection services, string connectionStringName) where TContext : DbContext
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetService<IConfiguration>();

        foreach (var serviceDescriptor in services.AddDbContext<TContext>(options =>
                     options.EnableDetailedErrors()
                         .EnableSensitiveDataLogging()
                         .EnableServiceProviderCaching()
                         .UseSqlServer(configuration.GetConnectionString(connectionStringName),
                             options =>
                             {
                                 options.UseRelationalNulls();
                                 options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                             })
                         .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)))))
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            
        services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

        return services;
    }

    public static void RegisterContext<TContext>(this ContainerBuilder builder, string connectionStringName, string databaseName = null) where TContext : DbContext
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
            .InstancePerLifetimeScope();
    }
}