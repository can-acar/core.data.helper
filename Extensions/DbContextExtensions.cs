using System;
using System.Collections.Generic;
using Autofac;
using Core.Data.Helper.Infrastructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Data.Helper.Extensions
{
#pragma warning disable CS8603
    public static class DbContextExtensions
    {
        public static IServiceCollection RegisterContext<TContext>(this IServiceCollection services, string connectionStringName) where TContext : DbContext
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            foreach (var ServiceDescriptor in services.AddDbContext<TContext>(options =>
                                                                                  options.UseSqlServer(configuration.GetConnectionString("DB"))
                                                                                         .ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)))))

                services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

            return services;
        }

        public static void RegisterContext<TContext>(this ContainerBuilder builder, string connectionStringName) where TContext : DbContext
        {
            builder.Register(componentContext =>
                   {
                       var ServiceProvider = componentContext.Resolve<IServiceProvider>();
                       var Configuration = componentContext.Resolve<IConfiguration>();
                       var DbContextOptions = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());
                       var OptionsBuilder = new DbContextOptionsBuilder<TContext>(DbContextOptions)
                                            .UseApplicationServiceProvider(ServiceProvider)
                                            .UseSqlServer(Configuration.GetConnectionString(connectionStringName))
                                            // .UseSqlServer(Configuration.GetConnectionString(connectionStringName),
                                            //               serverOptions => serverOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null))
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

            builder.RegisterType<UnitOfWork<TContext>>().As<IUnitOfWork<TContext>>().As<IUnitOfWork>().InstancePerLifetimeScope();
        }
    }
}