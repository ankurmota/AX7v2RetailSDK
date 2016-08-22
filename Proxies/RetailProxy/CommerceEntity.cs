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
    namespace Commerce.RetailProxy
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.Specialized;
        using System.ComponentModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Reflection;
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Class encapsulates commerce entity.
        /// </summary>
        public abstract class CommerceEntity : ICommerceEntity
        {
            private IList<string> changedProperties;

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceEntity" /> class.
            /// </summary>
            protected CommerceEntity()
            {
                this.changedProperties = new List<string>();
            }

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Gets or sets a value indicating whether this instance is notification disabled.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is notification disabled; otherwise, <c>false</c>.
            /// </value>
            [SuppressMessage("Microsoft.Design", "CA1033:Interface methods should be callable by child types", Justification = "Won't be overriden in derived classes.")]
            bool ICommerceEntity.IsNotificationDisabled { get; set; }

            /// <summary>
            /// Gets the collection of changed properties.
            /// </summary>
            internal IList<string> ChangedProperties
            {
                get { return this.changedProperties; }
            }

            /// <summary>
            /// Indexer property.
            /// </summary>
            /// <param name="propertyName">The property name.</param>
            /// <returns>An object.</returns>
            public object this[string propertyName]
            {
                get
                {
                    Type myType = this.GetType();
                    PropertyInfo myPropInfo = myType.GetTypeInfo().GetDeclaredProperty(propertyName);
                    return myPropInfo.GetValue(this, null);
                }

                set
                {
                    Type myType = this.GetType();
                    PropertyInfo myPropInfo = myType.GetTypeInfo().GetDeclaredProperty(propertyName);
                    myPropInfo.SetValue(this, value, null);
                }
            }

            /// <summary>
            /// Clears the changed properties.
            /// </summary>
            public void ClearChangedProperties()
            {
                this.changedProperties.Clear();
            }

            /// <summary>
            /// Clones the entity.
            /// </summary>
            /// <returns>A deep cloned object.</returns>
            internal object Clone()
            {
                var jsonString = this.SerializeToJsonObject();
                var cloned = jsonString.DeserializeJsonObject(this.GetType()) as CommerceEntity;

                // Clones the changed property collection.
                cloned.changedProperties = new List<string>(this.changedProperties);

                return cloned;
            }

            /// <summary>
            /// The property changed event handler.
            /// </summary>
            /// <param name="propertyName">The property name.</param>
            protected virtual void OnPropertyChanged(string propertyName)
            {
                if (!this.changedProperties.Contains(propertyName))
                {
                    this.changedProperties.Add(propertyName);
                }

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
