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
        using System.Collections.ObjectModel;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;
    
        internal class ShiftManager : IShiftManager
        {
            public Task<Shift> Create(Shift entity)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).TransferShift(entity));
            }
    
            public Task<Shift> Read(long shiftId, string terminalId)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<Shift>> ReadAll(QueryResultSettings queryResultSettings)
            {
                throw new NotSupportedException();
            }
    
            public Task<Shift> Update(Shift entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Delete(Shift entity)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).DeleteShift(entity));
            }
    
            public Task<PagedResult<Shift>> GetByStatus(int statusValue, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetAvailableShifts((ShiftStatus)statusValue, queryResultSettings));
            }
    
            public Task<Shift> Open(long? shiftId, string cashDrawer, bool isShared)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).OpenShift(shiftId, cashDrawer, isShared));
            }
    
            public Task<Shift> Close(long shiftId, string terminalId, string transactionId, bool forceClose)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).CloseShift(terminalId, shiftId, transactionId: transactionId, canForceClose: forceClose));
            }
    
            public Task<Shift> BlindClose(long shiftId, string terminalId, string transactionId, bool forceClose)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).BlindCloseShift(terminalId, shiftId, transactionId: transactionId, canForceClose: forceClose));
            }
    
            public Task<Shift> Resume(long shiftId, string terminalId, string cashDrawer)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).ResumeShift(terminalId, shiftId, cashDrawer));
            }
    
            public Task<Shift> Use(long shiftId, string terminalId)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).UseShift(terminalId, shiftId));
            }
    
            public Task<Shift> Suspend(long shiftId, string terminalId, string transactionId)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).SuspendShift(terminalId, shiftId, transactionId: transactionId));
            }
    
            public Task<Receipt> GetXReport(long shiftId, string terminalId, string transactionId, string hardwareProfileId)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetXZReport(ReceiptType.XReport, terminalId, shiftId, transactionId, hardwareProfileId));
            }
    
            public Task<Receipt> GetZReport(string transactionId, string hardwareProfileId)
            {
                return Task.Run(() => Runtime.Client.StoreOperationsManager.Create(CommerceRuntimeManager.Runtime).GetXZReport(ReceiptType.ZReport, terminalId: string.Empty, shiftId: null, transactionId: transactionId, hardwareProfileId: hardwareProfileId));
            }
        }
    }
}
