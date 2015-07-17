using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using NGator.Net.App_Start;
using NGator.Net.Infrastructure;
using Ninject;
using Ninject.Web.Common;
using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof (NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof (NinjectWebCommon), "Stop")]

namespace NGator.Net.App_Start
{
    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        ///     Kernel Accessor
        /// </summary>
        public static IKernel Kernel { get; private set; }

        /// <summary>
        ///     Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof (OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof (NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        ///     Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        ///     Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            Kernel = kernel;
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        ///     Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            var containerConfigurator = new NinjectConfigurator();
            containerConfigurator.Configure(kernel);
            DependencyResolver.SetResolver(new NinjectMvcDependencyResolver(kernel));
        }
    }
}