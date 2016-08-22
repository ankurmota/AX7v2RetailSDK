<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StartPage.aspx.cs" Inherits="Contoso.Retail.SampleConnector.MerchantWeb.StartPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript">
        // When the page is loaded.
        function actionSelected() {
            var tokenizeRadioButton = document.getElementById("TokenizeRadioButton");
            var authorizeRadioButton = document.getElementById("AuthorizeRadioButton");
            var captureRadioButton = document.getElementById("CaptureRadioButton");
            var supportCardTokenizationCheckBox = document.getElementById("SupportCardTokenizationCheckBox");
            var supportVoiceAuthorizationCheckBox = document.getElementById("SupportVoiceAuthorizationCheckBox");
            var TransactionTypeHiddenField = document.getElementById("TransactionTypeHiddenField");

            if (tokenizeRadioButton.checked) {
                TransactionTypeHiddenField.value = "None";
                supportCardTokenizationCheckBox.checked = true;
                supportCardTokenizationCheckBox.disabled = true;
                supportVoiceAuthorizationCheckBox.checked = false;
                supportVoiceAuthorizationCheckBox.disabled = true;
            }
            else {
                if (authorizeRadioButton.checked) {
                    TransactionTypeHiddenField.value = "Authorize";
                }
                else if (captureRadioButton.checked) {
                    TransactionTypeHiddenField.value = "Capture";
                }

                supportCardTokenizationCheckBox.disabled = false;
                supportVoiceAuthorizationCheckBox.disabled = false;
            }

            supportCardTokenizationCheckBoxChanged();
            supportVoiceAuthorizationCheckBoxChanged();
        }

        // When "support card tokenization" is checked/unchecked
        function supportCardTokenizationCheckBoxChanged() {
            var supportCardTokenizationCheckBox = document.getElementById("SupportCardTokenizationCheckBox");
            var supportCardTokenizationHiddenField = document.getElementById("SupportCardTokenizationHiddenField");
            supportCardTokenizationHiddenField.value = supportCardTokenizationCheckBox.checked;
        }

        // When "support card swipe" is checked/unchecked
        function supportCardSwipeCheckBoxChanged() {
            var supportCardSwipeCheckBox = document.getElementById("SupportCardSwipeCheckBox");
            var supportCardSwipeHiddenField = document.getElementById("SupportCardSwipeHiddenField");
            supportCardSwipeHiddenField.value = supportCardSwipeCheckBox.checked;
        }

        // When "support voice authorization" is checked/unchecked
        function supportVoiceAuthorizationCheckBoxChanged() {
            var supportVoiceAuthorizationCheckBox = document.getElementById("SupportVoiceAuthorizationCheckBox");
            var supportVoiceAuthorizationHiddenField = document.getElementById("SupportVoiceAuthorizationHiddenField");
            supportVoiceAuthorizationHiddenField.value = supportVoiceAuthorizationCheckBox.checked;
        }
    </script>
    <title>Start page</title>
</head>
<body>
    <form id="StartForm" runat="server">
    <div>
        <div>
            <h1>Select a transaction type</h1>
        </div>
        <!--Transaction types-->
        <div>
            <fieldset>
                <legend>Transaction setup</legend>
                <div>Industry type</div>
                <div>
                    <asp:DropDownList ID="IndustryTypeDropDownList" runat="server">
                        <asp:ListItem Text="Retail" />
                        <asp:ListItem Text="DirectMarketing" />
                        <asp:ListItem Text="Ecommerce" />
                    </asp:DropDownList>
                </div>
                <br/>
                <div>
                    <input id="TokenizeRadioButton" type="radio" name="Action" onclick="actionSelected();"/>Tokenize only<br/>
                    <input id="AuthorizeRadioButton" type="radio" name="Action" onclick="actionSelected();"/>Authorize<br/>
                    <input id="CaptureRadioButton" type="radio" name="Action" onclick="actionSelected();"/>Authorize+Capture
                </div>
                <br/>
                <div>
                    <input id="SupportCardTokenizationCheckBox" type="checkbox" onclick="supportCardTokenizationCheckBoxChanged();" />Support card tokenization
                </div>
                <div>
                    <input id="SupportCardSwipeCheckBox" type="checkbox" onclick="supportCardSwipeCheckBoxChanged();" />Support card swipe
                </div>
                <div>
                    <input id="SupportVoiceAuthorizationCheckBox" type="checkbox" onclick="supportVoiceAuthorizationCheckBoxChanged();" />Support voice authorization
                </div>
                <div>
                    <asp:CheckBox ID="ShowSameAsShippingAddressCheckBox" Text="Show 'Same as shipping address'" runat="server" />
                </div>
                <br/>
                <div>Locale</div>
                <div><asp:TextBox ID="LocaleTextBox" Text="en-US" runat="server"/></div>
            </fieldset>
            <br />
            <fieldset>
                <legend>Host page setup</legend>
                <div>Page background color</div>
                <div><asp:TextBox ID="PageBackgroundColorTextBox" Text="white" runat="server"/></div>
                <div>IFrame width</div>
                <div><asp:TextBox ID="PageWidthTextBox" Text="920px" runat="server"/></div>
            </fieldset>
            <br/>
            <fieldset>
                <legend>Accepting page URL parameters</legend>
                <div>Font size</div>
                <div><asp:TextBox ID="FontSizeTextBox" Text="12px" runat="server"/></div>
                <div>Font family</div>
                <div><asp:TextBox ID="FontFamilyTextBox" Text='"Segoe UI"' runat="server"/></div>
                <div>Label color</div>
                <div><asp:TextBox ID="LabelColorTextBox" Text="black" runat="server"/></div>
                <div>Text background color</div>
                <div><asp:TextBox ID="TextBackgroundColorTextBox" Text="white" runat="server"/></div>
                <div>Text color</div>
                <div><asp:TextBox ID="TextColorTextBox" Text="black" runat="server"/></div>
                <div>Disabled text background color</div>
                <div><asp:TextBox ID="DisabledTextBackgroundColorTextBox" Text="#E4E4E4" runat="server"/></div>
                <div>Column number</div>
                <div><asp:TextBox ID="ColumnNumberTextBox" Text="2" runat="server"/></div>
            </fieldset>
            
        </div>
        
        <!--Additional fields-->
        <div>
            <br/>
            <div><asp:Button ID="NextButton" Text="Next" OnClick="NextButton_Click" runat="server" /></div>
            <div><asp:HiddenField ID="TransactionTypeHiddenField" runat="server" /></div>
            <div><asp:HiddenField ID="SupportCardTokenizationHiddenField" runat="server" /></div>
            <div><asp:HiddenField ID="SupportCardSwipeHiddenField" runat="server" /></div>
            <div><asp:HiddenField ID="SupportVoiceAuthorizationHiddenField" runat="server" /></div>
        </div>
    </div>
    </form>
</body>
</html>
