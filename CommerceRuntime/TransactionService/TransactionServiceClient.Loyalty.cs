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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Xml.Linq;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service client method names.
            private const string PostLoyaltyCardRewardPointTransMethodName = "PostLoyaltyCardRewardPointTrans";
            private const string IssueLoyaltyCardMethodName = "IssueLoyaltyCard";
            private const string GetLoyaltyCardRewardPointsStatusMethodName = "GetLoyaltyCardRewardPointsStatus";
            private const string GetLoyaltyCardTransactionsMethodName = "GetLoyaltyCardTransactions";

            /// <summary>
            /// Posts the loyalty card reward points to AX HQ.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="entryType">The entry type of the reward points that need to be posted.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            public void PostLoyaltyCardRewardPointTrans(SalesTransaction transaction, LoyaltyRewardPointEntryType entryType, ChannelConfiguration channelConfiguration)
            {
                if (transaction == null
                    || transaction.LoyaltyRewardPointLines == null
                    || !transaction.LoyaltyRewardPointLines.Any()
                    || channelConfiguration == null)
                {
                    return;
                }

                // Get reward lines based on entry type
                var rewardLines = from l in transaction.LoyaltyRewardPointLines
                                  where l.EntryType == entryType
                                  select l;

                if (rewardLines == null || rewardLines.Count() == 0)
                {
                    return;
                }

                // Parse reward point lines into an XML
                XDocument rewardsDoc = new XDocument();
                XElement rewardsElmt = new XElement("RetailLoyaltyCardRewardPointTransList");
                rewardsDoc.Add(rewardsElmt);

                // Add nodes for reward lines
                foreach (var rewardLine in rewardLines)
                {
                    XElement rewardElmt = CreateXElementForRewardPointTrans(transaction, channelConfiguration, rewardLine);
                    rewardsElmt.Add(rewardElmt);
                }

                // Invoke the service
                string xml = rewardsDoc.ToString();
                this.InvokeMethodNoDataReturn(PostLoyaltyCardRewardPointTransMethodName, xml);
            }

            /// <summary>
            /// Issues a new loyalty card.
            /// </summary>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="cardTenderType">The loyalty card tender type.</param>
            /// <param name="partyRecordId">The record identifier of the party of the card owner.</param>
            /// <param name="channelRecordId">The record identifier of the channel.</param>
            /// <returns>The created loyalty card.</returns>
            public LoyaltyCard IssueLoyaltyCard(string loyaltyCardNumber, LoyaltyCardTenderType cardTenderType, long partyRecordId, long channelRecordId)
            {
                var parameters = new object[]
                {
                    loyaltyCardNumber,
                    (int)cardTenderType,
                    partyRecordId,
                    channelRecordId
                };

                var data = this.InvokeMethod(IssueLoyaltyCardMethodName, parameters);

                // Translate the result data into a loyalty card object with card tiers.
                var cardData = (object[])data[0];
                var cardTierListData = (object[])data[1];

                var loyaltyCard = new LoyaltyCard();
                loyaltyCard.RecordId = (long)cardData[0];
                loyaltyCard.CardNumber = (string)cardData[1];
                loyaltyCard.CardTenderType = (LoyaltyCardTenderType)cardData[2];
                loyaltyCard.PartyRecordId = (long)cardData[3];

                foreach (var cardTierListDataRow in cardTierListData)
                {
                    var cardTierData = (object[])cardTierListDataRow;

                    long loyaltyGroupRecordId = (long)cardTierData[1];
                    LoyaltyGroup loyaltyGroup = loyaltyCard.LoyaltyGroups.SingleOrDefault(lg => lg.RecordId == loyaltyGroupRecordId);
                    if (loyaltyGroup == null)
                    {
                        loyaltyGroup = new LoyaltyGroup();
                        loyaltyGroup.RecordId = loyaltyGroupRecordId;
                        loyaltyCard.LoyaltyGroups.Add(loyaltyGroup);
                    }

                    var cardTier = new LoyaltyCardTier();
                    cardTier.RecordId = (long)cardTierData[0];
                    cardTier.LoyaltyTierRecordId = (long)cardTierData[3];
                    cardTier.ValidFrom = string.IsNullOrWhiteSpace(cardTierData[4].ToString()) ? DateTimeOffset.MinValue : Convert.ToDateTime(cardTierData[4]);
                    cardTier.ValidTo = string.IsNullOrWhiteSpace(cardTierData[5].ToString()) ? DateTimeOffset.MaxValue : Convert.ToDateTime(cardTierData[5]);

                    loyaltyGroup.LoyaltyCardTiers.Add(cardTier);
                }

                return loyaltyCard;
            }

            /// <summary>
            /// Gets the status of the loyalty reward points by card number or directory party record identifier.
            /// </summary>
            /// <param name="channelLocalDate">The local date of the channel.</param>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="excludeBlocked">
            /// The flag indicating whether to exclude the card status if the card is blocked.
            /// </param>
            /// <param name="excludeNoTender">
            /// The flag indicating whether to exclude the card status if the card is no tender or blocked.
            /// </param>
            /// <param name="includeRelatedCardsForContactTender">
            /// The flag indicating whether to include the status of the related cards if the given card is contact tender.
            /// </param>
            /// <param name="includeNonRedeemablePoints">
            /// The flag indicating whether to include non-redeemable points status in the result.
            /// </param>
            /// <param name="includeActivePointsOnly">
            /// The flag indicating whether to return only the active points; otherwise, returns the status of issued, used and expired points as well.
            /// </param>
            /// <param name="locale">The locale of the translations.</param>
            /// <returns>The loyalty point status per loyalty card.</returns>
            public Collection<LoyaltyCard> GetLoyaltyCardRewardPointsStatus(
                DateTimeOffset channelLocalDate,
                string loyaltyCardNumber,
                bool excludeBlocked,
                bool excludeNoTender,
                bool includeRelatedCardsForContactTender,
                bool includeNonRedeemablePoints,
                bool includeActivePointsOnly,
                string locale)
            {
                string channelLocalDateStr = SerializationHelper.ConvertDateTimeToAXDateString(channelLocalDate.LocalDateTime, 213);
                var parameters = new object[]
                {
                    channelLocalDateStr,
                    loyaltyCardNumber,
                    excludeBlocked,
                    excludeNoTender,
                    includeRelatedCardsForContactTender,
                    includeNonRedeemablePoints,
                    includeActivePointsOnly,
                    locale
                };

                var data = this.InvokeMethod(GetLoyaltyCardRewardPointsStatusMethodName, parameters);

                // Translate the result data into a list of loyalty card with reward points status
                string statusXML = (string)data[0];

                XDocument doc = XDocument.Parse(statusXML);
                XElement root = doc.Elements("LoyaltyCardRewardPointsStatusList").SingleOrDefault();
                Collection<LoyaltyCard> cards = null;
                if (root != null)
                {
                    var cardStatusList = root.Elements("LoyaltyCardRewardPointsStatus");
                    cards = new Collection<LoyaltyCard>();
                    foreach (var cardStatus in cardStatusList)
                    {
                        var card = new LoyaltyCard();
                        card.CardNumber = TransactionServiceClient.GetAttributeValue(cardStatus, "LoyaltyCardNumber");
                        cards.Add(card);

                        var rewardPointStatusList = cardStatus.Elements("RewardPointStatus");
                        foreach (var rewardPointStatus in rewardPointStatusList)
                        {
                            var rewardPoint = new LoyaltyRewardPoint();
                            rewardPoint.RewardPointId = TransactionServiceClient.GetAttributeValue(rewardPointStatus, "RewardPointId");
                            rewardPoint.Description = TransactionServiceClient.GetAttributeValue(rewardPointStatus, "RewardPointDescription");
                            rewardPoint.RewardPointType = (LoyaltyRewardPointType)Convert.ToInt32(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "RewardPointType"));
                            rewardPoint.RewardPointCurrency = TransactionServiceClient.GetAttributeValue(rewardPointStatus, "Currency");
                            rewardPoint.IsRedeemable = Convert.ToBoolean(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "Redeemable"));
                            if (rewardPoint.IsRedeemable)
                            {
                                rewardPoint.RedeemRanking = Convert.ToInt32(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "RedeemRanking"));
                            }

                            rewardPoint.IssuedPoints = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "Issued"));
                            rewardPoint.UsedPoints = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "Used"));
                            rewardPoint.ExpiredPoints = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "Expired"));
                            rewardPoint.ActivePoints = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(rewardPointStatus, "Active"));

                            card.RewardPoints.Add(rewardPoint);
                        }
                    }
                }

                return cards;
            }

            /// <summary>
            /// Gets the loyalty card transaction of the given loyalty card and the reward point.
            /// </summary>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="rewardPointId">The readable identifier of the reward point.</param>
            /// <param name="top">The top count, i.e. the number of transactions to get.</param>
            /// <param name="skip">The skip number, i.e. the number of transactions to skip.</param>
            /// <param name="calculateRecordCount">
            /// The flag indicating whether the result should contains the total number of the transactions.
            /// </param>
            /// <returns>The page result containing the collection of loyalty card transactions.</returns>
            public PagedResult<LoyaltyCardTransaction> GetLoyaltyCardTransactions(string loyaltyCardNumber, string rewardPointId, long top, long skip, bool calculateRecordCount)
            {
                var parameters = new object[]
                {
                    loyaltyCardNumber,
                    rewardPointId,
                    top,
                    skip,
                    calculateRecordCount
                };

                var data = this.InvokeMethod(GetLoyaltyCardTransactionsMethodName, parameters);
                long? totalCount = null;

                // Translate the result data into a list of loyalty card transactions.
                string statusXML = (string)data[0];

                XDocument doc = XDocument.Parse(statusXML);
                XElement root = doc.Elements("LoyaltyCardRewardPointTransactions").SingleOrDefault();
                Collection<LoyaltyCardTransaction> transList = null;
                if (root != null)
                {
                    // Get total count
                    if (calculateRecordCount)
                    {
                        totalCount = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(root, "TotalTransactionNumber"));
                    }

                    // Get transaction list
                    var transElementList = root.Elements("LoyaltyCardRewardPointTransaction");
                    transList = new Collection<LoyaltyCardTransaction>();
                    foreach (var transElement in transElementList)
                    {
                        var trans = new LoyaltyCardTransaction();
                        trans.TransactionId = TransactionServiceClient.GetAttributeValue(transElement, "TransactionID");
                        trans.RewardPointAmountQuantity = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(transElement, "RewardPointAmountQty"));
                        trans.EntryType = (LoyaltyRewardPointEntryType)Convert.ToInt32(TransactionServiceClient.GetAttributeValue(transElement, "EntryType"));
                        trans.ChannelName = TransactionServiceClient.GetAttributeValue(transElement, "ChannelName");

                        string expirationDate = TransactionServiceClient.GetAttributeValue(transElement, "ExpirationDate");
                        if (!string.IsNullOrWhiteSpace(expirationDate))
                        {
                            trans.ExpirationDate = Convert.ToDateTime(expirationDate);
                        }

                        DateTime entryDateTime = Convert.ToDateTime(TransactionServiceClient.GetAttributeValue(transElement, "EntryDate"));
                        string entryTime = TransactionServiceClient.GetAttributeValue(transElement, "EntryTime");
                        TimeSpan entryTimeSpan = new TimeSpan();
                        if (TimeSpan.TryParse(entryTime, out entryTimeSpan))
                        {
                            entryDateTime += entryTimeSpan;
                        }

                        trans.EntryDateTime = entryDateTime;

                        transList.Add(trans);
                    }
                }

                var transactions = transList.AsReadOnly();
                return new PagedResult<LoyaltyCardTransaction>(transactions, new PagingInfo(top, skip), totalCount);
            }

            /// <summary>
            /// Creates an XElement representing a RetailLoyaltyCardRewardPointTrans record.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <param name="rewardLine">The loyalty reward line.</param>
            /// <returns>The XElement.</returns>
            private static XElement CreateXElementForRewardPointTrans(SalesTransaction transaction, ChannelConfiguration channelConfiguration, LoyaltyRewardPointLine rewardLine)
            {
                TimeSpan t = TimeSpan.FromSeconds(rewardLine.EntryTime);
                LoyaltyTransactionType loyaltyTransactionType;

                // For return customer order the local transaction object should have salesId empty.
                // The salesId for the return order has not been created yet.
                // The loyalty points should be catured against transactionId and latter updated to return salesId when it is created.            
                if (transaction.CustomerOrderMode == CustomerOrderMode.Return)
                {
                    loyaltyTransactionType = LoyaltyTransactionType.RetailTransaction;
                }
                else
                {
                    loyaltyTransactionType = string.IsNullOrEmpty(transaction.SalesId) ? LoyaltyTransactionType.RetailTransaction : LoyaltyTransactionType.SalesOrder;
                }

                XElement rewardElmt = new XElement(
                                        "RetailLoyaltyCardRewardPointTrans",
                                        new XElement("Affiliation", rewardLine.LoyaltyGroupRecordId),
                                        new XElement("CardNumber", rewardLine.LoyaltyCardNumber),
                                        new XElement("Channel", channelConfiguration.RecordId),
                                        new XElement("CustAccount", rewardLine.CustomerAccount),
                                        new XElement("CustAccountDataAreaId", channelConfiguration.InventLocationDataAreaId),
                                        new XElement("EntryDate", string.Format("{0:MM/dd/yyyy}", rewardLine.EntryDate)),
                                        new XElement("EntryTime", string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds)),
                                        new XElement("EntryType", (int)rewardLine.EntryType),
                                        new XElement("ExpirationDate", string.Format("{0:MM/dd/yyyy}", rewardLine.ExpirationDate)),
                                        new XElement("LoyaltyTier", rewardLine.LoyaltyTierRecordId),
                                        new XElement("LoyaltyTransactionType", (int)loyaltyTransactionType),
                                        new XElement("LoyaltyTransDataAreaId", channelConfiguration.InventLocationDataAreaId),
                                        new XElement("LoyaltyTransLineNum", rewardLine.LineNumber),
                                        new XElement("ReceiptId", transaction.ReceiptId ?? string.Empty),
                                        new XElement("RewardPoint", rewardLine.RewardPointRecordId),
                                        new XElement("RewardPointAmountQty", rewardLine.RewardPointAmountQuantity),
                                        new XElement("SalesId", LoyaltyTransactionType.SalesOrder == loyaltyTransactionType ? (transaction.SalesId ?? string.Empty) : string.Empty),
                                        new XElement("StaffId", transaction.StaffId ?? string.Empty),
                                        new XElement("StoreId", transaction.StoreId ?? string.Empty),
                                        new XElement("TerminalId", transaction.TerminalId ?? string.Empty),
                                        new XElement("TransactionId", LoyaltyTransactionType.SalesOrder == loyaltyTransactionType ? string.Empty : (transaction.Id ?? string.Empty)));

                return rewardElmt;
            }
        }
    }
}