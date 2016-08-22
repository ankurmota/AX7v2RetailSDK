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
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string CreateWishListMethodName = "CreateWishList";
            private const string DeleteWishListMethodName = "DeleteWishList";
            private const string GetWishListsMethodName = "GetWishLists";
            private const string UpdateWishListMethodName = "UpdateWishList";
            private const string CreateWishListLineMethodName = "CreateWishListLine";
            private const string DeleteWishListLineMethodName = "DeleteWishListLine";
            private const string UpdateWishListLineMethodName = "UpdateWishListLine";
            private const string MoveWishListLinesMethodName = "MoveWishListLines";
            private const string CopyWishListLinesMethodName = "CopyWishListLines";
            private const string CreateWishListContributorsMethodName = "CreateWishListContributors";
            private const string DeleteWishListContributorsMethodName = "DeleteWishListContributors";
            private const string CreateWishListInvitationsMethodName = "CreateWishListInvitations";
            private const string AcceptWishListInvitationMethodName = "AcceptWishListInvitation";
    
            /// <summary>
            /// Create a commerce list.
            /// </summary>
            /// <param name="commerceList">The commerce list object.</param>
            /// <returns>Returns the commerce list object with a record id.</returns>
            public CommerceList CreateCommerceList(CommerceList commerceList)
            {
                ThrowIf.Null(commerceList, "commerceList");
    
                string xmlInput = CommerceListToXml(commerceList);
                var data = this.InvokeMethod(CreateWishListMethodName, xmlInput);
    
                var response = data[0].ToString();
                if (string.IsNullOrEmpty(response))
                {
                    return null;
                }
    
                XDocument doc = XDocument.Parse(response);
                if (doc == null)
                {
                    return null;
                }
    
                var list = doc.Element("RetailWishListTable");
                if (list == null)
                {
                    return null;
                }
    
                long recId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(list, "RecId"));
                commerceList.Id = recId;
    
                var lines = list.Elements("RetailWishListLineTable");
                var linesList = lines.ToList();
                for (int i = 0; i < linesList.Count; i++)
                {
                    recId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(linesList[i], "RecId"));
                    commerceList.CommerceListLines[i].LineId = recId;
                }
    
                return commerceList;
            }
    
            /// <summary>
            /// Delete a commerce list.
            /// </summary>
            /// <param name="recordId">The commerce list id.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            public void DeleteCommerceList(long recordId, string filterAccountNumber)
            {
                this.InvokeMethodNoDataReturn(DeleteWishListMethodName, recordId, filterAccountNumber);
            }
    
            /// <summary>
            /// Retrieves the commerce lists from AX.
            /// </summary>
            /// <param name="commerceListId">The commerce list id.</param>
            /// <param name="customerId">The customer id.</param>
            /// <param name="favoriteFilter">Indicates whether or not to filter by favorite.</param>
            /// <param name="publicFilter">Indicates whether or not to filter by public.</param>
            /// <returns>Returns the commerce lists from AX.</returns>
            public PagedResult<CommerceList> GetCommerceLists(long commerceListId, string customerId, bool favoriteFilter, bool publicFilter)
            {
                var data = this.InvokeMethod(GetWishListsMethodName, commerceListId, customerId, favoriteFilter, publicFilter);
    
                IEnumerable<CommerceList> wishLists = new List<CommerceList>();
    
                var response = data[0].ToString();
                if (!string.IsNullOrEmpty(response))
                {
                    XDocument doc = XDocument.Parse(response);
                    XElement root = doc.Elements("WishLists").FirstOrDefault();
    
                    if (root != null)
                    {
                        wishLists = root.Elements("RetailWishListTable").Select<XElement, CommerceList>(
                            (wL) =>
                            {
                                CommerceList commerceList = TransactionServiceClient.ParseCommerceList(wL);
                                return commerceList;
                            });
                    }
                }
    
                return wishLists.AsPagedResult();
            }
    
            /// <summary>
            /// Update a commerce list.
            /// </summary>
            /// <param name="commerceList">The commerce list object.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList UpdateCommerceList(CommerceList commerceList, string filterAccountNumber)
            {
                ThrowIf.Null(commerceList, "commerceList");
    
                string xmlInput = CommerceListToXml(commerceList);
                var data = this.InvokeMethod(UpdateWishListMethodName, xmlInput, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Create a commerce list line.
            /// </summary>
            /// <param name="commerceListLine">The commerce list line object.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList CreateCommerceListLine(CommerceListLine commerceListLine, string filterAccountNumber)
            {
                ThrowIf.Null(commerceListLine, "commerceListLine");
    
                string xmlInput = CommerceListLineToXml(commerceListLine);
                var data = this.InvokeMethod(CreateWishListLineMethodName, xmlInput, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Delete a commerce list line.
            /// </summary>
            /// <param name="recordId">The commerce list line id.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList DeleteCommerceListLine(long recordId, string filterAccountNumber)
            {
                var data = this.InvokeMethod(DeleteWishListLineMethodName, recordId, 0, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Update a commerce list line.
            /// </summary>
            /// <param name="commerceListLine">The commerce list line object.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList UpdateCommerceListLine(CommerceListLine commerceListLine, string filterAccountNumber)
            {
                ThrowIf.Null(commerceListLine, "commerceListLine");
    
                string xmlInput = CommerceListLineToXml(commerceListLine);
                var data = this.InvokeMethod(UpdateWishListLineMethodName, xmlInput, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Moves the commerce list lines to another list.
            /// </summary>
            /// <param name="commerceListLines">The lines to move.</param>
            /// <param name="destinationCommerceListId">The destination list.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList MoveCommerceListLines(IEnumerable<CommerceListLine> commerceListLines, long destinationCommerceListId, string filterAccountNumber)
            {
                ThrowIf.Null(commerceListLines, "commerceListLines");
    
                string xml = TransactionServiceClient.BuildXmlFromCommerceListLines(commerceListLines);
                var data = this.InvokeMethod(MoveWishListLinesMethodName, xml, destinationCommerceListId, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Copies the commerce list lines to another list.
            /// </summary>
            /// <param name="commerceListLines">The lines to copy.</param>
            /// <param name="destinationCommerceListId">The destination list.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList CopyCommerceListLines(IEnumerable<CommerceListLine> commerceListLines, long destinationCommerceListId, string filterAccountNumber)
            {
                ThrowIf.Null(commerceListLines, "commerceListLines");
    
                string xml = TransactionServiceClient.BuildXmlFromCommerceListLines(commerceListLines);
                var data = this.InvokeMethod(CopyWishListLinesMethodName, xml, destinationCommerceListId, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Creates contributors to commerce lists.
            /// </summary>
            /// <param name="commerceListId">The commerce list to add the contributors to.</param>
            /// <param name="contributors">The contributors to create.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList CreateCommerceListContributors(long commerceListId, IEnumerable<CommerceListContributor> contributors, string filterAccountNumber)
            {
                ThrowIf.Null(contributors, "contributors");
    
                string xmlInput = CommerceListContributorsToXml(commerceListId, contributors);
                var data = this.InvokeMethod(CreateWishListContributorsMethodName, xmlInput, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Deletes contributors from commerce lists.
            /// </summary>
            /// <param name="commerceListId">The commerce list to delete the contributors from.</param>
            /// <param name="contributors">The contributors to delete.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList DeleteCommerceListContributors(long commerceListId, IEnumerable<CommerceListContributor> contributors, string filterAccountNumber)
            {
                ThrowIf.Null(contributors, "contributors");
    
                string xmlInput = CommerceListContributorsToXml(commerceListId, contributors);
                var data = this.InvokeMethod(DeleteWishListContributorsMethodName, xmlInput, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Creates invitations to commerce lists.
            /// </summary>
            /// <param name="commerceListId">The commerce list to create the invitation for.</param>
            /// <param name="invitations">The invitations to create.</param>
            /// <param name="filterAccountNumber">
            /// The filter account number. It is used to check the operation privilege and filter the result content.
            /// Set the value to null to skip filtering.
            /// </param>
            /// <returns>The result commerce list.</returns>
            public CommerceList CreateCommerceListInvitations(long commerceListId, IEnumerable<CommerceListInvitation> invitations, string filterAccountNumber)
            {
                ThrowIf.Null(invitations, "invitations");
    
                string xmlInput = CommerceListInvitationsToXml(commerceListId, invitations);
                var data = this.InvokeMethod(CreateWishListInvitationsMethodName, xmlInput, filterAccountNumber);
    
                return TransactionServiceClient.ParseCommerceList(data[0].ToString());
            }
    
            /// <summary>
            /// Accepts the invitation to the commerce list.
            /// </summary>
            /// <param name="token">The invitation token.</param>
            /// <param name="customerId">The identifier of the customer who accepts the invitation.</param>
            public void AcceptCommerceListInvitation(string token, string customerId)
            {
                ThrowIf.NullOrWhiteSpace(token, "token");
                ThrowIf.NullOrWhiteSpace(customerId, "customerId");
    
                this.InvokeMethodNoDataReturn(AcceptWishListInvitationMethodName, token, customerId);
            }
    
            /// <summary>
            /// Parses an XML into a commerce list.
            /// </summary>
            /// <param name="xml">The XML.</param>
            /// <returns>The commerce list.</returns>
            private static CommerceList ParseCommerceList(string xml)
            {
                CommerceList commerceList = null;
    
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    XDocument doc = XDocument.Parse(xml);
                    XElement wishlistElement = doc.Elements("RetailWishListTable").FirstOrDefault();
    
                    if (wishlistElement != null)
                    {
                        commerceList = TransactionServiceClient.ParseCommerceList(wishlistElement);
                    }
                }
    
                return commerceList;
            }
    
            /// <summary>
            /// Parse commerce list xml data into object.
            /// </summary>
            /// <param name="xmlCommerceList">Xml format of a commerce list.</param>
            /// <returns>Returns the commerce list object.</returns>
            private static CommerceList ParseCommerceList(XElement xmlCommerceList)
            {
                ThrowIf.Null(xmlCommerceList, "xmlCommerceList");
    
                var clist = new CommerceList();
    
                clist.Id = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(xmlCommerceList, "RecId"));
                clist.CustomerId = TransactionServiceClient.GetAttributeValue(xmlCommerceList, "CustomerId");
                clist.CustomerName = TransactionServiceClient.GetAttributeValue(xmlCommerceList, "CustomerName");
                clist.Name = TransactionServiceClient.GetAttributeValue(xmlCommerceList, "Name");
                clist.IsFavorite = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceList, "IsFavorite"));
                clist.IsRecurring = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceList, "IsRecurring"));
                clist.IsPrivate = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceList, "IsPrivate"));
                clist.IsCollaborative = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceList, "IsCollaborative"));
                clist.CreatedDateTime = TransactionServiceClient.ParseDateTimeOffset(xmlCommerceList, "CreatedDateTime");
                clist.DueDateTime = TransactionServiceClient.ParseDateTimeOffset(xmlCommerceList, "DueDateTime");
    
                var lineElem = xmlCommerceList.Element("WishListLines");
                IEnumerable<CommerceListLine> clistLines = Enumerable.Empty<CommerceListLine>();
    
                if (lineElem != null)
                {
                    clistLines = lineElem.Elements("RetailWishListLineTable").Select<XElement, CommerceListLine>(
                        (wLL) =>
                        {
                            CommerceListLine commerceListLine = TransactionServiceClient.ParseCommerceListLine(wLL);
                            return commerceListLine;
                        });
                    clist.CommerceListLines = clistLines.ToList();
                }
    
                var contributorElem = xmlCommerceList.Element("WishListContributors");
                if (contributorElem != null)
                {
                    var contributors = TransactionServiceClient.ParseCommerceListContributors(contributorElem.ToString());
                    clist.CommerceListContributors = contributors.ToList();
                }
    
                var invitationsElem = xmlCommerceList.Element("WishListInvitations");
                if (invitationsElem != null)
                {
                    var invitations = TransactionServiceClient.ParseCommerceListInvitations(invitationsElem.ToString());
                    clist.CommerceListInvitations = invitations.ToList();
                }
    
                return clist;
            }
    
            /// <summary>
            /// Parse commerce list line xml data into object.
            /// </summary>
            /// <param name="xmlCommerceListLine">Xml format of a commerce list line.</param>
            /// <returns>Returns the commerce list line object.</returns>
            private static CommerceListLine ParseCommerceListLine(XElement xmlCommerceListLine)
            {
                var clistLine = new CommerceListLine();
    
                clistLine.LineId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "RecId"));
                clistLine.CommerceListId = Convert.ToInt64(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "WishListId"));
                clistLine.CustomerId = TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "CustomerId");
                clistLine.CustomerName = TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "CustomerName");
                clistLine.UnitOfMeasure = TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "UnitOfMeasure");
                clistLine.ProductId = long.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "ProductId"));
                clistLine.Quantity = decimal.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "Quantity"));
                clistLine.IsFavorite = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "IsFavorite"));
                clistLine.IsRecurring = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "IsRecurring"));
                clistLine.IsPrivate = bool.Parse(TransactionServiceClient.GetAttributeValue(xmlCommerceListLine, "IsPrivate"));
                clistLine.CreatedDateTime = TransactionServiceClient.ParseDateTimeOffset(xmlCommerceListLine, "CreatedDateTime");
    
                return clistLine;
            }
    
            /// <summary>
            /// Builds an XML string from commerce list lines.
            /// </summary>
            /// <param name="commerceListLines">The lines.</param>
            /// <returns>The XML.</returns>
            private static string BuildXmlFromCommerceListLines(IEnumerable<CommerceListLine> commerceListLines)
            {
                StringBuilder xmlInput = new StringBuilder();
    
                xmlInput.Append("<WishListLines>");
                foreach (var line in commerceListLines)
                {
                    xmlInput.Append(CommerceListLineToXml(line));
                }
    
                xmlInput.Append("</WishListLines>");
                return xmlInput.ToString();
            }
    
            /// <summary>
            /// Serialize the commerce list to xml format.
            /// </summary>
            /// <param name="commerceList">The commerce list object.</param>
            /// <returns>The commerce list in xml format.</returns>
            private static string CommerceListToXml(CommerceList commerceList)
            {
                ThrowIf.Null(commerceList, "commerceList");
    
                StringBuilder strOutput = new StringBuilder();
    
                if (commerceList.DueDateTime == null || commerceList.DueDateTime < AxMinDateTime)
                {
                    commerceList.DueDateTime = AxMinDateTime;
                }
    
                strOutput.Append("<RetailWishListTable");
                strOutput.Append(" RecId = \"" + commerceList.Id + "\"");
                strOutput.Append(" CustomerId=\"" + commerceList.CustomerId + "\"");
                strOutput.Append(" Name=\"" + commerceList.Name + "\"");
                strOutput.Append(" IsFavorite=\"" + commerceList.IsFavorite + "\"");
                strOutput.Append(" IsRecurring=\"" + commerceList.IsRecurring + "\"");
                strOutput.Append(" IsPrivate=\"" + commerceList.IsPrivate + "\"");
                strOutput.Append(" IsCollaborative=\"" + commerceList.IsCollaborative + "\"");
                strOutput.Append(" DueDateTime=\"" + commerceList.DueDateTime.ToString("yyyy-MM-ddTHH:mm:ss") + "\"");
                strOutput.Append(">");
    
                bool hasLines = commerceList.CommerceListLines != null && commerceList.CommerceListLines.Count > 0;
    
                if (hasLines)
                {
                    strOutput.Append("<WishListLines>");
                }
    
                foreach (var line in commerceList.CommerceListLines)
                {
                    strOutput.Append(CommerceListLineToXml(line));
                }
    
                if (hasLines)
                {
                    strOutput.Append("</WishListLines>");
                }
    
                strOutput.Append("</RetailWishListTable>");
    
                return strOutput.ToString();
            }
    
            /// <summary>
            /// Serialize the commerce list line to xml format.
            /// </summary>
            /// <param name="commerceListLine">The commerce list line object.</param>
            /// <returns>The commerce list line in xml format.</returns>
            private static string CommerceListLineToXml(CommerceListLine commerceListLine)
            {
                ThrowIf.Null(commerceListLine, "commerceListLine");
    
                StringBuilder strOutput = new StringBuilder();
    
                strOutput.Append("<RetailWishListLineTable");
                strOutput.Append(" RecId = \"" + commerceListLine.LineId + "\"");
                strOutput.Append(" WishListId = \"" + commerceListLine.CommerceListId + "\"");
                strOutput.Append(" CustomerId=\"" + commerceListLine.CustomerId + "\"");
                strOutput.Append(" ProductId=\"" + commerceListLine.ProductId + "\"");
                strOutput.Append(" Quantity=\"" + commerceListLine.Quantity + "\"");
                strOutput.Append(" UnitOfMeasure=\"" + commerceListLine.UnitOfMeasure + "\"");
                strOutput.Append(" IsFavorite=\"" + commerceListLine.IsFavorite + "\"");
                strOutput.Append(" IsRecurring=\"" + commerceListLine.IsRecurring + "\"");
                strOutput.Append(" IsPrivate=\"" + commerceListLine.IsPrivate + "\"");
                strOutput.Append(">");
                strOutput.Append("</RetailWishListLineTable>");
    
                return strOutput.ToString();
            }
    
            /// <summary>
            /// Serializes the commerce list contributors to xml format.
            /// </summary>
            /// <param name="commerceListId">The commerce list to add the contributors to.</param>
            /// <param name="contributors">The contributors.</param>
            /// <returns>The xml result.</returns>
            private static string CommerceListContributorsToXml(long commerceListId, IEnumerable<CommerceListContributor> contributors)
            {
                ThrowIf.Null(contributors, "contributors");
    
                StringBuilder strOutput = new StringBuilder();
    
                strOutput.Append("<WishListContributors>");
    
                foreach (var contributor in contributors)
                {
                    strOutput.Append("<RetailWishListContributor");
                    strOutput.Append(" RecId = \"" + contributor.RecordId + "\"");
                    strOutput.Append(" WishListId = \"" + commerceListId + "\"");
                    strOutput.Append(" CustomerId=\"" + contributor.CustomerId + "\"");
                    strOutput.Append(" Invitation=\"" + contributor.InvitationId + "\"");
                    strOutput.Append(">");
                    strOutput.Append("</RetailWishListContributor>");
                }
    
                strOutput.Append("</WishListContributors>");
    
                return strOutput.ToString();
            }
    
            /// <summary>
            /// Parses XML into commerce list contributors.
            /// </summary>
            /// <param name="xml">The XML.</param>
            /// <returns>The contributors.</returns>
            private static IEnumerable<CommerceListContributor> ParseCommerceListContributors(string xml)
            {
                ThrowIf.NullOrWhiteSpace(xml, "xml");
    
                XDocument doc = XDocument.Parse(xml);
                var contributorElements = doc.Descendants("RetailWishListContributor");
    
                List<CommerceListContributor> contributors = new List<CommerceListContributor>();
                foreach (var contributorElement in contributorElements)
                {
                    CommerceListContributor contributor = new CommerceListContributor();
                    contributor.RecordId = Convert.ToInt64(contributorElement.Attribute("RecId").Value);
                    contributor.CustomerId = contributorElement.Attribute("CustomerId").Value;
                    contributor.CustomerName = contributorElement.Attribute("CustomerName").Value;
                    contributor.InvitationId = Convert.ToInt64(contributorElement.Attribute("Invitation").Value);
                    contributors.Add(contributor);
                }
    
                return contributors;
            }
    
            /// <summary>
            /// Serializes the commerce list invitations to xml format.
            /// </summary>
            /// <param name="commerceListId">The identifier of the commerce list.</param>
            /// <param name="invitations">The invitations.</param>
            /// <returns>The xml result.</returns>
            private static string CommerceListInvitationsToXml(long commerceListId, IEnumerable<CommerceListInvitation> invitations)
            {
                ThrowIf.Null(invitations, "invitations");
    
                StringBuilder strOutput = new StringBuilder();
    
                strOutput.Append("<WishListInvitations>");
    
                foreach (var invitation in invitations)
                {
                    strOutput.Append("<RetailWishListInvitation");
                    strOutput.Append(" RecId = \"" + invitation.RecordId + "\"");
                    strOutput.Append(" WishListId = \"" + commerceListId + "\"");
                    strOutput.Append(" Invitee =\"" + invitation.Invitee + "\"");
                    strOutput.Append(" Type =\"" + invitation.InvitationTypeValue + "\"");
                    strOutput.Append(">");
                    strOutput.Append("</RetailWishListInvitation>");
                }
    
                strOutput.Append("</WishListInvitations>");
    
                return strOutput.ToString();
            }
    
            /// <summary>
            /// Parses XML into commerce list invitations.
            /// </summary>
            /// <param name="xml">The XML.</param>
            /// <returns>The invitations.</returns>
            private static IEnumerable<CommerceListInvitation> ParseCommerceListInvitations(string xml)
            {
                ThrowIf.NullOrWhiteSpace(xml, "xml");
    
                XDocument doc = XDocument.Parse(xml);
                var invitationElements = doc.Descendants("RetailWishListInvitation");
    
                List<CommerceListInvitation> invitations = new List<CommerceListInvitation>();
                foreach (var invitationElement in invitationElements)
                {
                    CommerceListInvitation invitation = new CommerceListInvitation();
                    invitation.RecordId = Convert.ToInt64(invitationElement.Attribute("RecId").Value);
                    invitation.Invitee = invitationElement.Attribute("Invitee").Value;
                    invitation.IsSent = Convert.ToBoolean(invitationElement.Attribute("IsSent").Value);
                    invitation.LastRequestDateTime = TransactionServiceClient.ParseDateTimeOffset(invitationElement, "LastRequestDateTime");
                    invitation.LastSentDateTime = TransactionServiceClient.ParseDateTimeOffset(invitationElement, "LastSentDateTime");
                    invitation.StatusValue = Convert.ToInt32(invitationElement.Attribute("Status").Value);
                    invitation.Token = invitationElement.Attribute("Token").Value;
                    invitation.InvitationTypeValue = Convert.ToInt32(invitationElement.Attribute("Type").Value);
    
                    invitations.Add(invitation);
                }
    
                return invitations;
            }
        }
    }
}
