using UnityEngine;
using System;
using System.Collections.Generic;

namespace Core.Services
{
    /// <summary>
    /// Service Locator pattern implementation for dependency resolution
    /// </summary>
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the service locator
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("ServiceLocator already initialized!");
                return;
            }

            _services = new Dictionary<Type, object>();
            _isInitialized = true;
            Debug.Log("ServiceLocator initialized");
        }

        /// <summary>
        /// Register a service implementation
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="service">The service implementation</param>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"Cannot register null service for type {typeof(T)}");
                return;
            }

            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type} already registered! Replacing previous registration.");
            }

            _services[type] = service;
            Debug.Log($"Service registered: {type}");
        }

        /// <summary>
        /// Get a service implementation
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <returns>The service implementation, or null if not found</returns>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (!_isInitialized)
            {
                Debug.LogError($"ServiceLocator not initialized! Cannot get service of type {type}");
                return null;
            }

            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            Debug.LogError($"Service of type {type} not registered!");
            return null;
        }

        /// <summary>
        /// Check if a service is registered
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <returns>True if registered, false otherwise</returns>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }
    }
}