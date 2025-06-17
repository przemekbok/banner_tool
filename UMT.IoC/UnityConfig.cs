using System;
using System.Net.Http;
using UMT.IServices;
using UMT.IServices.Banner;
using UMT.IServices.Cache;
using UMT.Services;
using UMT.Services.Banner;
using UMT.Services.Cache;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace UMT.IoC
{
    public static class UnityConfig
    {
        public static IUnityContainer Container { get; private set; }

        public static void SetMainContainer(IUnityContainer container)
        {
            Container = container;
            container.RegisterInstance(container, InstanceLifetime.Singleton);
        }

        public static void ConfigureCommon(Action<string> showWarningLoggerAction)
        {
            Container.RegisterSingleton<HttpClient>();

            Container.RegisterType<IBannerService, BannerService>();

            //services
            Container.RegisterType<IApplicationSettingsService, ApplicationSettingsService>();
            Container.RegisterType<IUserSecretService, UserSecretService>();
            Container.RegisterType<IMemoryCacheService, MemoryCacheService>(new ContainerControlledLifetimeManager());
        }

        public static void ConfigureMigrationsDbContext()
        {
            Container.Resolve<MemoryCacheService>().Clear();

            //REPLACE IMigrationsDbContext registration with the factory bellow if you want to debug entity SQL queries
            //Container.RegisterFactory<IMigrationsDbContext>(c =>
            //{
            //    var _logger = c.Resolve<ILogger.ILogger>();
            //    var ctx = new MigrationsDbContext(sourceDto.DbName);
            //    ctx.Database.Log = s =>
            //    {
            //        if(s.Contains("Completed in"))
            //        {
            //            string ms = s.Substring(16, s.IndexOf(" ms") -16);
            //            int mss = int.Parse(ms);
            //            if(mss > 500)
            //            {

            //            }
            //        }
            //        _logger.Warn(s);
            //    };
            //    return ctx;
            //});
        }
    }
}
