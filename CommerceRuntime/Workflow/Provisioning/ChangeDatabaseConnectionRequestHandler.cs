/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages.ReleaseConnection;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Workflow required to change a connection of a runtime.
        /// </summary>
        public sealed class ChangeDatabaseConnectionRequestHandler : SingleRequestHandler<ChangeDatabaseConnectionRequest, ChangeDatabaseConnectionResponse>
        {
            /// <summary>
            /// Executes the workflow associated with changing a connection string of a runtime.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override ChangeDatabaseConnectionResponse Process(ChangeDatabaseConnectionRequest request)
            {
                ThrowIf.Null(request, "request");
    
                if (!request.RequestContext.Runtime.Configuration.IsConnectionStringOverridden)
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_ConnectionIsNotOverridden, "The workflow can be execute only for overridden connections.");
                }
    
                string currentConnectionString = this.Context.Runtime.Configuration.ConnectionString;
    
                this.Context.Runtime.Configuration.ConnectionString = request.ConnectionString;
    
                bool isConnectionReleased = this.ReleaseConnectionString(currentConnectionString);
    
                new CacheDataManager(this.Context).Clear();
    
                return new ChangeDatabaseConnectionResponse(isConnectionReleased);
            }
    
            /// <summary>
            /// Releases connections associated with a connection string from underlying database provider.
            /// </summary>
            /// <param name="connectionString">Connection string to release.</param>
            /// <returns>True if the connections are released otherwise false.</returns>
            private bool ReleaseConnectionString(string connectionString)
            {
                TimeSpan timeout = TimeSpan.FromMinutes(1);
                TimeSpan interval = TimeSpan.FromSeconds(10);
                DateTime started = DateTime.UtcNow;
                bool released;
                var request = new ReleaseConnectionServiceRequest(connectionString);
    
                while (!(released = this.ReleaseConnection(request)))
                {
                    Task.Delay(interval).Wait();
    
                    if (started + timeout <= DateTime.UtcNow)
                    {
                        break;
                    }
                }
    
                return released;
            }
    
            /// <summary>
            /// Releases the connection.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>True if the connections are released otherwise false.</returns>
            private bool ReleaseConnection(ReleaseConnectionServiceRequest request)
            {
                return this.Context.Runtime.Execute<ReleaseConnectionServiceResponse>(request, this.Context).Released;
            }
        }
    }
}
