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
    "use strict";

    export interface StockCountLine {
        originalLine: StockCountJournalTransaction;
        recordId: number;
        itemId: string;
        inventColorId: string;
        configId: string;
        inventSizeId: string;
        inventStyleId: string;
        colorTranslation?: string;
        configurationTranslation?: string;
        sizeTranslation?: string;
        styleTranslation?: string;
        itemName: string;
        counted: number;
        quantity: number;
        status: StockCountStatus;
    }

    export class StockCountLineClass implements StockCountLine {
        private _originalLine: StockCountJournalTransaction;
        private _colorTranslation: string;
        private _configurationTranslation: string;
        private _sizeTranslation: string;
        private _styleTranslation: string;

        public get originalLine(): StockCountJournalTransaction {
            return this._originalLine;
        }

        public get recordId(): number {
            return this._originalLine.RecordId;
        }

        public get itemId(): string {
            return this._originalLine.ItemId;
        }

        public get colorTranslation(): string {
            return this._colorTranslation;
        }

        public get configurationTranslation(): string {
            return this._configurationTranslation;
        }

        public get sizeTranslation(): string {
            return this._sizeTranslation;
        }

        public get styleTranslation(): string {
            return this._styleTranslation;
        }

        public get inventColorId(): string {
            return this._originalLine.InventColorId;
        }

        public get configId(): string {
            return this._originalLine.ConfigId;
        }

        public get inventSizeId(): string {
            return this._originalLine.InventSizeId;
        }

        public get inventStyleId(): string {
            return this._originalLine.InventStyleId;
        }

        public get itemName(): string {
            return this._originalLine.ItemName;
        }

        public set counted(newCounted: number) {
            this._originalLine.Counted = newCounted;
        }

        public get counted(): number {
            return this._originalLine.Counted;
        }

        public set quantity(newQuantity: number) {
            this._originalLine.Quantity = newQuantity;
        }

        public get quantity(): number {
            return this._originalLine.Quantity;
        }

        public set status(newStatus: StockCountStatus) {
            this._originalLine.Status = newStatus;
        }

        public get status(): StockCountStatus {
            return this._originalLine.Status;
        }

        constructor(commerceStockCountLine: StockCountJournalTransaction, productVariant: ProductVariant) {
            this._originalLine = commerceStockCountLine;
            this._colorTranslation = !StringExtensions.isNullOrWhitespace(productVariant.Color) ? productVariant.Color : productVariant.ColorId;
            this._configurationTranslation = !StringExtensions.isNullOrWhitespace(productVariant.Configuration) ? productVariant.Configuration : productVariant.ConfigId;
            this._sizeTranslation = !StringExtensions.isNullOrWhitespace(productVariant.Size) ? productVariant.Size : productVariant.SizeId;
            this._styleTranslation = !StringExtensions.isNullOrWhitespace(productVariant.Style) ? productVariant.Style : productVariant.StyleId;
        }
    }
}
