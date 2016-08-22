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
    namespace Retail.Ecommerce.Sdk.Core
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Commerce.RetailProxy;

        /// <summary>
        /// The error processing utilities.
        /// </summary>
        public partial class Utilities
        {
            /// <summary>
            /// Error code for generic error on ecommerce server.
            /// </summary>
            private const string GenericErrorMessage = "GENERICERRORMESSAGE";

            /// <summary>
            /// Checks for type of exception, gets proper exception message from resource and logs it if necessary.
            /// </summary>
            /// <param name="ex">Exception ex.</param>
            /// <returns>Response errors.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the exception parameter is null.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "It is safe to suppress a warning from this rule, or to ignore the rule completely, if performance is not a concern")]
            public static Collection<ResponseError> GetResponseErrorsFromException(Exception ex)
            {
                Collection<ResponseError> responseErrors = new Collection<ResponseError>();

                if (ex != null)
                {
                    Type exceptionType = ex.GetType();

                    if (exceptionType == typeof(DataValidationException) || exceptionType == typeof(CartValidationException))
                    {
                        // Convert the validation results in the exception to response errors.
                        // Convert the exception to response error.
                        DataValidationException serverException = (DataValidationException)ex;
                        responseErrors = CreateResponseErrorsFromFailures(serverException.ValidationResults, failure => new ResponseError(failure.ErrorResourceId, failure.LocalizedMessage));
                        responseErrors.Add(new ResponseError(serverException.ErrorResourceId, serverException.LocalizedMessage));
                    }
                    else if (exceptionType == typeof(PaymentException))
                    {
                        // Convert the payment SDK errors in the exception to response errors.
                        // Convert the exception to response error.
                        PaymentException serverException = ex as PaymentException;
                        responseErrors = CreateResponseErrorsFromFailures(serverException.PaymentSdkErrors, error => new ResponseError(error.Code, error.Message));
                        responseErrors.Add(new ResponseError(serverException.ErrorResourceId, serverException.LocalizedMessage));
                    }
                    else if (typeof(RetailProxyException).IsAssignableFrom(exceptionType))
                    {
                        // Convert the exception to response error.
                        RetailProxyException serverException = (RetailProxyException)ex;
                        responseErrors.Add(new ResponseError(serverException.ErrorResourceId, serverException.LocalizedMessage));
                    }
                }

                if (!responseErrors.Any())
                {
                    responseErrors.Add(new ResponseError(Utilities.GenericErrorMessage, "Sorry something went wrong, we cannot process your request at this time. Please try again later."));
                }

                return responseErrors;
            }

            /// <summary>
            /// Creates the response errors from failures.
            /// </summary>
            /// <typeparam name="T">The type of the failure.</typeparam>
            /// <param name="failures">The failures.</param>
            /// <param name="createResponseError">The function to create the <see cref="ResponseError"/>.</param>
            /// <returns>The collection of response errors.</returns>
            private static Collection<ResponseError> CreateResponseErrorsFromFailures<T>(IEnumerable<T> failures, Func<T, ResponseError> createResponseError)
                where T : CommerceEntity
            {
                IList<ResponseError> responseErrors = failures != null && failures.Any() ? failures.Select(createResponseError).ToList() : new List<ResponseError>();
                return new Collection<ResponseError>(responseErrors);
            }
        }
    }
}