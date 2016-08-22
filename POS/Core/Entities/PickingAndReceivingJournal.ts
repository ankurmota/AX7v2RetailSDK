/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

//<reference path ='../ProductPropertiesHelper.ts'/>
//<reference path ='../ObjectExtensions.ts'/>
//<reference path ='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {
    /**
     * Types of picking and receiving operation handler
     */
    export enum PickingAndReceivingOperationType {
        Save = 1,
        Commit = 2,
        GetJournalDetails = 3,
        GetAllJournals = 4
    }

    export class PickingAndReceivingOrderHelper {

        private static getUnitOfMeasure(product: Model.Entities.Product): string {
            var unitOfMeasure: string = "";
            var unitOfMeasureCollection: Model.Entities.UnitOfMeasure[] = ProductPropertiesHelper.GetUnitOfMeasures(product);

            if (ArrayExtensions.hasElements(unitOfMeasureCollection) && !StringExtensions.isNullOrWhitespace(unitOfMeasureCollection[0].Symbol)) {
                unitOfMeasure = unitOfMeasureCollection[0].Symbol;
            }

            return unitOfMeasure;
        }

        public static createPickingAndReceivingOrderLine(
            variantId: number,
            product: Model.Entities.Product,
            quantityOrdered: number,
            quantityReceived: number,
            quantityReceivedNow: number,
            orderType: Model.Entities.PurchaseTransferOrderType,
            orderId: string): PickingAndReceivingOrderLine {

            var productVariant: ProductVariant = new ProductVariantClass();

            if (product.IsMasterProduct) {
                productVariant = ProductPropertiesHelper.getVariant(variantId, product);
            }

            switch (orderType) {
                case Model.Entities.PurchaseTransferOrderType.PurchaseOrder:
                    return new PickingAndReceivingPurchaseOrderLine({
                        RecordId: 0,
                        OrderId: orderId,
                        ItemId: ProductPropertiesHelper.getProperty(product.RecordId, product, ProductPropertyNameEnum.ProductNumber),
                        ItemName: ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.ProductName),
                        InventColorId: productVariant.ColorId,
                        ConfigId: productVariant.ConfigId,
                        InventSizeId: productVariant.SizeId,
                        InventStyleId: productVariant.StyleId,
                        QuantityOrdered: quantityOrdered,
                        PurchaseQuantity: quantityReceivedNow,
                        PurchaseUnit: PickingAndReceivingOrderHelper.getUnitOfMeasure(product),
                        PurchaseReceived: quantityReceived,
                        PurchaseReceivedNow: quantityReceivedNow,
                    }, productVariant);
                    break;
                case Model.Entities.PurchaseTransferOrderType.TransferIn:
                    return new PickingAndReceivingTransferInOrderLine({
                        RecordId: 0,
                        OrderId: orderId,
                        ItemId: ProductPropertiesHelper.getProperty(product.RecordId, product, ProductPropertyNameEnum.ProductNumber),
                        ItemName: ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.ProductName),
                        InventColorId: productVariant.ColorId,
                        ConfigId: productVariant.ConfigId,
                        InventSizeId: productVariant.SizeId,
                        InventStyleId: productVariant.StyleId,
                        QuantityTransferred: quantityOrdered,
                        QuantityReceived: quantityReceived,
                        QuantityReceiveNow: quantityReceivedNow,
                        QuantityRemainReceive: quantityOrdered - quantityReceived,
                        PurchaseUnit: PickingAndReceivingOrderHelper.getUnitOfMeasure(product),
                    }, productVariant);
                    break;
                case Model.Entities.PurchaseTransferOrderType.TransferOut:
                    return new PickingAndReceivingTransferOutOrderLine({
                        RecordId: 0,
                        OrderId: orderId,
                        ItemId: ProductPropertiesHelper.getProperty(product.RecordId, product, ProductPropertyNameEnum.ProductNumber),
                        ItemName: ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.ProductName),
                        InventColorId: productVariant.ColorId,
                        ConfigId: productVariant.ConfigId,
                        InventSizeId: productVariant.SizeId,
                        InventStyleId: productVariant.StyleId,
                        QuantityTransferred: quantityOrdered,
                        QuantityShipped: quantityReceived,
                        QuantityShipNow: quantityReceivedNow,
                        QuantityRemainShip: quantityOrdered - quantityReceived,
                        PurchaseUnit: PickingAndReceivingOrderHelper.getUnitOfMeasure(product),
                    }, productVariant);
                    break;
            }

            return null;
        }

        public static createJournal(orderType: Model.Entities.PurchaseTransferOrderType, entity: any): PickingAndReceivingOrder {
            if (ObjectExtensions.isNullOrUndefined(entity)) {
                return null;
            }

            var result: Model.Entities.PickingAndReceivingOrder = null;
            switch (orderType) {
                case Model.Entities.PurchaseTransferOrderType.PurchaseOrder:
                    result = new PickingAndReceivingPurchaseOrder(entity);
                    break;
                case Model.Entities.PurchaseTransferOrderType.TransferIn:
                    result = new PickingAndReceivingTransferInOrder(entity);
                    break;
                case Model.Entities.PurchaseTransferOrderType.TransferOut:
                    result = new PickingAndReceivingTransferOutOrder(entity);
                    break;
                case Model.Entities.PurchaseTransferOrderType.PickingList:
                    result = new PickingAndReceivingPickingList(entity);
                    break;
                default:
                    throw "Unsupported order type: "+ orderType;
            }

            return result;
        }

        public static convertToCommerceTypes(genericJournal: PickingAndReceivingOrder): any {
            var originalJournal = genericJournal.originalOrder;

            var productLines: any[] = originalJournal.OrderLines;
            var originalJournalLinesLength: number = productLines.length;
            var genericJournalLinesLength: number = genericJournal.orderLines.length;
            var counter: number = originalJournalLinesLength;

            //add products that are newly added from client
            for (var counter = originalJournalLinesLength; counter < genericJournalLinesLength; counter++) {
                productLines.push(genericJournal.orderLines[counter].originalLine);
            }

            return originalJournal;
        }
    }

    export interface PickingAndReceivingOrderLine {
        recordId?: number;
        productNumber?: string;
        description?: string;
        quantityOrdered?: number;
        quantityReceived?: number;
        quantityReceivedNow?: number;
        unitOfMeasure?: string;
        colorId?: string;
        configurationId?: string;
        sizeId?: string;
        styleId?: string;
        colorTranslation?: string;
        configurationTranslation?: string;
        sizeTranslation?: string;
        styleTranslation?: string;
        inventDimId?: string;
        originalLine: any;
    }

    export interface PickingAndReceivingOrder {
        orderId?: string;
        orderType?: number;
        status?: string;
        lines?: number;
        totalOrdered?: number;
        totalReceived?: number;
        totalReceivedNow?: number;
        orderLines?: PickingAndReceivingOrderLine[];
        originalOrder?: any;
        allLinesReceived?: boolean;
    }

    export class PickingAndReceivingPickingListLine implements PickingAndReceivingOrderLine {
        private _pickingListLine: Model.Entities.PickingListLine;
        private _colorTranslation: string;
        private _configurationTranslation: string;
        private _sizeTranslation: string;
        private _styleTranslation: string;
        
        constructor(pickingListLine: Entities.PickingListLine, productVariant: Entities.ProductVariant, index: number) {
            this._pickingListLine = pickingListLine;
            this._colorTranslation = productVariant.Color;
            this._configurationTranslation = productVariant.Configuration;
            this._sizeTranslation = productVariant.Size;
            this._styleTranslation = productVariant.Style;
        }

        public get recordId(): number {
            return this._pickingListLine.RecordId;
        }

        public get productNumber(): string {
            return this._pickingListLine.ItemId;
        }

        public get description(): string {
            return this._pickingListLine.ItemName;
        }

        public get quantityOrdered(): number {
            return this._pickingListLine.QuantityOrdered;
        }

        public get quantityReceived(): number {
            return 0; // not implemented on server
        }

        public get quantityReceivedNow(): number {
            return this._pickingListLine.PurchaseReceivedNow;
        }

        public set quantityReceivedNow(newReceivedNow: number) {
            this._pickingListLine.PurchaseReceivedNow = newReceivedNow;
        }

        public get unitOfMeasure(): string {
            return "";
        }

        public get colorId(): string {
            return this._pickingListLine.InventColorId;
        }

        public get configurationId(): string {
            return this._pickingListLine.ConfigId;
        }

        public get sizeId(): string {
            return this._pickingListLine.InventSizeId;
        }

        public get styleId(): string {
            return this._pickingListLine.InventStyleId;
        }

        public get colorTranslation(): string {
            return this._colorTranslation;
        }

        public set colorTranslation(newColorValue: string) {
            this._colorTranslation = newColorValue;
        }

        public get configurationTranslation(): string {
            return this._configurationTranslation;
        }

        public set configurationTranslation(newConfigurationValue: string) {
            this._configurationTranslation = newConfigurationValue;
        }

        public get sizeTranslation(): string {
            return this._sizeTranslation;
        }

        public set sizeTranslation(newSizeValue: string) {
            this._sizeTranslation = newSizeValue;
        }

        public get styleTranslation(): string {
            return this._styleTranslation;
        }

        public set styleTranslation(newStyleValue: string) {
            this._styleTranslation = newStyleValue;
        }

        public get inventDimId(): string {
            return this._pickingListLine.InventDimId;
        }

        public get originalLine(): any {
            return this._pickingListLine;
        }
    }

    export class PickingAndReceivingPurchaseOrderLine implements PickingAndReceivingOrderLine {
        private _purchaseOrderLine: Entities.PurchaseOrderLine;
        private _colorTranslation: string;
        private _configurationTranslation: string;
        private _sizeTranslation: string;
        private _styleTranslation: string;

        constructor(purchaseOrderLine: Entities.PurchaseOrderLine, productVariant: Model.Entities.ProductVariant) {
            this._purchaseOrderLine = purchaseOrderLine;
            this._colorTranslation = productVariant.Color;
            this._configurationTranslation = productVariant.Configuration;
            this._sizeTranslation = productVariant.Size;
            this._styleTranslation = productVariant.Style;
        }

        public get recordId(): number {
            return this._purchaseOrderLine.RecordId;
        }

        public get productNumber(): string {
            return this._purchaseOrderLine.ItemId;
        }

        public get description(): string {
            return this._purchaseOrderLine.ItemName;
        }

        public get quantityOrdered(): number {
            return this._purchaseOrderLine.QuantityOrdered;
        }

        public get quantityReceived(): number {
            return this._purchaseOrderLine.PurchaseReceived;
        }

        public get quantityReceivedNow(): number {
            return this._purchaseOrderLine.PurchaseReceivedNow;
        }

        public set quantityReceivedNow(newReceivedNow: number) {
            this._purchaseOrderLine.PurchaseReceivedNow = newReceivedNow;
        }

        public get unitOfMeasure(): string {
            return this._purchaseOrderLine.PurchaseUnit;
        }

        public get colorId(): string {
            return this._purchaseOrderLine.InventColorId;
        }

        public get configurationId(): string {
            return this._purchaseOrderLine.ConfigId;
        }

        public get sizeId(): string {
            return this._purchaseOrderLine.InventSizeId;
        }

        public get styleId(): string {
            return this._purchaseOrderLine.InventStyleId;
        }

        public get colorTranslation(): string {
            return this._colorTranslation;
        }

        public set colorTranslation(newColorValue: string) {
            this._colorTranslation = newColorValue;
        }

        public get configurationTranslation(): string {
            return this._configurationTranslation;
        }

        public set configurationTranslation(newConfigurationValue: string) {
            this._configurationTranslation = newConfigurationValue;
        }

        public get sizeTranslation(): string {
            return this._sizeTranslation;
        }

        public set sizeTranslation(newSizeValue: string) {
            this._sizeTranslation = newSizeValue;
        }

        public get styleTranslation(): string {
            return this._styleTranslation;
        }

        public set styleTranslation(newStyleValue: string) {
            this._styleTranslation = newStyleValue;
        }

        public get inventDimId(): string {
            return this._purchaseOrderLine.InventDimId;
        }
        
        public get originalLine(): any {
            return this._purchaseOrderLine;
        }
    }

    export class PickingAndReceivingPurchaseOrder implements PickingAndReceivingOrder {

        private _purchaseOrder: Entities.PurchaseOrder;
        private _totalReceivedNow: number = 0;
        private _orderLines: Entities.PickingAndReceivingOrderLine[] = [];

        constructor(purchaseTransferOrder: Entities.PurchaseOrder) {
            this._purchaseOrder = purchaseTransferOrder;
            this.constructOrderLine();
        }

        private constructOrderLine(): void {
            var orderLines: PurchaseOrderLine[] = this._purchaseOrder.OrderLines;
            
            for (var i = 0; i < orderLines.length; i++) {
                this._orderLines.push(new PickingAndReceivingPurchaseOrderLine(orderLines[i], new ProductVariantClass()));
            }
        }

        public get orderId(): string {
            return this._purchaseOrder.OrderId;
        }

        public get recId(): string {
            return this._purchaseOrder.RecordId;
        }

        public get orderType(): number {
            return PurchaseTransferOrderType.PurchaseOrder;
        }

        public get status(): string {
            return this._purchaseOrder.Status;
        }

        public get lines(): number {
            return this._purchaseOrder.Lines;
        }

        public set lines(newLines: number) {
            this._purchaseOrder.Lines = newLines;
        }

        public get totalOrdered(): number {
            return this._purchaseOrder.TotalItems;
        }

        public get totalReceived(): number {
            return this._purchaseOrder.TotalReceived;
        }

        public get totalReceivedNow(): number {
            return this._totalReceivedNow;
        }

        public set totalReceivedNow(newReceivedNow: number) {
            this._totalReceivedNow = newReceivedNow;
        }

        public set orderLines(newLines: PickingAndReceivingOrderLine[]) {
            this._orderLines = newLines;
        }

        public get orderLines(): PickingAndReceivingOrderLine[] {
            return this._orderLines;
        }

        public get originalOrder(): any {
            return this._purchaseOrder;
        }

    }

    export class PickingAndReceivingPickingList implements PickingAndReceivingOrder {

        private _pickingList: Entities.PickingList;
        private _totalReceivedNow: number = 0;
        private _pickingLines: Entities.PickingAndReceivingOrderLine[] = [];

        constructor(pickingList: Entities.PickingList) {
            this._pickingList = pickingList;
            this.constructPickingLine();
        }

        private constructPickingLine(): void {
            var pickingLines: PickingListLine[] = this._pickingList.OrderLines;
            var productDetails: Product;
            var productVariant: ProductVariant;

            for (var i = 0; i < pickingLines.length; i++) {
                this._pickingLines.push(new PickingAndReceivingPickingListLine(pickingLines[i], new ProductVariantClass(), i));
            }
        }

        public get orderId(): string {
            return this._pickingList.OrderId;
        }

        public get recId(): string {
            return this._pickingList.RecordId;
        }

        public get orderType(): number {
            return PurchaseTransferOrderType.PickingList;
        }

        public get status(): string {
            return this._pickingList.Status;
        }

        public get lines(): number {
            return this._pickingList.OrderLines.length;
        }

        public set lines(newLines: number) {
            this._pickingList.Lines = newLines;
        }

        public get totalOrdered(): number {
            var totalOrdered: number = 0;
            this._pickingList.OrderLines.forEach(function (orderLine: Entities.PickingListLine) {
                totalOrdered += orderLine.QuantityOrdered
            });
            
            return totalOrdered;
        }

        public get totalReceived(): number {
            return 0; // not implemented
        }

        public get totalReceivedNow(): number {
            var totalReceivedNow: number = 0;
            this._pickingList.OrderLines.forEach(function (orderLine: Entities.PickingListLine) {
                totalReceivedNow += orderLine.PurchaseReceivedNow;
            });

            return totalReceivedNow;
        }

        public set totalReceivedNow(newReceivedNow: number) {
            this._totalReceivedNow = newReceivedNow;
        }

        public set orderLines(newLines: PickingAndReceivingOrderLine[]) {
            this._pickingLines = newLines;
        }

        public get orderLines(): PickingAndReceivingOrderLine[] {
            return this._pickingLines;
        }

        public get originalOrder(): any {
            return this._pickingList;
        }
    }

    export class PickingAndReceivingTransferInOrderLine implements PickingAndReceivingOrderLine {
        private _transferOrderLine: Entities.TransferOrderLine;
        private _colorTranslation: string;
        private _configurationTranslation: string;
        private _sizeTranslation: string;
        private _styleTranslation: string;

        constructor(transferOrderLine: Entities.TransferOrderLine, productVariant: ProductVariant) {
            this._transferOrderLine = transferOrderLine;
            this._colorTranslation = productVariant.Color;
            this._configurationTranslation = productVariant.Configuration;
            this._sizeTranslation = productVariant.Size;
            this._styleTranslation = productVariant.Style;
        }

        public get productNumber(): string {
            return this._transferOrderLine.ItemId;
        }

        public get recordId(): number {
            return this._transferOrderLine.RecordId;
        }

        public get description(): string {
            return this._transferOrderLine.ItemName;
        }

        public get quantityOrdered(): number {
            return this._transferOrderLine.QuantityTransferred;
        }

        public get quantityReceived(): number {
            var line: Entities.TransferOrderLine = this._transferOrderLine;
            return line.QuantityReceived + (line.QuantityTransferred - line.QuantityRemainReceive);
        }

        public get quantityReceivedNow(): number {
            return this._transferOrderLine.QuantityReceiveNow;
        }

        public set quantityReceivedNow(newReceivedNow: number) {
            this._transferOrderLine.QuantityReceiveNow = newReceivedNow;
        }

        public get unitOfMeasure(): string {
            return this._transferOrderLine.PurchaseUnit;
        }

        public get colorId(): string {
            return this._transferOrderLine.InventColorId;
        }

        public get configurationId(): string {
            return this._transferOrderLine.ConfigId;
        }

        public get sizeId(): string {
            return this._transferOrderLine.InventSizeId;
        }

        public get styleId(): string {
            return this._transferOrderLine.InventStyleId;
        }

        public get colorTranslation(): string {
            return this._colorTranslation;
        }

        public set colorTranslation(newColorValue: string) {
            this._colorTranslation = newColorValue;
        }

        public get configurationTranslation(): string {
            return this._configurationTranslation;
        }

        public set configurationTranslation(newConfigurationValue: string) {
            this._configurationTranslation = newConfigurationValue;
        }

        public get sizeTranslation(): string {
            return this._sizeTranslation;
        }

        public set sizeTranslation(newSizeValue: string) {
            this._sizeTranslation = newSizeValue;
        }

        public get styleTranslation(): string {
            return this._styleTranslation;
        }

        public set styleTranslation(newStyleValue: string) {
            this._styleTranslation = newStyleValue;
        }

        public get inventDimId(): string {
            return this._transferOrderLine.InventDimId;
        }

        public get originalLine(): any {
            return this._transferOrderLine;
        }
    }

    export class PickingAndReceivingTransferOutOrderLine implements PickingAndReceivingOrderLine {
        private _transferOrderLine: Entities.TransferOrderLine;
        private _colorTranslation: string;
        private _configurationTranslation: string;
        private _sizeTranslation: string;
        private _styleTranslation: string;

        constructor(transferOrderLine: Entities.TransferOrderLine, productVariant: ProductVariant) {
            this._transferOrderLine = transferOrderLine;
            this._colorTranslation = productVariant.Color;
            this._configurationTranslation = productVariant.Configuration;
            this._sizeTranslation = productVariant.Size;
            this._styleTranslation = productVariant.Style;
        }

        public get recordId(): number {
            return this._transferOrderLine.RecordId;
        }

        public get productNumber(): string {
            return this._transferOrderLine.ItemId;
        }

        public get description(): string {
            return this._transferOrderLine.ItemName;
        }

        public get quantityOrdered(): number {
            return this._transferOrderLine.QuantityTransferred;
        }

        public get quantityReceived(): number {
            var line: Entities.TransferOrderLine = this._transferOrderLine;
            return line.QuantityShipped + (line.QuantityTransferred - line.QuantityRemainShip);
        }

        public get quantityReceivedNow(): number {
            return this._transferOrderLine.QuantityShipNow;
        }

        public set quantityReceivedNow(newReceivedNow: number) {
            this._transferOrderLine.QuantityShipNow = newReceivedNow;
        }

        public get unitOfMeasure(): string {
            return this._transferOrderLine.PurchaseUnit;
        }

        public get colorId(): string {
            return this._transferOrderLine.InventColorId;
        }

        public get configurationId(): string {
            return this._transferOrderLine.ConfigId;
        }

        public get sizeId(): string {
            return this._transferOrderLine.InventSizeId;
        }

        public get styleId(): string {
            return this._transferOrderLine.InventStyleId;
        }

        public get colorTranslation(): string {
            return this._colorTranslation;
        }

        public set colorTranslation(newColorValue: string) {
            this._colorTranslation = newColorValue;
        }

        public get configurationTranslation(): string {
            return this._configurationTranslation;
        }

        public set configurationTranslation(newConfigurationValue: string) {
            this._configurationTranslation = newConfigurationValue;
        }

        public get sizeTranslation(): string {
            return this._sizeTranslation;
        }

        public set sizeTranslation(newSizeValue: string) {
            this._sizeTranslation = newSizeValue;
        }

        public get styleTranslation(): string {
            return this._styleTranslation;
        }

        public set styleTranslation(newStyleValue: string) {
            this._styleTranslation = newStyleValue;
        }

        public get inventDimId(): string {
            return this._transferOrderLine.InventDimId;
        }

        public get originalLine(): any {
            return this._transferOrderLine;
        }
    }

    export class PickingAndReceivingTransferInOrder implements PickingAndReceivingOrder {

        private _transferOrder: Entities.TransferOrder;
        private _orderLines: Entities.PickingAndReceivingOrderLine[] = [];

        constructor(transferOrder: Entities.TransferOrder) {
            this._transferOrder = transferOrder;
            this.constructOrderLine();
        }

        private constructOrderLine(): void {
            var orderLines: TransferOrderLine[] = this._transferOrder.OrderLines;

            for (var i = 0; i < orderLines.length; i++) {
                this._orderLines.push(new PickingAndReceivingTransferInOrderLine(orderLines[i], new ProductVariantClass()));
            }
        }

        public get orderId(): string {
            return this._transferOrder.OrderId;
        }

        public get recId(): string {
            return this._transferOrder.RecordId;
        }

        public get orderType(): number {
            return PurchaseTransferOrderType.TransferIn;
        }

        public get status(): string {
            return this._transferOrder.Status;
        }

        public get lines(): number {
            return this._transferOrder.Lines;
        }

        public set lines(newLines: number) {
            this._transferOrder.Lines = newLines;
        }

        public get totalOrdered(): number {
            return this._transferOrder.TotalItems;
        }

        public get totalReceived(): number {
            return this._transferOrder.QuantityReceived;
        }

        public get totalReceivedNow(): number {
            return this._transferOrder.QuantityReceiveNow;
        }

        public set totalReceivedNow(newReceivedNow: number) {
            this._transferOrder.QuantityReceiveNow = newReceivedNow;
        }

        public set orderLines(newLines: PickingAndReceivingOrderLine[]) {
            this._orderLines = newLines;
        }

        public get orderLines(): PickingAndReceivingOrderLine[] {
            return this._orderLines;
        }

        public get originalOrder(): any {
            return this._transferOrder;
        }

    }

    export class PickingAndReceivingTransferOutOrder implements PickingAndReceivingOrder {

        private _transferOrder: Entities.TransferOrder;
        private _orderLines: Entities.PickingAndReceivingOrderLine[] = [];

        constructor(transferOrder: Entities.TransferOrder) {
            this._transferOrder = transferOrder;
            this.constructOrderLine();
        }

        private constructOrderLine(): void {
            var orderLines: TransferOrderLine[] = this._transferOrder.OrderLines;
            
            for (var i = 0; i < orderLines.length; i++) {
                this._orderLines.push(new PickingAndReceivingTransferOutOrderLine(orderLines[i], new ProductVariantClass()));
            }
        }

        public get orderId(): string {
            return this._transferOrder.OrderId;
        }

        public get recId(): string {
            return this._transferOrder.RecordId;
        }

        public get orderType(): number {
            return PurchaseTransferOrderType.TransferOut;
        }

        public get status(): string {
            return this._transferOrder.Status;
        }

        public get lines(): number {
            return this._transferOrder.Lines;
        }

        public set lines(newLines: number) {
            this._transferOrder.Lines = newLines;
        }

        public get totalOrdered(): number {
            return this._transferOrder.TotalItems;
        }

        public get totalReceived(): number {
            return this._transferOrder.QuantityShipped;
        }

        public get totalReceivedNow(): number {
            return this._transferOrder.QuantityShipNow;
        }

        public set totalReceivedNow(newReceivedNow: number) {
            this._transferOrder.QuantityShipNow = newReceivedNow;
        }

        public set orderLines(newLines: PickingAndReceivingOrderLine[]) {
            this._orderLines = newLines;
        }

        public get orderLines(): PickingAndReceivingOrderLine[] {
            return this._orderLines;
        }

        public get originalOrder(): any {
            return this._transferOrder;
        }

    }
}