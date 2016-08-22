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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates helper function.
        /// </summary>
        internal static class ShippingHelper
        {
            /// <summary>
            /// Validates the address as well as resolves the state from zip code, if needed.
            /// </summary>
            /// <param name="context">Instance of <see cref="RequestContext"/>.</param>
            /// <param name="addressToValidate">Address to be validated and resolved.</param>
            internal static void ValidateAndResolveAddress(RequestContext context, Address addressToValidate)
            {
                if (addressToValidate == null)
                {
                    // Nothing to validate.
                    return;
                }
    
                List<DataValidationFailure> validationFailures = new List<DataValidationFailure>();
    
                ShippingHelper.ValidateAndResolveAddress(context, addressToValidate, validationFailures);
    
                if (validationFailures.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, validationFailures, "Failures when validating address.");
                }
            }
    
            /// <summary>
            /// Validate the addresses present in the Sales Transaction as well as resolves the state from zip code, if needed.
            /// </summary>
            /// <param name="context">Instance of <see cref="RequestContext"/>.</param>
            /// <param name="transaction">Current transaction.</param>
            internal static void ValidateAndResolveAddresses(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(transaction, "transaction");
    
                List<DataValidationFailure> validationFailures = new List<DataValidationFailure>();
    
                if (!Address.IsNullOrEmpty(transaction.ShippingAddress))
                {
                    ValidateAndResolveAddress(context, transaction.ShippingAddress, validationFailures);
                }
    
                // Consider only active lines. Ignore voided lines.
                foreach (SalesLine salesLine in transaction.ActiveSalesLines)
                {
                    if (salesLine != null && !Address.IsNullOrEmpty(salesLine.ShippingAddress))
                    {
                        ValidateAndResolveAddress(context, salesLine.ShippingAddress, validationFailures);
                    }
                }
    
                if (validationFailures.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, validationFailures, "Failures when validating addresses.");
                }
            }
    
            /// <summary>
            /// Validates the addresses and dates for shipping.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            internal static void ValidateShippingInformation(RequestContext context, SalesTransaction transaction)
            {
                if (!Address.IsNullOrEmpty(transaction.ShippingAddress))
                {
                    ValidateShippingAddress(
                        context,
                        transaction.ShippingAddress,
                        transaction.DeliveryMode);
                }
    
                ShippingHelper.ValidateDeliveryDate(transaction.RequestedDeliveryDate, "header");
    
                // Consider only active lines. Ignore voided lines.
                foreach (SalesLine salesLine in transaction.ActiveSalesLines)
                {
                    ValidateShippingAddress(
                        context,
                        salesLine.ShippingAddress,
                        string.IsNullOrWhiteSpace(salesLine.DeliveryMode) ? transaction.DeliveryMode : salesLine.DeliveryMode);
    
                    ShippingHelper.ValidateDeliveryDate(transaction.RequestedDeliveryDate, "line " + salesLine.LineId);
                }
            }
    
            /// <summary>
            /// Validates a delivery date.
            /// </summary>
            /// <param name="deliveryDate">The shipping date to be validated.</param>
            /// <param name="dateDescription">Description for date so it can be logged in case of failure.</param>
            private static void ValidateDeliveryDate(DateTimeOffset? deliveryDate, string dateDescription)
            {
                if (!deliveryDate.HasValue || !deliveryDate.Value.IsValidAxDateTime())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidShippingDate,
                        string.Format("Delivery date set in the order {0} is not valid.", dateDescription));
                }
            }
    
            /// <summary>
            /// Validates the address as well as resolves the state from zip code, if needed.
            /// </summary>
            /// <param name="context">Instance of <see cref="RequestContext"/>.</param>
            /// <param name="addressToValidate">Address to be validated and resolved.</param>
            /// <param name="validationFailures">The list to hold all exceptions.</param>
            private static void ValidateAndResolveAddress(RequestContext context, Address addressToValidate, List<DataValidationFailure> validationFailures)
            {
                if (addressToValidate != null)
                {
                    // Validates the address
                    ValidateAddressDataRequest validateAddressRequest = new ValidateAddressDataRequest(addressToValidate);
                    ValidateAddressDataResponse response = context.Runtime.Execute<ValidateAddressDataResponse>(validateAddressRequest, context);
    
                    if (!response.IsAddressValid)
                    {
                        validationFailures.Add(new DataValidationFailure(response.ErrorCode, new Collection<string> { response.InvalidAddressComponentName }, response.ErrorMessage));
                        return;
                    }
    
                    // If zip code is provided but not the state, resolve the state from zipcode.
                    if (string.IsNullOrWhiteSpace(addressToValidate.State) && !string.IsNullOrWhiteSpace(addressToValidate.ZipCode))
                    {
                        GetFromZipPostalCodeServiceRequest zipPostalCodeRequest = new GetFromZipPostalCodeServiceRequest(addressToValidate.ThreeLetterISORegionName, addressToValidate.ZipCode)
                        {
                            QueryResultSettings = QueryResultSettings.AllRecords
                        };

                        GetFromZipPostalCodeServiceResponse zipPostalCodeResponse
                            = context.Runtime.Execute<GetFromZipPostalCodeServiceResponse>(zipPostalCodeRequest, context);
    
                        if (zipPostalCodeResponse.Results != null)
                        {
                            ZipCodeInfo zipCodeResult = zipPostalCodeResponse.Results.Results.GetEnumerator().Current;
                            if (zipCodeResult != null)
                            {
                                addressToValidate.State = zipCodeResult.StateId;
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Validates the address for shipping.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="shippingAddress">The address to validate for shipping.</param>
            /// <param name="deliveryModeId">The delivery mode identifier.</param>
            private static void ValidateShippingAddress(
                RequestContext context,
                Address shippingAddress,
                string deliveryModeId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(context.Runtime, "context.Runtime");
    
                if (shippingAddress == null)
                {
                    return;
                }
    
                var request = new ValidateShippingAddressServiceRequest(shippingAddress, false, deliveryModeId);
    
                var response = context.Runtime.Execute<ValidateShippingAddressServiceResponse>(request, context);
    
                if (!response.IsAddressValid)
                {
                    // invalid address anomaly encountered and handle it
                    InvalidShippingAddressNotification notification = new InvalidShippingAddressNotification(shippingAddress);
                    request.RequestContext.Notify(notification);
                }
            }
        }
    }
}
