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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string MarkItemsReturnedMethodName = "MarkItemsReturned";
            private const string ValidateGiftCardMethodName = "ValidateGiftCard";
            private const string GiftCardPaymentMethodName = "GiftCardPayment";
            private const string VoidGiftCardPaymentMethodName = "VoidGiftCardPayment";
            private const string VoidGiftCardMethodName = "VoidGiftCard";
            private const string GiftCardReleaseMethodName = "GiftCardRelease";
            private const string IssueGiftCardMethodName = "IssueReservedGiftCard";
            private const string GetGiftCardBalanceMethodName = "GetGiftCardBalance";
            private const string AddToGiftCardMethodName = "AddToGiftCard";
            private const string IssueCreditMemoMethodName = "IssueCreditMemo";
            private const string ValidateCreditMemoMethodName = "ValidateCreditMemo";
            private const string UpdateCreditMemoMethodName = "UpdateCreditMemo";
            private const string VoidCreditMemoPaymentMethodName = "VoidCreditMemoPayment";
            private const string GetCreditMemoMethodName = "GetCreditMemo";
            private const string ValidateCustomerStatusMethodName = "ValidateCustomerStatus";
            private const string GetMerchantPaymentPropertiesForHardwareProfileMethodName = "GetMerchantPaymentPropertiesForHardwareProfile";
            private const string GetMerchantPaymentPropertiesForOnlineStoreMethodName = "GetMerchantPaymentPropertiesForOnlineStore";
    
            /// <summary>
            /// Reserves the gift card so it cannot be used from different terminal.
            /// </summary>
            /// <param name="giftCardId">Gift card identifier.</param>
            /// <param name="channelId">Channel identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="cardCurrencyCode">Currency code of specified gift card.</param>
            /// <param name="balance">Gift card balance.</param>
            public void LockGiftCard(string giftCardId, long channelId, string terminalId, out string cardCurrencyCode, out decimal balance)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
                ThrowIf.Null(terminalId, "terminalId");
    
                if (channelId <= 0)
                {
                    throw new ArgumentOutOfRangeException("channelId", "A valid channel identifier must be specified.");
                }
    
                object[] parameters =
                {
                    giftCardId,
                    string.Empty,   // Store is replaced by Channel id. Prameter is left for N-1 support.
                    terminalId,
                    false,          // Skip reserve validation is set to validate
                    channelId,
                };
    
                var data = this.InvokeMethod(
                    ValidateGiftCardMethodName,
                    parameters);
    
                // Parse response data
                cardCurrencyCode = (string)data[0];
                balance = (decimal)data[1];
            }
    
            /// <summary>
            /// Charge and unlock gift card.
            /// </summary>
            /// <param name="giftCardId">Gift card identifier.</param>
            /// <param name="amount">Amount to pay.</param>
            /// <param name="paymentCurrencyCode">Currency code to use for payment.</param>
            /// <param name="channelId">Channel identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="staffId">Staff identifier.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="receiptId">Receipt identifier.</param>
            /// <param name="cardCurrencyCode">Currency code of the gift card.</param>
            /// <param name="balance">New gift card balance.</param>
            public void PayGiftCard(
                string giftCardId,
                decimal amount,
                string paymentCurrencyCode,
                long channelId,
                string terminalId,
                string staffId,
                string transactionId,
                string receiptId,
                out string cardCurrencyCode,
                out decimal balance)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
                ThrowIf.Null(paymentCurrencyCode, "paymentCurrencyCode");
                ThrowIf.Null(terminalId, "terminalId");
                ThrowIf.Null(staffId, "staffId");
                ThrowIf.Null(transactionId, "transactionId");
                ThrowIf.Null(receiptId, "receiptId");
    
                if (channelId <= 0)
                {
                    throw new ArgumentOutOfRangeException("channelId", "A valid channel identifier must be specified.");
                }
    
                DateTimeOffset timeStamp = this.context.GetNowInChannelTimeZone();
                string dateString = SerializationHelper.ConvertDateTimeToAXDateString(timeStamp, AxDateSequence);
                string timeString = SerializationHelper.ConvertDateTimeToAXTimeString(timeStamp);
    
                object[] parameters =
                {
                    giftCardId,
                    string.Empty,       // Store is replaced by Channel id. Prameter is left for N-1 support.
                    terminalId,
                    staffId,
                    transactionId,
                    receiptId,
                    paymentCurrencyCode,
                    amount,
                    dateString,
                    timeString,
                    AxDateSequence,
                    channelId
                };
    
                var data = this.InvokeMethod(
                    GiftCardPaymentMethodName,
                    parameters);
    
                // Parse response data
                cardCurrencyCode = (string)data[0];
                balance = (decimal)data[1];
            }
    
            /// <summary>
            /// Void gift card payment.
            /// </summary>
            /// <param name="giftCardId">Gift card identifier.</param>
            /// <param name="channelId">Channel identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            public void VoidGiftCardPayment(string giftCardId, long channelId, string terminalId)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
                ThrowIf.Null(terminalId, "terminalId");
    
                object[] parameters =
                {
                    giftCardId,
                    string.Empty,       // Store is replaced by Channel id. Prameter is left for N-1 support.
                    terminalId,
                    channelId
                };
    
                this.InvokeMethodNoDataReturn(
                    VoidGiftCardPaymentMethodName,
                    parameters);
            }
    
            /// <summary>
            /// Voids the gift.
            /// </summary>
            /// <param name="giftCardId">Gift card identifier.</param>
            public void VoidGiftCard(string giftCardId)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
    
                this.InvokeMethodNoDataReturn(
                        VoidGiftCardMethodName,
                        giftCardId);
            }
    
            /// <summary>
            /// Unlocks/Releases an issued gift card so that it can now be used.
            /// </summary>
            /// <param name="giftCardId">The gift card id.</param>
            public void UnlockGiftCard(string giftCardId)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
    
                this.InvokeMethodNoDataReturn(
                        GiftCardReleaseMethodName,
                        giftCardId);
            }
    
            /// <summary>
            /// Get the gift card balance.
            /// </summary>
            /// <param name="giftCardId">Gift card identifier.</param>
            /// <param name="cardCurrencyCode">Currency code of specified gift card.</param>
            /// <param name="balance">Gift card balance.</param>
            public void GetGiftCardBalance(string giftCardId, out string cardCurrencyCode, out decimal balance)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
    
                object[] parameters =
                {
                    giftCardId
                };
    
                var serviceResponse = this.InvokeMethod(
                    GetGiftCardBalanceMethodName,
                    parameters);
    
                // Parse response data
                cardCurrencyCode = (string)serviceResponse[0];
                balance = (decimal)serviceResponse[1];
            }
    
            /// <summary>
            /// Add funds to gift card.
            /// </summary>
            /// <param name="giftCardId">Gift card identifier.</param>
            /// <param name="amount">Amount to deposit.</param>
            /// <param name="depositCurrencyCode">Currency code to use for deposit.</param>
            /// <param name="channelId">Channel identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="staffId">Staff identifier.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="receiptId">Receipt identifier.</param>
            /// <param name="cardCurrencyCode">Currency code of the gift card.</param>
            /// <param name="balance">New gift card balance.</param>
            public void AddToGiftCard(
                string giftCardId,
                decimal amount,
                string depositCurrencyCode,
                long channelId,
                string terminalId,
                string staffId,
                string transactionId,
                string receiptId,
                out string cardCurrencyCode,
                out decimal balance)
            {
                ThrowIf.NullOrWhiteSpace(giftCardId, "giftCardId");
                ThrowIf.Null(terminalId, "terminalId");
                ThrowIf.Null(staffId, "staffId");
                ThrowIf.Null(terminalId, "transactionId");
                ThrowIf.Null(terminalId, "receiptId");
    
                DateTime timeStamp = this.context.GetNowInChannelTimeZone().DateTime;
                string dateString = SerializationHelper.ConvertDateTimeToAXDateString(timeStamp, AxDateSequence);
                string timeString = SerializationHelper.ConvertDateTimeToAXTimeString(timeStamp);
    
                object[] parameters =
                {
                    giftCardId,
                    string.Empty,       // Store is replaced by Channel id. Prameter is left for N-1 support.
                    terminalId,
                    staffId,
                    transactionId,
                    receiptId,
                    depositCurrencyCode,
                    amount,
                    dateString,
                    timeString,
                    AxDateSequence,
                    channelId
                };
    
                var serviceResponse = this.InvokeMethod(
                    AddToGiftCardMethodName,
                    parameters);
    
                // Parse response data
                cardCurrencyCode = (string)serviceResponse[0];
                balance = (decimal)serviceResponse[1];
            }
    
            /// <summary>
            /// Issue new gift card.
            /// </summary>
            /// <param name="requestedGiftCardId">Gift card identifier.</param>
            /// <param name="amount">Amount to deposit.</param>
            /// <param name="depositCurrencyCode">Currency code to use for deposit.</param>
            /// <param name="channelId">Channel identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="staffId">Staff identifier.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="receiptId">Receipt identifier.</param>
            /// <param name="newGiftCardId">Gift card number returned from AX.</param>
            public void IssueGiftCard(
                string requestedGiftCardId,
                decimal amount,
                string depositCurrencyCode,
                long channelId,
                string terminalId,
                string staffId,
                string transactionId,
                string receiptId,
                out string newGiftCardId)
            {
                ThrowIf.NullOrWhiteSpace(requestedGiftCardId, "giftCardId");
                ThrowIf.Null(terminalId, "terminalId");
                ThrowIf.Null(terminalId, "staffId");
                ThrowIf.Null(terminalId, "transactionId");
                ThrowIf.Null(receiptId, "receiptId");
    
                DateTime timeStamp = this.context.GetNowInChannelTimeZone().DateTime;
                string dateString = SerializationHelper.ConvertDateTimeToAXDateString(timeStamp, AxDateSequence);
                string timeString = SerializationHelper.ConvertDateTimeToAXTimeString(timeStamp);
    
                object[] parameters =
                {
                    requestedGiftCardId,
                    string.Empty,       // Store is replaced by Channel id. Prameter is left for N-1 support.
                    terminalId,
                    staffId,
                    transactionId,
                    receiptId,
                    depositCurrencyCode,
                    amount,
                    dateString,
                    timeString,
                    AxDateSequence,
                    channelId
                };
    
                var serviceResponse = this.InvokeMethod(
                    IssueGiftCardMethodName,
                    parameters);
    
                // Parse response data
                newGiftCardId = (string)serviceResponse[0];
            }
    
            /// <summary>
            /// Issue new credit memo.
            /// </summary>
            /// <param name="storeId">Store identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="staffId">Staff identifier.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="receiptId">Receipt identifier.</param>
            /// <param name="currencyCode">Currency code.</param>
            /// <param name="amount">Amount of the credit memo.</param>
            /// <returns>Identifier of issued credit memo.</returns>
            public string IssueCreditMemo(
                string storeId,
                string terminalId,
                string staffId,
                string transactionId,
                string receiptId,
                string currencyCode,
                decimal amount)
            {
                ThrowIf.Null(storeId, "storeId");
                ThrowIf.Null(terminalId, "terminalId");
                ThrowIf.Null(staffId, "staffId");
                ThrowIf.Null(receiptId, "receiptId");
                ThrowIf.Null(transactionId, "transactionId");
                ThrowIf.Null(currencyCode, "currencyCode");
    
                DateTime timeStamp = this.context.GetNowInChannelTimeZone().DateTime;
                string dateString = SerializationHelper.ConvertDateTimeToAXDateString(timeStamp, AxDateSequence);
                string timeString = SerializationHelper.ConvertDateTimeToAXTimeString(timeStamp);
    
                object[] parameters =
                {
                    storeId,
                    terminalId,
                    staffId,
                    transactionId,
                    receiptId,
                    "1", // LineNum parameter, EPOS always passing hardcoded "1" value.
                    currencyCode,
                    amount,
                    dateString,
                    timeString,
                    AxDateSequence
                };
    
                var serviceResponse = this.InvokeMethod(
                    IssueCreditMemoMethodName,
                    parameters);
    
                // Parse response data
                string creditMemoId = (string)serviceResponse[0];
                return creditMemoId;
            }
    
            /// <summary>
            /// Pay with credit memo.
            /// </summary>
            /// <param name="creditMemoId">Identifier of issued credit memo.</param>
            /// <param name="storeId">Store identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="staffId">Staff identifier.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="receiptId">Receipt identifier.</param>
            /// <param name="currencyCode">Currency code.</param>
            /// <param name="amount">Amount of the credit memo.</param>
            public void PayCreditMemo(
                string creditMemoId,
                string storeId,
                string terminalId,
                string staffId,
                string transactionId,
                string receiptId,
                string currencyCode,
                decimal amount)
            {
                ThrowIf.NullOrWhiteSpace(creditMemoId, "creditMemoId");
                ThrowIf.Null(storeId, "storeId");
                ThrowIf.Null(terminalId, "terminalId");
                ThrowIf.Null(staffId, "staffId");
                ThrowIf.Null(transactionId, "transactionId");
                ThrowIf.Null(receiptId, "receiptId");
                ThrowIf.Null(currencyCode, "currencyCode");
    
                DateTime timeStamp = this.context.GetNowInChannelTimeZone().DateTime;
                string dateString = SerializationHelper.ConvertDateTimeToAXDateString(timeStamp, AxDateSequence);
                string timeString = SerializationHelper.ConvertDateTimeToAXTimeString(timeStamp);
    
                object[] parameters =
                {
                    creditMemoId,
                    storeId,
                    terminalId,
                    staffId,
                    transactionId,
                    receiptId,
                    "1", // LineNum parameter, EPOS always passing hardcoded "1" value.
                    amount,
                    dateString,
                    timeString,
                    AxDateSequence
                };
    
                this.InvokeMethodNoDataReturn(
                    UpdateCreditMemoMethodName,
                    parameters);
            }
    
            /// <summary>
            /// Reserves the credit memo so it cannot be used from different terminal.
            /// </summary>
            /// <param name="creditMemoId">Credit memo identifier.</param>
            /// <param name="storeId">Store identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            /// <param name="memoCurrencyCode">Currency code of specified credit memo.</param>
            /// <param name="amount">Amount of the credit memo.</param>
            public void LockCreditMemo(string creditMemoId, string storeId, string terminalId, out string memoCurrencyCode, out decimal amount)
            {
                ThrowIf.NullOrWhiteSpace(creditMemoId, "creditMemoId");
                ThrowIf.Null(storeId, "storeId");
                ThrowIf.Null(terminalId, "terminalId");
    
                object[] parameters =
                {
                    creditMemoId,
                    storeId,
                    terminalId
                };
    
                var data = this.InvokeMethod(
                    ValidateCreditMemoMethodName,
                    parameters);
    
                // Parse response data
                memoCurrencyCode = (string)data[0];
                amount = (decimal)data[1];
            }
    
            /// <summary>
            /// Unlock credit memo.
            /// </summary>
            /// <param name="creditMemoId">Credit memo identifier.</param>
            /// <param name="storeId">Store identifier.</param>
            /// <param name="terminalId">Terminal identifier.</param>
            public void UnlockCreditMemo(string creditMemoId, string storeId, string terminalId)
            {
                ThrowIf.NullOrWhiteSpace(creditMemoId, "creditMemoId");
                ThrowIf.Null(storeId, "storeId");
                ThrowIf.Null(terminalId, "terminalId");
    
                object[] parameters =
                {
                    creditMemoId,
                    storeId,
                    terminalId
                };
    
                this.InvokeMethodNoDataReturn(
                    VoidCreditMemoPaymentMethodName,
                    parameters);
            }
    
            /// <summary>
            /// Get the credit memo balance.
            /// </summary>
            /// <param name="creditMemoId">Credit memo identifier.</param>
            /// <param name="creditMemoCurrencyCode">Currency code of specified credit memo.</param>
            /// <param name="balance">Credit memo balance.</param>
            public void GetCreditMemo(string creditMemoId, out string creditMemoCurrencyCode, out decimal balance)
            {
                ThrowIf.NullOrWhiteSpace(creditMemoId, "creditMemoId");
    
                object[] parameters =
                {
                    creditMemoId
                };
    
                var data = this.InvokeMethod(
                    GetCreditMemoMethodName,
                    parameters);
    
                // Parse response data
                creditMemoCurrencyCode = (string)data[0];
                balance = (decimal)data[1];
            }
    
            /// <summary>
            /// Validate customer account payment.
            /// </summary>
            /// <param name="customerId">Customer identifier.</param>
            /// <param name="amount">Payment amount.</param>
            /// <param name="currencyCode">Currency code.</param>
            public void ValidateCustomerAccountPayment(string customerId, decimal amount, string currencyCode)
            {
                ThrowIf.NullOrWhiteSpace(customerId, "customerId");
                ThrowIf.Null(currencyCode, "currencyCode");
    
                object[] parameters =
                {
                    customerId,
                    amount,
                    currencyCode
                };
    
                this.InvokeMethodNoDataReturn(
                    ValidateCustomerStatusMethodName,
                    parameters);
            }
    
            /// <summary>
            /// Gets the terminal merchant payment provider properties.
            /// </summary>
            /// <param name="hardwareProfileId">The record identifier for the RetailHardwareProfile table to fetch the merchant payment provider properties from.</param>
            /// <returns>The merchant payment provider properties for the required channel.</returns>
            public string GetMerchantPaymentProviderDataForTerminal(string hardwareProfileId)
            {
                object[] parameters =
                {
                    hardwareProfileId
                };
    
                var data = this.InvokeMethod(
                    GetMerchantPaymentPropertiesForHardwareProfileMethodName,
                    parameters);
    
                return (string)data[0];
            }
    
            /// <summary>
            /// Gets the channel merchant payment provider properties.
            /// </summary>
            /// <param name="channelIdentifier">The channel identifier for the RetailChannelPaymentConnectorLine table to fetch the merchant payment provider properties from.</param>
            /// <returns>The merchant payment provider properties for the required channel.</returns>
            public string GetMerchantPaymentProviderDataForOnlineStore(long channelIdentifier)
            {
                object[] parameters =
                {
                    channelIdentifier
                };
    
                var data = this.InvokeMethod(
                    GetMerchantPaymentPropertiesForOnlineStoreMethodName,
                    parameters);
    
                return (string)data[0];
            }
        }
    }
}
