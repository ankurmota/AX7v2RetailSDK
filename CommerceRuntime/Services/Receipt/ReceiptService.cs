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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Text;
        using System.Text.RegularExpressions;
        using System.Xml;
        using System.Xml.Linq;
        using Commerce.Runtime.Services.ReceiptIndia;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Localization;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using PaymentSDK = Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

        /// <summary>
        /// The receipt service to get the formatted receipts.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
        public class ReceiptService : IRequestHandler
        {
            private const string LocalizationCustomerAccountDeposit = "Microsoft_Dynamics_Commerce_Runtime_Receipt_CustomerAccountDeposit";
            private const string LocalizationNotApplicable = "Microsoft_Dynamics_Commerce_Runtime_Receipt_NotApplicable";
            private const string LocalizationGiftCard = "Microsoft_Dynamics_Commerce_Runtime_Receipt_GiftCard";
            private const string LocalizationShipping = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Shipping";
            private const string LocalizationMixed = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Mixed";
            private const string LocalizationIncomeExpense = "Microsoft_Dynamics_Commerce_Runtime_Receipt_IncomeExpense";
            private const string LocalizationCustomerPickup = "Microsoft_Dynamics_Commerce_Runtime_Receipt_CustomerPickup";
            private const string LocalizationChargeBack = "Microsoft_Dynamics_Commerce_Runtime_Receipt_ChargeBack";
            private const string LocalizationChangeBack = "Microsoft_Dynamics_Commerce_Runtime_Receipt_ChangeBack";
            private const string LocalizationSalesTransaction = "Microsoft_Dynamics_Commerce_Runtime_Receipt_SalesTransaction";
            private const string LocalizationCustomerOrder = "Microsoft_Dynamics_Commerce_Runtime_Receipt_CustomerOrder";
            private const string LocalizationCustomerQuote = "Microsoft_Dynamics_Commerce_Runtime_Receipt_CustomerQuote";

            private const string LocalizationCanceled = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Canceled";
            private const string LocalizationConfirmed = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Confirmed";
            private const string LocalizationCreated = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Created";
            private const string LocalizationDelivered = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Delivered";
            private const string LocalizationInvoiced = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Invoiced";
            private const string LocalizationLost = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Lost";
            private const string LocalizationProcessing = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Processing";
            private const string LocalizationSent = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Sent";

            private const string LocalizationPayCard = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Card";
            private const string LocalizationPayCash = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Cash";
            private const string LocalizationPayCheck = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Check";
            private const string LocalizationPayCreditMemo = "Microsoft_Dynamics_Commerce_Runtime_Receipt_CreditMemo";
            private const string LocalizationPayCurrency = "Microsoft_Dynamics_Commerce_Runtime_Receipt_Currency";
            private const string LocalizationPayCustomerAccount = "Microsoft_Dynamics_Commerce_Runtime_Receipt_CustomerAccount";
            private const string LocalizationPayGiftCertificate = "Microsoft_Dynamics_Commerce_Runtime_Receipt_GiftCard";
            private const string LocalizationPayLoyalty = "Microsoft_Dynamics_Commerce_Runtime_Receipt_LoyaltyCard";

            private const string Esc = "&#x1B;";
            private const string LegacyLogoMessage = "<L>";
            private const string LogoMessage = "<L:{0}>";
            private const string SingleSpace = "|1C";
            private const string DoubleSpace = "|2C";
            private const string CRLF = "CRLF";
            private const string Text = "Text";
            private const string Taxes = "Taxes";
            private const string LoyaltyItem = "LoyaltyItem";
            private const string LoyaltyEarnText = "LoyaltyEarnText";
            private const string LoyaltyRedeemText = "LoyaltyRedeemText";
            private const string LoyaltyEarnLines = "LoyaltyEarnLines";
            private const string LoyaltyRedeemLines = "LoyaltyRedeemLines";
            private const string CarriageReturnLineFeed = "\r\n";
            private const string LineFeed = "\n";
            private const string CarriageReturn = "\r";
            private const string ReasonCodeSeparator = " - ";

            private const string EARNEDREWARDPOINTID = "EARNEDREWARDPOINTID";
            private const string REDEEMEDREWARDPOINTID = "REDEEMEDREWARDPOINTID";
            private const string EARNEDREWARDPOINTAMOUNTQUANTITY = "EARNEDREWARDPOINTAMOUNTQUANTITY";
            private const string REDEEMEDREWARDPOINTAMOUNTQUANTITY = "REDEEMEDREWARDPOINTAMOUNTQUANTITY";

            private const string LineFormat = "{0}:{1}";
            private const char DottedPadding = '.';
            private const int PaperWidth = 55;
            private const string TypeFormat = "{0} [{1}]";
            private const string CurrencyFormat = "{0} ({1})";
            private const string ForiegnCurrencyFormat = "{0} - {1}";
            private const string CharPos = "charpos";
            private const string LineId = "line_id";

            private static readonly string SingleLine = string.Empty.PadLeft(55, '-');
            private static readonly string DoubleLine = string.Empty.PadLeft(55, '=');

            // Represents the sql date time minimal value. 1/1/1753 00:00:00
            private static DateTimeOffset sqlDateTimeMinValue = new DateTimeOffset(1753, 1, 2, 0, 0, 0, TimeSpan.Zero);

            // The receipt message localizer.
            private static Lazy<RuntimeReceiptLocalizer> localizer = new Lazy<RuntimeReceiptLocalizer>();

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(GetReceiptServiceRequest),
                    typeof(GetEmailReceiptServiceRequest)
                };
                }
            }

            /// <summary>
            /// Executes the GetReceiptServiceRequest.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The GetReceiptServiceResponse that contains the formatted receipts.</returns>
            public Microsoft.Dynamics.Commerce.Runtime.Messages.Response Execute(Microsoft.Dynamics.Commerce.Runtime.Messages.Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestedType = request.GetType();

                if (requestedType == typeof(GetReceiptServiceRequest))
                {
                    return GetFormattedReceipt((GetReceiptServiceRequest)request);
                }
                else if (requestedType == typeof(GetEmailReceiptServiceRequest))
                {
                    return GetEmailReceipt((GetEmailReceiptServiceRequest)request);
                }

                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }

            internal static string CreateWhitespace(char seperator, int stringLength)
            {
                // If the length exceeds the printable length then no need to add additional white spaces.
                if (stringLength <= 0)
                {
                    stringLength = 0;
                }

                string whiteString = new string(seperator, stringLength);

                return whiteString.ToString();
            }

            internal static string GetFormattedCurrencyValue(decimal? value, string currencyCode, RequestContext context)
            {
                if (value == null)
                {
                    return string.Empty;
                }
                else
                {
                    return GetFormattedCurrencyValue(value.Value, currencyCode, context);
                }
            }

            internal static string GetFormattedCurrencyValue(decimal value, string currencyCode, RequestContext context)
            {
                GetRoundedStringServiceRequest roundingRequest = null;

                string currencySymbol = string.Empty;

                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    var getCurrenciesDataRequest = new GetCurrenciesDataRequest(currencyCode, QueryResultSettings.SingleRecord);
                    Currency currency = context.Runtime.Execute<EntityDataServiceResponse<Currency>>(getCurrenciesDataRequest, context).PagedEntityCollection.FirstOrDefault();
                    currencySymbol = currency.CurrencySymbol;
                }

                roundingRequest = new GetRoundedStringServiceRequest(
                                        value,
                                        currencyCode,
                                        0,
                                        false,
                                        true);

                string roundedValue = context.Execute<GetRoundedStringServiceResponse>(roundingRequest).RoundedValue;

                var formattingRequest = new GetFormattedCurrencyServiceRequest(decimal.Parse(roundedValue), currencySymbol);
                string formattedValue = context.Execute<GetFormattedContentServiceResponse>(formattingRequest).FormattedValue;
                return formattedValue;
            }

            /// <summary>
            /// Align the value on receipt.
            /// </summary>
            /// <param name="valueToBeAligned">The value to be aligned.</param>
            /// <param name="itemInfo">The receipt item info.</param>
            /// <returns>Aligned value.</returns>
            /// <remarks>
            /// The value to be aligned has following three cases:
            /// 1. valueToBeAligned = "Youth Accessory Combo Set" -- the value does not have new line marker, just trim and align it accordingly.
            /// 2. valueToBeAligned = "\r\n" or "" -- treat it as an empty string.
            /// 3. valueToBeAligned = "636 140TH CT\r\nApt C200\r\nWA, 98007" -- the value has multiple lines, we need to trim and align each line.
            /// </remarks>
            internal static string GetAlignmentSettings(string valueToBeAligned, ReceiptItemInfo itemInfo)
            {
                if (string.IsNullOrEmpty(valueToBeAligned))
                {
                    // The second case in the comment.
                    return AlignString(string.Empty, itemInfo);
                }
                else
                {
                    // if the value to be aligned has multiple lines then treat each line as a single line.
                    if (valueToBeAligned.Contains(LineFeed) ||
                        valueToBeAligned.Contains(CarriageReturn))
                    {
                        string[] lines = valueToBeAligned.Split(new string[] { CarriageReturnLineFeed, CarriageReturn, LineFeed }, StringSplitOptions.RemoveEmptyEntries);
                        if (lines.Length == 0)
                        {
                            // The second case in the comment.
                            return AlignString(string.Empty, itemInfo);
                        }
                        else
                        {
                            // The third case in the comment.
                            StringBuilder alignedLines = new StringBuilder();
                            for (int i = 0; i < lines.Length; ++i)
                            {
                                if (i == lines.Length - 1)
                                {
                                    // do not append new line marker for last line.
                                    lines[i] = AlignString(lines[i], itemInfo);
                                    alignedLines.Append(lines[i]);
                                }
                                else
                                {
                                    // append the new line marker back to each line.
                                    lines[i] += Environment.NewLine;
                                    lines[i] = AlignString(lines[i], itemInfo);
                                    alignedLines.Append(lines[i]);
                                }
                            }

                            return alignedLines.ToString();
                        }
                    }
                    else
                    {
                        // The first case in the commet.
                        return AlignString(valueToBeAligned, itemInfo);
                    }
                }
            }

            /// <summary>
            /// Align a string. The value passed, as it is now, should have at most one newline marker and 
            /// there should not be any characters after this new line marker.
            /// </summary>
            /// <param name="line">The single line.</param>
            /// <param name="itemInfo">The receipt item info.</param>
            /// <returns>Aligned string.</returns>
            private static string AlignString(string line, ReceiptItemInfo itemInfo)
            {
                // the indication of a logo is passed on unchanged.
                if (line.Equals(LegacyLogoMessage) || itemInfo.ImageId != 0)
                {
                    return line;
                }

                if (line.Contains(Environment.NewLine))
                {
                    // if this line contains new line marker, then we need to remove marker before
                    // processing this line. But after that we should append the new line marker back to it.
                    line = line.Replace(Environment.NewLine, string.Empty);
                    line = AlignSingleLine(line, itemInfo);
                    line += Environment.NewLine;

                    // If it contains new line marker, don't forget to insert indents
                    string indent = Environment.NewLine + CreateWhitespace(' ', itemInfo.CharIndex - 1);
                    line = line.Replace(Environment.NewLine, indent);
                }
                else
                {
                    line = AlignSingleLine(line, itemInfo);
                }

                return line;
            }

            /// <summary>
            /// Align a single line. Single line means it does not have new line marker.
            /// </summary>
            /// <param name="line">The single line (should not contain new line marker).</param>
            /// <param name="itemInfo">The receipt item info.</param>
            /// <returns>Aligned string.</returns>
            private static string AlignSingleLine(string line, ReceiptItemInfo itemInfo)
            {
                if (line.Length > itemInfo.Length)
                {
                    switch (itemInfo.VerticalAlignment)
                    {
                        case Alignment.Right:
                            line = line.Substring(line.Length - itemInfo.Length, itemInfo.Length);
                            break;
                        default:
                            line = line.Substring(0, itemInfo.Length);
                            break;
                    }
                }
                else if (line.Length < itemInfo.Length)
                {
                    // The value seems to need to be filled
                    switch (itemInfo.VerticalAlignment)
                    {
                        case Alignment.Left:
                            line = line.PadRight(itemInfo.Length, itemInfo.Fill);
                            break;
                        case Alignment.Center:
                            int charCountUsableForSpace = itemInfo.Length - line.Length;
                            int spaceOnLeftSide = (int)charCountUsableForSpace / 2;
                            int spaceOnRightSide = charCountUsableForSpace - spaceOnLeftSide;
                            line = line.PadLeft(spaceOnLeftSide + line.Length, itemInfo.Fill);
                            line = line.PadRight(spaceOnRightSide + line.Length, itemInfo.Fill);
                            break;
                        case Alignment.Right:
                            line = line.PadLeft(itemInfo.Length, itemInfo.Fill);
                            break;
                    }
                }

                return line;
            }

            /// <summary>
            /// Get the formatted receipt for email.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The request response.</returns>
            private static GetEmailReceiptServiceResponse GetEmailReceipt(GetEmailReceiptServiceRequest request)
            {
                SalesOrder salesOrder = request.SalesOrder;
                List<Receipt> receipts = new List<Receipt>();

                OrgUnit store = ReceiptService.GetStoreFromContext(request.RequestContext);
                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(request.RequestContext.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var tenderTypes = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, request.RequestContext).PagedEntityCollection;

                foreach (var receiptType in request.ReceiptTypes)
                {
                    if (!string.IsNullOrWhiteSpace(store.EmailReceiptProfileId))
                    {
                        GetReceiptLayoutIdDataRequest getReceiptLayoutIdDataRequest = new GetReceiptLayoutIdDataRequest(store.EmailReceiptProfileId, receiptType, QueryResultSettings.SingleRecord);
                        string receiptLayout = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getReceiptLayoutIdDataRequest, request.RequestContext).Entity;

                        GetReceiptInfoDataRequest getReceiptInfoDataRequest = new GetReceiptInfoDataRequest(receiptLayout, false, QueryResultSettings.SingleRecord);
                        ReceiptInfo receiptInfo = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptInfo>>(getReceiptInfoDataRequest, request.RequestContext).Entity;

                        if (receiptInfo == null)
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidReceiptTemplate,
                                string.Format("The specified receipt layout ({0}) was not valid.", receiptLayout));
                        }

                        switch (receiptType)
                        {
                            // Email is only sent out for sales receipt.
                            case ReceiptType.SalesReceipt:
                                receipts.Add(GetReceiptFromTransaction(tenderTypes.Results, receiptType, receiptLayout, receiptInfo, salesOrder, null, request.RequestContext));
                                break;
                            default:
                                throw new DataValidationException(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReceiptTypeNotSupported,
                                    string.Format("Receipt for an invalid receipt type {0} was requested", receiptType));
                        }
                    }
                }

                var response = new GetEmailReceiptServiceResponse(receipts.AsPagedResult());
                return response;
            }

            /// <summary>
            /// Gets the formatted receipt. The logic for bringing the receipt templates and creating the formatted string goes here
            /// this is similar to the FormModulation class in POS.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The response containing the set of receipts.</returns>
            private static GetReceiptServiceResponse GetFormattedReceipt(GetReceiptServiceRequest request)
            {
                SalesOrder salesOrder = request.SalesOrder;

                // the request may contain no sales order (e.g. non sales operation receipt)
                if (salesOrder != null)
                {
                    salesOrder.TenderLines.Clear();
                    salesOrder.TenderLines.AddRange(request.TenderLines);
                }

                List<Receipt> receipts = new List<Receipt>();

                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(request.RequestContext.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var tenderTypes = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, request.RequestContext).PagedEntityCollection.Results;

                foreach (var receiptType in request.ReceiptTypes)
                {
                    if (request.IsPreview)
                    {
                        // When previewing, the first element at the receipts collection is the preview receipt.
                        // We also include the printable receipt in the same call for performance improvements at the client.
                        receipts.Add(GetReceiptForPreview(tenderTypes, salesOrder, request.RequestContext, receiptType));
                    }

                    GetPrintersDataRequest getPrintersByReceiptTypeDataRequest = new GetPrintersDataRequest(request.RequestContext.GetPrincipal().TerminalId.ToString(), receiptType, QueryResultSettings.AllRecords, request.HardwareProfileId);
                    IEnumerable<Printer> printers = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<Printer>>(getPrintersByReceiptTypeDataRequest, request.RequestContext).PagedEntityCollection.Results.GroupBy(p => p.ReceiptLayoutId).Select(l => l.First());

                    foreach (Printer printer in printers)
                    {
                        GetReceiptInfoDataRequest getReceiptInfoDataRequest = new GetReceiptInfoDataRequest(printer.ReceiptLayoutId, request.IsCopy, QueryResultSettings.SingleRecord);
                        ReceiptInfo receiptInfo = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptInfo>>(getReceiptInfoDataRequest, request.RequestContext).Entity;

                        if (receiptInfo == null)
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidReceiptTemplate,
                                string.Format("The specified receipt layout ({0}) was not valid.", printer.ReceiptLayoutId));
                        }

                        switch (receiptType)
                        {
                            case ReceiptType.CardReceiptForShop:
                            case ReceiptType.CardReceiptForCustomer:
                            case ReceiptType.CardReceiptForShopReturn:
                            case ReceiptType.CardReceiptForCustomerReturn:
                                receipts.AddRange(GetReceiptFromCard(tenderTypes, receiptType, printer.ReceiptLayoutId, receiptInfo, salesOrder, request));
                                break;

                            case ReceiptType.CustomerAccountReceiptForShop:
                            case ReceiptType.CustomerAccountReceiptForCustomer:
                            case ReceiptType.CustomerAccountReceiptForShopReturn:
                            case ReceiptType.CustomerAccountReceiptForCustomerReturn:
                            case ReceiptType.CreditMemo:
                                receipts.AddRange(GetReceiptFromTenderLine(tenderTypes, receiptType, printer.ReceiptLayoutId, receiptInfo, salesOrder, request));
                                break;

                            case ReceiptType.SalesReceipt:
                            case ReceiptType.SalesOrderReceipt:
                            case ReceiptType.CustomerAccountDeposit:
                            case ReceiptType.PickupReceipt:
                            case ReceiptType.PackingSlip:
                            case ReceiptType.QuotationReceipt:
                            case ReceiptType.GiftReceipt:
                                receipts.Add(GetReceiptFromTransaction(tenderTypes, receiptType, printer.ReceiptLayoutId, receiptInfo, salesOrder, GetHardwareProfileId(request), request.RequestContext));
                                break;

                            case ReceiptType.ReturnLabel:
                                receipts.AddRange(GetReturnLables(tenderTypes, receiptType, printer.ReceiptLayoutId, receiptInfo, salesOrder, GetHardwareProfileId(request), request.RequestContext));
                                break;

                            case ReceiptType.CustomReceipt1:
                            case ReceiptType.CustomReceipt2:
                            case ReceiptType.CustomReceipt3:
                            case ReceiptType.CustomReceipt4:
                            case ReceiptType.CustomReceipt5:
                                receipts.Add(GetReceiptFromTransaction(tenderTypes, receiptType, printer.ReceiptLayoutId, receiptInfo, salesOrder, GetHardwareProfileId(request), request.RequestContext));
                                break;

                            default:
                                throw new DataValidationException(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReceiptTypeNotSupported,
                                    string.Format("Receipt for an invalid receipt type {0} was requested", receiptType));
                        }
                    }

                    // Check if the receipt is for a hardcoded template
                    switch (receiptType)
                    {
                        case ReceiptType.SafeDrop:
                            receipts.Add(GetReceiptForSafeDrop(request.DropAndDeclareTransaction, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.BankDrop:
                            receipts.Add(GetReceiptForBankDrop(request.DropAndDeclareTransaction, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.TenderDeclaration:
                            receipts.Add(GetReceiptForTenderDeclaration(request.DropAndDeclareTransaction, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.RemoveTender:
                            receipts.Add(GetReceiptForRemoveTender(request.NonSalesTenderTransaction, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.FloatEntry:
                            receipts.Add(GetReceiptForFloatEntry(request.NonSalesTenderTransaction, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.StartingAmount:
                            receipts.Add(GetReceiptForStartingAmount(request.NonSalesTenderTransaction, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.XReport:
                        case ReceiptType.ZReport:
                            receipts.Add(GetReceiptForXZReport(request.ShiftDetails, receiptType, request.RequestContext, request.HardwareProfileId));
                            break;
                        case ReceiptType.GiftCertificate:
                            GetAllGiftCardReceipts(receipts, salesOrder, request.RequestContext, request.HardwareProfileId);
                            break;
                        default:
                            // Do nothing on default.
                            break;
                    }

                    if (request.IsPreview)
                    {
                        // When previewing, we only include printable receipt of the same previewed receipt type.
                        break;
                    }
                }

                var response = new GetReceiptServiceResponse(receipts.AsReadOnly());
                return response;
            }

            private static Receipt GetReceiptForPreview(ReadOnlyCollection<TenderType> tenderTypes, SalesOrder salesOrder, RequestContext context, ReceiptType receiptType)
            {
                Terminal terminal = context.GetTerminal();
                GetHardwareProfileDataRequest getHardwareProfileDataRequest = new GetHardwareProfileDataRequest(terminal.HardwareProfile, QueryResultSettings.SingleRecord);
                HardwareProfile hardwareProfile = context.Runtime.Execute<SingleEntityDataServiceResponse<HardwareProfile>>(getHardwareProfileDataRequest, context).Entity;

                // The hardware profile should always return two printers and at least one printer should have a receipt profile, even with DeviceType set to None.
                IEnumerable<HardwareProfilePrinter> printersWithReceiptProfile = hardwareProfile.Printers.Where(p => !string.IsNullOrWhiteSpace(p.ReceiptProfileId));
                if (printersWithReceiptProfile.IsNullOrEmpty())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NoPrintersReturned, "A receipt profile identifier could not be retrieved from the hardware profile.");
                }

                foreach (var printer in printersWithReceiptProfile)
                {
                    GetReceiptLayoutIdDataRequest getReceiptLayoutIdDataRequest = new GetReceiptLayoutIdDataRequest(printer.ReceiptProfileId, receiptType, QueryResultSettings.SingleRecord);
                    string receiptLayoutId = context.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getReceiptLayoutIdDataRequest, context).Entity;

                    // We should iterate all printers to check if its receipt profile contain given receipt type 
                    if (!string.IsNullOrEmpty(receiptLayoutId))
                    {
                        GetReceiptInfoDataRequest getReceiptInfoDataRequest = new GetReceiptInfoDataRequest(receiptLayoutId, true, QueryResultSettings.SingleRecord);
                        ReceiptInfo receiptInfo = context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptInfo>>(getReceiptInfoDataRequest, context).Entity;

                        if (receiptInfo == null)
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidReceiptTemplate,
                                string.Format("The specified receipt layout ({0}) was not valid.", receiptLayoutId));
                        }

                        // We return the first receipt of the given type found at the printers
                        return GetReceiptFromTransaction(tenderTypes, receiptType, receiptLayoutId, receiptInfo, salesOrder, null, context);
                    }
                }

                // If not receipt is found, than throw exception
                throw new DataValidationException(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ReceiptTypeNotFound,
                    string.Format("The requested receipt type ({0}) was not found.", receiptType.ToString()))
                {
                    LocalizedMessageParameters = new object[] { receiptType.ToString() }
                };
            }

            /// <summary>
            /// Get receipts for all gift cards. Each gift card has a separate receipt that needs to be printed.
            /// </summary>
            /// <param name="receipts">The collection of receipts.</param>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            private static void GetAllGiftCardReceipts(List<Receipt> receipts, SalesOrder salesOrder, RequestContext context, string hardwareProfileId)
            {
                foreach (SalesLine line in salesOrder.SalesLines)
                {
                    if (line.IsGiftCardLine && !line.IsVoided)
                    {
                        var serviceRequest = new GetGiftCardServiceRequest(line.Comment);

                        GetGiftCardServiceResponse serviceResponse = context.Execute<GetGiftCardServiceResponse>(serviceRequest);

                        receipts.Add(GetReceiptForGiftCard(salesOrder, serviceResponse.GiftCard, context, hardwareProfileId));
                    }
                }
            }

            /// <summary>
            /// Get formatted receipt for a single gift card.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="giftCard">The gift card.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForGiftCard(SalesOrder salesOrder, GiftCard giftCard, RequestContext context, string hardwareProfileId)
            {
                StringBuilder reportLayout = new StringBuilder();
                Receipt receipt = new Receipt();
                string currency = context.GetOrgUnit().Currency;

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareGiftCardHeader(reportLayout, salesOrder, channelDateTimeOffset);
                reportLayout.AppendLine(SingleLine);
                reportLayout.AppendLine();
                receipt.Header = reportLayout.ToString();

                reportLayout.Clear();
                reportLayout.AppendLine(ReceiptHelper.FormatTenderLine("<T:string_6100>.", giftCard.Id));
                reportLayout.AppendLine(ReceiptHelper.FormatTenderLine(
                    "<T:string_6101>",
                    GetFormattedCurrencyValue(giftCard.Balance, currency, context)));
                reportLayout.AppendLine();
                reportLayout.AppendLine(DoubleLine);
                reportLayout.AppendLine();
                receipt.Footer = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6150>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);
                return receipt;
            }

            /// <summary>
            /// Get the receipt for safe drop.
            /// </summary>
            /// <param name="transaction">The drop and declare transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The safe drop receipt.</returns>
            private static Receipt GetReceiptForSafeDrop(DropAndDeclareTransaction transaction, RequestContext context, string hardwareProfileId)
            {
                StringBuilder reportLayout = new StringBuilder();
                Receipt receipt = new Receipt();

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareReceiptHeader("<T:string_6148>", reportLayout, transaction, channelDateTimeOffset);
                reportLayout.AppendLine(string.Empty.PadLeft(55, '-'));
                receipt.Header = reportLayout.ToString();

                reportLayout.Clear();
                PrepareDropAndDeclareTenders(reportLayout, transaction, context);
                reportLayout.AppendLine(string.Empty.PadLeft(55, '='));
                reportLayout.AppendLine();
                receipt.Body = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6148>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);

                return receipt;
            }

            /// <summary>
            /// Get the bank drop receipt.
            /// </summary>
            /// <param name="transaction">The bank drop transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForBankDrop(DropAndDeclareTransaction transaction, RequestContext context, string hardwareProfileId)
            {
                StringBuilder reportLayout = new StringBuilder();
                Receipt receipt = new Receipt();

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareReceiptHeader("<T:string_6151>", reportLayout, transaction, channelDateTimeOffset);

                string bagId = string.Empty;
                if (transaction.TenderDetails != null && transaction.TenderDetails.Count > 0)
                {
                    bagId = transaction.TenderDetails.First().BankBagNumber;
                }

                reportLayout.AppendLine(ReceiptHelper.FormatHeaderLine("<T:string_6102>", bagId ?? string.Empty, true));
                reportLayout.AppendLine(SingleLine);
                receipt.Header = reportLayout.ToString();

                reportLayout.Clear();
                PrepareDropAndDeclareTenders(reportLayout, transaction, context);
                reportLayout.AppendLine(DoubleLine);
                reportLayout.AppendLine();
                receipt.Body = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6151>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);
                return receipt;
            }

            /// <summary>
            /// Get receipt for tender declaration.
            /// </summary>
            /// <param name="transaction">The tender declaration transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForTenderDeclaration(DropAndDeclareTransaction transaction, RequestContext context, string hardwareProfileId)
            {
                Receipt receipt = new Receipt();
                StringBuilder reportLayout = new StringBuilder();

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareReceiptHeader("<T:string_6154>", reportLayout, transaction, channelDateTimeOffset);
                reportLayout.AppendLine(SingleLine);
                receipt.Header = reportLayout.ToString();

                reportLayout.Clear();
                PrepareDropAndDeclareTenders(reportLayout, transaction, context);
                reportLayout.AppendLine(DoubleLine);
                reportLayout.AppendLine();
                receipt.Body = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6154>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);
                return receipt;
            }

            /// <summary>
            /// Get receipt for tender removal.
            /// </summary>
            /// <param name="transaction">The remove tender transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForRemoveTender(NonSalesTransaction transaction, RequestContext context, string hardwareProfileId)
            {
                Receipt receipt = new Receipt();
                StringBuilder reportLayout = new StringBuilder();

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareReceiptHeader("<T:string_6152>", reportLayout, transaction, channelDateTimeOffset);
                reportLayout.AppendLine(SingleLine);
                receipt.Header = reportLayout.ToString();

                reportLayout.Clear();
                reportLayout.AppendLine();
                reportLayout.AppendLine(ReceiptHelper.FormatTenderLine("<T:string_6103>", GetFormattedCurrencyValue(transaction.Amount, transaction.ForeignCurrency, context)));

                reportLayout.AppendLine(transaction.Description.ToString());
                reportLayout.AppendLine();
                reportLayout.AppendLine(DoubleLine);
                reportLayout.AppendLine();
                receipt.Body = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6152>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);
                return receipt;
            }

            /// <summary>
            /// Get receipt for float entry.
            /// </summary>
            /// <param name="transaction">The float entry transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForFloatEntry(NonSalesTransaction transaction, RequestContext context, string hardwareProfileId)
            {
                StringBuilder reportLayout = new StringBuilder();
                Receipt receipt = new Receipt();

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareReceiptHeader("<T:string_6153>", reportLayout, transaction, channelDateTimeOffset);
                reportLayout.AppendLine(SingleLine);

                reportLayout.AppendLine();
                reportLayout.AppendLine(ReceiptHelper.FormatTenderLine("<T:string_6104>", GetFormattedCurrencyValue(transaction.Amount, transaction.ForeignCurrency, context)));
                reportLayout.AppendLine(transaction.Description.ToString());
                receipt.Header = reportLayout.ToString();
                reportLayout.Clear();
                reportLayout.AppendLine();
                reportLayout.AppendLine(DoubleLine);
                reportLayout.AppendLine();
                receipt.Body = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6153>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);
                return receipt;
            }

            /// <summary>
            /// Get receipt for declaring starting amount.
            /// </summary>
            /// <param name="transaction">The float entry transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForStartingAmount(NonSalesTransaction transaction, RequestContext context, string hardwareProfileId)
            {
                StringBuilder reportLayout = new StringBuilder();
                Receipt receipt = new Receipt();

                DateTimeOffset channelDateTimeOffset = context.GetNowInChannelTimeZone();

                ReceiptHelper.PrepareReceiptHeader("<T:string_6155>", reportLayout, transaction, channelDateTimeOffset);
                reportLayout.AppendLine(SingleLine);
                receipt.Header = reportLayout.ToString();

                reportLayout.Clear();
                reportLayout.AppendLine();
                reportLayout.AppendLine(ReceiptHelper.FormatTenderLine("<T:string_6105>", GetFormattedCurrencyValue(transaction.Amount, transaction.ForeignCurrency, context)));

                reportLayout.AppendLine(transaction.Description.ToString());
                reportLayout.AppendLine();
                reportLayout.AppendLine(DoubleLine);
                reportLayout.AppendLine();
                receipt.Body = reportLayout.ToString();
                receipt.ReceiptTitle = "<T:string_6155>";
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Always print.
                ChangePrinterBehavior(receipt.Printers, true);
                return receipt;
            }

            /// <summary>
            /// Get all printers for a terminal. Used for hardcoded templates.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>A collection of printers.</returns>
            private static ReadOnlyCollection<Printer> GetAllPrintersForTerminal(RequestContext context, string hardwareProfileId)
            {
                GetPrintersDataRequest getPrintersDataRequest = new GetPrintersDataRequest(context.GetPrincipal().TerminalId.ToString(), QueryResultSettings.AllRecords, hardwareProfileId);
                return context.Runtime.Execute<EntityDataServiceResponse<Printer>>(getPrintersDataRequest, context).PagedEntityCollection.Results.GroupBy(p => p.Name).Select(l => l.First()).AsReadOnly();
            }

            /// <summary>
            /// Get receipt for X or Z report.
            /// </summary>
            /// <param name="shift">The current shift.</param>
            /// <param name="receiptType">The receipt type.</param>
            /// <param name="context">The request context.</param>
            /// <param name="hardwareProfileId">The hardware profile for which formatted receipts are to be generated.</param>
            /// <returns>The formatted receipt.</returns>
            private static Receipt GetReceiptForXZReport(Shift shift, ReceiptType receiptType, RequestContext context, string hardwareProfileId)
            {
                // TextID's for the Z/X Report are reserved at 7000 - 7099
                StringBuilder reportLayout = new StringBuilder(2500);
                Receipt receipt = new Receipt();

                // Header
                PrepareHeader(reportLayout, shift, receiptType, context);
                receipt.Header = reportLayout.ToString();
                reportLayout.Clear();
                string currency = context.GetOrgUnit().Currency;

                // Total Amounts
                ReceiptHelper.AppendReportLine(reportLayout, "<T:string_6106>");
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6107>", GetFormattedCurrencyValue(shift.SalesTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6108>", GetFormattedCurrencyValue(shift.ReturnsTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6109>", GetFormattedCurrencyValue(shift.TaxTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6110>", GetFormattedCurrencyValue(shift.DiscountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6111>", GetFormattedCurrencyValue(shift.RoundedAmountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6112>", GetFormattedCurrencyValue(shift.PaidToAccountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6113>", GetFormattedCurrencyValue(shift.IncomeAccountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6114>", GetFormattedCurrencyValue(shift.ExpenseAccountTotal, currency, context));
                reportLayout.AppendLine();

                // Statistics
                ReceiptHelper.AppendReportLine(reportLayout, "<T:string_6115>");
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6107>", shift.SaleTransactionCount);
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6116>", shift.CustomerCount);
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6117>", shift.VoidTransactionCount);
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6119>", shift.NoSaleTransactionCount);

                if (receiptType == ReceiptType.XReport)
                {
                    ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6120>", shift.SuspendedTransactionCount);
                }

                reportLayout.AppendLine();

                // Tender totals
                ReceiptHelper.AppendReportLine(reportLayout, "<T:string_6121>");
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6122>", GetFormattedCurrencyValue(shift.TenderedTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6123>", GetFormattedCurrencyValue(shift.ChangeTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6105>", GetFormattedCurrencyValue(shift.StartingAmountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6124>", GetFormattedCurrencyValue(shift.FloatingEntryAmountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6125>", GetFormattedCurrencyValue(shift.RemoveTenderAmountTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6126>", GetFormattedCurrencyValue(shift.BankDropTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6127>", GetFormattedCurrencyValue(shift.SafeDropTotal, currency, context));
                ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6128>", GetFormattedCurrencyValue(shift.DeclareTenderAmountTotal, currency, context));

                bool amountShort = shift.OverShortTotal < 0;

                ReceiptHelper.AppendReportLine(reportLayout, amountShort ? "<F:string_6129>" : "<F:string_6130>", GetFormattedCurrencyValue(amountShort ? decimal.Negate(shift.OverShortTotal) : shift.OverShortTotal, currency, context));
                reportLayout.AppendLine();

                // Income/Expense
                if (shift.AccountLines.Count > 0)
                {
                    ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6131>/<F:string_6132>");
                    foreach (ShiftAccountLine accountLine in shift.AccountLines.OrderBy(a => a.AccountType))
                    {
                        string typeResourceId = string.Empty;

                        switch (accountLine.AccountType)
                        {
                            case IncomeExpenseAccountType.Income:
                                typeResourceId = "<F:string_6131>";
                                break;

                            case IncomeExpenseAccountType.Expense:
                                typeResourceId = "<F:string_6132>";
                                break;

                            default:
                                string message = string.Format("Unsupported account Type '{0}'.", accountLine.AccountType);
                                throw new NotSupportedException(message);
                        }

                        ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, accountLine.AccountNumber, typeResourceId), GetFormattedCurrencyValue(accountLine.Amount, currency, context));
                    }

                    reportLayout.AppendLine();
                }

                // Tenders
                if (receiptType == ReceiptType.ZReport && shift.TenderLines.Count > 0)
                {
                    ReceiptHelper.AppendReportLine(reportLayout, "<T:string_6133>");
                    foreach (ShiftTenderLine tenderLine in shift.TenderLines.OrderBy(t => t.TenderTypeName))
                    {
                        string formatedTenderName = tenderLine.TenderTypeName;

                        if (currency != tenderLine.TenderCurrency)
                        {
                            formatedTenderName = string.Format(CurrencyFormat, tenderLine.TenderTypeName, tenderLine.TenderCurrency);
                        }

                        ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, formatedTenderName, "<F:string_6124>"), GetFormattedCurrencyValue(tenderLine.AddToTenderAmountOfTenderCurrency, tenderLine.TenderCurrency, context));
                        ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, formatedTenderName, "<F:string_6134>"), GetFormattedCurrencyValue(tenderLine.ShiftAmountOfTenderCurrency, tenderLine.TenderCurrency, context));
                        ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, formatedTenderName, "<F:string_6125>"), GetFormattedCurrencyValue(tenderLine.TotalRemovedFromTenderAmountOfTenderCurrency, tenderLine.TenderCurrency, context));

                        if (tenderLine.CountingRequired)
                        {
                            ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, formatedTenderName, "<F:string_6128>"), GetFormattedCurrencyValue(tenderLine.DeclareTenderAmountOfTenderCurrency, tenderLine.TenderCurrency, context));

                            amountShort = tenderLine.OverShortAmountOfTenderCurrency < 0;

                            ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, formatedTenderName, amountShort ? "<F:string_6129>" : "<F:string_6130>"), GetFormattedCurrencyValue(amountShort ? decimal.Negate(tenderLine.OverShortAmountOfTenderCurrency) : tenderLine.OverShortAmountOfTenderCurrency, tenderLine.TenderCurrency, context));
                        }

                        ReceiptHelper.AppendReportLine(reportLayout, string.Format(TypeFormat, formatedTenderName, "<F:string_6135>"), tenderLine.Count);

                        reportLayout.AppendLine();
                    }
                }

                reportLayout.AppendLine();
                reportLayout.AppendLine();
                reportLayout.AppendLine();

                receipt.Body = reportLayout.ToString();
                receipt.Printers = GetAllPrintersForTerminal(context, hardwareProfileId);

                // Whether or not print the x/z report is determined by DeviceConfig.PrintXZReportsOnTerminal(in AX's FunctionalityProfile)
                DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
                ChangePrinterBehavior(receipt.Printers, deviceConfiguration.PrintXZReportsOnTerminal);

                return receipt;
            }

            /// <summary>
            /// Prepare the tender part of receipt.
            /// </summary>
            /// <param name="reportLayout">The receipt string.</param>
            /// <param name="transaction">The drop and declare transaction.</param>
            /// <param name="context">The request context.</param>
            private static void PrepareDropAndDeclareTenders(StringBuilder reportLayout, DropAndDeclareTransaction transaction, RequestContext context)
            {
                string tenderName = string.Empty;
                string amount = string.Empty;
                GetChannelTenderTypesDataRequest getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                ReadOnlyCollection<TenderType> tenderTypes = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, context).PagedEntityCollection.Results;

                reportLayout.AppendLine();
                string storeCurrency = context.GetOrgUnit().Currency;

                foreach (TenderDetail tenderLine in transaction.TenderDetails)
                {
                    TenderType tenderTypeId = tenderTypes.Where(type => type.TenderTypeId == tenderLine.TenderTypeId).Single();
                    if (tenderLine.ForeignCurrency == storeCurrency)
                    {
                        // Tenders in the store currency
                        tenderName = tenderTypeId.Name;
                        amount = GetFormattedCurrencyValue(tenderLine.Amount, tenderLine.ForeignCurrency, context);
                    }
                    else
                    {
                        // Foreign currency {Currency - CAD}
                        tenderName = string.Format(ForiegnCurrencyFormat, tenderTypeId.Name, tenderLine.ForeignCurrency);
                        amount = GetFormattedCurrencyValue(tenderLine.AmountInForeignCurrency, tenderLine.ForeignCurrency, context);
                    }

                    // {Credit Card:......$50}
                    reportLayout.AppendLine(ReceiptHelper.FormatTenderLine(tenderName, amount));
                }

                reportLayout.AppendLine();
            }

            /// <summary>
            /// Prepare report header.
            /// </summary>
            /// <param name="reportLayout">The receipt string.</param>
            /// <param name="shift">The current shift.</param>
            /// <param name="receiptType">The receipt type to print.</param>
            /// <param name="context">The request context.</param>
            private static void PrepareHeader(StringBuilder reportLayout, Shift shift, ReceiptType receiptType, RequestContext context)
            {
                // check for nulls
                ThrowIf.Null(context, "requestContext");
                ThrowIf.Null(shift, "shift");

                // get channel configuration
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                ThrowIf.Null(channelConfiguration, "context.GetChannelConfiguration()");

                // convert shift start and end date / time in channel date / time
                string shiftStartDate = string.Empty;
                string shiftStartTime = string.Empty;
                string shiftCloseDate = string.Empty;
                string shiftCloseTime = string.Empty;

                if (shift.StartDateTime.HasValue)
                {
                    // get shift's startdatetime in channel datetimeoffset
                    var shiftChannelStartDateTimeOffset =
                        channelConfiguration.TimeZoneRecords.GetChannelDateTimeOffset(shift.StartDateTime.Value);

                    if (receiptType == ReceiptType.ZReport)
                    {
                        var shiftStartDateTime = shiftChannelStartDateTimeOffset.DateTime;
                        shiftStartDate = shiftStartDateTime.ToString("d");
                        shiftStartTime = shiftStartDateTime.ToString("T");
                    }
                    else
                    {
                        shiftStartDate = shift.StartDateTime.Value.DateTime.ToString("d");
                        shiftStartTime = shift.StartDateTime.Value.DateTime.ToString("T");
                    }
                }

                if (shift.CloseDateTime.HasValue)
                {
                    // get shift's closedatetime in channel datetimeoffset
                    var shiftChannelCloseDateTimeOffset =
                        channelConfiguration.TimeZoneRecords.GetChannelDateTimeOffset(shift.CloseDateTime.Value);

                    var shiftCloseDateTime = shiftChannelCloseDateTimeOffset.DateTime;
                    shiftCloseDate = shiftCloseDateTime.ToString("d");
                    shiftCloseTime = shiftCloseDateTime.ToString("T");
                }

                string staffId = string.Empty;

                reportLayout.AppendLine(SingleLine);

                switch (receiptType)
                {
                    case ReceiptType.XReport:
                        staffId = context.GetPrincipal().UserId;     // for X report, uses the current staff identifier
                        ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6136>");
                        break;

                    case ReceiptType.ZReport:
                        staffId = shift.StaffId;     // for Z report, uses the staff identifier from the RETAILPOSBATCHTABLE
                        ReceiptHelper.AppendReportLine(reportLayout, "<F:string_6137>");
                        break;

                    default:
                        string message = string.Format("Unsupported Report Type '{0}'.", receiptType);
                        throw new NotSupportedException(message);
                }

                // get report's datetime in channel datetimeoffset.
                var reportChannelDateTimeOffset =
                    channelConfiguration.TimeZoneRecords.GetChannelDateTimeOffset(DateTime.Now);

                string reportStartDate = reportChannelDateTimeOffset.DateTime.ToString("d");
                string reportStartTime = reportChannelDateTimeOffset.DateTime.ToString("T");

                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6138>", shift.StoreId, true);
                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6139>", reportStartDate, false);
                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6140>", shift.TerminalId, true);
                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6141>", reportStartTime, false);
                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6142>", staffId, true);
                reportLayout.AppendLine();
                reportLayout.AppendLine();
                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6143>", string.Format(ReceiptService.LineFormat, shift.TerminalId, shift.ShiftId), true);
                reportLayout.AppendLine();

                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6144>", shiftStartDate, true);

                if (receiptType == ReceiptType.ZReport)
                {
                    ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6145>", shiftCloseDate, false);
                }
                else
                {
                    reportLayout.AppendLine();
                }

                ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6146>", shiftStartTime, true);

                if (receiptType == ReceiptType.ZReport)
                {
                    ReceiptHelper.AppendReportHeaderLine(reportLayout, "<F:string_6147>", shiftCloseTime, false);
                }
                else
                {
                    reportLayout.AppendLine();
                }

                reportLayout.AppendLine();
            }

            private static ReadOnlyCollection<Receipt> GetReturnLables(ReadOnlyCollection<TenderType> tenderTypes, ReceiptType receiptType, string receiptLayoutId, ReceiptInfo receiptInfo, SalesOrder salesOrder, string hardwareProfileId, RequestContext context)
            {
                if (receiptInfo == null)
                {
                    throw new ArgumentNullException("receiptInfo");
                }

                List<Receipt> returnLables = new List<Receipt>();

                foreach (SalesLine line in salesOrder.SalesLines)
                {
                    if (line.IsReturnByReceipt || line.Quantity < 0)
                    {
                        Receipt receipt = new Receipt();

                        // Getting a dataset containing the headerpart of the current receipt
                        DataSet ds = ConvertToDataSet(receiptInfo.HeaderTemplate, receiptInfo);
                        receipt.Header += ReadTemplateDataset(ds, tenderTypes, null, salesOrder, context);
                        receiptInfo.HeaderLines = ds.Tables[0].Rows.Count;

                        // Getting a dataset containing the footerpart of the current receipt
                        ds = ConvertToDataSet(receiptInfo.FooterTemplate, receiptInfo);
                        receipt.Footer = ReadTemplateDataset(ds, tenderTypes, null, salesOrder, context);
                        receiptInfo.FooterLines = ds.Tables[0].Rows.Count;

                        for (int i = 0; i < Math.Abs(line.Quantity); ++i)
                        {
                            ds = ConvertToDataSet(receiptInfo.BodyTemplate, receiptInfo);

                            // Getting a dataset containing the linepart of the current receipt
                            receipt.Body = ReadItemDataSet(ds, line, salesOrder, context);
                            receiptInfo.Bodylines = ds.Tables[0].Rows.Count;

                            PopulateReceiptData(receipt, receiptType, receiptLayoutId, receiptInfo, salesOrder, hardwareProfileId, context);
                            returnLables.Add(receipt);
                        }

                        ds.Dispose();
                    }
                }

                return returnLables.AsReadOnly();
            }

            private static Receipt GetReceiptFromTransaction(ReadOnlyCollection<TenderType> tenderTypes, ReceiptType receiptType, string receiptLayoutId, ReceiptInfo receiptInfo, SalesOrder salesOrder, string hardwareProfileId, RequestContext context)
            {
                Receipt receipt = null;
                receipt = GetTransformedTransaction(tenderTypes, receiptInfo, salesOrder, context, receiptType);
                PopulateReceiptData(receipt, receiptType, receiptLayoutId, receiptInfo, salesOrder, hardwareProfileId, context);

                return receipt;
            }

            private static List<Receipt> GetReceiptFromCard(ReadOnlyCollection<TenderType> tenderTypes, ReceiptType receiptType, string receiptLayoutId, ReceiptInfo receiptInfo, SalesOrder salesOrder, GetReceiptServiceRequest request)
            {
                Receipt receipt = null;
                List<Receipt> receipts = new List<Receipt>();

                foreach (var tenderLine in request.TenderLines)
                {
                    if (tenderLine.Status != TenderLineStatus.Committed)
                    {
                        // Ignore invalid tender line.
                        continue;
                    }

                    TenderType tenderType = tenderTypes.Where(type => type.TenderTypeId == tenderLine.TenderTypeId).Single();

                    if (tenderType.OperationId == (int)RetailOperation.PayCard)
                    {
                        receipt = GetTransformedCardTender(receiptInfo, tenderLine, tenderType, salesOrder, request, receiptType);
                        PopulateReceiptData(receipt, receiptType, receiptLayoutId, receiptInfo, salesOrder, GetHardwareProfileId(request), request.RequestContext);
                        receipts.Add(receipt);
                    }
                }

                return receipts;
            }

            private static List<Receipt> GetReceiptFromTenderLine(ReadOnlyCollection<TenderType> tenderTypes, ReceiptType receiptType, string receiptLayoutId, ReceiptInfo receiptInfo, SalesOrder salesOrder, GetReceiptServiceRequest request)
            {
                Receipt receipt = null;
                List<Receipt> receipts = new List<Receipt>();

                foreach (var tenderLine in request.TenderLines)
                {
                    if (tenderLine.Status != TenderLineStatus.Committed)
                    {
                        // Ignore invalid tender line.
                        continue;
                    }

                    IEnumerable<TenderType> tenderTypesForId = tenderTypes.Where(type => type.TenderTypeId == tenderLine.TenderTypeId);

                    if (tenderTypesForId.Count() != 1)
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MoreThanOneTenderTypeForTenderTypeId,
                            string.Format("More than one tender type was returned for tender type id {0}", tenderLine.TenderTypeId));
                    }

                    TenderType tenderType = tenderTypesForId.First();

                    RetailOperation operation = (RetailOperation)tenderType.OperationId;

                    switch (operation)
                    {
                        case RetailOperation.PayCustomerAccount:
                            {
                                receipt = GetTransformedTender(receiptInfo, salesOrder, tenderLine, request);
                                PopulateReceiptData(receipt, receiptType, receiptLayoutId, receiptInfo, salesOrder, GetHardwareProfileId(request), request.RequestContext);
                                receipts.Add(receipt);
                            }

                            break;
                        case RetailOperation.PayCreditMemo:
                            {
                                if (tenderLine.Amount <= 0)
                                {
                                    receipt = GetTransformedTender(receiptInfo, salesOrder, tenderLine, request);
                                    PopulateReceiptData(receipt, receiptType, receiptLayoutId, receiptInfo, salesOrder, GetHardwareProfileId(request), request.RequestContext);
                                    receipts.Add(receipt);
                                }
                            }

                            break;
                        default:
                            // Nothing else needs to be done for other tender types except credit cards which is handled separately. So do nothing.
                            break;
                    }
                }

                return receipts;
            }

            /// <summary>
            /// Gets the hardware profile to print the receipt.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The hardware profile identifier.</returns>
            private static string GetHardwareProfileId(GetReceiptServiceRequest request)
            {
                string hardwareProfileId = request.HardwareProfileId;
                if (string.IsNullOrEmpty(hardwareProfileId))
                {
                    Terminal terminal = request.RequestContext.GetTerminal();
                    hardwareProfileId = terminal.HardwareProfile;
                }

                return hardwareProfileId;
            }

            private static void PopulateReceiptData(Receipt receipt, ReceiptType receiptType, string receiptLayoutId, ReceiptInfo receiptInfo, SalesOrder salesOrder, string hardwareProfileId, RequestContext context)
            {
                // So far, email receipt and receipt for preview are not sending hardware profile id since we do not need to print them.
                if (!string.IsNullOrEmpty(hardwareProfileId))
                {
                    GetPrintersDataRequest getPrintersDataRequest = new GetPrintersDataRequest(context.GetPrincipal().TerminalId.ToString(), receiptLayoutId, receiptType, QueryResultSettings.AllRecords, hardwareProfileId);
                    ReadOnlyCollection<Printer> printersForReceipt = context.Runtime.Execute<EntityDataServiceResponse<Printer>>(getPrintersDataRequest, context).PagedEntityCollection.Results;
                    receipt.Printers = printersForReceipt;
                }
                else
                {
                    receipt.Printers = new List<Printer>();
                }

                receipt.TransactionId = salesOrder.Id;
                receipt.ReceiptId = salesOrder.ReceiptId;
                receipt.ReceiptType = receiptType;
                receipt.ReceiptTitle = receiptInfo.Title;
                receipt.LayoutId = receiptLayoutId;
            }

            /// <summary>
            /// Reads all the sales line in a sales order for receipt printing.
            /// </summary>
            /// <param name="receiptDataset">The dataset.</param>
            /// <param name="theTransaction">The sales order or transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="receiptType">The receipt type.</param>
            /// <returns>The string in printable format.</returns>
            [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "incomeExpenseLine", Justification = "Suppressing to unblock integration.")]
            private static string ReadItemDataSet(DataSet receiptDataset, SalesOrder theTransaction, RequestContext context, ReceiptType receiptType)
            {
                ReceiptItemInfo itemInfo = null;
                StringBuilder lineStringBuilder = new StringBuilder(string.Empty);

                // Go through the sale items and parse each line
                if (theTransaction != null)
                {
                    foreach (SalesLine saleLineItem in theTransaction.SalesLines)
                    {
                        if (receiptType == ReceiptType.GiftReceipt)
                        {
                            ParseSalesItemForGiftReceipt(receiptDataset, out itemInfo, lineStringBuilder, saleLineItem, theTransaction, context);
                        }
                        else
                        {
                            ParseSaleItem(receiptDataset, out itemInfo, lineStringBuilder, saleLineItem, theTransaction, context);
                        }
                    }

                    foreach (IncomeExpenseLine incomeExpenseLine in theTransaction.IncomeExpenseLines)
                    {
                        ParseSaleItem(receiptDataset, out itemInfo, lineStringBuilder, null, theTransaction, context);
                    }

                    if (theTransaction.CustomerAccountDepositLines.Any())
                    {
                        ParseSaleItem(receiptDataset, out itemInfo, lineStringBuilder, null, theTransaction, context);
                    }
                }

                return lineStringBuilder.ToString();
            }

            private static string ReadItemDataSet(DataSet receiptDataset, SalesLine salesLine, SalesOrder theTransaction, RequestContext context)
            {
                ReceiptItemInfo itemInfo = null;
                StringBuilder lineStringBuilder = new StringBuilder(string.Empty);

                // Go through the sale items and parse each line
                if (salesLine != null)
                {
                    ParseSaleItem(receiptDataset, out itemInfo, lineStringBuilder, salesLine, theTransaction, context);
                }

                return lineStringBuilder.ToString();
            }

            /// <summary>
            /// Determines the value of a sales line item variable to be used at the gift receipt.
            /// </summary>
            /// <param name="ds">The dataset.</param>
            /// <param name="itemInfo">Receipt properties.</param>
            /// <param name="lineStringBuilder">The string that contains the receipt variables.</param>
            /// <param name="saleLineItem">The sales line.</param>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="context">The request context.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "By design."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            private static void ParseSalesItemForGiftReceipt(DataSet ds, out ReceiptItemInfo itemInfo, StringBuilder lineStringBuilder, SalesLine saleLineItem, SalesOrder salesOrder, RequestContext context)
            {
                itemInfo = null;

                if (saleLineItem.Quantity > 0 && !saleLineItem.IsGiftCardLine)
                {
                    ParseSaleItem(ds, out itemInfo, lineStringBuilder, saleLineItem, salesOrder, context);
                }
            }

            /// <summary>
            /// Determines the value of a sales line item variable.
            /// </summary>
            /// <param name="ds">The dataset.</param>
            /// <param name="itemInfo">Receipt properties.</param>
            /// <param name="lineStringBuilder">The string that contains the receipt variables.</param>
            /// <param name="saleLineItem">The sales line.</param>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="context">The request context.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "By design."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            private static void ParseSaleItem(DataSet ds, out ReceiptItemInfo itemInfo, StringBuilder lineStringBuilder, SalesLine saleLineItem, SalesOrder salesOrder, RequestContext context)
            {
                string variable = string.Empty;
                string lineId = string.Empty;
                itemInfo = null;

                // Only non-voided items or income/expense lines will be printed
                if (salesOrder.IncomeExpenseLines.Any() || salesOrder.CustomerAccountDepositLines.Any() || (saleLineItem != null && !saleLineItem.IsVoided))
                {
                    DataTable lineTable = ds.Tables.Where(p => p.TableName == "line").Single();
                    DataTable charPosTable;

                    foreach (DataRow dr in lineTable.Rows.Where(p => p != null).OrderBy(p => p["nr"]))
                    {
                        variable = dr["ID"].ToString();
                        switch (variable)
                        {
                            case CRLF:
                                {
                                    lineStringBuilder.Append(Environment.NewLine);
                                    break;
                                }

                            default:
                                {
                                    lineId = dr[LineId].ToString();

                                    if (variable.Equals("PharmacyLine"))
                                    {
                                        /*donothing*/
                                    }
                                    else if (variable.Equals("TotalDiscount") && (salesOrder.IncomeExpenseLines.Any() || salesOrder.CustomerAccountDepositLines.Any() || saleLineItem == null || saleLineItem.TotalDiscount == 0))
                                    {
                                        /*donothing*/
                                    }
                                    else if (variable.Equals("LineDiscount") && (salesOrder.IncomeExpenseLines.Any() || salesOrder.CustomerAccountDepositLines.Any() || saleLineItem == null || saleLineItem.LineDiscount == 0))
                                    {
                                        /*donothing*/
                                    }
                                    else if (variable.Equals("PeriodicDiscount") && (salesOrder.IncomeExpenseLines.Any() || salesOrder.CustomerAccountDepositLines.Any() || saleLineItem == null || saleLineItem.PeriodicDiscount == 0))
                                    {
                                        /*donothing*/
                                    }
                                    else if (variable.Equals("Dimension")
                                        && (salesOrder.IncomeExpenseLines.Any()
                                        || salesOrder.CustomerAccountDepositLines.Any()
                                        || (saleLineItem == null || saleLineItem.Variant == null ||
                                          (string.IsNullOrEmpty(saleLineItem.Variant.ColorId)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.Color)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.SizeId)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.Size)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.StyleId)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.Style)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.ConfigId)
                                        && string.IsNullOrEmpty(saleLineItem.Variant.Configuration)))))
                                    {
                                        /*donothing*/
                                    }
                                    else if (variable.Equals("Comment")
                                        && (salesOrder.IncomeExpenseLines.Any() 
                                        || (salesOrder.CustomerAccountDepositLines.Any() && string.IsNullOrEmpty(salesOrder.CustomerAccountDepositLines.SingleOrDefault().Comment))
                                        || (saleLineItem != null && string.IsNullOrEmpty(FormatLineCommentWithReasonCodes(saleLineItem, context)))))
                                    {
                                        /*donothing, FormatLineCommentWithReasonCodes() return [salesline.comment + Reason code string] if any*/
                                    }
                                    else if (variable.Equals("KitComponentName"))
                                    {
                                        DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();

                                        // Only parse kit components if functionality profile indicates that components should be printed
                                        if (deviceConfiguration.IncludeKitComponents && saleLineItem != null)
                                        {
                                            // Get the product entity for the sale line item
                                            List<long> productIds = new List<long>();
                                            productIds.Add(saleLineItem.ProductId);
                                            var request = new GetProductsServiceRequest(context.GetPrincipal().ChannelId, productIds, QueryResultSettings.SingleRecord);
                                            SimpleProduct product = context.Runtime.Execute<GetProductsServiceResponse>(request, context).Products.SingleOrDefault();

                                            // Parse kit variables if item is a kit and if functionality profile indicates that components should be printed
                                            if (product != null && product.ProductType == ProductType.KitVariant)
                                            {
                                                // For each component in the kit, parse the kit component data and append the line data to lineStringBuilder
                                                foreach (ProductComponent component in product.Components)
                                                {
                                                    ParseKitComponent(salesOrder, lineStringBuilder, ds, lineTable, lineId, component, salesOrder.IsTaxIncludedInPrice, context);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // options for idVariable:
                                        // Itemlines
                                        // TotalDiscount
                                        // LineDiscount
                                        charPosTable = ds.Tables.Where(p => p.TableName == CharPos).Single();
                                        if (charPosTable != null)
                                        {
                                            int nextCharNr = 1;
                                            foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                                            {
                                                itemInfo = new ReceiptItemInfo(row);

                                                // Adding possible whitespace at the beginning of line
                                                lineStringBuilder.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                                                if (itemInfo.FontStyle == FontStyle.Bold)
                                                {
                                                    lineStringBuilder.Append(Esc + DoubleSpace);
                                                }
                                                else
                                                {
                                                    lineStringBuilder.Append(Esc + SingleSpace);
                                                }

                                                // Parsing the itemInfo
                                                string itemVariable = ParseItemVariable(salesOrder, itemInfo, saleLineItem, salesOrder.IsTaxIncludedInPrice, context);
                                                lineStringBuilder.Append(itemVariable);

                                                // Closing the string with a single space command to make sure spaces are always single spaced
                                                lineStringBuilder.Append(Esc + SingleSpace);

                                                // Specifying the position of the next char in the current line - bold take twice as much space
                                                nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                                            }
                                        }

                                        lineStringBuilder.Append(Environment.NewLine);
                                    }

                                    break;
                                }
                        }
                    }
                }
            }

            /// <summary>
            /// Parses a kit component for printing.
            /// </summary>
            /// <param name="order">The sales transaction.</param>
            /// <param name="lineStringBuilder">The string that contains the receipt variables.</param>
            /// <param name="ds">The dataset.</param>
            /// <param name="lineTable">The line table.</param>
            /// <param name="lineId">The line id.</param>
            /// <param name="component">The kit component.</param>
            /// <param name="isTaxIncludedInPrice">Indicates whether the tax is included in the price.</param>
            /// <param name="context">The request context.</param>
            private static void ParseKitComponent(SalesOrder order, StringBuilder lineStringBuilder, DataSet ds, DataTable lineTable, string lineId, ProductComponent component, bool isTaxIncludedInPrice, RequestContext context)
            {
                DataTable charPosTable = ds.Tables.Where(p => p.TableName == CharPos).Single();
                if (component != null && charPosTable != null)
                {
                    // Format and print each field in the line being printed
                    int nextCharNr = 1;
                    foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                    {
                        ReceiptItemInfo itemInfo = new ReceiptItemInfo(row);

                        // Adding possible whitespace at the beginning of line
                        lineStringBuilder.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                        // Formatting for different font styles
                        if (itemInfo.FontStyle == FontStyle.Bold)
                        {
                            lineStringBuilder.Append(Esc + DoubleSpace);
                        }
                        else
                        {
                            lineStringBuilder.Append(Esc + SingleSpace);
                        }

                        // Create a sale item to store kit component information that may be printed
                        // this item gets passed to ParseItemVariable to parse component fields in the same manner as other line fields
                        SalesLine componentItem = new SalesLine();
                        componentItem.ItemId = component.ItemId;
                        componentItem.Quantity = component.Quantity;
                        componentItem.SalesOrderUnitOfMeasure = component.UnitOfMeasure;

                        // Parsing the itemInfo for kit component field information
                        lineStringBuilder.Append(ParseItemVariable(order, itemInfo, componentItem, isTaxIncludedInPrice, context));

                        // Closing the string with a single space command to make sure spaces are always single spaced
                        lineStringBuilder.Append(Esc + SingleSpace);

                        // Specifying the position of the next char in the current line - bold take twice as much space
                        nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                    }
                }

                lineStringBuilder.Append(Environment.NewLine);
            }

            private static string ParseCardTenderVariable(ReceiptItemInfo itemInfo, TenderLine eftInfo, SalesOrder theTransaction, GetReceiptServiceRequest request, GetCardPaymentPropertiesServiceResponse cardPaymentPropertiesResponse)
            {
                string tmpString;

                if (itemInfo.IsVariable)
                {
                    tmpString = GetInfoFromTransaction(itemInfo, eftInfo, theTransaction, request.RequestContext, cardPaymentPropertiesResponse);
                }
                else
                {
                    tmpString = itemInfo.ValueString;
                }

                tmpString = tmpString ?? string.Empty;

                tmpString = GetAlignmentSettings(tmpString, itemInfo);

                return tmpString;
            }

            /// <summary>
            /// For a tender template variable return the data for the receipt.
            /// </summary>
            /// <param name="itemInfo">The template properties.</param>
            /// <param name="tenderLineItem">The tender line.</param>
            /// <param name="theTransaction">The sales order.</param>
            /// <param name="request">The request parameter.</param>
            /// <returns>The value for the template variable.</returns>
            private static string ParseTenderVariable(ReceiptItemInfo itemInfo, TenderLine tenderLineItem, SalesOrder theTransaction, GetReceiptServiceRequest request)
            {
                string tmpString;

                if (itemInfo.IsVariable)
                {
                    tmpString = GetInfoFromTransaction(itemInfo, tenderLineItem, theTransaction, request.RequestContext, null);
                }
                else
                {
                    tmpString = itemInfo.ValueString;
                }

                tmpString = tmpString ?? string.Empty;
                tmpString = GetAlignmentSettings(tmpString, itemInfo);

                return tmpString;
            }

            private static string ParseTenderLineVariable(ReadOnlyCollection<TenderType> tenderTypes, ReceiptItemInfo itemInfo, TenderLine tenderLineItem, RequestContext context)
            {
                string templateVariable = GetInfoFromTenderLineItem(tenderTypes, itemInfo, tenderLineItem, context);

                // Setting the align if necessary
                return GetAlignmentSettings(templateVariable, itemInfo);
            }

            /// <summary>
            /// Determines if a sales line item is a variable for receipt printing.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="itemInfo">The receipt properties.</param>
            /// <param name="saleLineItem">The sales line.</param>
            /// <param name="isTaxIncludedInPrice">Indicates whether the tax is included in price of the sale line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with the data to print.</returns>
            private static string ParseItemVariable(SalesOrder salesOrder, ReceiptItemInfo itemInfo, SalesLine saleLineItem, bool isTaxIncludedInPrice, RequestContext context)
            {
                string parsedString;

                if (itemInfo.IsVariable)
                {
                    parsedString = GetInfoFromSaleLineItem(salesOrder, itemInfo, saleLineItem, isTaxIncludedInPrice, context);
                }
                else
                {
                    parsedString = itemInfo.ValueString;
                }

                parsedString = parsedString ?? string.Empty;
                parsedString = GetAlignmentSettings(parsedString, itemInfo);

                return parsedString;
            }

            /// <summary>
            /// Returns transformed card tender as string.
            /// </summary>
            /// <param name="receiptInfo">The receipt info.</param>
            /// <param name="eftInfo">The tender line.</param>
            /// <param name="tenderType">The tender type.</param>
            /// <param name="theTransaction">The sales order or transaction.</param>
            /// <param name="request">The request parameter.</param>
            /// <param name="receiptType">The receipt type parameter.</param>
            /// <returns>The formatted receipt.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Xml", "CA3053:UseXmlSecureResolver",
                        Justification = @"For the call to XmlReader.Create() below, CA3053 recommends setting the
XmlReaderSettings.XmlResolver property to either null or an instance of XmlSecureResolver.
However, the said XmlResolver property no longer exists in .NET portable framework (i.e. core framework) which means there is no way to set it.
So we suppress this error until the reporting for CA3053 has been updated to account for .NET portable framework.")]
            private static Receipt GetTransformedCardTender(ReceiptInfo receiptInfo, TenderLine eftInfo, TenderType tenderType, SalesOrder theTransaction, GetReceiptServiceRequest request, ReceiptType receiptType)
            {
                if (receiptInfo == null)
                {
                    throw new ArgumentNullException("receiptInfo");
                }

                // Get the Authorization Properties For Receipt if present
                GetCardPaymentPropertiesServiceResponse cardPaymentPropertiesResponse = null;
                StringBuilder returnString = new StringBuilder();
                StringBuilder externalReceiptString = new StringBuilder();
                Receipt receipt = new Receipt();
                bool hasExternalReceipt = false;

                if (eftInfo != null && !string.IsNullOrWhiteSpace(eftInfo.Authorization))
                {
                    GetCardPaymentPropertiesServiceRequest cardPaymentPropertiesRequest = new GetCardPaymentPropertiesServiceRequest(eftInfo.Authorization);
                    cardPaymentPropertiesResponse = request.RequestContext.Execute<GetCardPaymentPropertiesServiceResponse>(cardPaymentPropertiesRequest);

                    // check for external receipt
                    if (cardPaymentPropertiesResponse != null && !string.IsNullOrEmpty(cardPaymentPropertiesResponse.ExternalReceipt))
                    {
                        StringReader sr = new StringReader(cardPaymentPropertiesResponse.ExternalReceipt);
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.DtdProcessing = DtdProcessing.Prohibit;
                        XmlReader reader = XmlReader.Create(sr, settings);

                        string receiptTypeAttribute;

                        switch (receiptType)
                        {
                            case ReceiptType.CardReceiptForShopReturn:
                            case ReceiptType.CardReceiptForShop:
                                receiptTypeAttribute = PaymentSDK.ExternalReceiptConstants.ReceiptTypeMerchant;
                                break;

                            case ReceiptType.CardReceiptForCustomer:
                            case ReceiptType.CardReceiptForCustomerReturn:
                                receiptTypeAttribute = PaymentSDK.ExternalReceiptConstants.ReceiptTypeCustomer;
                                break;
                            default:
                                receiptTypeAttribute = string.Empty;
                                break;
                        }

                        bool readLines = false;

                        while (reader.Read())
                        {
                            if (readLines)
                            {
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    externalReceiptString.AppendLine(reader.Value);
                                }
                                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals(PaymentSDK.ExternalReceiptConstants.Receipt, StringComparison.OrdinalIgnoreCase))
                                {
                                    readLines = false;
                                }
                            }

                            if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(PaymentSDK.ExternalReceiptConstants.Receipt, StringComparison.OrdinalIgnoreCase) && reader.GetAttribute(PaymentSDK.ExternalReceiptConstants.TypeName).Equals(receiptTypeAttribute, StringComparison.OrdinalIgnoreCase))
                            {
                                readLines = true;
                            }
                        }

                        if (externalReceiptString.Length != 0)
                        {
                            hasExternalReceipt = true;
                        }
                    }
                }

                DataSet ds = null;

                // Getting a dataset containing the headerpart of the current receipt
                ds = ConvertToDataSet(receiptInfo.HeaderTemplate, receiptInfo);
                returnString.Append(ReadCardTenderDataSet(ds, eftInfo, theTransaction, tenderType, request, cardPaymentPropertiesResponse));
                receipt.Header = ReadCardTenderDataSet(ds, eftInfo, theTransaction, tenderType, request, cardPaymentPropertiesResponse);

                if (!hasExternalReceipt)
                {
                    // Getting a dataset containing the footerpart of the current receipt
                    ds = ConvertToDataSet(receiptInfo.BodyTemplate, receiptInfo);
                    returnString.Append(ReadCardTenderDataSet(ds, eftInfo, theTransaction, tenderType, request, cardPaymentPropertiesResponse));
                    receipt.Body = ReadCardTenderDataSet(ds, eftInfo, theTransaction, tenderType, request, cardPaymentPropertiesResponse);
                }
                else
                {
                    string externalReceipt = externalReceiptString.ToString();
                    returnString.Append(externalReceipt);
                    receipt.Body = externalReceipt;
                }

                // Getting a dataset containing the footerpart of the current receipt
                ds = ConvertToDataSet(receiptInfo.FooterTemplate, receiptInfo);
                returnString.Append(ReadCardTenderDataSet(ds, eftInfo, theTransaction, tenderType, request, cardPaymentPropertiesResponse));
                receipt.Footer = ReadCardTenderDataSet(ds, eftInfo, theTransaction, tenderType, request, cardPaymentPropertiesResponse);

                return receipt;
            }

            /// <summary>
            /// Gets the transformed receipt from a transaction.
            /// </summary>
            /// <param name="tenderTypes">The tender types.</param>
            /// <param name="receiptInfo">The receipt properties.</param>
            /// <param name="theTransaction">The sales order or transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="receiptType">The receipt type.</param>
            /// <returns>The receipt used for printing.</returns>
            private static Receipt GetTransformedTransaction(ReadOnlyCollection<TenderType> tenderTypes, ReceiptInfo receiptInfo, SalesOrder theTransaction, RequestContext context, ReceiptType receiptType)
            {
                if (receiptInfo == null)
                {
                    throw new ArgumentNullException("receiptInfo");
                }

                Receipt receipt = new Receipt();

                DataSet ds = ConvertToDataSet(receiptInfo.HeaderTemplate, receiptInfo);

                // Getting a dataset containing the headerpart of the current receipt
                receipt.Header += ReadTemplateDataset(ds, tenderTypes, null, theTransaction, context);
                if (ds.Tables == null || ds.Tables.Count == 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidReceiptTemplate);
                }

                receiptInfo.HeaderLines = ds.Tables[0].Rows.Count;

                ds = ConvertToDataSet(receiptInfo.BodyTemplate, receiptInfo);

                // Getting a dataset containing the linepart of the current receipt
                receipt.Body = ReadItemDataSet(ds, theTransaction, context, receiptType);
                receiptInfo.Bodylines = ds.Tables[0].Rows.Count;

                ds = ConvertToDataSet(receiptInfo.FooterTemplate, receiptInfo);

                // Getting a dataset containing the footerpart of the current receipt
                receipt.Footer = ReadTemplateDataset(ds, tenderTypes, null, theTransaction, context);
                receiptInfo.FooterLines = ds.Tables[0].Rows.Count;

                return receipt;
            }

            /// <summary>
            /// Gets the receipt object when payment is made using a customer account or credit memo.
            /// </summary>
            /// <param name="receiptInfo">The receipt properties.</param>
            /// <param name="theTransaction">The sales order.</param>
            /// <param name="tenderLineItem">The tender line.</param>
            /// <param name="request">The request parameter.</param>
            /// <returns>The receipt used for printing.</returns>
            private static Receipt GetTransformedTender(ReceiptInfo receiptInfo, SalesOrder theTransaction, TenderLine tenderLineItem, GetReceiptServiceRequest request)
            {
                if (receiptInfo == null)
                {
                    throw new ArgumentNullException("receiptInfo");
                }

                Receipt receipt = new Receipt();

                DataSet ds = ConvertToDataSet(receiptInfo.HeaderTemplate, receiptInfo);

                // Getting a dataset containing the headerpart of the current form
                receipt.Header += ReadTenderDataSet(ds, tenderLineItem, theTransaction, request);

                ds = ConvertToDataSet(receiptInfo.FooterTemplate, receiptInfo);

                // Getting a dataset containing the footerpart of the current form
                receipt.Footer += ReadTenderDataSet(ds, tenderLineItem, theTransaction, request);

                return receipt;
            }

            /// <summary>
            /// Determines if a tag is a variable for receipt or not.
            /// </summary>
            /// <param name="itemInfo">The receipt properties.</param>
            /// <param name="tenderItem">The tender line.</param>
            /// <param name="theTransaction">The sales order or transaction.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with printable data.</returns>
            private static string ParseVariable(ReceiptItemInfo itemInfo, TenderLine tenderItem, SalesOrder theTransaction, RequestContext context)
            {
                string tmpString;

                if (itemInfo.IsVariable)
                {
                    tmpString = GetInfoFromTransaction(itemInfo, tenderItem, theTransaction, context, null);
                }
                else
                {
                    tmpString = itemInfo.ValueString;
                }

                tmpString = tmpString ?? string.Empty;
                tmpString = GetAlignmentSettings(tmpString, itemInfo);

                return tmpString;
            }

            private static string ReadTenderDataSet(DataSet ds, TenderLine tenderLineItem, SalesOrder theTransaction, GetReceiptServiceRequest request)
            {
                string returnString = string.Empty;

                DataTable lineTable = ds.Tables.Where(p => p.TableName == "line").Single();
                DataTable table;
                ReceiptItemInfo itemInfo = null;

                if (lineTable != null)
                {
                    foreach (DataRow dr in lineTable.Rows.Where(p => p != null).OrderBy(p => p["nr"]))
                    {
                        string lineString = string.Empty;
                        string variable = (string)dr["ID"];

                        switch (variable)
                        {
                            case CRLF:
                                lineString += Environment.NewLine;
                                break;
                            case Text:
                                string lineId = dr[LineId].ToString();
                                table = ds.Tables.Where(p => p.TableName == CharPos).Single();
                                if (table != null)
                                {
                                    int nextChar = 1;
                                    foreach (DataRow row in table.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                                    {
                                        try
                                        {
                                            itemInfo = new ReceiptItemInfo(row);

                                            // Adding possible whitespace at the beginning of line
                                            lineString += CreateWhitespace(' ', itemInfo.CharIndex - nextChar);

                                            if (itemInfo.FontStyle == FontStyle.Bold)
                                            {
                                                lineString += Esc + DoubleSpace;
                                            }
                                            else
                                            {
                                                lineString += Esc + SingleSpace;
                                            }

                                            // Parsing the itemInfo
                                            lineString += ParseTenderVariable(itemInfo, tenderLineItem, theTransaction, request);

                                            // Closing the string with a single space command to make sure spaces are always single spaced
                                            lineString += Esc + SingleSpace;

                                            // Specifying the position of the next char in the current line - bold take twice as much space
                                            nextChar = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }

                                lineString += Environment.NewLine;
                                break;
                        }

                        returnString += lineString;
                    }
                }

                return returnString.ToString();
            }

            private static string ReadCardTenderDataSet(DataSet ds, TenderLine eftInfo, SalesOrder theTransaction, TenderType tenderType, GetReceiptServiceRequest request, GetCardPaymentPropertiesServiceResponse cardPaymentPropertiesResponse)
            {
                string returnString = string.Empty;

                // Note: Receipt templates are persisted as serialized DataTable objects. Don't change the case
                // of these tables/fields for backward compatibility.
                DataTable lineTable = ds.Tables.Where(p => p.TableName == "line").Single();
                DataTable charPosTable;
                ReceiptItemInfo itemInfo = null;

                if (lineTable != null)
                {
                    foreach (DataRow dr in lineTable.Rows.Where(p => p != null).OrderBy(p => p["nr"]))
                    {
                        string lineString = string.Empty;
                        string idVariable = (string)dr["ID"];

                        if (idVariable == CRLF)
                        {
                            lineString += Environment.NewLine;
                        }
                        else if ((idVariable == "CardHolderSignature") && (tenderType.OperationId != (int)RetailOperation.PayCard))
                        {
                            // Skip card holder signature line for other than Credit Cards.
                        }
                        else
                        {
                            string lineId = dr[LineId].ToString();
                            charPosTable = ds.Tables.Where(p => p.TableName == CharPos).Single();
                            if (charPosTable != null)
                            {
                                int nextCharNr = 1;
                                foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                                {
                                    try
                                    {
                                        itemInfo = new ReceiptItemInfo(row);

                                        // Adding possible whitespace at the beginning of line
                                        lineString += CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr);

                                        if (itemInfo.FontStyle == FontStyle.Bold)
                                        {
                                            lineString += Esc + DoubleSpace;
                                        }
                                        else
                                        {
                                            lineString += Esc + SingleSpace;
                                        }

                                        // Parsing the itemInfo
                                        lineString += ParseCardTenderVariable(itemInfo, eftInfo, theTransaction, request, cardPaymentPropertiesResponse);

                                        // Closing the string with a single space command to make sure spaces are always single spaced
                                        lineString += Esc + SingleSpace;

                                        // Specifying the position of the next char in the current line - bold take twice as much space
                                        nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }

                            lineString += Environment.NewLine;
                        }

                        returnString += lineString;
                    }
                }

                return returnString.ToString();
            }

            /// <summary>
            /// Reads the dataset for validations.
            /// </summary>
            /// <param name="templateDataset">The receipt template as a dataset.</param>
            /// <param name="tenderTypes">The tender types.</param>
            /// <param name="tenderItem">The tender line.</param>
            /// <param name="theTransaction">The sales order or transaction.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with printable data.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode", Justification = "To be refactored.")]
            private static string ReadTemplateDataset(DataSet templateDataset, ReadOnlyCollection<TenderType> tenderTypes, TenderLine tenderItem, SalesOrder theTransaction, RequestContext context)
            {
                StringBuilder tempString = new StringBuilder();
                DataTable lineTable = templateDataset.Tables.Where(p => p.TableName == "line").Single();

                ReceiptItemInfo itemInfo = null;

                // foreach (DataRow dr in lineTable.Rows)
                foreach (DataRow dr in lineTable.Rows.Where(p => p != null).OrderBy(p => p["nr"]))
                {
                    string idVariable = dr["ID"].ToString();
                    string lineId = string.Empty;
                    DataTable charPosTable;
                    switch (idVariable)
                    {
                        case CRLF:
                            tempString.Append(Environment.NewLine);
                            break;
                        case Text:
                            lineId = dr[LineId].ToString();
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == "charpos").Single();
                            if (charPosTable != null)
                            {
                                int nextCharNr = 1;

                                foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                                {
                                    try
                                    {
                                        itemInfo = new ReceiptItemInfo(row);

                                        // Adding possible whitespace at the beginning of line
                                        tempString.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                                        // Parsing the itemInfo
                                        if (itemInfo.FontStyle == FontStyle.Bold)
                                        {
                                            tempString.Append(Esc + DoubleSpace);
                                        }
                                        else
                                        {
                                            tempString.Append(Esc + SingleSpace);
                                        }

                                        tempString.Append(ParseVariable(itemInfo, tenderItem, theTransaction, context));

                                        // Closing the string with a single space command to make sure spaces are always single spaced
                                        tempString.Append(Esc + SingleSpace);

                                        // Specifying the position of the next char in the current line - bold take twice as much space
                                        nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }

                            tempString.Append(Environment.NewLine);
                            break;
                        case "Tenders":
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();
                            lineId = dr[LineId].ToString();

                            if (charPosTable != null)
                            {
                                foreach (var tenderLineItem in theTransaction.TenderLines)
                                {
                                    if (tenderLineItem.Status == TenderLineStatus.Committed)
                                    {
                                        int nextCharNr = 1;
                                        foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                                        {
                                            try
                                            {
                                                itemInfo = new ReceiptItemInfo(row);

                                                // If tender is a Change Back tender, then a carriage return is put in front of the next line
                                                if ((tenderLineItem.Amount < 0) && (nextCharNr == 1))
                                                {
                                                    tempString.Append(Environment.NewLine);
                                                }

                                                // Adding possible whitespace at the beginning of line
                                                tempString.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                                                if (itemInfo.FontStyle == FontStyle.Bold)
                                                {
                                                    tempString.Append(Esc + DoubleSpace);
                                                }
                                                else
                                                {
                                                    tempString.Append(Esc + SingleSpace);
                                                }

                                                // Parsing the itemInfo
                                                tempString.Append(ParseTenderLineVariable(tenderTypes, itemInfo, tenderLineItem, context));

                                                // Closing the string with a single space command to make sure spaces are always single spaced
                                                tempString.Append(Esc + SingleSpace);

                                                // Specifying the position of the next char in the current line - bold take twice as much space
                                                nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                                            }
                                            catch (Exception)
                                            {
                                            }
                                        }

                                        tempString.Append(Environment.NewLine);
                                    }
                                }
                            }

                            break;

                        case Taxes:
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();
                            tempString.Append(ParseTax(charPosTable, lineTable, dr[LineId].ToString(), theTransaction, context));
                            break;

                        case LoyaltyItem:
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();
                            if (charPosTable != null && !string.IsNullOrEmpty(theTransaction.LoyaltyCardId))
                            {
                                tempString.Append(ParseLoyaltyText(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), tenderItem, context));
                            }

                            break;

                        case LoyaltyEarnText:
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();

                            // check if there are loyalty card and earned reward lines with redeemable points
                            if (charPosTable != null
                                && theTransaction.LoyaltyRewardPointLines != null
                                && theTransaction.LoyaltyRewardPointLines.Any(l => (l.EntryType == LoyaltyRewardPointEntryType.Earn || l.EntryType == LoyaltyRewardPointEntryType.ReturnEarned) && l.RewardPointIsRedeemable))
                            {
                                tempString.Append(ParseLoyaltyText(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), tenderItem, context));
                            }

                            break;

                        case LoyaltyRedeemText:
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();

                            // check if there are loyalty card and redeemed reward lines
                            if (charPosTable != null
                                && theTransaction.LoyaltyRewardPointLines != null
                                && theTransaction.LoyaltyRewardPointLines.Any(l => l.EntryType == LoyaltyRewardPointEntryType.Redeem || l.EntryType == LoyaltyRewardPointEntryType.Refund))
                            {
                                tempString.Append(ParseLoyaltyText(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), tenderItem, context));
                            }

                            break;

                        case LoyaltyEarnLines:
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();
                            if (charPosTable != null)
                            {
                                tempString.Append(ParseLoyaltyLines(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), LoyaltyRewardPointEntryType.ReturnEarned, context));
                                tempString.Append(ParseLoyaltyLines(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), LoyaltyRewardPointEntryType.Earn, context));
                            }

                            break;

                        case LoyaltyRedeemLines:
                            charPosTable = templateDataset.Tables.Where(p => p.TableName == CharPos).Single();
                            if (charPosTable != null)
                            {
                                tempString.Append(ParseLoyaltyLines(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), LoyaltyRewardPointEntryType.Refund, context));
                                tempString.Append(ParseLoyaltyLines(theTransaction, charPosTable, lineTable, dr[LineId].ToString(), LoyaltyRewardPointEntryType.Redeem, context));
                            }

                            break;
                    }
                }

                return tempString.ToString();
            }

            /// <summary>
            /// Parses the tax related receipt entries.
            /// </summary>
            /// <param name="charPosTable">The character position table.</param>
            /// <param name="lineTable">The line table.</param>
            /// <param name="lineId">The line identifier.</param>
            /// <param name="salesOrder">The sales order object.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The generated receipt string.</returns>
            private static string ParseTax(DataTable charPosTable, DataTable lineTable, string lineId, SalesOrder salesOrder, RequestContext context)
            {
                StringBuilder tempString = new StringBuilder();
                ReceiptItemInfo itemInfo = null;

                if (context.GetChannelConfiguration().CountryRegionISOCode == CountryRegionISOCode.IN)
                {
                    GetTaxSummarySettingIndiaDataRequest getTaxSummarySettingIndiaDataRequest = new GetTaxSummarySettingIndiaDataRequest(QueryResultSettings.SingleRecord);
                    TaxSummarySettingIndia taxSummarySettingIndia = context.Runtime.Execute<SingleEntityDataServiceResponse<TaxSummarySettingIndia>>(getTaxSummarySettingIndiaDataRequest, context).Entity;
                    IndiaReceiptServiceHelper.PopulateTaxSummaryForIndia(salesOrder, taxSummarySettingIndia);
                }

                if (charPosTable != null)
                {
                    foreach (TaxLine taxItem in salesOrder.TaxLines)
                    {
                        int nextCharNr = 1;
                        foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                        {
                            itemInfo = new ReceiptItemInfo(row);

                            // Adding possible whitespace at the beginning of line
                            tempString.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                            tempString.Append(Esc + (itemInfo.FontStyle == FontStyle.Bold ? DoubleSpace : SingleSpace));

                            // Parsing the itemInfo
                            tempString.Append(ParseTaxVariable(itemInfo, taxItem, context));

                            // Closing the string with a single space command to make sure spaces are always single spaced
                            tempString.Append(Esc + SingleSpace);

                            // Specifing the position of the next char in the current line - bold take twice as much space
                            nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                        }

                        tempString.Append(Environment.NewLine);
                    }
                }

                return tempString.ToString();
            }

            /// <summary>
            /// Determines if a tax line item is a variable for receipt printing.
            /// </summary>
            /// <param name="itemInfo">The receipt properties.</param>
            /// <param name="taxItem">The tax line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with the data to print.</returns>
            private static string ParseTaxVariable(ReceiptItemInfo itemInfo, TaxLine taxItem, RequestContext context)
            {
                string tmpString = string.Empty;

                if (itemInfo.IsVariable)
                {
                    tmpString = GetInfoFromTaxItem(itemInfo, taxItem, context);
                }
                else
                {
                    tmpString = itemInfo.ValueString;
                }

                if (tmpString == null)
                {
                    tmpString = string.Empty;
                }

                return ReceiptService.GetAlignmentSettings(tmpString, itemInfo);
            }

            /// <summary>
            /// Returns the receipt data for a loyalty text.
            /// </summary>
            /// <param name="theTransaction">The sales order.</param>
            /// <param name="charPosTable">The char Pos data table.</param>
            /// <param name="lineTable">The line data table.</param>
            /// <param name="lineId">The line identifier.</param>
            /// <param name="tenderItem">The tender line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with the receipt data to print.</returns>
            private static string ParseLoyaltyText(SalesOrder theTransaction, DataTable charPosTable, DataTable lineTable, string lineId, TenderLine tenderItem, RequestContext context)
            {
                StringBuilder tempString = new StringBuilder();

                int nextCharNr = 1;
                foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                {
                    ReceiptItemInfo itemInfo = new ReceiptItemInfo(row);

                    // Adding possible whitespace at the beginning of line
                    tempString.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                    tempString.Append(Esc + (itemInfo.FontStyle == FontStyle.Bold ? DoubleSpace : SingleSpace));

                    // Parsing the itemInfo
                    tempString.Append(ParseVariable(itemInfo, tenderItem, theTransaction, context));

                    // Closing the string with a single space command to make sure spaces are always single spaced
                    tempString.Append(Esc + SingleSpace);

                    // Specifying the position of the next char in the current line - bold take twice as much space
                    nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                }

                tempString.Append(Environment.NewLine);

                return tempString.ToString();
            }

            /// <summary>
            /// Returns the receipt data for a loyalty reward point line.
            /// </summary>
            /// <param name="theTransaction">The sales order.</param>
            /// <param name="charPosTable">The char Pos data table.</param>
            /// <param name="lineTable">The line data table.</param>
            /// <param name="lineId">The line identifier.</param>
            /// <param name="entryType">The loyalty entry type.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with the receipt data to print.</returns>
            private static string ParseLoyaltyLines(SalesOrder theTransaction, DataTable charPosTable, DataTable lineTable, string lineId, LoyaltyRewardPointEntryType entryType, RequestContext context)
            {
                StringBuilder tempString = new StringBuilder();

                // Grouping reward point lines
                IEnumerable<LoyaltyRewardPointLine> groupedRewardPointLines =
                    from l in theTransaction.LoyaltyRewardPointLines
                    where l.EntryType == entryType && l.RewardPointIsRedeemable
                    group l by new { l.RewardPointRecordId, l.RewardPointId, l.RewardPointType, l.RewardPointIsRedeemable, l.RewardPointCurrency }
                        into g
                    select new LoyaltyRewardPointLine
                    {
                        RewardPointRecordId = g.Key.RewardPointRecordId,
                        RewardPointId = g.Key.RewardPointId,
                        RewardPointType = g.Key.RewardPointType,
                        RewardPointIsRedeemable = g.Key.RewardPointIsRedeemable,
                        RewardPointCurrency = g.Key.RewardPointCurrency,
                        RewardPointAmountQuantity = g.Sum(a => a.RewardPointAmountQuantity)
                    };

                // iterate over loyalty reward points
                foreach (LoyaltyRewardPointLine rewardPointLine in groupedRewardPointLines)
                {
                    int nextCharNr = 1;
                    foreach (DataRow row in charPosTable.Rows.Where(p => p[lineTable.TableName + "_id"].ToString() == lineId).OrderBy(p => p["nr"]))
                    {
                        ReceiptItemInfo itemInfo = new ReceiptItemInfo(row);

                        // Adding possible whitespace at the beginning of line
                        tempString.Append(CreateWhitespace(' ', itemInfo.CharIndex - nextCharNr));

                        tempString.Append(Esc + (itemInfo.FontStyle == FontStyle.Bold ? DoubleSpace : SingleSpace));

                        // Parsing the itemInfo
                        tempString.Append(ParseLoyaltyRewardPointLine(itemInfo, rewardPointLine, entryType, context));

                        // Closing the string with a single space command to make sure spaces are always single spaced
                        tempString.Append(Esc + SingleSpace);

                        // Specifying the position of the next char in the current line - bold take twice as much space
                        nextCharNr = itemInfo.CharIndex + (itemInfo.Length * itemInfo.SizeFactor);
                    }

                    tempString.Append(Environment.NewLine);
                }

                return tempString.ToString();
            }

            /// <summary>
            /// Returns the receipt data for a loyalty reward point line.
            /// </summary>
            /// <param name="itemInfo">The receipt item information.</param>
            /// <param name="rewardPointLine">The reward point line.</param>
            /// <param name="entryType">The loyalty entry type.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string with the receipt data to print.</returns>
            private static string ParseLoyaltyRewardPointLine(ReceiptItemInfo itemInfo, LoyaltyRewardPointLine rewardPointLine, LoyaltyRewardPointEntryType entryType, RequestContext context)
            {
                string tempString = string.Empty;

                if (rewardPointLine != null)
                {
                    if (!itemInfo.IsVariable)
                    {
                        tempString = itemInfo.ValueString;
                    }
                    else
                    {
                        switch (itemInfo.Variable.ToUpperInvariant())
                        {
                            case EARNEDREWARDPOINTID:
                            case REDEEMEDREWARDPOINTID:
                                tempString = rewardPointLine.RewardPointId;
                                break;
                            case EARNEDREWARDPOINTAMOUNTQUANTITY:
                            case REDEEMEDREWARDPOINTAMOUNTQUANTITY:
                                decimal rewardPoints = rewardPointLine.RewardPointAmountQuantity;
                                if (entryType == LoyaltyRewardPointEntryType.Redeem || entryType == LoyaltyRewardPointEntryType.Refund)
                                {
                                    rewardPoints = rewardPoints * -1;
                                }

                                switch (rewardPointLine.RewardPointType)
                                {
                                    case LoyaltyRewardPointType.Quantity:
                                        tempString = string.Format("{0:0}", rewardPoints);
                                        break;
                                    case LoyaltyRewardPointType.Amount:
                                        tempString = GetFormattedCurrencyValue(rewardPoints, rewardPointLine.RewardPointCurrency, context);
                                        break;
                                }

                                break;
                        }
                    }
                }

                return GetAlignmentSettings(tempString, itemInfo);
            }

            /// <summary>
            /// Converts a template to a dataset.
            /// </summary>
            /// <param name="encodedTemplateXml">The hex encoded XML in string.</param>
            /// <param name="receiptInfo">The receipt properties.</param>
            /// <returns>The dataset.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The scope exists more than the method")]
            private static DataSet ConvertToDataSet(string encodedTemplateXml, ReceiptInfo receiptInfo)
            {
                DataSet template = null;

                if (encodedTemplateXml.Length > 0)
                {
                    template = new DataSet();
                    byte[] buffer = null;
                    int discarded;
                    buffer = ReceiptService.GetBytes(encodedTemplateXml, out discarded);

                    using (MemoryStream myStream = new MemoryStream())
                    {
                        myStream.Write(buffer, 0, buffer.Length);
                        myStream.Position = 0;

                        XDocument xml = XDocument.Load(myStream);
                        HashSet<string> tableNames = new HashSet<string>();
                        int lineId = 0;

                        foreach (XElement element in xml.Root.Elements())
                        {
                            foreach (XElement child in element.DescendantsAndSelf())
                            {
                                if (!tableNames.Contains(child.Name.LocalName))
                                {
                                    template = CreateDataTable(template, child);
                                    AddDataRow(template.Tables.Where(p => p.TableName == child.Name.LocalName).Single(), child, lineId);
                                    tableNames.Add(child.Name.LocalName);
                                }
                                else
                                {
                                    DataTable table = template.Tables.Where(p => p.TableName == child.Name.LocalName).Single();
                                    DataRow dataRow = table.NewRow();

                                    foreach (XAttribute attribute in child.Attributes())
                                    {
                                        dataRow[attribute.Name.LocalName] = attribute.Value;
                                    }

                                    dataRow["line_id"] = lineId;
                                    table.Rows.Add(dataRow);
                                }
                            }

                            lineId++;
                        }
                    }

                    // Adding detail table to the dataset
                    DataTable receiptDetails = new DataTable();
                    receiptDetails.TableName = "FORMDETAILS";

                    // Adding columns to items data
                    receiptDetails.Columns.Add("ID", typeof(string));
                    receiptDetails.Columns.Add("TITLE", typeof(string));
                    receiptDetails.Columns.Add("DESCRIPTION", typeof(string));
                    receiptDetails.Columns.Add("UPPERCASE", typeof(bool));

                    object row = new object[]
                    {
                    receiptInfo.ReceiptLayoutId,
                    receiptInfo.Title,
                    receiptInfo.Description,
                    receiptInfo.Uppercase == 1
                    };

                    receiptDetails.Rows.Add(row);
                    template.Tables.Add(receiptDetails);
                }

                return template;
            }

            private static DataSet CreateDataTable(DataSet dataSet, XElement element)
            {
                DataTable table = new DataTable(element.Name.LocalName);
                table.Columns.Add(new DataColumn("line_id", typeof(int)));

                foreach (XAttribute attribute in element.Attributes())
                {
                    table.Columns.Add(new DataColumn(attribute.Name.LocalName, typeof(string)));
                }

                dataSet.Tables.Add(table);

                return dataSet;
            }

            private static void AddDataRow(DataTable table, XElement element, int id)
            {
                DataRow row = table.NewRow();
                row["line_id"] = id;

                foreach (XAttribute attribute in element.Attributes())
                {
                    row[attribute.Name.LocalName] = attribute.Value;
                }

                table.Rows.Add(row);
            }

            /// <summary>
            /// Creates a byte array from the hexadecimal string. Each two characters are combined
            /// to create one byte. First two hexadecimal characters become first byte in returned array.
            /// Non-hexadecimal characters are ignored.
            /// </summary>
            /// <param name="hexEncodedValue">String to convert to byte array.</param>
            /// <param name="discarded">Number of characters in string ignored.</param>
            /// <returns>Byte array, in the same left-to-right order as the hexString.</returns>
            private static byte[] GetBytes(string hexEncodedValue, out int discarded)
            {
                discarded = 0;

                // string newString = "";
                System.Text.StringBuilder newString = new StringBuilder();
                char c;

                // remove all none A-F, 0-9, characters
                for (int i = 0; i < hexEncodedValue.Length; i++)
                {
                    c = hexEncodedValue[i];
                    if (IsHexDigit(c))
                    {
                        newString.Append(c); // newString += c;
                    }
                    else
                    {
                        discarded++;
                    }
                }

                // if odd number of characters, discard last character
                if (newString.Length % 2 != 0)
                {
                    discarded++;
                    newString = new StringBuilder(newString.ToString(0, newString.Length - 1));
                }

                // Converts the hexadecimal receipt template header, lines or footer to a bytes array
                // which is then consumed to convert it to a XML
                int byteLength = newString.Length / 2;
                byte[] bytes = new byte[byteLength];
                string hex;
                int j = 0;
                int k = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    hex = new string(new char[] { newString[j], newString[j + 1] });
                    byte byteValue = HexToByte(hex);

                    if (byteValue != 0x00)
                    {
                        bytes[k] = byteValue;
                        k++;
                    }

                    j = j + 2;
                }

                return bytes.Take(k).ToArray();
            }

            /// <summary>
            /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9).
            /// </summary>
            /// <param name="c">Character to test.</param>
            /// <returns>True if hex digit, false if not.</returns>
            private static bool IsHexDigit(char c)
            {
                byte convertedByte;
                return byte.TryParse(c.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier, null, out convertedByte);
            }

            /// <summary>
            /// Converts 1 or 2 character string into equivalent byte value.
            /// </summary>
            /// <param name="hex">1 or 2 character string.</param>
            /// <returns>Byte converted from hex.</returns>
            private static byte HexToByte(string hex)
            {
                if (hex.Length > 2 || hex.Length == 0)
                {
                    throw new ArgumentException("Hex string must be 1 or 2 characters in length. The Hex string passed was " + hex);
                }

                byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);

                return newByte;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "By design.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "By design.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "By design.")]
            private static string GetInfoFromTransaction(ReceiptItemInfo itemInfo, TenderLine tenderItem, SalesOrder theTransaction, RequestContext context, GetCardPaymentPropertiesServiceResponse cardPaymentPropertiesResponse)
            {
                DeliveryOption deliveryOption;

                if (theTransaction != null)
                {
                    switch (itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty))
                    {
                        case "DATE":
                        case "EFTDATE":
                            return FormatDate(theTransaction.CreatedDateTime, context);
                        case "TIME24H":
                        case "EFTTIME24H":
                            {
                                var request = new GetFormattedTimeServiceRequest(theTransaction.CreatedDateTime, TimeFormattingType.Hour24);
                                string formattedValue = context.Execute<GetFormattedContentServiceResponse>(request).FormattedValue;
                                return formattedValue;
                            }

                        case "EFTACCOUNTTYPE":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    if (cardPaymentPropertiesResponse != null)
                                    {
                                        return cardPaymentPropertiesResponse.AccountType;
                                    }
                                }

                                return string.Empty;
                            }

                        case "EFTAPPLICATIONID":
                            return string.Empty;
                        case "TIME12H":
                        case "EFTTIME12H":
                            {
                                var request = new GetFormattedTimeServiceRequest(theTransaction.CreatedDateTime, TimeFormattingType.Hour12);
                                string formattedValue = context.Execute<GetFormattedContentServiceResponse>(request).FormattedValue;
                                return formattedValue;
                            }

                        case "TRANSNO":
                        case "TRANSACTIONNUMBER":
                            {
                                switch (theTransaction.TransactionType)
                                {
                                    case SalesTransactionType.CustomerOrder:
                                        {
                                            if (!string.IsNullOrEmpty(theTransaction.SalesId))
                                            {
                                                return theTransaction.SalesId;
                                            }
                                            else if (!string.IsNullOrEmpty(theTransaction.Id))
                                            {
                                                return theTransaction.Id;
                                            }
                                            else
                                            {
                                                return string.Empty;
                                            }
                                        }

                                    default:
                                        return theTransaction.Id ?? string.Empty;
                                }
                            }

                        case "RECEIPTNUMBER":
                            return theTransaction.ReceiptId;
                        case "STAFF_ID":
                        case "OPERATORID":
                        case "EMPLOYEEID":
                        case "SALESPERSONID":
                            return theTransaction.StaffId ?? string.Empty;
                        case "OPERATORNAMEONRECEIPT":
                        case "SALESPERSONNAMEONRECEIPT":
                            {
                                GetEmployeesServiceRequest getEmployeeRequest = new GetEmployeesServiceRequest(theTransaction.StaffId, QueryResultSettings.SingleRecord);
                                GetEmployeesServiceResponse employeeResponse = context.Execute<GetEmployeesServiceResponse>(getEmployeeRequest);
                                Employee employee = employeeResponse.Employees.SingleOrDefault();

                                return (employee == null || string.IsNullOrEmpty(employee.NameOnReceipt)) ? string.Empty : employee.NameOnReceipt;
                            }

                        case "EMPLOYEENAME":
                        case "OPERATORNAME":
                        case "SALESPERSONNAME":
                        case "CASHIER":
                            {
                                GetEmployeesServiceRequest getEmployeeRequest = new GetEmployeesServiceRequest(theTransaction.StaffId, QueryResultSettings.SingleRecord);
                                GetEmployeesServiceResponse employeeResponse = context.Execute<GetEmployeesServiceResponse>(getEmployeeRequest);
                                Employee employee = employeeResponse.Employees.SingleOrDefault();

                                return (employee == null || string.IsNullOrEmpty(employee.Name)) ? string.Empty : employee.Name;
                            }

                        case "TOTALWITHTAX":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.TotalAmount, currency, context);
                            }

                        case "REMAININGBALANCE":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    if (cardPaymentPropertiesResponse != null)
                                    {
                                        return GetFormattedCurrencyValue(cardPaymentPropertiesResponse.AvailableBalance, context.GetOrgUnit().Currency, context);
                                    }

                                    return string.Empty;
                                }

                                return string.Empty;
                            }

                        case "TOTAL":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.SubtotalAmount, currency, context);
                            }

                        case "TAXTOTAL":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.TaxAmount, currency, context);
                            }

                        case "SUBTOTALWITHOUTTAX":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.SubtotalAmountWithoutTax, currency, context);
                            }

                        case "SUMTOTALDISCOUNT":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.TotalDiscount, currency, context);
                            }

                        case "SUMLINEDISCOUNT":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.LineDiscount, currency, context);
                            }

                        case "SUMALLDISCOUNT":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.DiscountAmount, currency, context);
                            }

                        case "TERMINALID":
                            return theTransaction.TerminalId;
                        case "CUSTOMERNAME":
                            {
                                if (string.IsNullOrEmpty(theTransaction.CustomerId))
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var getCustomerDataRequest = new GetCustomerDataRequest(theTransaction.CustomerId);
                                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                                    Customer customer = getCustomerDataResponse.Entity;

                                    return customer.Name;
                                }
                            }

                        case "CUSTOMERACCOUNTNUMBER":
                            return theTransaction.CustomerId;
                        case "CUSTOMERADDRESS":
                            {
                                if (string.IsNullOrEmpty(theTransaction.CustomerId))
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var getCustomerDataRequest = new GetCustomerDataRequest(theTransaction.CustomerId);
                                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                                    Customer customer = getCustomerDataResponse.Entity;

                                    if (customer == null)
                                    {
                                        return string.Empty;
                                    }

                                    Address customerAddress = customer.GetPrimaryAddress();
                                    return (customerAddress == null) ? string.Empty : customerAddress.FullAddress;
                                }
                            }

                        case "CUSTOMERAMOUNT":
                            return GetFormattedCurrencyValue(tenderItem.Amount, tenderItem.Currency, context);
                        case "CUSTOMERVAT":
                            {
                                if (string.IsNullOrEmpty(theTransaction.CustomerId))
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var getCustomerDataRequest = new GetCustomerDataRequest(theTransaction.CustomerId);
                                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                                    Customer customer = getCustomerDataResponse.Entity;

                                    if (customer != null)
                                    {
                                        return customer.VatNumber ?? string.Empty;
                                    }

                                    return string.Empty;
                                }
                            }

                        case "CUSTOMERTAXOFFICE":
                            {
                                if (string.IsNullOrEmpty(theTransaction.CustomerId))
                                {
                                    return string.Empty;
                                }
                                else
                                {
                                    var getCustomerDataRequest = new GetCustomerDataRequest(theTransaction.CustomerId);
                                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                                    Customer customer = getCustomerDataResponse.Entity;

                                    if (customer != null)
                                    {
                                        return customer.TaxOffice ?? string.Empty;
                                    }

                                    return string.Empty;
                                }
                            }

                        case "CARDEXPIREDATE":
                            // Not supported
                            return string.Empty;
                        case "CARDNUMBER":
                            return tenderItem.MaskedCardNumber;
                        case "CARDNUMBERPARTLYHIDDEN":
                            return tenderItem.MaskedCardNumber;
                        case "CARDTYPE":
                            return tenderItem.CardTypeId;
                        case "CARDISSUERNAME":
                            // Not supported
                            return string.Empty;
                        case "CARDAMOUNT":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(tenderItem.Amount, currency, context);
                            }

                        case "CARDAUTHNUMBER":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    return cardPaymentPropertiesResponse.ApprovalCode;
                                }

                                return string.Empty;
                            }

                        case "BATCHCODE":
                            // Not supported
                            return string.Empty;
                        case "ACQUIRERNAME":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    return cardPaymentPropertiesResponse.ConnectorName;
                                }

                                return string.Empty;
                            }

                        case "VISAAUTHCODE":
                        case "EUROAUTHCODE":
                            return string.Empty;
                        case "EFTSTORECODE":
                            return theTransaction.StoreId;
                        case "EFTTERMINALNUMBER":
                            return theTransaction.TerminalId;
                        case "EFTINFOMESSAGE":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    return cardPaymentPropertiesResponse.InfoMessage;
                                }

                                return string.Empty;
                            }

                        case "EFTTERMINALID":
                            {
                                if (cardPaymentPropertiesResponse == null || string.IsNullOrEmpty(cardPaymentPropertiesResponse.EFTTerminalId))
                                {
                                    DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
                                    return deviceConfiguration.EFTTerminalId;
                                }
                                else
                                {
                                    return cardPaymentPropertiesResponse.EFTTerminalId;
                                }
                            }

                        case "EFTMERCHANTID":
                            return string.Empty;
                        case "ENTRYSOURCECODE":
                            // Not supported
                            return string.Empty;
                        case "AUTHSOURCECODE":
                            // Not supported
                            return string.Empty;
                        case "AUTHORIZATIONCODE":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    return cardPaymentPropertiesResponse.ApprovalCode;
                                }

                                return string.Empty;
                            }

                        case "SEQUENCECODE":
                            // Not supported
                            return string.Empty;
                        case "EFTMESSAGE":
                            // Not supported
                            return string.Empty;
                        case "EFTRETRIEVALREFERENCENUMBER":
                            {
                                if (!string.IsNullOrWhiteSpace(tenderItem.Authorization))
                                {
                                    return cardPaymentPropertiesResponse.ProviderTransactionId;
                                }

                                return string.Empty;
                            }

                        case "CUSTOMERTENDERAMOUNT":
                            {
                                return GetFormattedCurrencyValue(tenderItem.Amount, tenderItem.Currency, context);
                            }

                        case "TENDERROUNDING":
                            {
                                decimal tenderRoundingAmount = decimal.Negate(theTransaction.AmountPaid - theTransaction.GrossAmount);
                                return GetFormattedCurrencyValue(tenderRoundingAmount, context.GetOrgUnit().Currency, context);
                            }

                        case "INVOICECOMMENT":
                            return theTransaction.InvoiceComment;
                        case "TRANSACTIONCOMMENT":
                            return theTransaction.Comment;
                        case "LOGO":
                            if (itemInfo.ImageId == 0)
                            {
                                return LegacyLogoMessage;
                            }
                            else
                            {
                                var getImageByImageIdDataRequest = new GetImageByImageIdDataRequest(itemInfo.ImageId);
                                string pictureAsBase64 = context.Runtime.Execute<SingleEntityDataServiceResponse<RetailImage>>(getImageByImageIdDataRequest, context).Entity.PictureAsBase64;

                                if (pictureAsBase64 == null)
                                {
                                    return LegacyLogoMessage;
                                }
                                else
                                {
                                    return string.Format(LogoMessage, pictureAsBase64);
                                }
                            }

                        case "RECEIPTNUMBERBARCODE":
                            return "<B: " + theTransaction.ReceiptId + ">";
                        case "CUSTOMERORDERBARCODE":
                            return "<B: " + theTransaction.SalesId + ">";
                        case "REPRINTMESSAGE":
                            return string.Empty;
                        case "OFFLINEINDICATOR":
                            // Not supported
                            return string.Empty;
                        case "STOREID":
                            return theTransaction.StoreId;
                        case "STORENAME":
                            return context.GetOrgUnit().OrgUnitName;
                        case "STOREADDRESS":
                            return context.GetOrgUnit().OrgUnitFullAddress;
                        case "STOREPHONE":
                            {
                                OrgUnit orgUnit = context.GetOrgUnit();
                                string phone = orgUnit.OrgUnitAddress == null ? string.Empty : orgUnit.OrgUnitAddress.Phone;
                                string phoneExtension = orgUnit.OrgUnitAddress == null ? string.Empty : orgUnit.OrgUnitAddress.PhoneExt;

                                if (string.IsNullOrEmpty(phone))
                                {
                                    return string.Empty;
                                }
                                else if (!string.IsNullOrEmpty(phoneExtension))
                                {
                                    return string.Format("{0}-{1}", phone, phoneExtension);
                                }
                                else
                                {
                                    return phone;
                                }
                            }

                        case "STORETAXIDENTIFICATIONNUMBER":
                            // Not supported
                            return string.Empty;
                        case "TENDERAMOUNT":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(tenderItem.Amount, currency, context);
                            }

                        case "LOYALTYCARDNUMBER":
                            {
                                return theTransaction.LoyaltyCardId;
                            }

                        case "CREDITMEMONUMBER":
                            {
                                return tenderItem.CreditMemoId;
                            }

                        case "CREDITMEMOAMOUNT":
                            {
                                GetCreditMemoServiceRequest serviceRequest = null;
                                GetCreditMemoServiceResponse serviceResponse = null;

                                serviceRequest = new GetCreditMemoServiceRequest(
                                    tenderItem.CreditMemoId);
                                serviceResponse = context.Execute<GetCreditMemoServiceResponse>(serviceRequest);

                                if (serviceResponse.CreditMemo != null)
                                {
                                    return GetFormattedCurrencyValue(serviceResponse.CreditMemo.Balance, serviceResponse.CreditMemo.CurrencyCode, context);
                                }
                                else
                                {
                                    return string.Empty;
                                }
                            }

                        case "ALLTENDERCOMMENTS":
                            {
                                StringBuilder reasonCodeComments = new StringBuilder();
                                var reasonCodeIds = theTransaction.TenderLines
                                    .SelectMany(s => s.ReasonCodeLines)
                                    .Select(r => r.ReasonCodeId)
                                    .Distinct(StringComparer.OrdinalIgnoreCase);

                                if (reasonCodeIds.Any())
                                {
                                    GetReasonCodesServiceRequest reasonCodeServiceRequest = new GetReasonCodesServiceRequest(QueryResultSettings.AllRecords, reasonCodeIds);
                                    var reasonCodeServiceResponse = context.Runtime.Execute<GetReasonCodesServiceResponse>(reasonCodeServiceRequest, context);
                                    var reasonCodesById = reasonCodeServiceResponse.ReasonCodes.Results.ToDictionary(r => r.ReasonCodeId, r => r);

                                    if (reasonCodesById.Any())
                                    {
                                        foreach (TenderLine line in theTransaction.TenderLines)
                                        {
                                            reasonCodeComments.Append(GetCommentFromReasonCodeLines(line.ReasonCodeLines, reasonCodesById));
                                        }
                                    }
                                }

                                return reasonCodeComments.ToString();
                            }

                        case "ALLITEMCOMMENTS":
                            {
                                string comments = string.Empty;
                                foreach (SalesLine line in theTransaction.SalesLines)
                                {
                                    if (line.Comment != null)
                                    {
                                        comments += line.Comment;
                                    }
                                }

                                return comments;
                            }

                        case "DEPOSITDUE":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                if (theTransaction.IsDepositOverridden)
                                {
                                    return GetFormattedCurrencyValue(theTransaction.OverriddenDepositAmount, currency, context);
                                }
                                else
                                {
                                    return GetFormattedCurrencyValue(theTransaction.RequiredDepositAmount, currency, context);
                                }
                            }

                        case "DEPOSITPAID":
                            {
                                if (theTransaction.TransactionType == SalesTransactionType.CustomerOrder ||
                                    theTransaction.TransactionType == SalesTransactionType.AsyncCustomerOrder)
                                {
                                    if (theTransaction.CustomerOrderType == CustomerOrderType.SalesOrder)
                                    {
                                        return GetFormattedCurrencyValue(theTransaction.AmountPaid, context.GetOrgUnit().Currency, context);
                                    }
                                }

                                return string.Empty;
                            }

                        case "DEPOSITAPPLIED":
                        case "DEPOSITREMAINING":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                return GetFormattedCurrencyValue(theTransaction.AvailableDepositAmount, currency, context);
                            }

                        case "DELIVERYTYPE":
                            {
                                if (theTransaction.TransactionType == SalesTransactionType.CustomerOrder ||
                                    theTransaction.TransactionType == SalesTransactionType.AsyncCustomerOrder ||
                                    theTransaction.TransactionType == SalesTransactionType.PendingSalesOrder ||
                                    theTransaction.TransactionType == SalesTransactionType.AsyncCustomerQuote)
                                {
                                    // if all sales lines are going to be picked up by customer
                                    if (theTransaction.SalesLines.All(line => line.DeliveryMode == context.GetChannelConfiguration().PickupDeliveryModeCode))
                                    {
                                        return GetLocalizedString(LocalizationCustomerPickup, context);
                                    }
                                    else if (theTransaction.SalesLines.All(line => line.DeliveryMode != context.GetChannelConfiguration().PickupDeliveryModeCode))
                                    {
                                        // if none of the sales lines is going to be picked up by the customer
                                        return GetLocalizedString(LocalizationShipping, context);
                                    }
                                    else
                                    {
                                        // if mixed.
                                        return GetLocalizedString(LocalizationMixed, context);
                                    }
                                }
                                else
                                {
                                    return string.Empty;
                                }
                            }

                        case "DELIVERYMETHOD":
                            var deliveryOptionDataRequest = new GetDeliveryOptionDataRequest(theTransaction.DeliveryMode, new QueryResultSettings(new ColumnSet("TXT"), PagingInfo.AllRecords));
                            var deliveryOptionDataResponse = context.Execute<EntityDataServiceResponse<DeliveryOption>>(deliveryOptionDataRequest);
                            deliveryOption = deliveryOptionDataResponse.PagedEntityCollection.FirstOrDefault();
                            return (deliveryOption != null) ? deliveryOption.Description : theTransaction.DeliveryMode;
                        case "DELIVERYDATE":
                            return FormatDate(theTransaction.RequestedDeliveryDate, context);
                        case "ORDERTYPE":
                            {
                                switch (theTransaction.TransactionType)
                                {
                                    case SalesTransactionType.Sales:
                                        return GetLocalizedString(LocalizationSalesTransaction, context);

                                    case SalesTransactionType.IncomeExpense:
                                        return GetLocalizedString(LocalizationIncomeExpense, context);

                                    case SalesTransactionType.CustomerAccountDeposit:
                                        return GetLocalizedString(LocalizationCustomerAccountDeposit, context);

                                    case SalesTransactionType.CustomerOrder:
                                    case SalesTransactionType.AsyncCustomerOrder:
                                    case SalesTransactionType.AsyncCustomerQuote:
                                    case SalesTransactionType.PendingSalesOrder:
                                        {
                                            if (theTransaction.CustomerOrderType == CustomerOrderType.Quote)
                                            {
                                                return GetLocalizedString(LocalizationCustomerQuote, context);
                                            }
                                            else
                                            {
                                                return GetLocalizedString(LocalizationCustomerOrder, context);
                                            }
                                        }

                                    default:
                                        return string.Empty;
                                }
                            }
                            
                        case "ORDERSTATUS":
                            {
                                switch (theTransaction.Status)
                                {
                                    case SalesStatus.Canceled:
                                        return GetLocalizedString(LocalizationCanceled, context);
                                    case SalesStatus.Confirmed:
                                        return GetLocalizedString(LocalizationConfirmed, context);
                                    case SalesStatus.Created:
                                        return GetLocalizedString(LocalizationCreated, context);
                                    case SalesStatus.Delivered:
                                        return GetLocalizedString(LocalizationDelivered, context);
                                    case SalesStatus.Invoiced:
                                        return GetLocalizedString(LocalizationInvoiced, context);
                                    case SalesStatus.Lost:
                                        return GetLocalizedString(LocalizationLost, context);
                                    case SalesStatus.Processing:
                                        return GetLocalizedString(LocalizationProcessing, context);
                                    case SalesStatus.Sent:
                                        return GetLocalizedString(LocalizationSent, context);
                                    default:
                                        return GetLocalizedString(LocalizationNotApplicable, context);
                                }
                            }

                        case "REFERENCENO":
                            return theTransaction.SalesId;
                        case "EXPIRYDATE":
                            return FormatDate(theTransaction.QuotationExpiryDate, context);
                        case "ORDERID":
                            return theTransaction.SalesId;
                        case "TOTALSHIPPIINGCHARGES":
                        // Fall through to ShippingCharge case
                        case "TOTALLINEITEMSHIPPINGCHARGES":
                        case "ORDERSHIPPINGCHARGE":
                        case "SHIPPINGCHARGE":
                            {
                                decimal shippingCharges = theTransaction.SalesLines.Where(line => !line.IsVoided).Sum(salesLine => salesLine.DeliveryModeChargeAmount ?? 0M);
                                string currency = context.GetOrgUnit().Currency;

                                return GetFormattedCurrencyValue(shippingCharges, currency, context);
                            }

                        case "CANCELLATIONCHARGE":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                decimal sum = decimal.Zero;

                                string cancellationChargeCode = context.GetChannelConfiguration().CancellationChargeCode ?? string.Empty;
                                var cancellationChargeLines = theTransaction.ChargeLines.Where(c => cancellationChargeCode.Equals(c.ChargeCode, StringComparison.OrdinalIgnoreCase));
                                if (IsExcludeTaxInCancellationCharge(context))
                                {
                                    sum = cancellationChargeLines.Sum(c => c.CalculatedAmount - c.TaxAmountInclusive);
                                }
                                else
                                {
                                    sum = cancellationChargeLines.Sum(c => c.CalculatedAmount);
                                }

                                return GetFormattedCurrencyValue(sum, currency, context);
                            }

                        case "TAXONCANCELLATIONCHARGE":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                string cancellationChargeCode = context.GetChannelConfiguration().CancellationChargeCode ?? string.Empty;
                                var cancellationChargeLines = theTransaction.ChargeLines.Where(c => cancellationChargeCode.Equals(c.ChargeCode, StringComparison.OrdinalIgnoreCase));
                                decimal sum = cancellationChargeLines.Sum(c => c.TaxAmount);
                                return GetFormattedCurrencyValue(sum, currency, context);
                            }

                        case "TOTALCANCELLATIONCHARGE":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                string cancellationChargeCode = context.GetChannelConfiguration().CancellationChargeCode ?? string.Empty;
                                var cancellationChargeLines = theTransaction.ChargeLines.Where(c => cancellationChargeCode.Equals(c.ChargeCode, StringComparison.OrdinalIgnoreCase));
                                decimal sum = cancellationChargeLines.Sum(c => c.CalculatedAmount + c.TaxAmountExclusive);
                                return GetFormattedCurrencyValue(sum, currency, context);
                            }

                        case "TAXONSHIPPING":
                            {
                                string currency = context.GetOrgUnit().Currency;
                                string shippingChargeCode = context.GetChannelConfiguration().ShippingChargeCode ?? string.Empty;
                                var shippingChargeLines = theTransaction.ChargeLines.Where(c => shippingChargeCode.Equals(c.ChargeCode, StringComparison.OrdinalIgnoreCase));
                                decimal sum = shippingChargeLines.Sum(c => c.TaxAmount);
                                return GetFormattedCurrencyValue(sum, currency, context);
                            }

                        case "MISCCHARGETOTAL":
                            return string.Empty;
                        case "TOTALPAYMENTS":
                            return GetFormattedCurrencyValue(theTransaction.AmountPaid, context.GetOrgUnit().Currency, context);
                        case "BALANCE":
                            {
                                if (theTransaction.TransactionType == SalesTransactionType.CustomerOrder)
                                {
                                    if (theTransaction.CustomerOrderType == CustomerOrderType.SalesOrder)
                                    {
                                        decimal balance = theTransaction.TotalAmount - theTransaction.AmountPaid;
                                        return GetFormattedCurrencyValue(balance, context.GetOrgUnit().Currency, context);
                                    }
                                }

                                return string.Empty;
                            }

                        // India receipt tax summary
                        case "COMPANYPANNO_IN":
                            {
                                GetReceiptHeaderInfoIndiaDataRequest getReceiptHeaderInfoIndiaDataRequest = new GetReceiptHeaderInfoIndiaDataRequest(QueryResultSettings.SingleRecord);
                                var receiptHeaderInfoIndia = context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptHeaderInfoIndia>>(getReceiptHeaderInfoIndiaDataRequest, context).Entity;
                                return receiptHeaderInfoIndia != null ? receiptHeaderInfoIndia.CompanyPermanentAccountNumber : string.Empty;
                            }

                        case "VATTINNO_IN":
                            {
                                GetReceiptHeaderTaxInfoIndiaDataRequest getReceiptHeaderTaxInfoIndiaDataRequest = new GetReceiptHeaderTaxInfoIndiaDataRequest(QueryResultSettings.SingleRecord);
                                var receiptHeaderTaxInfoIndia = context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptHeaderTaxInfoIndia>>(getReceiptHeaderTaxInfoIndiaDataRequest, context).Entity;
                                return receiptHeaderTaxInfoIndia != null ? receiptHeaderTaxInfoIndia.ValueAddedTaxTINNumber : string.Empty;
                            }

                        case "CSTTINNO_IN":
                            {
                                GetReceiptHeaderTaxInfoIndiaDataRequest getReceiptHeaderTaxInfoIndiaDataRequest = new GetReceiptHeaderTaxInfoIndiaDataRequest(QueryResultSettings.SingleRecord);
                                var receiptHeaderTaxInfoIndia = context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptHeaderTaxInfoIndia>>(getReceiptHeaderTaxInfoIndiaDataRequest, context).Entity;
                                return receiptHeaderTaxInfoIndia != null ? receiptHeaderTaxInfoIndia.CentralSalesTaxTINNumber : string.Empty;
                            }

                        case "STCNUMBER_IN":
                            {
                                GetReceiptHeaderTaxInfoIndiaDataRequest getReceiptHeaderTaxInfoIndiaDataRequest = new GetReceiptHeaderTaxInfoIndiaDataRequest(QueryResultSettings.SingleRecord);
                                var receiptHeaderTaxInfoIndia = context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptHeaderTaxInfoIndia>>(getReceiptHeaderTaxInfoIndiaDataRequest, context).Entity;
                                return receiptHeaderTaxInfoIndia != null ? receiptHeaderTaxInfoIndia.ServiceTaxNumber : string.Empty;
                            }

                        case "ECCNUMBER_IN":
                            {
                                GetReceiptHeaderTaxInfoIndiaDataRequest getReceiptHeaderTaxInfoIndiaDataRequest = new GetReceiptHeaderTaxInfoIndiaDataRequest(QueryResultSettings.SingleRecord);
                                var receiptHeaderTaxInfoIndia = context.Runtime.Execute<SingleEntityDataServiceResponse<ReceiptHeaderTaxInfoIndia>>(getReceiptHeaderTaxInfoIndiaDataRequest, context).Entity;
                                return receiptHeaderTaxInfoIndia != null ? receiptHeaderTaxInfoIndia.ExciseTaxNumber : string.Empty;
                            }

                        default:
                            return string.Empty;
                    }
                }

                return string.Empty;
            }

            private static string GetInfoFromCustomerAccountDepositLines(RequestContext context, string variable, SalesTransaction transaction)
            {
                string returnValue = string.Empty;

                switch (variable)
                {
                    case "ITEMNAME":
                        {
                            if (transaction.TransactionType == SalesTransactionType.CustomerAccountDeposit)
                            {
                                returnValue = GetLocalizedString(LocalizationCustomerAccountDeposit, context);
                            }
                        }

                        break;

                    case "TOTALPRICE":
                        {
                            string currency = context.GetOrgUnit().Currency;
                            returnValue = GetFormattedCurrencyValue(transaction.CustomerAccountDepositLines[0].Amount, currency, context);
                        }

                        break;
                    case "ITEMCOMMENT":
                        {
                            returnValue = transaction.CustomerAccountDepositLines[0].Comment;
                        }

                        break;
                }

                return returnValue;
            }

            private static string GetInfoFromIncomeExpenseLines(string variable, SalesOrder transaction, RequestContext context)
            {
                string returnValue = string.Empty;

                switch (variable)
                {
                    case "ITEMNAME":
                        {
                            if (transaction.TransactionType == SalesTransactionType.IncomeExpense)
                            {
                                returnValue = GetLocalizedString(LocalizationIncomeExpense, context);
                            }
                        }

                        break;
                }

                return returnValue;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "To be refactored.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode", Justification = "To be refactored.")]
            private static string GetInfoFromSaleLineItem(SalesOrder salesOrder, ReceiptItemInfo itemInfo, SalesLine saleLine, bool isTaxIncludedInPrice, RequestContext context)
            {
                string returnValue = string.Empty;

                if (salesOrder.IncomeExpenseLines.Any())
                {
                    return GetInfoFromIncomeExpenseLines(itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty), salesOrder, context);
                }

                if (salesOrder.CustomerAccountDepositLines.Any())
                {
                    return GetInfoFromCustomerAccountDepositLines(context, itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty), salesOrder);
                }

                if (saleLine == null)
                {
                    return returnValue;
                }

                switch (itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty))
                {
                    case "TAXID":
                        returnValue = saleLine.ItemTaxGroupId;
                        break;
                    case "TAXPERCENT":
                        returnValue = FormatNumber(saleLine.TaxRatePercent, context);
                        break;
                    case "ITEMNAME":
                        {
                            if (saleLine.IsGiftCardLine)
                            {
                                // Return "Gift Card"
                                returnValue = GetLocalizedString(LocalizationGiftCard, context);
                            }
                            else if (saleLine.IsInvoiceLine)
                            {
                                returnValue = saleLine.Comment;
                            }
                            else
                            {
                                List<string> itemIds = new List<string>();
                                itemIds.Add(saleLine.ItemId);

                                var getItemsRequest = new GetItemsDataRequest(itemIds);
                                var getItemsResponse = context.Execute<GetItemsDataResponse>(getItemsRequest);

                                ReadOnlyCollection<Item> items = getItemsResponse.Items;
                                if (items != null && items.Any())
                                {
                                    returnValue = items[0].Name;
                                }
                                else
                                {
                                    return string.Empty;
                                }
                            }

                            break;
                        }

                    case "ITEMID":
                        returnValue = saleLine.ItemId;
                        break;
                    case "ITEMBARCODE":
                        returnValue = saleLine.Barcode ?? string.Empty;
                        break;
                    case "QTY":
                        returnValue = FormatNumber(saleLine.Quantity, context);
                        break;
                    case "UNITPRICE":
                        {
                            string currency = context.GetOrgUnit().Currency;
                            returnValue = GetFormattedCurrencyValue(saleLine.Price, currency, context);
                        }

                        break;

                    case "UNITPRICEWITHTAX":
                        {
                            var store = ReceiptService.GetStoreFromContext(context);

                            if (isTaxIncludedInPrice)
                            {
                                returnValue = GetFormattedCurrencyValue(saleLine.NetAmountWithTaxPerUnit(), store.Currency, context);
                            }
                            else
                            {
                                returnValue = GetFormattedCurrencyValue((saleLine.NetAmountWithTax() + saleLine.TaxAmount) / saleLine.Quantity, store.Currency, context);
                            }
                        }

                        break;

                    case "TOTALPRICE":
                        {
                            string currency = context.GetOrgUnit().Currency;
                            returnValue = GetFormattedCurrencyValue(saleLine.GrossAmount, currency, context);
                        }

                        break;
                    case "TOTALPRICEWITHTAX":
                        {
                            string currency = context.GetOrgUnit().Currency;
                            returnValue = GetFormattedCurrencyValue(saleLine.TotalAmount, currency, context);
                        }

                        break;
                    case "ITEMUNITID":
                        returnValue = saleLine.SalesOrderUnitOfMeasure;
                        break;
                    case "ITEMUNITIDNAME":
                        if (string.IsNullOrEmpty(saleLine.UnitOfMeasureSymbol))
                        {
                            returnValue = string.Empty;
                        }
                        else
                        {
                            var request = new GetUnitsOfMeasureDataRequest(new string[] { saleLine.UnitOfMeasureSymbol }, QueryResultSettings.SingleRecord);
                            UnitOfMeasure uom = context.Execute<EntityDataServiceResponse<UnitOfMeasure>>(request).PagedEntityCollection.Results.FirstOrDefault();
                            returnValue = uom == null ? string.Empty : uom.Description;
                        }

                        break;
                    case "LINEDISCOUNTAMOUNT":
                        {
                            if (saleLine.LineDiscount != 0)
                            {
                                string currency = context.GetOrgUnit().Currency;
                                returnValue = GetFormattedCurrencyValue(decimal.Negate(saleLine.LineDiscount), currency, context);
                            }
                        }

                        break;
                    case "LINEDISCOUNTPERCENT":
                        {
                            if (saleLine.LineDiscount != 0)
                            {
                                returnValue = FormatNumber(saleLine.LinePercentageDiscount, context);
                            }
                        }

                        break;
                    case "PERIODICDISCOUNTAMOUNT":
                        {
                            if (saleLine.PeriodicDiscount != 0)
                            {
                                string currency = context.GetOrgUnit().Currency;
                                returnValue = GetFormattedCurrencyValue(decimal.Negate(saleLine.PeriodicDiscount), currency, context);
                            }
                        }

                        break;
                    case "PERIODICDISCOUNTPERCENT":
                        {
                            if (saleLine.PeriodicDiscount != 0)
                            {
                                returnValue = FormatNumber(saleLine.PeriodicPercentageDiscount, context);
                            }
                        }

                        break;
                    case "PERIODICDISCOUNTNAME":
                        {
                            returnValue = string.Empty;
                            foreach (DiscountLine discountLine in saleLine.DiscountLines)
                            {
                                if (discountLine.DiscountLineType == DiscountLineType.PeriodicDiscount)
                                {
                                    returnValue += discountLine.OfferName + ", ";
                                }
                            }

                            if (returnValue.EndsWith(", ", StringComparison.OrdinalIgnoreCase))
                            {
                                returnValue = returnValue.Substring(0, returnValue.Length - 1);
                            }
                        }

                        break;
                    case "TOTALDISCOUNTAMOUNT":
                        {
                            string currency = context.GetOrgUnit().Currency;
                            returnValue = GetFormattedCurrencyValue(decimal.Negate(saleLine.TotalDiscount), currency, context);
                        }

                        break;
                    case "TOTALDISCOUNTPERCENT":
                        returnValue = FormatNumber(saleLine.TotalPercentageDiscount, context);
                        break;
                    case "DIMENSIONCOLORID":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.ColorId : string.Empty;
                        break;
                    case "DIMENSIONCOLORVALUE":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.Color : string.Empty;
                        break;
                    case "DIMENSIONSIZEID":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.SizeId : string.Empty;
                        break;
                    case "DIMENSIONSIZEVALUE":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.Size : string.Empty;
                        break;
                    case "DIMENSIONSTYLEID":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.StyleId : string.Empty;
                        break;
                    case "DIMENSIONSTYLEVALUE":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.Style : string.Empty;
                        break;
                    case "DIMENSIONCONFIGID":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.ConfigId : string.Empty;
                        break;
                    case "DIMENSIONCONFIGVALUE":
                        returnValue = saleLine.Variant != null ? saleLine.Variant.Configuration : string.Empty;
                        break;

                    case "LINEITEMSHIPPINGCHARGE":
                        {
                            string currency = context.GetOrgUnit().Currency;
                            if (!saleLine.IsVoided)
                            {
                                returnValue = GetFormattedCurrencyValue(saleLine.DeliveryModeChargeAmount, currency, context);
                            }
                        }

                        break;
                    case "LINEDELIVERYTYPE":
                        {
                            if (string.IsNullOrEmpty(saleLine.DeliveryMode))
                            {
                                returnValue = string.Empty;
                            }
                            else if (saleLine.DeliveryMode == context.GetChannelConfiguration().PickupDeliveryModeCode)
                            {
                                returnValue = GetLocalizedString(LocalizationCustomerPickup, context);
                            }
                            else
                            {
                                returnValue = GetLocalizedString(LocalizationShipping, context);
                            }
                        }

                        break;
                    case "LINEDELIVERYMETHOD":
                        {
                            GetDeliveryOptionDataRequest deliveryOptionDataRequest = new GetDeliveryOptionDataRequest(saleLine.DeliveryMode, new QueryResultSettings(new ColumnSet("TXT"), PagingInfo.AllRecords));
                            var deliveryOptionDataResponse = context.Execute<EntityDataServiceResponse<DeliveryOption>>(deliveryOptionDataRequest);
                            DeliveryOption deliveryOption = deliveryOptionDataResponse.PagedEntityCollection.Results.FirstOrDefault();
                            returnValue = (deliveryOption != null) ? deliveryOption.Description : saleLine.DeliveryMode;
                        }

                        break;
                    case "LINEDELIVERYDATE":
                        returnValue = FormatDate(saleLine.RequestedDeliveryDate, context);
                        break;
                    case "PICKUPQTY":
                        returnValue = FormatNumber(saleLine.Quantity, context);
                        break;
                    case "ITEMTAX":
                        string currencyCode = context.GetOrgUnit().Currency;
                        returnValue = GetFormattedCurrencyValue(saleLine.TaxAmount, currencyCode, context);
                        break;
                    case "KITCOMPONENTNAME":
                        {
                            List<string> itemIds = new List<string>();
                            itemIds.Add(saleLine.ItemId);
                            QueryResultSettings settings = new QueryResultSettings(new PagingInfo(itemIds.Count, 0));
                            GetItemsDataRequest dataRequest = new GetItemsDataRequest(itemIds);
                            dataRequest.QueryResultSettings = settings;
                            ReadOnlyCollection<Item> items = context.Runtime.Execute<GetItemsDataResponse>(dataRequest, context).Items;

                            if (items != null && items.Any())
                            {
                                returnValue = items[0].Name;
                            }
                            else
                            {
                                returnValue = string.Empty;
                            }

                            break;
                        }

                    case "KITCOMPONENTQTY":
                        returnValue = FormatNumber(saleLine.Quantity, context);
                        break;
                    case "KITCOMPONENTUNIT":
                        returnValue = saleLine.SalesOrderUnitOfMeasure;
                        break;
                    case "ITEMCOMMENT":
                        {
                            // If the line is an invoice line then item name will contain the comment
                            if (!saleLine.IsInvoiceLine)
                            {
                                returnValue = FormatLineCommentWithReasonCodes(saleLine, context);
                            }

                            break;
                        }

                    case "RETURNREASON":
                        {
                            if (saleLine.ReturnLabelProperties != null &&
                                saleLine.ReturnLabelProperties.ReturnReasonText != null)
                            {
                                returnValue = saleLine.ReturnLabelProperties.ReturnReasonText;
                            }
                            else
                            {
                                returnValue = string.Empty;
                            }
                        }

                        break;
                    case "RETURNLOCATION":
                        {
                            if (saleLine.ReturnLabelProperties != null &&
                                saleLine.ReturnLabelProperties.ReturnLocationText != null)
                            {
                                returnValue = saleLine.ReturnLabelProperties.ReturnLocationText;
                            }
                            else
                            {
                                returnValue = string.Empty;
                            }
                        }

                        break;
                    case "RETURNWAREHOUSE":
                        {
                            if (saleLine.ReturnLabelProperties != null &&
                                saleLine.ReturnLabelProperties.ReturnWarehouseText != null)
                            {
                                returnValue = saleLine.ReturnLabelProperties.ReturnWarehouseText;
                            }
                            else
                            {
                                returnValue = string.Empty;
                            }
                        }

                        break;
                    case "RETURNPALLETE":
                        {
                            if (saleLine.ReturnLabelProperties != null &&
                                saleLine.ReturnLabelProperties.ReturnPalleteText != null)
                            {
                                returnValue = saleLine.ReturnLabelProperties.ReturnPalleteText;
                            }
                            else
                            {
                                returnValue = string.Empty;
                            }
                        }

                        break;
                    case "SERIALID":
                        {
                            returnValue = saleLine.SerialNumber ?? string.Empty;
                        }

                        break;
                    default:
                        returnValue = string.Empty;
                        break;
                }

                if (returnValue == null)
                {
                    returnValue = string.Empty;
                }
                else
                {
                    if (itemInfo.Prefix.Length > 0)
                    {
                        returnValue = itemInfo.Prefix + returnValue;
                    }
                }

                return returnValue;
            }

            /// <summary>
            /// Gets Tax Line Info.
            /// </summary>
            /// <param name="itemInfo">The item information.</param>
            /// <param name="taxLine">The tax line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The string returned to receipt.</returns>
            private static string GetInfoFromTaxItem(ReceiptItemInfo itemInfo, TaxLine taxLine, RequestContext context)
            {
                string returnValue = string.Empty;
                string currency = context.GetOrgUnit().Currency;

                if (taxLine != null)
                {
                    switch (itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty))
                    {
                        // Correspond to TaxID in the footer
                        case "TAXID":
                            returnValue = taxLine.TaxCode;
                            break;

                        // Correspond to TaxGroup in the footer
                        case "TAXGROUP":
                            returnValue = taxLine.TaxGroup;
                            break;
                        case "TAXPERCENTAGE":
                            {
                                var request = new GetFormattedNumberServiceRequest(taxLine.Percentage);
                                returnValue = context.Execute<GetFormattedContentServiceResponse>(request).FormattedValue;
                            }

                            break;
                        case "TOTAL":
                            {
                                returnValue = ReceiptService.GetFormattedCurrencyValue(taxLine.TaxBasis + taxLine.Amount, currency, context);
                            }

                            break;
                        case "TAXAMOUNT":
                            {
                                returnValue = ReceiptService.GetFormattedCurrencyValue(taxLine.Amount, currency, context);
                            }

                            break;
                        case "BASICAMOUNT_IN":
                            {
                                returnValue = ReceiptService.GetFormattedCurrencyValue(taxLine.TaxBasis, currency, context);
                            }

                            break;
                        case "TOTALAMOUNT_IN":
                            {
                                returnValue = ReceiptService.GetFormattedCurrencyValue(taxLine.TaxBasis + taxLine.Amount, currency, context);
                            }

                            break;
                        case "TAXCOMPONENT_IN":
                            {
                                TaxLineIndia taxLineIN = taxLine as TaxLineIndia;
                                if (taxLineIN != null)
                                {
                                    returnValue = taxLineIN.TaxComponent;
                                }
                            }

                            break;
                        case "TAXBASIS":
                            {
                                returnValue = ReceiptService.GetFormattedCurrencyValue(taxLine.TaxBasis, currency, context);
                            }

                            break;
                        default:
                            break;
                    }
                }

                return returnValue;
            }

            /// <summary>
            /// Format sales line comment for reason codes prompt and captured value to display/print in receipt.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Formatted sales line comment and reason code string.</returns>
            private static string FormatLineCommentWithReasonCodes(SalesLine salesLine, RequestContext context)
            {
                List<string> reasonCodeLineText = new List<string>();
                StringBuilder returnValue = new StringBuilder(string.IsNullOrWhiteSpace(salesLine.Comment) ? string.Empty : salesLine.Comment);

                foreach (ReasonCodeLine reasonCodeLine in salesLine.ReasonCodeLines)
                {
                    GetReasonCodesServiceRequest reasonCodeServiceRequest = new GetReasonCodesServiceRequest(QueryResultSettings.FirstRecord, new string[] { reasonCodeLine.ReasonCodeId });
                    var reasonCodeServiceResponse = context.Execute<GetReasonCodesServiceResponse>(reasonCodeServiceRequest);

                    if (reasonCodeServiceResponse.ReasonCodes != null && reasonCodeServiceResponse.ReasonCodes.Results.Count != 0)
                    {
                        ReasonCode reasonCode = reasonCodeServiceResponse.ReasonCodes.SingleOrDefault();
                        reasonCodeLineText.Clear();

                        if (reasonCode.PrintPromptToReceipt && !string.IsNullOrWhiteSpace(reasonCode.Prompt))
                        {
                            reasonCodeLineText.Add(reasonCode.Prompt);
                        }

                        if (reasonCode.PrintInputToReceipt && (!string.IsNullOrWhiteSpace(reasonCodeLine.SubReasonCodeId)))
                        {
                            reasonCodeLineText.Add(reasonCodeLine.SubReasonCodeId);
                        }

                        if (reasonCode.PrintInputNameOnReceipt && (!string.IsNullOrWhiteSpace(reasonCodeLine.Information)))
                        {
                            reasonCodeLineText.Add(reasonCodeLine.Information);
                        }

                        if (reasonCodeLineText.Count > 0)
                        {
                            if (returnValue.Length > 0)
                            {
                                returnValue.Append(Environment.NewLine);
                            }

                            returnValue.Append(string.Join(ReasonCodeSeparator, reasonCodeLineText.ToArray()));
                        }
                    }
                }

                return returnValue.ToString();
            }

            private static string GetInfoFromTenderLineItem(ReadOnlyCollection<TenderType> tenderTypes, ReceiptItemInfo itemInfo, TenderLine tenderLine, RequestContext context)
            {
                string returnValue = string.Empty;
                TenderType tenderTypeId = tenderTypes.Where(type => type.TenderTypeId == tenderLine.TenderTypeId).Single();
                OrgUnit orgUnit = context.GetOrgUnit();

                if (tenderLine != null)
                {
                    switch (itemInfo.Variable.ToUpperInvariant().Replace(" ", string.Empty))
                    {
                        case "TENDERNAME":
                            returnValue = GetTenderName(tenderTypeId, tenderLine, orgUnit, context);
                            break;
                        case "TENDERAMOUNT":
                            {
                                string currency = orgUnit.Currency;
                                returnValue = GetFormattedCurrencyValue(tenderLine.Amount, currency, context);
                            }

                            break;
                        case "TENDERCOMMENT":
                            {
                                switch ((RetailOperation)tenderTypeId.OperationId)
                                {
                                    case RetailOperation.PayCreditMemo:
                                        returnValue = tenderLine.CreditMemoId;
                                        break;
                                    case RetailOperation.PayGiftCertificate:
                                        returnValue = tenderLine.GiftCardId;
                                        break;
                                    case RetailOperation.PayLoyalty:
                                        returnValue = tenderLine.LoyaltyCardId;
                                        break;
                                    default:
                                        returnValue = string.Empty;
                                        break;
                                }
                            }

                            break;
                    }

                    if (returnValue == null)
                    {
                        returnValue = string.Empty;
                    }
                }

                return returnValue;
            }

            private static string GetTenderName(TenderType tenderType, TenderLine tenderLine, OrgUnit orgUnit, RequestContext context)
            {
                string tenderName;
                string returnValue;
                switch (tenderType.OperationType)
                {
                    case RetailOperation.PayCard:
                        tenderName = GetLocalizedString(LocalizationPayCard, context);
                        break;
                    case RetailOperation.PayCash:
                    case RetailOperation.PayCashQuick:
                        tenderName = GetLocalizedString(LocalizationPayCash, context);
                        break;
                    case RetailOperation.PayCheck:
                        tenderName = GetLocalizedString(LocalizationPayCheck, context);
                        break;
                    case RetailOperation.PayCreditMemo:
                        tenderName = GetLocalizedString(LocalizationPayCreditMemo, context);
                        break;
                    case RetailOperation.PayCurrency:
                        tenderName = GetLocalizedString(LocalizationPayCurrency, context);
                        break;
                    case RetailOperation.PayCustomerAccount:
                        tenderName = GetLocalizedString(LocalizationPayCustomerAccount, context);
                        break;
                    case RetailOperation.PayGiftCertificate:
                        tenderName = GetLocalizedString(LocalizationPayGiftCertificate, context);
                        break;
                    case RetailOperation.PayLoyalty:
                        tenderName = GetLocalizedString(LocalizationPayLoyalty, context);
                        break;
                    default:
                        tenderName = string.Empty;
                        break;
                }

                if (tenderLine.Amount < 0)
                {
                    if (tenderType.OperationId == (int)RetailOperation.PayCard)
                    {
                        returnValue = GetLocalizedString(LocalizationChargeBack, context);
                    }
                    else
                    {
                        returnValue = GetLocalizedString(LocalizationChangeBack, context);
                    }

                    returnValue += " (" + tenderName + ")";
                }
                else
                {
                    returnValue = tenderName;
                }

                // Check if the tenderline contains foreign currency.
                if (!orgUnit.Currency.Equals(tenderLine.Currency, StringComparison.CurrentCultureIgnoreCase))
                {
                    returnValue += " (" + GetFormattedCurrencyValue(tenderLine.AmountInTenderedCurrency, tenderLine.Currency, context) + " " + tenderLine.Currency + ")";
                }

                return returnValue;
            }

            /// <summary>
            /// This method decide whether display cancellation charge without any tax on customer order.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The flag indicating whether to display cancellation charge without any tax.</returns>
            private static bool IsExcludeTaxInCancellationCharge(RequestContext context)
            {
                return context.GetChannelConfiguration().CountryRegionISOCode == CountryRegionISOCode.IN;
            }

            /// <summary>
            /// Gets the store by identifier.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The store.</returns>
            private static OrgUnit GetStoreFromContext(RequestContext context)
            {
                SearchOrgUnitDataRequest request = new SearchOrgUnitDataRequest(context.GetPrincipal().ChannelId);
                return context.Execute<EntityDataServiceResponse<OrgUnit>>(request).PagedEntityCollection.SingleOrDefault();
            }

            /// <summary>
            /// Gets the comment from a collection of reason code lines.
            /// </summary>
            /// <param name="reasonCodeLines">The reason code lines.</param>
            /// <param name="reasonCodesById">The dictionary of reason codes by identifier.</param>
            /// <returns>The comment from a collection of reason code lines.</returns>
            private static string GetCommentFromReasonCodeLines(IEnumerable<ReasonCodeLine> reasonCodeLines, IDictionary<string, ReasonCode> reasonCodesById)
            {
                StringBuilder comments = new StringBuilder();
                foreach (ReasonCodeLine line in reasonCodeLines)
                {
                    ReasonCode reasonCode;
                    if (reasonCodesById.TryGetValue(line.ReasonCodeId, out reasonCode)
                        && (reasonCode.PrintInputNameOnReceipt || reasonCode.PrintInputToReceipt || reasonCode.PrintPromptToReceipt))
                    {
                        comments.Append(line.Information).Append(Environment.NewLine);
                    }
                }

                return comments.ToString();
            }

            /// <summary>
            /// Changes the print behavior of all printers.
            /// </summary>
            /// <param name="printers">The collection of printers to be changed.</param>
            /// <param name="print">A boolean value indicating whether or not this printer will print the receipt.</param>
            private static void ChangePrinterBehavior(ICollection<Printer> printers, bool print)
            {
                foreach (Printer printer in printers)
                {
                    if (print)
                    {
                        printer.PrintBehavior = PrintBehavior.Always;
                    }
                    else
                    {
                        printer.PrintBehavior = PrintBehavior.Never;
                    }
                }
            }

            /// <summary>
            /// Checks if the input has meaningful value or not. "1/1/1753" is not considered as a meaningful value.
            /// </summary>
            /// <param name="dateTime">The date time to be checked.</param>
            /// <returns>A boolean value to indicate whether or not the value is meaningful.</returns>
            private static bool HasMeaningfulValue(DateTimeOffset dateTime)
            {
                return dateTime > sqlDateTimeMinValue;
            }

            /// <summary>
            /// Formats the date.
            /// </summary>
            /// <param name="date">The date to be formatted.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The formatted value if the date is not null and has meaningful value, otherwise "N/A".</returns>
            private static string FormatDate(DateTimeOffset? date, RequestContext context)
            {
                if (!date.HasValue || !HasMeaningfulValue(date.Value))
                {
                    return GetLocalizedString(LocalizationNotApplicable, context);
                }
                else
                {
                    var request = new GetFormattedDateServiceRequest(date.Value);
                    string result = context.Execute<GetFormattedContentServiceResponse>(request).FormattedValue;
                    return result;
                }
            }

            /// <summary>
            /// Formats the number.
            /// </summary>
            /// <param name="number">The number to be formatted.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The formatted number.</returns>
            private static string FormatNumber(decimal number, RequestContext context)
            {
                var request = new GetFormattedNumberServiceRequest(number);
                string result = context.Execute<GetFormattedContentServiceResponse>(request).FormattedValue;
                return result;
            }

            /// <summary>
            /// Gets the language to translate the receipt content.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The language identifier.</returns>
            private static string GetLanguage(RequestContext context)
            {
                ChannelConfiguration channelConfig = context.GetChannelConfiguration();
                string language = string.IsNullOrEmpty(channelConfig.DefaultLanguageId) ? channelConfig.CompanyLanguageId : channelConfig.DefaultLanguageId;

                return language;
            }

            /// <summary>
            /// Gets the localized string.
            /// </summary>
            /// <param name="textId">The text identifier.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The localized string.</returns>
            private static string GetLocalizedString(string textId, RequestContext context)
            {
                string language = GetLanguage(context);
                string result = localizer.Value.GetLocalizedString(language, textId);

                return result == null ? string.Empty : result;
            }
        }
    }
}
