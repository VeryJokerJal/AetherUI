using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 简单的服务定位器，用于依赖注入
    /// </summary>
    public sealed class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> _instance = new Lazy<ServiceLocator>(() => new ServiceLocator());
        private readonly ConcurrentDictionary<Type, object> _services = new();
        private readonly ConcurrentDictionary<Type, Func<object>> _factories = new();

        /// <summary>
        /// 获取服务定位器的单例实例
        /// </summary>
        public static ServiceLocator Instance => _instance.Value;

        private ServiceLocator()
        {
        }

        /// <summary>
        /// 注册服务实例
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <param name="implementation">服务实现实例</param>
        public void RegisterInstance<TInterface>(TInterface implementation) where TInterface : class
        {
            if (implementation == null)
                throw new ArgumentNullException(nameof(implementation));

            Type serviceType = typeof(TInterface);
            _services[serviceType] = implementation;
            Debug.WriteLine($"Registered service instance: {serviceType.Name}");
        }

        /// <summary>
        /// 注册服务工厂
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <param name="factory">服务工厂函数</param>
        public void RegisterFactory<TInterface>(Func<TInterface> factory) where TInterface : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            Type serviceType = typeof(TInterface);
            _factories[serviceType] = () => factory();
            Debug.WriteLine($"Registered service factory: {serviceType.Name}");
        }

        /// <summary>
        /// 注册单例服务
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <typeparam name="TImplementation">服务实现类型</typeparam>
        public void RegisterSingleton<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            Type serviceType = typeof(TInterface);
            _factories[serviceType] = () =>
            {
                if (_services.TryGetValue(serviceType, out object? existingInstance))
                {
                    return existingInstance;
                }

                TImplementation newInstance = new TImplementation();
                _services[serviceType] = newInstance;
                return newInstance;
            };
            Debug.WriteLine($"Registered singleton service: {serviceType.Name} -> {typeof(TImplementation).Name}");
        }

        /// <summary>
        /// 注册瞬态服务
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <typeparam name="TImplementation">服务实现类型</typeparam>
        public void RegisterTransient<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            Type serviceType = typeof(TInterface);
            _factories[serviceType] = () => new TImplementation();
            Debug.WriteLine($"Registered transient service: {serviceType.Name} -> {typeof(TImplementation).Name}");
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <returns>服务实例</returns>
        /// <exception cref="InvalidOperationException">服务未注册时抛出</exception>
        public TInterface GetService<TInterface>() where TInterface : class
        {
            Type serviceType = typeof(TInterface);

            // 首先检查是否有已注册的实例
            if (_services.TryGetValue(serviceType, out object? instance))
            {
                Debug.WriteLine($"Retrieved service instance: {serviceType.Name}");
                return (TInterface)instance;
            }

            // 然后检查是否有工厂
            if (_factories.TryGetValue(serviceType, out Func<object>? factory))
            {
                object newInstance = factory();
                Debug.WriteLine($"Created service instance: {serviceType.Name}");
                return (TInterface)newInstance;
            }

            throw new InvalidOperationException($"Service of type '{serviceType.Name}' is not registered");
        }

        /// <summary>
        /// 尝试获取服务实例
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <param name="service">输出的服务实例</param>
        /// <returns>如果成功获取服务则返回true</returns>
        public bool TryGetService<TInterface>(out TInterface? service) where TInterface : class
        {
            try
            {
                service = GetService<TInterface>();
                return true;
            }
            catch
            {
                service = null;
                return false;
            }
        }

        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        /// <returns>如果服务已注册则返回true</returns>
        public bool IsRegistered<TInterface>() where TInterface : class
        {
            Type serviceType = typeof(TInterface);
            return _services.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <typeparam name="TInterface">服务接口类型</typeparam>
        public void Unregister<TInterface>() where TInterface : class
        {
            Type serviceType = typeof(TInterface);
            _services.TryRemove(serviceType, out _);
            _factories.TryRemove(serviceType, out _);
            Debug.WriteLine($"Unregistered service: {serviceType.Name}");
        }

        /// <summary>
        /// 清除所有注册的服务
        /// </summary>
        public void Clear()
        {
            _services.Clear();
            _factories.Clear();
            Debug.WriteLine("Cleared all registered services");
        }
    }
}
