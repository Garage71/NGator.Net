using NGator.Net.Models;
using Ninject;

namespace NGator.Net.Infrastructure
{
    /// <summary>
    /// Ninject DI Container configuration class
    /// </summary>
    public class NinjectConfigurator
    {
        /// <summary>
        /// Performs all the kernel configuration
        /// </summary>
        /// <param name="container">Ninject kernel</param>
        public void Configure(IKernel container)
        {            
            AddBindings(container);
        }

        private void AddBindings(IKernel container)
        {
            container.Bind<IRssSourcesProvider>().To<RssSourcesProvider>().InSingletonScope();
            container.Bind<INewsProvider>().To<NewsProvider>().InSingletonScope();
            container.Bind<IContentStorage>().To<MemoryContentStorage>().InSingletonScope();
            container.Bind<IParserProvider>().To<ParserProvider>().InSingletonScope();
        }
    }
}