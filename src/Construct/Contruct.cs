using System;
using System.Collections.Generic;
using System.Reflection;

namespace Construct
{
    public interface IRegistration
    {
        IRegistration As<T>();

        IRegistration SingleInstance();

        IRegistration InstancePerDependency();
    }

    public interface IRegistrationInfo : IRegistration
    {
        bool IsSingleton { get; }

        Type RegistrationType { get; }

        Func<object> CreateInstance { get; }
    }

    public interface IContainerBuilder
    {
        IRegistration Register<T>(Func<T> createInstance)
            where T : class;

        IRegistration Register<T>()
            where T : class;

        IContainer Build();
    }

    public interface IContainer
    {
        T Resolve<T>()
            where T : class;

        object Resolve(Type type);
    }

    public class Registration<T> : IRegistrationInfo
        where T : class
    {
        private readonly Func<object> createInstance;
        private Type registrationType;
        private bool isSingleton;

        public Registration(Func<object> createInstance)
        {
            this.createInstance = createInstance;
            this.registrationType = typeof(T);
            this.isSingleton = false;
        }

        public bool IsSingleton { get { return isSingleton; } }

        public Type RegistrationType { get { return registrationType; } }

        public Func<object> CreateInstance { get { return createInstance; } }

        public IRegistration As<TAs>()
        {
            registrationType = typeof(TAs);
            return this;
        }

        public IRegistration SingleInstance()
        {
            isSingleton = true;
            return this;
        }

        /// <summary>
        /// Is default
        /// </summary>
        /// <returns>The per dependency.</returns>
        public IRegistration InstancePerDependency()
        {
            isSingleton = false;
            return this;
        }
    }

    public class ContainerBuilder : IContainerBuilder
    {
        private readonly Container container;
        private readonly List<IRegistrationInfo> registrations;

        public ContainerBuilder(Container container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
            registrations = new List<IRegistrationInfo>();
        }


        private ParameterInfo[] GetParameterInfos(Type type)
        {
#if NOT_RUNNING_ON_4
            var constructors = type.GetTypeInfo().DeclaredConstructors;
#else
            var constructors = type.GetType().GetConstructors();
#endif
            var parameterInfos = default(ParameterInfo[]);
            var parameters = default(ParameterInfo[]);
            // Get the constructor with the most parameters
            foreach (var constructor in constructors)
            {
                parameters = constructor.GetParameters();
                if (parameterInfos == null || parameters.Length > parameterInfos.Length)
                {
                    parameterInfos = parameters;
                }
            }
            return parameterInfos;
        }

        private object CreateInstance(Type type, ParameterInfo[] parameterInfos)
        {
            var length = parameterInfos.Length;
            var args = new object[length];
            var parameterType = default(Type);
            for (var i = 0; i < length; i++)
            {
                parameterType = parameterInfos[i].ParameterType;
                args[i] = container.InternalResolve(parameterType);
                if (args[i] == null)
                {
                    throw new InvalidOperationException(string.Concat("Could not register ", type.FullName, " dependency of type ", parameterType.FullName, " could not be found."));
                }
            }
            return Activator.CreateInstance(type, args);
        }

        public IRegistration Register<T>(Func<T> createInstance)
            where T : class
        {
            if (createInstance == null)
            {
                throw new ArgumentNullException("createInstance");
            }

            var registration = new Registration<T>(createInstance);
            registrations.Add(registration);
            return registration;
        }

        public IRegistration Register<T>()
            where T : class
        {
            var type = typeof(T);
            var parameterInfos = GetParameterInfos(type);
            var registration = new Registration<T>(() => CreateInstance(type, parameterInfos));
            registrations.Add(registration);
            return registration;
        }

        public IContainer Build()
        {
            // TODO Do some locking (in the Container) here? 
            foreach (var registration in registrations)
            {
                if (registration.IsSingleton)
                {
                    var instance = registration.CreateInstance();
                    container.AddRegistration(registration.RegistrationType, () => instance);
                }
                else
                {
                    container.AddRegistration(registration.RegistrationType, registration.CreateInstance);
                }
            }

            return container;
        }
    }

    public class Container : IContainer
    {
        private static readonly object sync = new object();
        private readonly IDictionary<Type, Func<object>> registrations = new Dictionary<Type, Func<object>>();
        //		private static readonly Container current = new Container ();
        //
        //		public static Container Current {
        //			get {
        //				return current;
        //			}
        //		}

        internal void AddRegistration(Type type, Func<object> getInstance)
        {
            registrations.Add(type, getInstance);
        }

        public IContainerBuilder Configure(Action<IContainerBuilder, IContainer> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var builder = new ContainerBuilder(this);
            configuration(builder, this);

            return builder;
        }

        internal object InternalResolve(Type type)
        {
            return registrations.ContainsKey(type) ? registrations[type]() : null;
        }

        public object Resolve(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            lock (sync)
            {
                if (registrations.ContainsKey(type))
                {
                    return registrations[type]();
                }
            }
            throw new InvalidOperationException(string.Concat("An instance was not registered in the container of type ", type.FullName));
        }

        public T Resolve<T>()
            where T : class
        {
            return Resolve(typeof(T)) as T;
        }
    }
}

