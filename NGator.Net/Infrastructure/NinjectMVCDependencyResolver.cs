using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ninject;

namespace NGator.Net.Infrastructure
{
    /// <summary>
    /// Ninject dependency resolver for MVC Infrastructure
    /// </summary>
    public class NinjectMvcDependencyResolver : IDependencyResolver
    {
        private readonly IKernel _kernel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kernel">Ninject kernel</param>
        public NinjectMvcDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns>
        /// The requested service or object.
        /// </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        /// <summary>
        /// Resolves multiply registered services.
        /// </summary>
        /// <returns>
        /// The requested services.
        /// </returns>
        /// <param name="serviceType">The type of the requested services.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }
    }
}