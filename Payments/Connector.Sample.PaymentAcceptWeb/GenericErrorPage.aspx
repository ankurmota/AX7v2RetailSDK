<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GenericErrorPage.aspx.cs" Inherits="Contoso.Retail.SampleConnector.PaymentAcceptWeb.GenericErrorPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="background-color:white">
    <form id="ErrorForm" runat="server">
    <div>
        <label id="ErrorLabel"><asp:Literal runat="server" Text="<%$ Resources:WebResources, GenericErrorPage_ErrorLabel %>"/></label>
    </div>
    </form>
</body>
</html>
