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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        
        internal class ScanResultManager : IScanResultManager
        {
            public Task<ScanResult> Create(ScanResult entity)
            {
                throw new NotSupportedException();
            }

            public Task Delete(ScanResult entity)
            {
                throw new NotSupportedException();
            }

            public Task<ScanResult> Read(string scannedText)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetScanResult(scannedText));
            }

            public Task<PagedResult<ScanResult>> ReadAll(QueryResultSettings queryResultSettings)
            {
                throw new NotSupportedException();
            }

            public Task<ScanResult> Update(ScanResult entity)
            {
                throw new NotSupportedException();
            }
        }
    }
}
