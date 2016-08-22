/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='EntityValidatorBase.ts'/>
///<reference path='FieldValidator.ts'/>

module Commerce.Proxy.Entities {
    "use strict";

    /**
     * Validator for Address entity.
     */
    export class AddressValidator extends EntityValidatorBase {
        public Phone;
        public Url;
        public Email;
        public Street;
        public City;
        public ZipCode;
        public County;
        public DistrictName;
        public BuildingCompliment;
        public StreetNumber;

        constructor() {
            super();

            this.Phone = new FieldValidator({ maxLength: 255 });
            this.Url = new FieldValidator({ maxLength: 255 });
            this.Email = new EmailFieldValidator(Commerce.ViewModelAdapter.getResourceString("string_1331"));
            this.Street = new FieldValidator({ maxLength: 150, required: false});
            this.City = new FieldValidator({ maxLength: 60, required: false});
            this.ZipCode = new FieldValidator({ maxLength: 10, required: false });
            this.County = new FieldValidator({ maxLength: 10, required: false });
            this.DistrictName = new FieldValidator({ maxLength: 60, required: false });
            this.BuildingCompliment = new FieldValidator({ maxLength: 60 });
            this.StreetNumber = new FieldValidator({ maxLength: 20 });
        }
    }

    /**
     * Validator for Cart entity.
     */
    export class CartValidator extends EntityValidatorBase {
        public CancellationChargeAmount;
        public OverriddenDepositAmount;
        public ShippingChargeAmount;

        constructor() {
            super();

            this.CancellationChargeAmount = new FieldValidator({ maxLength: 10 });
            this.OverriddenDepositAmount = new FieldValidator({ maxLength: 10 });
            this.ShippingChargeAmount = new FieldValidator({ maxLength: 10 });
        }
    }

    /**
     * Validator for Cart Line entity.
     */
    export class CartLineValidator extends EntityValidatorBase {
        public LineManualDiscountAmount;
        public PriceOverrideAmount;
        public ItemQuantity;
        public ItemSerialNumber;

        constructor() {
            super();

            this.LineManualDiscountAmount = new FieldValidator({ maxLength: 10 });
            this.PriceOverrideAmount = new FieldValidator({ maxLength: 10 });
            this.ItemQuantity = new FieldValidator({ maxLength: 10 });
            this.ItemSerialNumber = new FieldValidator({ maxLength: 20 });
        }
    }

    /**
     * Validator for PaymentCardLine entity.
     */
    export class PaymentCardValidator extends EntityValidatorBase {
        public CardNumber;
        public CCID;
        public VoiceAuthorizationCode;
        public CardAddress;
        public CardAddressZip;

        constructor() {
            super();
            this.CardNumber = new FieldValidator({ maxLength: 19 }); // maxLength according to the ISO/IEC 7812 standart
            this.CCID = new FieldValidator({ maxLength: 10 });
            this.VoiceAuthorizationCode = new FieldValidator({ maxLength: 50 });
            this.CardAddress = new FieldValidator({ maxLength: 150 });
            this.CardAddressZip = new FieldValidator({ maxLength: 10 });
        }
    }
    
    /**
     * Validator for ConnectionRequest entity.
     */
    export class ConnectionRequestValidator extends EntityValidatorBase {
        public UserId;
        public Password;

        constructor() {
            super();

            this.UserId = new FieldValidator({ maxLength: 30});
            this.Password = new FieldValidator({ maxLength: 30 });
        }
    }

    /**
     * Validator for Customer entity.
     */
    export class CustomerValidator extends EntityValidatorBase {
        public FirstName;
        public MiddleName;
        public LastName;
        public Name;

        private static _nameRegex: string = "^(?=\\s*\\S).*$"; // not empty or whitespace
         
        constructor() {
            super();

            // v-dabull: note, adding a pattern attribute also requires you to add a title attribute which contains
            // a textual UI description to the user about how the input field must be formatted.

            this.FirstName = new FieldValidator({ maxLength: 25, required: true, title: Commerce.ViewModelAdapter.getResourceString("string_1361"), pattern: CustomerValidator._nameRegex });
            this.MiddleName = new FieldValidator({ maxLength: 25, title: Commerce.ViewModelAdapter.getResourceString("string_1361"), pattern: CustomerValidator._nameRegex });
            this.LastName = new FieldValidator({ maxLength: 25, required: true, title: Commerce.ViewModelAdapter.getResourceString("string_1361"), pattern: CustomerValidator._nameRegex });
            this.Name = new FieldValidator({ maxLength: 100, required: true });
        }
    }
    
    /**
     * Validator for DeliveryOption entity.
     */
    export class DeliveryOptionValidator extends EntityValidatorBase {
        public Price;

        constructor() {
            super();

            this.Price = new FieldValidator({ maxLength: 10 });
        }
    }

    /**
     * Validator for GiftCard entity.
     */
    export class GiftCardValidator extends EntityValidatorBase {
        public CardNumber;
        public Amount;

        constructor() {
            super();

            this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Commerce.ViewModelAdapter.getResourceString("string_4362"), pattern: "^[A-Za-z0-9]+$" });
            this.Amount = new FieldValidator({ maxLength: 10 });
        }
    }

    /**
     * Validator for LoyaltyCard entity.
     */
    export class LoyaltyCardValidator extends EntityValidatorBase {
        public CardNumber;

        constructor() {
            super();

            this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Commerce.ViewModelAdapter.getResourceString("string_4362"), pattern: "^[A-Za-z0-9]+$" });
        }
    }
    
    /**
     * Validator for SalesOrderSearchCriteria entity.
     */
    export class SalesOrderSearchCriteriaValidator extends EntityValidatorBase {
        public TransactionId;
        public SalesId;
        public ReceiptId;
        public ChannelReferenceId;
        public CustomerAccountNumber;
        public CustomerFirstName;
        public CustomerLastName;
        public StoreId;
        public TerminalId;
        public ItemId;
        public Barcode;
        public StaffId;
        public ReceiptEmailAddress;

        constructor() {
            super();

            var regexForId = "^[A-Za-z0-9#-]+$";
            var lengthForId = 30;
            var errorForId = Commerce.ViewModelAdapter.getResourceString("string_4362");

            var regexForName = "^[A-Za-z]+$";
            var lengthForName = 25;
            var errorForName = Commerce.ViewModelAdapter.getResourceString("string_1361");

            this.TransactionId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.SalesId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.ReceiptId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.ChannelReferenceId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.CustomerAccountNumber = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.CustomerFirstName = new FieldValidator({ maxLength: lengthForName, title: errorForName, pattern: regexForName });
            this.CustomerLastName = new FieldValidator({ maxLength: lengthForName, title: errorForName, pattern: regexForName });
            this.StoreId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.TerminalId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.ItemId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.Barcode = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.StaffId = new FieldValidator({ maxLength: lengthForId, title: errorForId, pattern: regexForId });
            this.ReceiptEmailAddress = new EmailFieldValidator(Commerce.ViewModelAdapter.getResourceString("string_1331"));
        }
    }

    /**
     * Validator for Search.
     */
    export class SearchValidator extends EntityValidatorBase {
        public SearchText;
        public SearchReceiptText; // Validator for input text for Search Receipts
        public SearchOrderText;

        constructor() {
            super();

            this.SearchText = new FieldValidator({ maxLength: 30 });
            this.SearchReceiptText = new FieldValidator({ maxLength: 30 });
            this.SearchOrderText = new FieldValidator({ maxLength: 50, required: true });
        }
    }
}