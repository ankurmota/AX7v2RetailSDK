/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {

    /**
     * List of available customer order operations
     */
    export enum CustomerOrderOperations {
        Edit = 0,
        Cancel = 1,
        PickUpFromStore = 2,
        CreatePickingList = 3,
        CreatePackingSlip = 4,
        PrintPackingSlip = 5,
        Return = 6,
    }

    /**
     * Parameters for customer order update operation
     */
    export interface CustomerOrderOperationParameters {
        UpdateParameter?: CustomerOrderCreateOrUpdateParameter;
        CreatePickingListParameter?: CustomerOrderPickingAndPackingParameter;
        CreatePackingSlipParameter?: CustomerOrderPickingAndPackingParameter;
        PickUpInStoreParameter?: CustomerOrderPickUpInStoreParameter;
        ReturnCustomerOrderParameter?: ReturnCustomerOrderParameter;
    }

    export interface CustomerOrderPaymentCard {
        cardTypeId: string;
        paymentCard: Entities.PaymentCard;
        tenderTypeId: string;
        cardTypeValue: Entities.CardType;
    }

    export interface ReturnCustomerOrderParameter {
        returnCart: Model.Entities.Cart;
    }

    export interface CustomerOrderCreateOrUpdateParameter {
        CustomerOrderModeValue: Model.Entities.CustomerOrderMode;
    }

    export interface CustomerOrderPickingAndPackingParameter {
        SalesId: string;
    }

    export interface CustomerOrderPickUpInStoreParameter {
        CartLines: Model.Entities.CartLine[];
    }

    /**
     * List of available customer order recall operations
     */
    export enum CustomerOrderRecallType {
        OrderRecall = 0,
        QuoteRecall = 1,
    }

    export interface SalesInvoiceLineWithReasonCodes {
        salesInvoiceLine: SalesInvoiceLine;
        reasonCodeLines: ReasonCode[];
    }
}