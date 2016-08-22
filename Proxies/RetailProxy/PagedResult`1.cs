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
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.OData.Client;
        using Newtonsoft.Json;
    
        /// <summary>
        /// Encapsulates the paged query data result.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        [JsonObject]
        public sealed class PagedResult<T> : IEnumerable<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.Dynamics.Commerce.RetailProxy.PagedResult`1"/> class.
            /// </summary>
            public PagedResult()
            {
                this.Results = new ObservableCollection<T>();
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.Dynamics.Commerce.RetailProxy.PagedResult`1"/> class.
            /// </summary>
            /// <param name="response">The response returned by OData operations.</param>
            /// <remarks>Use for short, non-paged results only.</remarks>
            public PagedResult(QueryOperationResponse<T> response)
            {
                if (response != null)
                {
                    this.Results = new ObservableCollection<T>(response);
                    this.HasNextPage = response.GetContinuation() != null;
                }
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.Dynamics.Commerce.RetailProxy.PagedResult`1"/> class.
            /// </summary>
            /// <param name="results">The collection of results.</param>
            /// <remarks>Loads all collection results. Use for short, non-paged results only.</remarks>
            public PagedResult(IEnumerable<T> results)
            {
                this.Results = results;
                this.HasNextPage = false;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.Dynamics.Commerce.RetailProxy.PagedResult`1"/> class.
            /// </summary>
            /// <param name="pagedResults">The collection of results.</param>
            public PagedResult(Microsoft.Dynamics.Commerce.Runtime.PagedResult<T> pagedResults)
            {
                if (pagedResults != null)
                {
                    this.Results = pagedResults.Results;
                    this.HasNextPage = pagedResults.HasNextPage;
                    this.TotalCount = pagedResults.TotalCount;
                }
            }
    
            /// <summary>
            /// Gets or sets the paged results.
            /// </summary>
            public IEnumerable<T> Results { get; set; } // Make private as part of fix for bug 1746333 : [CRT] Fix StoreView to not duplicate stores due to multiple electornic addresses.
    
            /// <summary>
            /// Gets a value indicating whether next page is available.
            /// </summary>
            public bool HasNextPage { get; internal set; }
    
            /// <summary>
            /// Gets the total count for all pages.
            /// </summary>
            public long? TotalCount { get; internal set; }
    
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>A System.Collections.Generic.IEnumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<T> GetEnumerator()
            {
                return this.Results.GetEnumerator();
            }
    
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>A System.Collections.Generic.IEnumerator that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
