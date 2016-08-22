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

        internal class Level3Data
        {
            internal Level3Data()
            {
            }

            internal string SequenceNumber { get; set; }

            internal string CommodityCode { get; set; }

            internal string ProductCode { get; set; }

            internal string ProductName { get; set; }

            internal string ProductSKU { get; set; }

            internal string Descriptor { get; set; }

            internal string UnitOfMeasure { get; set; }

            internal decimal? UnitPrice { get; set; }

            internal decimal? Discount { get; set; }

            internal decimal? DiscountRate { get; set; }

            internal decimal? Quantity { get; set; }

            internal decimal? MiscCharge { get; set; }

            internal decimal? NetTotal { get; set; }

            internal decimal? TaxAmount { get; set; }

            internal decimal? TaxRate { get; set; }

            internal decimal? TotalAmount { get; set; }

            internal string CostCenter { get; set; }

            internal decimal? FreightAmount { get; set; }

            internal decimal? HandlingAmount { get; set; }

            internal string CarrierTrackingNumber { get; set; }

            internal string MerchantTaxID { get; set; }

            internal string MerchantCatalogNumber { get; set; }

            internal string TaxCategoryApplied { get; set; }

            internal string PickupAddress { get; set; }

            internal string PickupCity { get; set; }

            internal string PickupState { get; set; }

            internal string PickupCounty { get; set; }

            internal string PickupZip { get; set; }

            internal string PickupCountry { get; set; }

            internal DateTime? PickupDateTime { get; set; }

            internal string PickupRecordNumber { get; set; }

            internal string CarrierShipmentNumber { get; set; }

            internal string UNSPSCCode { get; set; }

            internal IEnumerable<TaxDetail> TaxDetails { get; set; }

            internal IEnumerable<MiscellaneousCharge> MiscellaneousCharges { get; set; }
        }
    }
}
