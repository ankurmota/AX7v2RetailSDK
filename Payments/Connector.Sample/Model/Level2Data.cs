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

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
namespace Contoso
{
    namespace Retail.SampleConnector.Portable
    {
        using System;
        using System.Collections.Generic;

        internal class Level2Data
        {
            internal Level2Data()
            {
            }

            internal DateTime? OrderDateTime { get; set; }

            internal string OrderNumber { get; set; }

            internal DateTime? InvoiceDateTime { get; set; }

            internal string InvoiceNumber { get; set; }

            internal string OrderDescription { get; set; }

            internal string SummaryCommodityCode { get; set; }

            internal string MerchantContact { get; set; }

            internal string MerchantTaxId { get; set; }

            internal string MerchantType { get; set; }

            internal string PurchaserId { get; set; }

            internal string PurchaserTaxId { get; set; }

            internal string ShipToCity { get; set; }

            internal string ShipToCounty { get; set; }

            internal string ShipToState_ProvinceCode { get; set; }

            internal string ShipToPostalCode { get; set; }

            internal string ShipToCountryCode { get; set; }

            internal string ShipFromCity { get; set; }

            internal string ShipFromCounty { get; set; }

            internal string ShipFromState_ProvinceCode { get; set; }

            internal string ShipFromPostalCode { get; set; }

            internal string ShipFromCountryCode { get; set; }

            internal decimal? DiscountAmount { get; set; }

            internal decimal? MiscCharge { get; set; }

            internal decimal? DutyAmount { get; set; }

            internal decimal? FreightAmount { get; set; }

            internal bool? IsTaxable { get; set; }

            internal decimal? TotalTaxAmount { get; set; }

            internal decimal? TotalTaxRate { get; set; }

            internal string MerchantName { get; set; }

            internal string MerchantStreet { get; set; }

            internal string MerchantCity { get; set; }

            internal string MerchantState { get; set; }

            internal string MerchantCounty { get; set; }

            internal string MerchantCountryCode { get; set; }

            internal string MerchantZip { get; set; }

            internal decimal? TaxRate { get; set; }

            internal decimal? TaxAmount { get; set; }

            internal string TaxDescription { get; set; }

            internal string TaxTypeIdentifier { get; set; }

            internal string RequesterName { get; set; }

            internal decimal? TotalAmount { get; set; }

            internal string PurchaseCardType { get; set; }

            internal string AmexLegacyDescription1 { get; set; }

            internal string AmexLegacyDescription2 { get; set; }

            internal string AmexLegacyDescription3 { get; set; }

            internal string AmexLegacyDescription4 { get; set; }

            internal IEnumerable<TaxDetail> TaxDetails { get; set; }

            internal IEnumerable<MiscellaneousCharge> MiscellaneousCharges { get; set; }
        }
    }
}
