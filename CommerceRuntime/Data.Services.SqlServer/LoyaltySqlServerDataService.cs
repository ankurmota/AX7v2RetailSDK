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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Pricing and discount data service class.
        /// </summary>
        public class LoyaltySqlServerDataService : IRequestHandler
        {
            // Type names
            private const string LoyaltyCardTypeName = "LOYALTYCARDTABLETYPE";
            private const string LoyaltyCardTierTypeName = "LOYALTYCARDTIERTABLETYPE";
    
            // Stored procedure names
            private const string GetLoyaltyCardSprocName = "GETLOYALTYCARD";
            private const string GetLoyaltyTiersSprocName = "GETLOYALTYTIERS";
            private const string GetCustomerLoyaltyCardsSprocName = "GETCUSTOMERLOYALTYCARDS";
            private const string GetLoyaltyGroupsByLoyaltyCardSprocName = "GETLOYALTYGROUPSBYLOYALTYCARD";
            private const string GetActiveOrFutureLoyaltyCardTierSprocName = "GETACTIVEORFUTURELOYALTYCARDTIERS";
            private const string GetActiveLoyaltyCardTiersSprocName = "GETACTIVELOYALTYCARDTIERS";
            private const string GetLoyaltyEarnSchemeLinesSprocName = "GETLOYALTYEARNSCHEMELINES";
            private const string GetLoyaltyRedeemSchemeLinesSprocName = "GETLOYALTYREDEEMSCHEMELINES";
            private const string InsertLoyaltyCardSprocName = "INSERTLOYALTYCARD";
            private const string InsertLoyaltyCardTierSprocName = "INSERTLOYALTYCARDTIER";
    
            // Column names
            private const string RecIdColumn = "RECID";
            private const string CardNumberColumn = "CARDNUMBER";
            private const string CardTenderTypeColumn = "CARDTENDERTYPE";
            private const string PartyColumn = "PARTY";
            private const string AffiliationColumn = "AFFILIATION";
            private const string LoyaltyCardColumn = "LOYALTYCARD";
            private const string LoyaltyTierColumn = "LOYALTYTIER";
            private const string ValidFromColumn = "VALIDFROM";
            private const string ValidToColumn = "VALIDTO";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetCustomerLoyaltyCardsDataRequest),
                        typeof(GetLoyaltyCardDataRequest),
                        typeof(GetLoyaltyGroupsAndTiersDataRequest),
                        typeof(GetLoyaltyCardAffiliationsDataRequest),
                        typeof(GetLoyaltySchemeLineEarnDataRequest),
                        typeof(GetLoyaltySchemeLineRedeemDataRequest),
                        typeof(InsertLoyaltyCardDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetCustomerLoyaltyCardsDataRequest))
                {
                    response = this.GetCustomerLoyaltyCards((GetCustomerLoyaltyCardsDataRequest)request);
                }
                else if (requestType == typeof(GetLoyaltyCardDataRequest))
                {
                    response = this.GetLoyaltyCard((GetLoyaltyCardDataRequest)request);
                }
                else if (requestType == typeof(GetLoyaltyGroupsAndTiersDataRequest))
                {
                    response = this.GetLoyaltyGroupsAndTiersByCardNumber((GetLoyaltyGroupsAndTiersDataRequest)request);
                }
                else if (requestType == typeof(GetLoyaltyCardAffiliationsDataRequest))
                {
                    response = this.GetLoyaltyCardAffiliations((GetLoyaltyCardAffiliationsDataRequest)request);
                }
                else if (requestType == typeof(GetLoyaltySchemeLineEarnDataRequest))
                {
                    response = this.GetLoyaltySchemeLineEarn((GetLoyaltySchemeLineEarnDataRequest)request);
                }
                else if (requestType == typeof(GetLoyaltySchemeLineRedeemDataRequest))
                {
                    response = this.GetLoyaltySchemeLineRedeem((GetLoyaltySchemeLineRedeemDataRequest)request);
                }
                else if (requestType == typeof(InsertLoyaltyCardDataRequest))
                {
                    response = this.InsertLoyaltyCard((InsertLoyaltyCardDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Adds loyalty card table schema into the data table.
            /// </summary>
            /// <param name="table">The data table.</param>
            private static void AddLoyaltyCardTableTypeSchema(DataTable table)
            {
                ThrowIf.Null(table, "table");
    
                // NOTE: The order of colums here MUST match the TVP_LOYALTYCARDTABLETYPE.
                table.Columns.Add(LoyaltySqlServerDataService.RecIdColumn, typeof(long));
                table.Columns.Add(LoyaltySqlServerDataService.CardNumberColumn, typeof(string));
                table.Columns.Add(LoyaltySqlServerDataService.CardTenderTypeColumn, typeof(int));
                table.Columns.Add(LoyaltySqlServerDataService.PartyColumn, typeof(long));
            }
    
            /// <summary>
            /// Adds loyalty card tier table schema into the data table.
            /// </summary>
            /// <param name="table">The data table.</param>
            private static void AddLoyaltyCardTierTableTypeSchema(DataTable table)
            {
                ThrowIf.Null(table, "table");
    
                // NOTE: The order of colums here MUST match the @TVP_LOYALTYCARDTIERTABLETYPE.
                table.Columns.Add(LoyaltySqlServerDataService.RecIdColumn, typeof(long));
                table.Columns.Add(LoyaltySqlServerDataService.AffiliationColumn, typeof(long));
                table.Columns.Add(LoyaltySqlServerDataService.LoyaltyCardColumn, typeof(long));
                table.Columns.Add(LoyaltySqlServerDataService.LoyaltyTierColumn, typeof(long));
                table.Columns.Add(LoyaltySqlServerDataService.ValidFromColumn, typeof(DateTime));
                table.Columns.Add(LoyaltySqlServerDataService.ValidToColumn, typeof(DateTime));
            }
    
            /// <summary>
            /// Converts the loyalty card into a data row.
            /// </summary>
            /// <param name="table">The data table.</param>
            /// <param name="loyaltyCard">The loyalty card.</param>
            /// <returns>The data row.</returns>
            private static DataRow ConvertLoyaltyCardToDataRow(DataTable table, LoyaltyCard loyaltyCard)
            {
                DataRow row = table.NewRow();
    
                row[LoyaltySqlServerDataService.RecIdColumn] = loyaltyCard.RecordId;
                row[LoyaltySqlServerDataService.CardNumberColumn] = loyaltyCard.CardNumber;
                row[LoyaltySqlServerDataService.CardTenderTypeColumn] = (int)loyaltyCard.CardTenderType;
                row[LoyaltySqlServerDataService.PartyColumn] = loyaltyCard.PartyRecordId;
    
                return row;
            }
    
            /// <summary>
            /// Converts the loyalty card tier into a data row.
            /// </summary>
            /// <param name="table">The data table.</param>
            /// <param name="loyaltyCard">The loyalty card.</param>
            /// <param name="loyaltyGroup">The card loyalty group.</param>
            /// <param name="loyaltyCardTier">The loyalty card tier.</param>
            /// <returns>The data row.</returns>
            private static DataRow ConvertLoyaltyCardTierToDataRow(DataTable table, LoyaltyCard loyaltyCard, LoyaltyGroup loyaltyGroup, LoyaltyCardTier loyaltyCardTier)
            {
                DataRow row = table.NewRow();
    
                row[LoyaltySqlServerDataService.RecIdColumn] = loyaltyCardTier.RecordId;
                row[LoyaltySqlServerDataService.AffiliationColumn] = loyaltyGroup.RecordId;
                row[LoyaltySqlServerDataService.LoyaltyCardColumn] = loyaltyCard.RecordId;
                row[LoyaltySqlServerDataService.LoyaltyTierColumn] = loyaltyCardTier.LoyaltyTierRecordId;
                row[LoyaltySqlServerDataService.ValidFromColumn] = loyaltyCardTier.ValidFrom.DateTime;
                row[LoyaltySqlServerDataService.ValidToColumn] = loyaltyCardTier.ValidTo.DateTime;
    
                return row;
            }
    
            private EntityDataServiceResponse<LoyaltyCard> GetCustomerLoyaltyCards(GetCustomerLoyaltyCardsDataRequest request)
            {
                ThrowIf.NullOrWhiteSpace(request.CustomerAccountNumber, "customerAccountNumber");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_CustomerAccountNumber"] = request.CustomerAccountNumber;
                parameters["@nvc_CustomerDataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                ReadOnlyCollection<LoyaltyCard> loyaltyCards;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    loyaltyCards = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltyCard>(LoyaltySqlServerDataService.GetCustomerLoyaltyCardsSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<LoyaltyCard>(loyaltyCards.AsPagedResult());
            }
    
            private SingleEntityDataServiceResponse<LoyaltyCard> GetLoyaltyCard(GetLoyaltyCardDataRequest request)
            {
                ThrowIf.NullOrWhiteSpace(request.LoyaltyCardNumber, "loyaltyCardNumber");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_LoyaltyCardNumber"] = request.LoyaltyCardNumber;
                parameters["@nvc_CustomerDataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                LoyaltyCard loyaltyCard;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    loyaltyCard = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltyCard>(LoyaltySqlServerDataService.GetLoyaltyCardSprocName, parameters).SingleOrDefault();
                }
    
                return new SingleEntityDataServiceResponse<LoyaltyCard>(loyaltyCard);
            }
    
            private EntityDataServiceResponse<LoyaltyGroup> GetLoyaltyGroupsAndTiersByCardNumber(GetLoyaltyGroupsAndTiersDataRequest request)
            {
                ThrowIf.Null(request.LoyaltyCardNumber, "loyaltyCardNumber");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_LoyaltyCardNumber"] = request.LoyaltyCardNumber;
                parameters["@nvc_Locale"] = request.RequestContext.LanguageId;
    
                // Get all loyalty groups of the loyalty cards.
                ReadOnlyCollection<LoyaltyGroup> loyaltyGroups;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    loyaltyGroups = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltyGroup>(LoyaltySqlServerDataService.GetLoyaltyGroupsByLoyaltyCardSprocName, parameters);
                }
    
                // Get all loyalty tiers of each loyalty group
                foreach (var loyaltyGroup in loyaltyGroups)
                {
                    loyaltyGroup.LoyaltyTiers = this.GetLoyaltyTiers(request, loyaltyGroup.RecordId);
                    loyaltyGroup.LoyaltyCardTiers = this.GetActiveOrFutureLoyaltyCardTiers(request, loyaltyGroup.RecordId);
                }
    
                return new EntityDataServiceResponse<LoyaltyGroup>(loyaltyGroups.AsPagedResult());
            }
    
            private EntityDataServiceResponse<SalesAffiliationLoyaltyTier> GetLoyaltyCardAffiliations(GetLoyaltyCardAffiliationsDataRequest request)
            {
                ThrowIf.Null(request.LoyaltyCardNumber, "loyaltyCardNumber");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_LoyaltyCardNumber"] = request.LoyaltyCardNumber;
                parameters["@dt_channelLocalDate"] = request.RequestContext.GetNowInChannelTimeZone().Date;
    
                ReadOnlyCollection<AffiliationLoyaltyTier> affiliations;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    affiliations = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<AffiliationLoyaltyTier>(LoyaltySqlServerDataService.GetActiveLoyaltyCardTiersSprocName, parameters);
                }
    
                ReadOnlyCollection<SalesAffiliationLoyaltyTier> salesAffiliations = affiliations.Select(loyaltyAffiliation =>
                    new SalesAffiliationLoyaltyTier()
                    {
                        AffiliationId = loyaltyAffiliation.AffiliationId,
                        LoyaltyTierId = loyaltyAffiliation.LoyaltyTierId,
                        AffiliationType = RetailAffiliationType.Loyalty,
                        ChannelId = request.RequestContext.GetPrincipal().ChannelId,
                        ReceiptId = request.Transaction.ReceiptId,
                        StaffId = request.Transaction.StaffId,
                        TerminalId = request.Transaction.TerminalId,
                        TransactionId = request.Transaction.Id
                    }).AsReadOnly();
    
                return new EntityDataServiceResponse<SalesAffiliationLoyaltyTier>(salesAffiliations.AsPagedResult());
            }
    
            private EntityDataServiceResponse<LoyaltySchemeLineEarn> GetLoyaltySchemeLineEarn(GetLoyaltySchemeLineEarnDataRequest request)
            {
                ThrowIf.Null(request.LoyaltyCardNumber, "loyaltyCardNumber");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@bi_ChannelId"] = request.ChannelId;
                parameters["@dt_ChannelLocalDate"] = request.RequestContext.GetNowInChannelTimeZone().Date;
                parameters["@nvc_LoyaltyCardNumber"] = request.LoyaltyCardNumber;
    
                ReadOnlyCollection<LoyaltySchemeLineEarn> schemeLines;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    schemeLines = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltySchemeLineEarn>(LoyaltySqlServerDataService.GetLoyaltyEarnSchemeLinesSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<LoyaltySchemeLineEarn>(schemeLines.AsPagedResult());
            }
    
            private EntityDataServiceResponse<LoyaltySchemeLineRedeem> GetLoyaltySchemeLineRedeem(GetLoyaltySchemeLineRedeemDataRequest request)
            {
                ThrowIf.Null(request.LoyaltyCardNumber, "loyaltyCardNumber");
                ThrowIf.Null(request.LoyaltyRewardPointId, "loyaltyRewardPointId");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@bi_ChannelId"] = request.ChannelId;
                parameters["@dt_ChannelLocalDate"] = request.RequestContext.GetNowInChannelTimeZone().Date;
                parameters["@nvc_LoyaltyCardNumber"] = request.LoyaltyCardNumber;
                parameters["@nvc_RewardPointRewardId"] = request.LoyaltyRewardPointId;
    
                ReadOnlyCollection<LoyaltySchemeLineRedeem> schemeLines;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    schemeLines = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltySchemeLineRedeem>(LoyaltySqlServerDataService.GetLoyaltyRedeemSchemeLinesSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<LoyaltySchemeLineRedeem>(schemeLines.AsPagedResult());
            }
    
            private NullResponse InsertLoyaltyCard(InsertLoyaltyCardDataRequest request)
            {
                ThrowIf.Null(request.LoyaltyCard, "loyaltyCard");
    
                using (DataTable loyaltyCardTable = new DataTable(LoyaltyCardTypeName))
                using (DataTable loyaltyCardTierTable = new DataTable(LoyaltyCardTierTypeName))
                {
                    // Prepare loyalty card data
                    AddLoyaltyCardTableTypeSchema(loyaltyCardTable);
                    loyaltyCardTable.Rows.Add(ConvertLoyaltyCardToDataRow(loyaltyCardTable, request.LoyaltyCard));
    
                    // Prepare loyalty card tier data
                    AddLoyaltyCardTierTableTypeSchema(loyaltyCardTierTable);
                    foreach (var cardLoyaltyGroup in request.LoyaltyCard.LoyaltyGroups)
                    {
                        foreach (var cardTier in cardLoyaltyGroup.LoyaltyCardTiers)
                        {
                            loyaltyCardTierTable.Rows.Add(ConvertLoyaltyCardTierToDataRow(loyaltyCardTierTable, request.LoyaltyCard, cardLoyaltyGroup, cardTier));
                        }
                    }
    
                    // Insert
                    var parameters = new ParameterSet();
                    parameters["@TVP_LOYALTYCARDTABLETYPE"] = loyaltyCardTable;
                    parameters["@TVP_LOYALTYCARDTIERTABLETYPE"] = loyaltyCardTierTable;
    
                    int errorCode;
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        errorCode = sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(LoyaltySqlServerDataService.InsertLoyaltyCardSprocName, parameters);
                    }
    
                    if (errorCode != (int)DatabaseErrorCodes.Success)
                    {
                        throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save the loyalty card in the channel database.");
                    }
                }
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets all the loyalty tiers of the given loyalty group.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="loyaltyGroupRecordId">The record identifier of the loyalty group.</param>
            /// <returns>The collection of the loyalty tiers.</returns>
            private ReadOnlyCollection<LoyaltyTier> GetLoyaltyTiers(Request request, long loyaltyGroupRecordId)
            {
                ParameterSet parameters = new ParameterSet();
                parameters["@bi_loyaltyGroupRecordId"] = loyaltyGroupRecordId;
                parameters["@nvc_Locale"] = request.RequestContext.LanguageId;
    
                ReadOnlyCollection<LoyaltyTier> loyaltyTiers;
    
                // Get all loyalty tiers of the given loyalty group.
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    loyaltyTiers = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltyTier>(LoyaltySqlServerDataService.GetLoyaltyTiersSprocName, parameters);
                }
    
                return loyaltyTiers;
            }
    
            /// <summary>
            /// Gets all the loyalty card tiers that the given card is currently and will be qualified for.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="loyaltyGroupRecordId">The record identifier of the loyalty group.</param>
            /// <returns>The collection of the loyalty card tiers.</returns>
            private ReadOnlyCollection<LoyaltyCardTier> GetActiveOrFutureLoyaltyCardTiers(GetLoyaltyGroupsAndTiersDataRequest request, long loyaltyGroupRecordId)
            {
                ThrowIf.Null(request.LoyaltyCardNumber, "cardNumber");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_LoyaltyCardNumber"] = request.LoyaltyCardNumber;
                parameters["@bi_LoyaltyGroupRecordId"] = loyaltyGroupRecordId;
                parameters["@dt_ChannelLocalDate"] = request.RequestContext.GetNowInChannelTimeZone().Date;
    
                // The flag indicating whether to retrieve the loyalty card tiers that take effect in the future.
                parameters["@b_retrieveFutureCardTiers"] = request.RetrieveFutureLoyaltyCardTiers;
    
                // Get the active or future loyalty card tiers
                ReadOnlyCollection<LoyaltyCardTier> loyaltyCardTiers;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    loyaltyCardTiers = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<LoyaltyCardTier>(LoyaltySqlServerDataService.GetActiveOrFutureLoyaltyCardTierSprocName, parameters);
                }
    
                return loyaltyCardTiers;
            }
        }
    }
}
