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
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Represents the parameters that will be used to make a method call.
        /// </summary>
        public class ParametersGroup
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParametersGroup" /> class.
            /// </summary>
            /// <param name="entitySet">The name of entity set. e.g. Carts, Customers.</param>
            /// <param name="entitySetType">The name of the entity set type. e.g. Cart, Customer.</param>
            /// <param name="operation">The name of the operation.</param>
            /// <param name="typeParameter">The type of the result.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="expandProperties">The navigation properties to be expanded.</param>
            /// <param name="operationParameters">The operation parameters.</param>
            public ParametersGroup(string entitySet, string entitySetType, string operation, Type typeParameter, QueryResultSettings settings, ICollection<string> expandProperties, params OperationParameter[] operationParameters)
            {
                this.EntitySet = entitySet;
                this.EntitySetType = entitySetType;
                this.OperationName = operation;
                this.TypeParameter = typeParameter;
                this.QueryResultSettings = settings;
                this.ExpandProperties = expandProperties;
                this.OperationParameters = operationParameters;
            }
    
            /// <summary>
            /// Gets or sets the type of returned result.
            /// </summary>
            public Type TypeParameter { get; set; }
    
            /// <summary>
            /// Gets or sets the name of entity set.
            /// </summary>
            public string EntitySet { get; set; }
    
            /// <summary>
            /// Gets or sets the name of entity set type.
            /// </summary>
            public string EntitySetType { get; set; }
    
            /// <summary>
            /// Gets or sets the name of operation.
            /// </summary>
            public string OperationName { get; set; }
    
            /// <summary>
            /// Gets or sets the query result settings.
            /// </summary>
            public QueryResultSettings QueryResultSettings { get; set; }

            /// <summary>
            /// Gets or sets the expand properties.
            /// </summary>
            public ICollection<string> ExpandProperties { get; set; }

            /// <summary>
            /// Gets or sets the operation parameters.
            /// </summary>
            public OperationParameter[] OperationParameters { get; set; }
        }
    }
}
