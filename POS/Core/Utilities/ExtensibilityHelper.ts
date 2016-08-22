/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Extensions/ArrayExtensions.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>

module Commerce {
    "use strict";

    /**
     * Represents a property bag.
     */
    export interface IPropertyBag {
        ExtensionProperties?: Model.Entities.CommerceProperty[];
    }

    
    /**
     * Key, value and type of a property.
     */
    export interface PropertyKeyValueType {
        key: string;
        value: any;
        type: PropertyTypeEnum;
    }
    

    /**
     * Supported property value types.
     */
    export enum PropertyTypeEnum {
        BooleanValue,
        ByteValue,
        DateTimeOffsetValue,
        DecimalValue,
        IntegerValue,
        LongValue,
        StringValue,
    }

    /**
     * Provides a way to extend commerce entities by adding a first-class property that can be accessed in a type-safe straightforward manner.
     */
    export class ExtensibilityHelper {
        /**
         * Extends a type by adding a new property to the entity as a first class citizen.
         * The new property will write and read to the ExtensionsProperties property of the type.
         *
         * @param {any} typeToExtend The type to be extended with a new property.
         * @param {string} propertyName The property name.
         * @param {string} key The key of the commerce property.
         * @param {PropertyTypeEnum} propertyType The property type.
         * @remarks This method should be preferentially used with commerce entity types.
         */
        public static extend(typeToExtend: any, propertyName: string, key: string, propertyType: PropertyTypeEnum): void {
            if (StringExtensions.isNullOrWhitespace(propertyName)) {
                throw "Cannot extend type with a null or whitespace property name";
            }

            if (!Object.isExtensible(typeToExtend)) {
                throw "The type is not extensible.";
            }

            var prototypeToExtend: any = typeToExtend.prototype;
            if (!prototypeToExtend) {
                throw "The object passed does not have a prototype.";
            }

            // the property is already defined in this prototype
            if (Object.getOwnPropertyDescriptor(prototypeToExtend, propertyName)) {
                return;
            }

            Object.defineProperty(prototypeToExtend, propertyName, {
                get: function (): any {
                    var value: Proxy.Entities.CommercePropertyValue = ExtensibilityHelper.getPropertyValue(<IPropertyBag>this, key);
                    if (!value) {
                        return undefined;
                    }

                    return value[PropertyTypeEnum[propertyType]];
                },
                set: function (newValue: any): void {
                    var value: Proxy.Entities.CommercePropertyValue = ExtensibilityHelper.getPropertyValue(
                        <IPropertyBag>this, key, true /* create if not present */);
                    value[PropertyTypeEnum[propertyType]] = newValue;
                },
                enumerable: true
            });
        }

        /**
         * Flattens the collection of extension properties in a key-value-type array.
         *
         * @param {IPropertyBag} bag The property bag containing the extension properties array.
         * @return {PropertyKeyValueType[]} The array of properties' key-value-type.
         */
        public static flatten(bag: IPropertyBag): PropertyKeyValueType[] {
            if (ObjectExtensions.isNullOrUndefined(bag) || !ArrayExtensions.hasElements(bag.ExtensionProperties)) {
                return [];
            }

            return bag.ExtensionProperties.map((property: Proxy.Entities.CommerceProperty) => ExtensibilityHelper.getKeyValueType(property));
        }

        /**
         * Gets the property value from the property bag, by its key. Optionally creates the property value on the bag, if it does not exist.
         */
        private static getPropertyValue(bag: IPropertyBag, key: string, createIfNotPresent: boolean = false): Model.Entities.CommercePropertyValue {
            var property: Proxy.Entities.CommerceProperty =
                ArrayExtensions.firstOrUndefined(bag.ExtensionProperties, (property: Proxy.Entities.CommerceProperty) => {
                    return property.Key === key;
                });

            if (!property) {
                if (!createIfNotPresent) {
                    return undefined;
                }

                property = new Model.Entities.CommercePropertyClass({ Key: key, Value: new Model.Entities.CommercePropertyValueClass() });

                bag.ExtensionProperties = bag.ExtensionProperties || [];
                bag.ExtensionProperties.push(property);
            }

            return property.Value;
        }

        /**
         * Gets the key, value and value type from the property.
         */
        private static getKeyValueType(property: Model.Entities.CommerceProperty): PropertyKeyValueType {
            // find the property name that contains a non-undefined value
            var propertyName: string = ArrayExtensions.firstOrUndefined(
                Object.keys(property.Value), (name: string) => property.Value[name] !== undefined);

            if (propertyName) {
                return { key: property.Key, value: property.Value[propertyName], type: PropertyTypeEnum[propertyName] };
            }

            return { key: property.Key, value: undefined, type: undefined };
        }
    }
}