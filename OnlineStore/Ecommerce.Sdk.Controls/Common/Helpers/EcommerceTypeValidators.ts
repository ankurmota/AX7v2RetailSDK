/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    var stringValidationErrorMessage = Resources.String_72;
    var stringValidationRegex = Resources.String_73;


    /**
     * Interface for FieldValidator constructor parameters.
     * To be able to initialize any subset of FieldValidator fields.
     * @param {number} [maxLength] - maxLength HTML5 input attribute.
     * @param {number} [max] - max HTML5 input attribute.
     * @param {number} [min] - min HTML5 input attribute.
     * @param {string} [pattern] - pattern HTML5 input attribute.
     * @param {boolean} [required] - required HTML5 input attribute.
     */
    export interface IFieldValidatorParams {
        maxLength?: number;
        max?: number;
        min?: number;
        pattern?: string;
        required?: boolean;
        title?: string;
    };

    /**
    * Validator class for entity fields. Sets up HTML5 attributes.
    */
    export class FieldValidator {
        private _validationAttributes: IFieldValidatorParams;

        /**
        * Constructs instance of FieldValidator class. Sets up HTML5 validation attributes.
        *
        * @param {IFieldValidatorParams} params - HTML5 validation attributes to set.
        */
        constructor(params: IFieldValidatorParams) {
            this._validationAttributes = params;
        }

        /**
         * Sets validation attributes to an element.
         *
         * @param {Element} element to set validation attributes for.
         */
        public setValidationAttributes(element: Element): void {
            for (var attrName in this._validationAttributes) {
                if (attrName != "title") {
                    var value: any = this._validationAttributes[attrName];
                    if (value) {
                        element.setAttribute(attrName, value);
                    }

                    // Remove "required" attribute from element if it is not set to true.
                    if (this._validationAttributes.required !== true) {
                        element.removeAttribute("required");
                    }
                }
            }
        }

        /**
         * Sets title attribute to an element if the validation on element fails.
         * The title attribute will indicate what the expected value for element is.
         *
         * @param {Element} element to check validation and set title attribute for.
         */
        public setTitleAttributeIfInvalid(element: Element): void {
            var value: any = this._validationAttributes["title"];
            if (value && element.getAttribute("msax-isValid") == "false") {
                element.setAttribute("title", value);
            }
            else {
                element.removeAttribute("title");
            }
        }
    }

    /**
     * Base class for entity validators.
     * Fields of derived classes are used for validation.
     */
    export class EntityValidatorBase {

        constructor() {
        }

        /**
         * Set validation attributes of specified field to an element.
         *
         * @param {Element} element to set validation attributes for.
         * @param {string} fieldName - field of validation object to be used for attributes set.
         */
        public setValidationAttributes(element: Element, fieldName: string): void {
            var fieldValidator: FieldValidator = this[fieldName];
            if (fieldValidator) {
                fieldValidator.setValidationAttributes(element);
            }
        }
    }

    /**
     * Validator for ShoppingCartItem entity.
     */
    export class ShoppingCartItemValidator extends EntityValidatorBase {
        public Quantity: IFieldValidatorParams;

        constructor() {
            super();

            this.Quantity = new FieldValidator({
                maxLength: 3, required: true, title: Resources.String_74 });
        }
    }

    /**
     * Validator for SelectedOrderDeliveryOption entity.
     */
    export class SelectedOrderDeliveryOptionValidator extends EntityValidatorBase {
        public DeliveryModeId: IFieldValidatorParams;
        public DeliveryModeText: IFieldValidatorParams;
        public DeliveryPreferenceId: IFieldValidatorParams;
        public CustomAddress: CommerceProxy.Entities.Address;
        public ElectronicDeliveryEmail: IFieldValidatorParams;
        public ElectronicDeliveryEmailContent: IFieldValidatorParams;

        constructor() {
            super();

            this.DeliveryModeId = new FieldValidator({
                required: true, title: Resources.String_75
            });
        }
    }

    /**
     * Validator for Address entity.
     */
    export class CustomerValidator extends EntityValidatorBase {
        public FirstName: IFieldValidatorParams;
        public MiddleName: IFieldValidatorParams;
        public LastName: IFieldValidatorParams;
        public Name: IFieldValidatorParams;

        constructor() {
            super();

            this.FirstName = new FieldValidator({ maxLength: 25, required: true, title: stringValidationErrorMessage, pattern: stringValidationRegex });
            this.MiddleName = new FieldValidator({ maxLength: 25, title: stringValidationErrorMessage, pattern: stringValidationRegex });
            this.LastName = new FieldValidator({ maxLength: 25, required: true, title: stringValidationErrorMessage, pattern: stringValidationRegex });
            this.Name = new FieldValidator({ maxLength: 100, required: true });
        }
    }

    /**
     * Validator for Address entity.
     */
    export class AddressValidator extends EntityValidatorBase {
        public Phone;
        public Url;
        public Email;
        public Name;
        public StreetNumber;
        public Street;
        public City;
        public ZipCode;
        public State;
        public Country;

        constructor() {
            super();

            this.Phone = new FieldValidator({ maxLength: 20 });
            this.Url = new FieldValidator({ maxLength: 255 });
            this.Email = new FieldValidator({ maxLength: 80, required: true, title: Resources.String_76, pattern: "^[-0-9a-zA-Z.+_]+@[-0-9a-zA-Z.+_]+.[a-zA-Z]{2,4}$" });
            this.Name = new FieldValidator({ maxLength: 60, required: true, title: Resources.String_77 });
            this.StreetNumber = new FieldValidator({ maxLength: 20, title: Resources.String_78 });
            this.Street = new FieldValidator({ maxLength: 250, required: true, title: Resources.String_79 });
            this.City = new FieldValidator({ maxLength: 60, required: true, title: Resources.String_80 });
            this.ZipCode = new FieldValidator({ maxLength: 10, required: true, title: Resources.String_81 });
            this.State = new FieldValidator({ maxLength: 10, required: true, title: Resources.String_82 });
            this.Country = new FieldValidator({ required: true, title: Resources.String_83 });
        }
    }

    /**
     * Validator for PaymentCardLine entity.
     */
    export class PaymentCardTypeValidator extends EntityValidatorBase {
        public NameOnCard;
        public CardNumber;
        public CCID;
        public PaymentAmount;
        public ExpirationMonth;
        public ExpirationYear;

        constructor() {
            super();

            this.NameOnCard = new FieldValidator({ maxLength: 100, required: true, title: Resources.String_84 });
            this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Resources.String_85 }); //, pattern: "^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$" });
            this.CCID = new FieldValidator({ maxLength: 50, required: true, title: Resources.String_86, pattern: "^[0-9]{3,4}$" });
            this.PaymentAmount = new FieldValidator({ maxLength: 100, required: true, title: Resources.String_87, pattern: "\w+([0123456789.]\w+)*" });
        }
    }

    /**
     * Validator for gift card entity.
     */
    export class GiftCardTypeValidator extends EntityValidatorBase {
        public CardNumber;
        public PaymentAmount;

        constructor() {
            super();

            this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Resources.String_141 }); // Please enter a valid gift card number
            this.PaymentAmount = new FieldValidator({ maxLength: 100, required: true, title: Resources.String_87 }); //Please specify a valid amount
        }
    }

    /**
     * Validator for loyalty card entity.
     */
    export class LoyaltyCardTypeValidator extends EntityValidatorBase {
        public CardNumber;
        public PaymentAmount;

        constructor() {
            super();

            this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Resources.String_151 }); // Please enter a valid loyalty card number
            this.PaymentAmount = new FieldValidator({ maxLength: 100, required: true, title: Resources.String_87 }); //Please specify a valid amount
        }
    }

    /**
     * Validator for discount code entity.
     */
    export class DiscountCardTypeValidator extends EntityValidatorBase {
        public CardNumber;

        constructor() {
            super();

            this.CardNumber = new FieldValidator({ maxLength: 100, required: true, title: Resources.String_184 }); // Please enter a valid discount code
        }
    }
} 