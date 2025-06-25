using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MongoDBBackend
{
    // The types of database connections, add more types as needed / for multiple databases of the same type
    public enum PersistenceType
    {
        Nhibernate,
        MongoConnection,
        Cache
    }

    /// <summary>
    /// Core class which encapsulates Dependency Injection / IOC
    /// Access instances of various objects (Factories / Repositories / Services / etc) using functions provided by this class.
    /// </summary>
    public static class ResourceLocator
    {
        private static readonly Dictionary<string, IPersistenceContextFactory> DbContextFactories;

        private static IServiceProvider _serviceProvider;

        ////////////////////////////////////////////////////////////////////////////////////
        static ResourceLocator()
        {
            DbContextFactories = new Dictionary<string, IPersistenceContextFactory>();
        }

        /// <summary>
        /// Scans the assemblies in the list and registers various classes with IOC
        /// Call this method at the start of your application to link various modules.
        /// </summary>
        /// <param name="services">IServiceCollection from Startup.cs</param>
        /// <param name="additionalAssemblies">List of assemblies to scan and register in IOC</param>
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



        // Registers database context factory, pass additional param to enable multiple registrations of same type (rare case)
        public static void RegisterPersistenceContextFactory(PersistenceType contextType, IPersistenceContextFactory dbContextFactory)
        {
            DbContextFactories[$"{contextType}"] = dbContextFactory;
        }

        public static void RegisterServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        ////////////////////////////////////////////////////////////////////////////////////

        // Creates a new database context / session.
        public static IPersistenceContext GetPersistenceContext(PersistenceType contextType = PersistenceType.MongoConnection)
        {
            return DbContextFactories[$"{contextType}"].CreateContext();
        }

        public static IPersistenceContextMongo GetMongoContext(string dbName = null)
        {
            return DbContextFactories[$"{PersistenceType.MongoConnection}"].CreateContext(dbName) as IPersistenceContextMongo;
        }


        //public static IPersistenceContextCache GetCacheContext() //uncomment and use for Redis implementation
        //{
        //    return DbContextFactories[$"{PersistenceType.Cache}"].CreateContext() as IPersistenceContextCache;
        //}


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
    }
}
