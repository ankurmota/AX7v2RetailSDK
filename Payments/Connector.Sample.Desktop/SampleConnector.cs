/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

/* 
 IMPORTANT!!!
 THIS IS SAMPLE CODE ONLY.  
 THE CODE SHOULD BE UPDATED TO WORK WITH THE APPROPRIATE PAYMENT PROVIDERS.
 PROPER MESASURES SHOULD BE TAKEN TO ENSURE THAT THE PA-DSS AND PCI DSS REQUIREMENTS ARE MET. 
*/

namespace Contoso
{
    namespace Retail.SampleConnector
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.ComponentModel.Composition;
        using System.Text;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.Dynamics.Retail.PaymentProcessor.Common;
        using Microsoft.Dynamics.Retail.PaymentSDK;
        using Microsoft.Dynamics.Retail.PaymentSDK.Constants;
        using PORTABLESDK = Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    
        /// <summary>
        /// SampleConnector class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "SAMPLE CODE ONLY")]
        [Export(typeof(IPaymentProcessor))]
        [Serializable]
        public class SampleConnector : SampleTenderProcessor, IPaymentProcessor
        {
            private const string Platform = "Desktop";
            
            [NonSerializedAttribute]
            private Portable.SampleConnector portableSampleConnector = null;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="SampleConnector"/> class.
            /// </summary>
            public SampleConnector()
            {
                this.portableSampleConnector = new Portable.SampleConnector();
            }
    
            #region Payment processing public APIs
    
            /// <summary>
            /// Authorize the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the authorize transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by authorization process.</param>
            /// <returns>Response object.</returns>
            public Response Authorize(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.Authorize(request.ToPortable(), requiredInteractionProperties.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// Capture the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the Capture transaction.</param>
            /// <returns>Response object.</returns>
            public Response Capture(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.Capture(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// ImmediateCapture the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the ImmediateCapture transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by ImmediateCapture process.</param>
            /// <returns>Response object.</returns>
            public Response ImmediateCapture(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.ImmediateCapture(request.ToPortable(), requiredInteractionProperties.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// Void the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the Void transaction.</param>
            /// <returns>Response object.</returns>
            public Response Void(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.Void(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// Refund the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the Refund transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by Refund process.</param>
            /// <returns>Response object.</returns>
            public Response Refund(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.Refund(request.ToPortable(), requiredInteractionProperties.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// Reversal the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the Reversal transaction.</param>
            /// <returns>Response object.</returns>
            public Response Reversal(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.Reversal(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// ReAuthorize the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the ReAuthorize transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by ReAuthorize process.</param>
            /// <returns>Response object.</returns>
            public Response Reauthorize(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.Reauthorize(request.ToPortable(), requiredInteractionProperties.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// GenerateCardToken get the token for the requested credit card from the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the GenerateCardToken transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by GenerateCardToken process.</param>
            /// <returns>Response object.</returns>
            public Response GenerateCardToken(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.GenerateCardToken(request.ToPortable(), requiredInteractionProperties.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// GetPaymentAcceptPoint gets the payment accepting point from the payment provider, e.g. a payment page URL.
            /// </summary>
            /// <param name="request">Request object needed to process the GetPaymentAcceptPoint transaction.</param>
            /// <returns>Response object.</returns>
            public Response GetPaymentAcceptPoint(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.GetPaymentAcceptPoint(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// RetrievePaymentAcceptResult retrieves the payment accepting result from the payment provider after the payment is processed externally.
            /// This method pairs with GetPaymentAcceptPoint.
            /// </summary>
            /// <param name="request">Request object needed to process the RetrievePaymentAcceptResult transaction.</param>
            /// <returns>Response object.</returns>
            public Response RetrievePaymentAcceptResult(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.RetrievePaymentAcceptResult(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// ActivateGiftCard the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the ActivateGiftCard transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by ActivateGiftCard process.</param>
            /// <returns>Response object.</returns>
            public Response ActivateGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                var ex = new NotImplementedException("ActivateGiftCard NotImplemented");
                RetailLogger.Log.PaymentConnectorLogException("ActivateGiftCard", this.Name, Platform, ex);
                throw ex;
            }
    
            /// <summary>
            /// LoadGiftCard the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the LoadGiftCard transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by LoadGiftCard process.</param>
            /// <returns>Response object.</returns>
            public Response LoadGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                var ex = new NotImplementedException("LoadGiftCard NotImplemented");
                RetailLogger.Log.PaymentConnectorLogException("LoadGiftCard", this.Name, Platform, ex);
                throw ex;
            }
    
            /// <summary>
            /// BalanceOnGiftCard the request with the payment provider.
            /// </summary>
            /// <param name="request">Request object needed to process the BalanceOnGiftCard transaction.</param>
            /// <param name="requiredInteractionProperties">Properties required by BalanceOnGiftCard process.</param>
            /// <returns>Response object.</returns>
            public Response BalanceOnGiftCard(Request request, PaymentProperty[] requiredInteractionProperties)
            {
                var ex = new NotImplementedException("BalanceOnGiftCard NotImplemented");
                RetailLogger.Log.PaymentConnectorLogException("BalanceOnGiftCard", this.Name, Platform, ex);
                throw ex;
            }
    
            /// <summary>
            /// GetMerchantAccountPropertyMetadata returns the merchant account properties need by the payment provider.
            /// </summary>
            /// <param name="request">Request object.</param>
            /// <returns>Response object.</returns>
            public Response GetMerchantAccountPropertyMetadata(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.GetMerchantAccountPropertyMetadata(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            /// <summary>
            /// ValidateMerchantAccount the passed merchant account properties with the payment provider.
            /// </summary>
            /// <param name="request">Request object to validate.</param>
            /// <returns>Response object.</returns>
            public Response ValidateMerchantAccount(Request request)
            {
                Microsoft.Dynamics.Retail.PaymentSDK.Portable.Response responsePortable = this.portableSampleConnector.ValidateMerchantAccount(request.ToPortable());
                return responsePortable.ToDesktop();
            }
    
            #endregion
        }
    }
}
