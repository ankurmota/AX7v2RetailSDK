<!--
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
-->
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResultPage.aspx.cs" Inherits="Contoso.Retail.SampleConnector.MerchantWeb.ResultPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Result page</title>
</head>
<body>
    <form id="ResultPageForm" runat="server">
    <div>
        <h1>Transaction results</h1>
        <div>Card token: <%= this.CardToken %></div>
        <div>Authorization result: <%= this.AuthorizationResult %></div>
        <div>Capture result: <%= this.CaptureResult %></div>
        <div>Void result: <%= this.VoidResult %></div>
        <div>Payment errors: <%= this.Errors %></div>
        <br/>
        <div>* DO NOT return the token to the client in production.</div>
        <br/>
        <div><asp:Button ID="StartOverButton" Text="Start Over" OnClick="StartOverButton_Click" runat="server" /></div>
    </div>
    </form>
</body>
</html>
