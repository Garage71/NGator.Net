using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin;
using NGator.Net;
using NGator.Net.App_Start;
using NGator.Net.Controllers;
using NGator.Net.Models;
using Ninject;
using Owin;

[assembly: OwinStartup(typeof (Startup))]

namespace NGator.Net
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);

            var kernel = NinjectWebCommon.Kernel;
            var dependencyResolver = new NinjectSignalRDependencyResolver(kernel);
            kernel.Bind(typeof (IHubConnectionContext<dynamic>)).ToMethod(context =>
                dependencyResolver.Resolve<IConnectionManager>().GetHubContext<NewsHub>().Clients
                ).WhenInjectedInto<INewsProvider>(); //SignalR hub configuration for Ninject

            app.MapSignalR(new HubConfiguration
            {
                Resolver = dependencyResolver
            });
        }

        private class NinjectSignalRDependencyResolver : DefaultDependencyResolver
        {
            private readonly IKernel _kernel;

            public NinjectSignalRDependencyResolver(IKernel kernel)
            {
                _kernel = kernel;
            }

            public override object GetService(Type serviceType)
            {
                return _kernel.TryGet(serviceType) ?? base.GetService(serviceType);
            }

            public override IEnumerable<object> GetServices(Type serviceType)
            {
                return _kernel.GetAll(serviceType).Concat(base.GetServices(serviceType));
            }
        }
    }
}