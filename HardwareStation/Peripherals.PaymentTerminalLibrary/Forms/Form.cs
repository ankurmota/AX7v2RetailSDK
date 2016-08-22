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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.Forms
    {
        /// <summary>
        ///  Contains device form names and property names.
        /// </summary>
        public static class Form
        {
            #region ProcessingForm
            /// <summary>
            ///  Form name for processing screen.
            /// </summary>
            public const string Processing = "Processing";
    
            /// <summary>
            ///  Property name for approve text.
            /// </summary>
            public const string ProcessingTextProperty = "ProcessingText";
            #endregion
    
            #region IdleForm
            /// <summary>
            ///  Form name for idle screen.
            /// </summary>
            public const string Idle = "Idle";
    
            /// <summary>
            ///  Property name on form for idle text.
            /// </summary>
            public const string IdleTextProperty = "IdleText";
            #endregion
    
            #region CardSelectionForm
            /// <summary>
            ///  Form name for card selection screen.
            /// </summary>
            public const string CardSelection = "CardSelection";
    
            /// <summary>
            ///  Property name on form for card selection text.
            /// </summary>
            public const string CardSelectionTextProperty = "SelectionText";
    
            /// <summary>
            ///  Button name for "Credit".
            /// </summary>
            public const string CreditButton = "Credit";
    
            /// <summary>
            ///  Button name for "Debit".
            /// </summary>
            public const string DebitButton = "Debit";
    
            /// <summary>
            ///  Button name for "EMV".
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Emv", Justification = "Europay, MasterCard, Visa card type")]
            public const string EmvButton = "EMV";
            #endregion
    
            #region CardSwipeForm
            /// <summary>
            ///  Form name for card swipe screen.
            /// </summary>
            public const string CardSwipe = "CardSwipe";
    
            /// <summary>
            ///  Property name on form for swipe card text.
            /// </summary>
            public const string CardSwipeTextProperty = "SwipeText";
            #endregion
    
            #region DebitCashbackForm
            /// <summary>
            ///  Form name for cash back screen.
            /// </summary>
            public const string Cashback = "Cashback";
            #endregion
    
            #region SignatureForm
            /// <summary>
            ///  Form name for signature screen.
            /// </summary>
            public const string Signature = "Signature";
    
            /// <summary>
            ///  Property name on form for signature text.
            /// </summary>
            public const string ProvideSignatureTextProperty = "SignatureText";
            #endregion
    
            #region ThankYouForm
            /// <summary>
            ///  Form name for thank you screen.
            /// </summary>
            public const string ThankYou = "Thankyou";
    
            /// <summary>
            ///  Property name on form for thank you text.
            /// </summary>
            public const string ThankYouTextProperty = "ThankyouText";
            #endregion
    
            #region TotalForm
            /// <summary>
            ///  Form name for transaction totals screen.
            /// </summary>
            public const string Total = "Total";
    
            /// <summary>
            ///  Property name on form for item list box.
            /// </summary>
            public const string ItemListProperty = "Items";
    
            /// <summary>
            ///  Property name on form for subtotal text.
            /// </summary>
            public const string SubtotalProperty = "Subtotal";
    
            /// <summary>
            ///  Property name on form for discount text.
            /// </summary>
            public const string DiscountProperty = "Discount";
    
            /// <summary>
            ///  Property name on form for tax text.
            /// </summary>
            public const string TaxProperty = "Tax";
    
            /// <summary>
            ///  Property name on form for total text.
            /// </summary>
            public const string TotalProperty = "Total";
            #endregion
    
            #region WelcomeForm
            /// <summary>
            ///  Form name for welcome screen.
            /// </summary>
            public const string Welcome = "Welcome";
    
            /// <summary>
            ///  Property name on form for welcome text.
            /// </summary>
            public const string WelcomeTextProperty = "WelcomeText";
            #endregion
        }
    }
}
