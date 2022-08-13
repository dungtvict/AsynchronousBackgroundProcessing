using System.Collections.Concurrent;

namespace AsynchronousBackgroundProcessing.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Type>> Concurrent = new();
        private const string duplicateTypeRegistration = "A service of type '{0}' has already been registered.";
        private const string missingTypeRegistration = "Could not find any registered services for type '{0}'.";

        public static IServiceCollection AddScoped<TInterface, TClass>(this IServiceCollection serviceCollection,
            string key) where TInterface : class where TClass : class, TInterface
        {
            serviceCollection.AddScoped<TInterface, TClass>();
            serviceCollection.AddScoped<TClass, TClass>();

            if (Concurrent.TryGetValue(typeof(TInterface), out ConcurrentDictionary<string, Type>? maps))
            {
                if (maps.ContainsKey(key))
                {
                    throw new InvalidOperationException(string.Format(duplicateTypeRegistration, typeof(TInterface)));
                }

                maps.TryAdd(key, typeof(TClass));
                return serviceCollection;
            }

            var dictionary = new ConcurrentDictionary<string, Type>();
            dictionary.TryAdd(key, typeof(TClass));
            Concurrent.TryAdd(typeof(TInterface), dictionary);
            return serviceCollection;
        }

        public static T GetService<T>(this IServiceProvider serviceProvider, string key)
        {
            if (!Concurrent.TryGetValue(typeof(T), out ConcurrentDictionary<string, Type>? maps))
            {
                throw new InvalidOperationException(string.Format(missingTypeRegistration, typeof(T)));
            }

            if (!maps.ContainsKey(key))
            {
                throw new InvalidOperationException(string.Format(missingTypeRegistration, typeof(T)));
            }

            maps.TryGetValue(key, out Type? tClass);
            if (tClass is null)
            {
                throw new InvalidOperationException(string.Format(missingTypeRegistration, typeof(T)));
            }

            object? service = serviceProvider.GetService(tClass);
            if (service is T result)
            {
                return result;
            }

            throw new InvalidOperationException(string.Format(missingTypeRegistration, typeof(T)));
        }
    }
}