using Microsoft.Extensions.DependencyInjection;
using MongoDBBackend;
using System.Reflection;

namespace MyWarehouse.Infrastructure
{
    public static class ResourceLocator
    {
        private static IServiceProvider _serviceProvider;

        public static void RegisterServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Gets the Factory specified by T. (T should be derived from FactoryBase).
        public static T GetFactory<T>() where T : FactoryBase
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        // Generic get method to get misc objects, services, etc.
        public static T Get<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        public static void RegisterAssemblies(IServiceCollection services, IEnumerable<Assembly> additionalAssemblies = null)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            RegisterAssembly(services, thisAssembly);

            if (additionalAssemblies == null)
                return;

            foreach (Assembly assembly in additionalAssemblies.Distinct())
            {
                if (assembly != thisAssembly)
                {
                    RegisterAssembly(services, assembly);     // Registers Factories, Processes, etc.
                }
            }
        }

        private static void RegisterAssembly(IServiceCollection services, Assembly assembly)
        {
            var factoryBaseType = typeof(FactoryBase);
            var processBaseType = typeof(ProcessBase);

            // Factories and Processes have no private data and can be application-wide singletons.
            foreach (var type in assembly.DefinedTypes.Where(x => !x.IsAbstract && (x.IsSubclassOf(factoryBaseType) || x.IsSubclassOf(processBaseType))))
            {
                services.AddSingleton(type, type);
            }

            //StandardKernel.Bind(x => x.From(assembly)
            //                   .SelectAllClasses().InheritedFrom(typeof(ConstraintBase))
            //                   .BindToSelf()
            //                   .Configure(b => b.InScope(context => context.Parameters.First().GetValue(context, context.Request.Target))));
        }
    }
}
