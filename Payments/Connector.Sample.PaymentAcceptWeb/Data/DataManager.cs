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
    namespace Retail.SampleConnector.PaymentAcceptWeb.Data
    {
        using System;
        using System.Collections.Generic;
        using System.Configuration;
        using System.Data;
        using System.Data.SqlClient;
        using SampleConnector.PaymentAcceptWeb.Models;

        /// <summary>
        /// The data manager that handles reading and writing to the database.
        /// </summary>
        public class DataManager
        {
            private const string CardPaymentConnectionString = "CardPaymentAccept";
            private const string CreateCardPaymentEntrySprocName = "CREATECARDPAYMENTENTRY";
            private const string CreateCardPaymentResultSprocName = "CREATECARDPAYMENTRESULT";
            private const string GetCardPaymentEntryByEntryIdSprocName = "GETCARDPAYMENTENTRYBYENTRYID";
            private const string GetCardPaymentResultByResultAccessCodeSprocName = "GETCARDPAYMENTRESULTBYRESULTACCESSCODE";
            private const string GetCountryListByLocaleSprocName = "GETCOUNTRYLISTBYLOCALE";
            private const string UpdateCardPaymentEntryAsUsedSprocName = "UpdateCardPaymentEntryAsUsed";
            private const string UpdateCardPaymentResultAsRetrievedSprocName = "UpdateCardPaymentResultAsRetrieved";
            private const string UpdateCardPaymentResultDataSprocName = "UpdateCardPaymentResultData";

            private string connectionString = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataManager" /> class.
            /// </summary>
            public DataManager()
            {
                this.connectionString = ConfigurationManager.ConnectionStrings[CardPaymentConnectionString].ConnectionString;
            }

            /// <summary>
            /// Inserts a record into CardPaymentEntry table.
            /// </summary>
            /// <param name="cardPaymentEntry">The record.</param>
            public void CreateCardPaymentEntry(CardPaymentEntry cardPaymentEntry)
            {
                if (cardPaymentEntry == null)
                {
                    throw new ArgumentNullException("cardPaymentEntry");
                }

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(CreateCardPaymentEntrySprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@b_AllowVoiceAuthorization", cardPaymentEntry.AllowVoiceAuthorization);
                        command.Parameters.AddWithValue("@nvc_CardTypes", cardPaymentEntry.CardTypes);
                        command.Parameters.AddWithValue("@nvc_DefaultCardHolderName", Object2DBValue(cardPaymentEntry.DefaultCardHolderName));
                        command.Parameters.AddWithValue("@nvc_DefaultCity", Object2DBValue(cardPaymentEntry.DefaultCity));
                        command.Parameters.AddWithValue("@nvc_DefaultCountryCode", Object2DBValue(cardPaymentEntry.DefaultCountryCode));
                        command.Parameters.AddWithValue("@nvc_DefaultPostalCode", Object2DBValue(cardPaymentEntry.DefaultPostalCode));
                        command.Parameters.AddWithValue("@nvc_DefaultStateOrProvince", Object2DBValue(cardPaymentEntry.DefaultStateOrProvince));
                        command.Parameters.AddWithValue("@nvc_DefaultStreet1", Object2DBValue(cardPaymentEntry.DefaultStreet1));
                        command.Parameters.AddWithValue("@nvc_DefaultStreet2", Object2DBValue(cardPaymentEntry.DefaultStreet2));
                        command.Parameters.AddWithValue("@nvc_EntryData", cardPaymentEntry.EntryData);
                        command.Parameters.AddWithValue("@id_EntryId", cardPaymentEntry.EntryId);
                        command.Parameters.AddWithValue("@nvc_EntryLocale", cardPaymentEntry.EntryLocale);
                        command.Parameters.AddWithValue("@dt_EntryUtcTime", cardPaymentEntry.EntryUtcTime);
                        command.Parameters.AddWithValue("@nvc_HostPageOrigin", cardPaymentEntry.HostPageOrigin);
                        command.Parameters.AddWithValue("@nvc_IndustryType", cardPaymentEntry.IndustryType);
                        command.Parameters.AddWithValue("@nvc_ServiceAccountId", cardPaymentEntry.ServiceAccountId);
                        command.Parameters.AddWithValue("@b_ShowSameAsShippingAddress", cardPaymentEntry.ShowSameAsShippingAddress);
                        command.Parameters.AddWithValue("@b_SupportCardSwipe", cardPaymentEntry.SupportCardSwipe);
                        command.Parameters.AddWithValue("@b_SupportCardTokenization", cardPaymentEntry.SupportCardTokenization);
                        command.Parameters.AddWithValue("@nvc_TransactionType", cardPaymentEntry.TransactionType);
                        command.Parameters.AddWithValue("@b_Used", cardPaymentEntry.Used);

                        command.ExecuteNonQuery();
                    }
                }
            }

            /// <summary>
            /// Updates a record in CardPaymentEntry table, sets it as used.
            /// </summary>
            /// <param name="serviceAccountId">The service account ID. It guarantees only the account that generates a record can access it.</param>
            /// <param name="entryId">The entry identifier.</param>
            public void UpdateCardPaymentEntryAsUsed(string serviceAccountId, string entryId)
            {
                if (serviceAccountId == null)
                {
                    throw new ArgumentNullException("serviceAccountId");
                }

                if (entryId == null)
                {
                    throw new ArgumentNullException("entryId");
                }

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(UpdateCardPaymentEntryAsUsedSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@nvc_ServiceAccountId", serviceAccountId);
                        command.Parameters.AddWithValue("@id_EntryId", entryId);

                        command.ExecuteNonQuery();
                    }
                }
            }

            /// <summary>
            /// Finds a record in CardPaymentEntry table by entry identifier.
            /// </summary>
            /// <param name="entryId">The entry identifier.</param>
            /// <returns>The found record.</returns>
            public CardPaymentEntry GetCardPaymentEntryByEntryId(string entryId)
            {
                if (entryId == null)
                {
                    throw new ArgumentNullException("entryId");
                }

                CardPaymentEntry cardPaymentEntry = null;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(GetCardPaymentEntryByEntryIdSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@id_EntryId", entryId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cardPaymentEntry = new CardPaymentEntry();
                                cardPaymentEntry.AllowVoiceAuthorization = reader.GetBoolean(reader.GetOrdinal("ALLOWVOICEAUTHORIZATION"));
                                cardPaymentEntry.CardTypes = GetString(reader, reader.GetOrdinal("CARDTYPES"));
                                cardPaymentEntry.DefaultCardHolderName = GetString(reader, reader.GetOrdinal("DEFAULTCARDHOLDERNAME"));
                                cardPaymentEntry.DefaultCity = GetString(reader, reader.GetOrdinal("DEFAULTCITY"));
                                cardPaymentEntry.DefaultCountryCode = GetString(reader, reader.GetOrdinal("DEFAULTCOUNTRYCODE"));
                                cardPaymentEntry.DefaultPostalCode = GetString(reader, reader.GetOrdinal("DEFAULTPOSTALCODE"));
                                cardPaymentEntry.DefaultStateOrProvince = GetString(reader, reader.GetOrdinal("DEFAULTSTATEORPROVINCE"));
                                cardPaymentEntry.DefaultStreet1 = GetString(reader, reader.GetOrdinal("DEFAULTSTREET1"));
                                cardPaymentEntry.DefaultStreet2 = GetString(reader, reader.GetOrdinal("DEFAULTSTREET2"));
                                cardPaymentEntry.EntryData = GetString(reader, reader.GetOrdinal("ENTRYDATA"));
                                cardPaymentEntry.EntryId = reader.GetGuid(reader.GetOrdinal("ENTRYID")).ToString();
                                cardPaymentEntry.EntryLocale = GetString(reader, reader.GetOrdinal("ENTRYLOCALE"));
                                cardPaymentEntry.EntryUtcTime = reader.GetDateTime(reader.GetOrdinal("ENTRYUTCTIME"));
                                cardPaymentEntry.HostPageOrigin = GetString(reader, reader.GetOrdinal("HOSTPAGEORIGIN"));
                                cardPaymentEntry.IndustryType = GetString(reader, reader.GetOrdinal("INDUSTRYTYPE"));
                                cardPaymentEntry.ServiceAccountId = GetString(reader, reader.GetOrdinal("SERVICEACCOUNTID"));
                                cardPaymentEntry.ShowSameAsShippingAddress = reader.GetBoolean(reader.GetOrdinal("SHOWSAMEASSHIPPINGADDRESS"));
                                cardPaymentEntry.SupportCardSwipe = reader.GetBoolean(reader.GetOrdinal("SUPPORTCARDSWIPE"));
                                cardPaymentEntry.SupportCardTokenization = reader.GetBoolean(reader.GetOrdinal("SUPPORTCARDTOKENIZATION"));
                                cardPaymentEntry.TransactionType = GetString(reader, reader.GetOrdinal("TRANSACTIONTYPE"));
                                cardPaymentEntry.Used = reader.GetBoolean(reader.GetOrdinal("USED"));
                            }
                        }
                    }
                }

                return cardPaymentEntry;
            }

            /// <summary>
            /// Inserts a record into CardPaymentResult table.
            /// </summary>
            /// <param name="cardPaymentResult">The record.</param>
            public void CreateCardPaymentResult(CardPaymentResult cardPaymentResult)
            {
                if (cardPaymentResult == null)
                {
                    throw new ArgumentNullException("cardPaymentResult");
                }

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(CreateCardPaymentResultSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@id_EntryId", cardPaymentResult.EntryId);
                        command.Parameters.AddWithValue("@b_Retrieved", cardPaymentResult.Retrieved);
                        command.Parameters.AddWithValue("@id_ResultAccessCode", cardPaymentResult.ResultAccessCode);
                        command.Parameters.AddWithValue("@nvc_ResultData", cardPaymentResult.ResultData);
                        command.Parameters.AddWithValue("@nvc_ServiceAccountId", cardPaymentResult.ServiceAccountId);

                        command.ExecuteNonQuery();
                    }
                }
            }

            /// <summary>
            /// Updates the result data of a record in CardPaymentResult table.
            /// </summary>
            /// <param name="serviceAccountId">The service account ID. It guarantees only the account that generates a record can access it.</param>
            /// <param name="resultAccessCode">The result access code.</param>
            /// <param name="newResultData">The new result data.</param>
            public void UpdateCardPaymentResultData(string serviceAccountId, string resultAccessCode, string newResultData)
            {
                if (serviceAccountId == null)
                {
                    throw new ArgumentNullException("serviceAccountId");
                }

                if (resultAccessCode == null)
                {
                    throw new ArgumentNullException("resultAccessCode");
                }

                if (newResultData == null)
                {
                    throw new ArgumentNullException("newResultData");
                }

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(UpdateCardPaymentResultDataSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@nvc_ServiceAccountId", serviceAccountId);
                        command.Parameters.AddWithValue("@id_ResultAccessCode", resultAccessCode);
                        command.Parameters.AddWithValue("@nvc_ResultData", newResultData);

                        command.ExecuteNonQuery();
                    }
                }
            }

            /// <summary>
            /// Updates a record in CardPaymentResult table, sets it as retrieved.
            /// </summary>
            /// <param name="serviceAccountId">The service account ID. It guarantees only the account that generates a record can access it.</param>
            /// <param name="resultAccessCode">The result access code.</param>
            public void UpdateCardPaymentResultAsRetrieved(string serviceAccountId, string resultAccessCode)
            {
                if (serviceAccountId == null)
                {
                    throw new ArgumentNullException("serviceAccountId");
                }

                if (resultAccessCode == null)
                {
                    throw new ArgumentNullException("resultAccessCode");
                }

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(UpdateCardPaymentResultAsRetrievedSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@nvc_ServiceAccountId", serviceAccountId);
                        command.Parameters.AddWithValue("@id_ResultAccessCode", resultAccessCode);

                        command.ExecuteNonQuery();
                    }
                }
            }

            /// <summary>
            /// Finds a record in CardPaymentResult table by result access code.
            /// </summary>
            /// <param name="serviceAccountId">The service account ID. It guarantees only the account that generates a record can access it.</param>
            /// <param name="resultAccessCode">The result access code.</param>
            /// <returns>The found record.</returns>
            public CardPaymentResult GetCardPaymentResultByResultAccessCode(string serviceAccountId, string resultAccessCode)
            {
                if (serviceAccountId == null)
                {
                    throw new ArgumentNullException("serviceAccountId");
                }

                if (resultAccessCode == null)
                {
                    throw new ArgumentNullException("resultAccessCode");
                }

                CardPaymentResult cardPaymentResult = null;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(GetCardPaymentResultByResultAccessCodeSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@nvc_ServiceAccountId", serviceAccountId);
                        command.Parameters.AddWithValue("@id_ResultAccessCode", resultAccessCode);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cardPaymentResult = new CardPaymentResult();
                                cardPaymentResult.EntryId = reader.GetGuid(reader.GetOrdinal("ENTRYID")).ToString();
                                cardPaymentResult.ResultAccessCode = reader.GetGuid(reader.GetOrdinal("RESULTACCESSCODE")).ToString();
                                cardPaymentResult.ResultData = GetString(reader, reader.GetOrdinal("RESULTDATA"));
                                cardPaymentResult.Retrieved = reader.GetBoolean(reader.GetOrdinal("RETRIEVED"));
                                cardPaymentResult.ServiceAccountId = GetString(reader, reader.GetOrdinal("SERVICEACCOUNTID"));
                            }
                        }
                    }
                }

                return cardPaymentResult;
            }

            /// <summary>
            /// Gets the list of countries or regions by locale.
            /// </summary>
            /// <param name="locale">The locale.</param>
            /// <returns>The result list.</returns>
            public IEnumerable<CountryOrRegion> GetCountryRegionListByLocale(string locale)
            {
                if (locale == null)
                {
                    throw new ArgumentNullException("locale");
                }

                var countries = new List<CountryOrRegion>();
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(GetCountryListByLocaleSprocName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@nvc_Locale", locale);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var country = new CountryOrRegion();
                                country.TwoLetterCode = GetString(reader, reader.GetOrdinal("TWOLETTERISOCODE"));
                                country.Locale = GetString(reader, reader.GetOrdinal("LOCALE"));
                                country.ShortName = GetString(reader, reader.GetOrdinal("SHORTNAME"));
                                country.LongName = GetString(reader, reader.GetOrdinal("LONGNAME"));

                                countries.Add(country);
                            }
                        }
                    }
                }

                // If no country or region is found for the requested locale, fall back to en-US.
                string fallbackLocale = "en-US";
                if (countries.Count == 0 && !locale.Equals(fallbackLocale, StringComparison.OrdinalIgnoreCase))
                {
                    countries.AddRange(this.GetCountryRegionListByLocale(fallbackLocale));
                }

                return countries;
            }

            /// <summary>
            /// Converts an object to a DB value.
            /// </summary>
            /// <param name="value">The object value.</param>
            /// <returns>The DB value.</returns>
            private static object Object2DBValue(object value)
            {
                if (value == null)
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }

            /// <summary>
            /// Gets string-type column value.
            /// </summary>
            /// <param name="reader">The data reader.</param>
            /// <param name="ordinal">The column ordinal.</param>
            /// <returns>The column value.</returns>
            private static string GetString(SqlDataReader reader, int ordinal)
            {
                if (!reader.IsDBNull(ordinal))
                {
                    return reader.GetString(ordinal);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}