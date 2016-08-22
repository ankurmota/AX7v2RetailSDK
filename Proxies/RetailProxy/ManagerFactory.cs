/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.RetailProxy
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using System.Reflection;
        using Commerce.RetailProxy.Authentication;
    
        /// <summary>
        /// Class encapsulates manager factory.
        /// </summary>
        public sealed class ManagerFactory
        {
            private static IEnumerable<Type> managerTypes;
    
            /// <summary>
            /// Initializes static members of the <see cref="ManagerFactory"/> class.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Better for code structure.")]
            static ManagerFactory()
            {
                var entityManagerType = typeof(IEntityManager);
    
                // Gets all manager types defined in the assembly.
                managerTypes = from t in typeof(ManagerFactory).GetTypeInfo().Assembly.DefinedTypes
                               where t.Namespace == typeof(ManagerFactory).Namespace
                                    && t.IsClass
                                    && t.ImplementedInterfaces.Any(i => entityManagerType.GetTypeInfo().IsAssignableFrom(i.GetTypeInfo()))
                               select t.AsType();
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ManagerFactory"/> class.
            /// </summary>
            /// <param name="context">The context.</param>
            private ManagerFactory(IContext context)
            {
                this.Context = context;
            }
    
            /// <summary>
            /// Gets the context.
            /// </summary>
            /// <value>
            /// The context.
            /// </value>
            public IContext Context { get; private set; }
    
            /// <summary>
            /// Creates an instance of commerce client with specified configuration.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The instance of commerce client.</returns>
            public static ManagerFactory Create(IContext context)
            {
                return new ManagerFactory(context);
            }
    
            /// <summary>
            /// Gets the manager.
            /// </summary>
            /// <typeparam name="T">The type of entity manager.</typeparam>
            /// <returns>The instance of the manager.</returns>
            public T GetManager<T>() where T : IEntityManager
            {
                var requiredManagerTypeInfo = typeof(T).GetTypeInfo();
                var managerType = managerTypes.Where(m => requiredManagerTypeInfo.IsAssignableFrom(m.GetTypeInfo())).FirstOrDefault();
    
                if (managerType != null)
                {
                    return (T)Activator.CreateInstance(managerType, this.Context);
                }
    
                throw new NotImplementedException(string.Format("'{0}' is not implemented.", requiredManagerTypeInfo.Name));
            }
        }
    }
}
