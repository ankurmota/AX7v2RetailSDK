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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Services;
        
        /// <summary>
        /// The number sequence request handler.
        /// </summary>
        public sealed class NumberSequenceRequestHandler :
            SingleRequestHandler<GetNumberSequenceRequest, GetNumberSequenceResponse>
        {
            /// <summary>
            /// Executes the workflow to get the next available number sequence value.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetNumberSequenceResponse Process(GetNumberSequenceRequest request)
            {
                ThrowIf.Null(request, "request");
    
                string terminalId;
    
                // if terminalId is not provided in the request
                if (string.IsNullOrWhiteSpace(request.TerminalId))
                {
                    // use the terminal associated with the request context
                    Terminal terminal = request.RequestContext.GetTerminal();
                    terminalId = terminal.TerminalId;
                }
                else
                {
                    terminalId = request.TerminalId;
                }
    
                IEnumerable<NumberSequenceSeedData> seedDataFromHeadquarters = this.GetNumberSequenceFromHeadquarters(terminalId);
                IEnumerable<NumberSequenceSeedData> seedDataFromChannel = this.GetNumberSequenceFromChannelDatabase(terminalId);            
                IEnumerable<NumberSequenceSeedData> mergedSeedData = this.MergeNumberSequenceSeedData(seedDataFromHeadquarters, seedDataFromChannel);
    
                return new GetNumberSequenceResponse(mergedSeedData.AsPagedResult());
            }
    
            private static void AddOrUpdateNumberSequence(IList<NumberSequenceSeedData> numberSequenceList, NumberSequenceSeedData newNumberSeqValue)
            {
                var numberSequenceReceipt = numberSequenceList.Where(ns => ns.DataType == newNumberSeqValue.DataType);
                var numberSequenceSeedData = numberSequenceReceipt.FirstOrDefault();
    
                if (numberSequenceSeedData != null)
                {
                    if (newNumberSeqValue.DataValue > numberSequenceSeedData.DataValue)
                    {
                        numberSequenceSeedData.DataValue = newNumberSeqValue.DataValue;
                    }
                }
                else
                {
                    numberSequenceList.Add(newNumberSeqValue);    
                }
            }        
    
            private static NumberSequenceSeedData GetNumberSequenceDataByShift(Shift shift)
            {
                return new NumberSequenceSeedData { DataType = NumberSequenceSeedType.BatchId, DataValue = IncrementByOne(shift.ShiftId) };
            }
            
            private static long ExtractNumberSequenceValueForTransactionId(string transactionId)
            {
                long transactionIdNumberSequence;
    
                if (!long.TryParse(transactionId, out transactionIdNumberSequence))
                {
                    string[] transactionIdArr = transactionId.Split('-');
    
                    if (transactionIdArr != null && transactionIdArr.Any())
                    {
                        if (long.TryParse(transactionIdArr[transactionIdArr.Length - 1], out transactionIdNumberSequence))
                        {
                            return transactionIdNumberSequence;
                        }
                    }
                }
    
                return transactionIdNumberSequence;
            }
    
            private static long IncrementByOne(long numberSequenceId)
            {
                return numberSequenceId + 1;
            }
    
            private NumberSequenceSeedData GetNumberSequenceDataByTransactionId(SalesTransaction salesTransaction)
            {
                long transactionId = ExtractNumberSequenceValueForTransactionId(salesTransaction.Id);
    
                var numberSequenceData = new NumberSequenceSeedData
                {
                    DataType = NumberSequenceSeedType.TransactionId,
                    DataValue = IncrementByOne(transactionId)
                };
    
                return numberSequenceData;
            }
    
            private NumberSequenceSeedData GetNumberSequenceDataByReceiptId(SalesTransaction salesTransaction)
            {
                ReceiptTransactionType receiptType = NumberSequenceSeedTypeHelper.GetReceiptTransactionType(salesTransaction.TransactionType, salesTransaction.NetAmountWithNoTax, salesTransaction.CustomerOrderMode);
                NumberSequenceSeedType seedType = NumberSequenceSeedTypeHelper.GetNumberSequenceSeedType(receiptType);
    
                // get receipt mask from db
                string functionalityProfileId = this.GetOrgUnit().FunctionalityProfileId;
                var getReceiptMaskRequest = new GetReceiptMaskDataRequest(functionalityProfileId, receiptType);
                ReceiptMask mask = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptMask>>(
                    getReceiptMaskRequest, 
                    this.Context).Entity;
    
                // parse receipt number
                long numberSequenceId;
    
                // if mask is not available, try parsing it as a integer
                if (mask == null)
                {
                    if (!long.TryParse(salesTransaction.ReceiptId, out numberSequenceId))
                    {
                        string message = string.Format("Receipt mask is not available for receipt type {0} when using functionality profile {1}. Parsing receipt identifier as a number failed.", receiptType, functionalityProfileId);
                        RetailLogger.Log.CrtWorkflowParsingReceiptIdentifierFailure(receiptType.ToString(), functionalityProfileId);
                        throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, ExceptionSeverity.Warning, message);
                    }
                }
                else
                {
                    numberSequenceId = ReceiptMaskFiller.GetNumberSequenceFromReceipt(mask.Mask, salesTransaction.ReceiptId);
                }
    
                // compose number sequence seed object
                return new NumberSequenceSeedData { DataType = seedType, DataValue = IncrementByOne(numberSequenceId) };
            }
    
            private OrgUnit GetOrgUnit()
            {
                if (this.Context.GetOrgUnit() != null)
                {
                    return this.Context.GetOrgUnit();
                }
    
                long channelId = this.Context.GetPrincipal().ChannelId;
                var request = new SearchOrgUnitDataRequest(channelId);
                return this.Context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(request, this.Context).PagedEntityCollection.SingleOrDefault();
            }
    
            private IEnumerable<NumberSequenceSeedData> GetNumberSequenceFromChannelDatabase(string terminalId)
            {
                // Get the next number sequence value by invoking the number sequence data service.
                var numberSequenceRequest = new GetLatestNumberSequenceDataRequest(terminalId);
    
                var response = this.Context.Runtime.Execute<GetLatestNumberSequenceDataResponse>(numberSequenceRequest, this.Context);
    
                IList<NumberSequenceSeedData> numberSequenceList = new List<NumberSequenceSeedData>();
    
                // Gets the number sequence value for shift.
                if (response.NumberSequenceValueForShift != null)
                {
                    NumberSequenceSeedData numberSequenceSeedDataForShift = GetNumberSequenceDataByShift(response.NumberSequenceValueForShift);
                    AddOrUpdateNumberSequence(numberSequenceList, numberSequenceSeedDataForShift);
                }
    
                // Gets the number sequence value for transaction identifier.
                if (response.NumberSequenceValueForTransaction != null)
                {
                    NumberSequenceSeedData numberSequenceSeedDataForTransactionId = this.GetNumberSequenceDataByTransactionId(response.NumberSequenceValueForTransaction);
    
                    if (numberSequenceSeedDataForTransactionId != null)
                    {
                        AddOrUpdateNumberSequence(numberSequenceList, numberSequenceSeedDataForTransactionId);
                    }
                }
    
                // Gets the number sequence value for the receipt identifier.
                if (response.NumberSequenceValueForReceipts != null)
                {
                    foreach (var transItem in response.NumberSequenceValueForReceipts)
                    {
                        NumberSequenceSeedData numberSequenceSeedDataForReceiptId = this.GetNumberSequenceDataByReceiptId(transItem);
    
                        if (numberSequenceSeedDataForReceiptId != null)
                        {
                            AddOrUpdateNumberSequence(numberSequenceList, numberSequenceSeedDataForReceiptId);
                        }
                    }
                }
    
                return numberSequenceList;
            }
    
            private IEnumerable<NumberSequenceSeedData> GetNumberSequenceFromHeadquarters(string terminalId)
            {
                var request = new GetNumberSequenceSeedDataRealtimeRequest(terminalId);
                GetNumberSequenceSeedDataRealtimeResponse response = this.Context.Execute<GetNumberSequenceSeedDataRealtimeResponse>(request);
                return response.NumberSequenceSeedData.Results;
            }
    
            private IEnumerable<NumberSequenceSeedData> MergeNumberSequenceSeedData(
                IEnumerable<NumberSequenceSeedData> seedDataFromHeadquarters,
                IEnumerable<NumberSequenceSeedData> seedDataFromChannel)
            {
                ThrowIf.Null(seedDataFromHeadquarters, "seedDataFromHeadquarters");
    
                long channelId = this.Context.GetPrincipal().ChannelId;
    
                if (seedDataFromChannel == null)
                {
                    NetTracer.Warning("Channel {0} returned an empty number sequence seed data collection. Default values for number sequence will be used if they cannot be retrieved from Headquarters.", channelId);
                    seedDataFromChannel = new NumberSequenceSeedData[0];
                }
    
                IDictionary<NumberSequenceSeedType, NumberSequenceSeedData> headquartersNumberSequenceByType = seedDataFromHeadquarters.ToDictionary(ns => ns.DataType);
                IDictionary<NumberSequenceSeedType, NumberSequenceSeedData> channelNumberSequenceByType = seedDataFromChannel.ToDictionary(ns => ns.DataType);
    
                // iterate over all posible number sequence values and get the largest number sequence value available (either from AX or channel)
                foreach (NumberSequenceSeedType seedType in Enum.GetValues(typeof(NumberSequenceSeedType)))
                {
                    NumberSequenceSeedData numberSequence;
                    long headquartersValue = 1;
                    long channelValue = 1;
    
                    if (headquartersNumberSequenceByType.TryGetValue(seedType, out numberSequence))
                    {
                        headquartersValue = numberSequence.DataValue;
                    }
    
                    if (channelNumberSequenceByType.TryGetValue(seedType, out numberSequence))
                    {
                        channelValue = numberSequence.DataValue;
                    }
    
                    yield return new NumberSequenceSeedData()
                    {
                        DataType = seedType,
                        DataValue = Math.Max(headquartersValue, channelValue)
                    };
                }
            }
        }
    }
}
