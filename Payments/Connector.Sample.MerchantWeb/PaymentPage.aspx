<!--
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
-->
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaymentPage.aspx.cs" Inherits="Contoso.Retail.SampleConnector.MerchantWeb.PaymentPage" Async="true"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript">
        // When the body is loaded.
        function bodyOnload() {
            // Set page background color
            document.bgColor = "<%= this.PageBackgroundColor %>";

            // Register key press handler to hanle card swipe.
            // We eat card swipe and alert user.
            document.onkeypress = handleDocumentKeyPress;

            // Do nothing if the origin of the card page (i.e. child page) is unavailable
            var cardPageOrigin = document.getElementById("CardPageOriginHiddenField").value;
            if (cardPageOrigin != "") {
                // Register event to listen message from the tokenization page.
                window.addEventListener("message", receiveMessage, false);
            }
        }

        // When "Place order" is clicked.
        function submitCard() {
            // Do nothing if the origin of the card page (i.e. child page) is unavailable
            var cardPageOrigin = document.getElementById("CardPageOriginHiddenField").value;
            if (cardPageOrigin != "") {
                // Send a message to the card page to trigger it submit
                var iframe = document.getElementById("CardPageFrame");

                var messageObject = new Object();
                messageObject.type = "msax-cc-amount";
                messageObject.value = document.getElementById("AmountTextBox").value;
                iframe.contentWindow.postMessage(JSON.stringify(messageObject), cardPageOrigin);

                messageObject.type = "msax-cc-submit";
                messageObject.value = "true";
                iframe.contentWindow.postMessage(JSON.stringify(messageObject), cardPageOrigin);
            }
        }

        // When a message is received from the card page (i.e. child page)
        function receiveMessage(event) {
            // Validate origin
            var cardPageOrigin = document.getElementById("CardPageOriginHiddenField").value;
            if (event.origin != cardPageOrigin)
                return;

            // Parse messages
            var message = event.data;
            if (typeof message == "string" && message.length > 0) {

                // Handle various messages from the card page
                var messageObject = JSON.parse(message);
                switch (messageObject.type) {
                    case "msax-cc-height":
                        // Set height of the iframe
                        document.getElementById("CardPageFrame").height = messageObject.value;
                        // Set focus on iframe
                        var iframe = document.getElementById("CardPageFrame");
                        iframe.contentWindow.focus();
                        break;
                    case "msax-cc-cardprefix":
                        // Use the card prefix
                        // In real product, the prefix may used for transaction validation.
                        document.getElementById("CardPrefixTextBox").value = messageObject.value;
                        break;
                    case "msax-cc-error":
                        // Show input errors
                        var paymentErrors = messageObject.value;
                        var errorLabel = document.getElementById("ErrorMessageLabel");
                        errorLabel.innerText = "";
                        for (var i = 0; i < paymentErrors.length; i++) {
                            errorLabel.innerText += paymentErrors[i].Message + " ";
                        }
                        break;
                    case "msax-cc-partialamount":
                        // Payment amount is partially approved, ask for user confirmation.
                        var approvedAmount = messageObject.value;
                        var confirmMessage = 'Payment is only partially approved. The approved amount is ' + approvedAmount + '. Do you want to accept the partial payment? ';
                        if (confirm(confirmMessage)) {
                            this.sendPartialConfirmation("True");
                        } else {
                            this.sendPartialConfirmation("False");
                        }
                        break;
                    case "msax-cc-result":
                        // Submit the order
                        document.getElementById("ResultAccessCodeHiddenField").value = messageObject.value;
                        document.getElementById("PaymentForm").submit();
                        break;
                    default:
                        // Unexpected message, just ignore it.
                }
            }
        }

        // Card swipe variables
        var keyPressedSourceElement; // the source element (e.g. an input) when key press happens
        var isEatingFastKeyStrokesEnabled = false; // the flag to enable eating some fast key strokes e.g. track 3 that we don't use.
        var eatenFastKeyStrokeChars = ""; // The fast key strokes that are eaten, e.g. track 3.
        var isLoggingSwipeEnabled = false; // The flag to enable logging of the card swipe including track 1 and track 2.
        var loggedSwipeChars = ""; // The characters that are logged as the card swipe.
        var detectCardSwipeTimerId = -1; // The timer Id of detectCardSwipe()
        var stopLoggingCardSwipeTimerId = -1; // Time Id of stopLoggingCardSwipe();
        var startChar = ""; // The first character of track data, e.g. '%', ';'

        // Handles a key press
        function handleDocumentKeyPress(e) {

            // Convert key code to a character
            var pressedChar = String.fromCharCode(e.keyCode);

            // Eat the character if the flag is enable, e.g. track 3 that we don't use.
            if (isEatingFastKeyStrokesEnabled) {
                eatenFastKeyStrokeChars += pressedChar;
                return false; // hide the character
            }

            // Log the character as part of card swipe if the flag is enabled.
            if (isLoggingSwipeEnabled) {
                logCharactor(pressedChar);
                return false;	// hide the character
            }
            else if (pressedChar == '%') { // track data begins from track1
                keyPressedSourceElement = e.srcElement;
                startChar = '%';
                startLogging(pressedChar);
                return false;	// hide the character
            }
            else if (pressedChar == ';') { // track data begins from track2
                keyPressedSourceElement = e.srcElement;
                startChar = ';';
                startLogging(pressedChar);
                return false;	// hide the character
            }
            else {
                return true;	// pass through char
            }
        }

        // Logs a character as part of card swipe.
        function logCharactor(pressedChar) {

            // Append the charracter
            loggedSwipeChars = loggedSwipeChars + pressedChar;

            // Recieved a '?'. It could be the end of track 1 or track 2.
            if (pressedChar == '?') {

                // This is the end of the track.
                // Eat the rest of the fast key strokes, e.g. track 3 that we don't use.
                isEatingFastKeyStrokesEnabled = true;
                window.setTimeout('eatFastKeyStrokes();', 250);

                // Disable logging for card swipe
                isLoggingSwipeEnabled = false;

                // Clear any open timers
                clearTimers();

                // Handle the swipe data
                handleSwipe(loggedSwipeChars);

                //clear track data
                loggedSwipeChars = '';
            }
        }
        
        // Starts the logging of card swipe.
        function startLogging(pressedChar, sourceElement) {

            // Clear any open timers.
            clearTimers();

            // Enable logging of card swipe
            isLoggingSwipeEnabled = true;
            loggedSwipeChars = pressedChar;

            // Set a timer to detect if this is really a swipe.
            detectCardSwipeTimerId = window.setTimeout('detectCardSwipe();', 250);

            // Set a timer to stop logging the card swipe.
            // The log can only stay open for 7.5 seconds. After that all card read input is lost.
            // This handles the scenario where the card swipe was busted.
            stopLoggingCardSwipeTimerId = window.setTimeout('stopLoggingCardSwipe();', 7500);
        }

        // Detects whether the read data is really from a card swipe.
        // The IDTech reads 22 chars in 1/4 second.  Human typing is max 3 chars in 1/4 second.
        // The key press must get at least 5 chars in 1/4 second to be considered a swipe.
        function detectCardSwipe() {

            // Notify the main execution that the detectCardSwipe() has executed. No need to clear timer.
            detectCardSwipeTimerId = -1;

            // Check the second character.
            // If it is also the start character ('%', ';'), the fast key strokes could be the result of long press '%' or ';' key.
            // Then it is not a card swipe.
            var secondChar = loggedSwipeChars.substring(1, 2);
            if (loggedSwipeChars.length < 5 || secondChar == startChar) {

                // This is not a swipe. Instead, the user manually entered a swipe key character.
                // isLoggingSwipeEnabled = false will tell the calling code to allow the characters to pass through.
                isLoggingSwipeEnabled = false;

                // Append the logged characters back into the source element, e.g. textbox
                if (keyPressedSourceElement.tagName.toLowerCase() == "input"
                    && (keyPressedSourceElement.type.toLowerCase() == "text"
                        || keyPressedSourceElement.type.toLowerCase() == "password")) {

                    // NOTE: all textfield attributes have the .maxLength property with a default value
                    // of 2^31 (or, 0 if it was set to an improper value) if the value was not programatically set...
                    // Check maxlength property of this field before appending the additional chars
                    var maxLength = keyPressedSourceElement.maxLength;
                    if (maxLength > 0) {
                        keyPressedSourceElement.value = (keyPressedSourceElement.value + loggedSwipeChars).substring(0, maxLength);
                    }
                    else {
                        keyPressedSourceElement.value += loggedSwipeChars;
                    }
                }

                loggedSwipeChars = '';
            }
        }

        // Stops the logging of the card swipe.
        function stopLoggingCardSwipe() {

            // Notify the main execution that the stopLoggingCardSwipe() has executed. No need to clear timer.
            stopLoggingCardSwipeTimerId = -1;

            if (isLoggingSwipeEnabled) {

                // We are going to clear the keyboard log resulting in a loss of all logged chars.
                isLoggingSwipeEnabled = false;
                loggedSwipeChars = '';
            }
        }

        // Clear the timers
        function clearTimers() {
            if (detectCardSwipeTimerId != -1) {
                window.clearTimeout(detectCardSwipeTimerId);
                detectCardSwipeTimerId = -1;
            }

            if (stopLoggingCardSwipeTimerId != -1) {
                window.clearTimeout(stopLoggingCardSwipeTimerId);
                stopLoggingCardSwipeTimerId = -1;
            }
        }

        // Eats the fast key strokes, used for track 3.
        function eatFastKeyStrokes() {

            // Stop eating when the key stroke is not fast. We must be at the end of the line
            if (eatenFastKeyStrokeChars.length < 5) {

                isEatingFastKeyStrokesEnabled = false;
                eatenFastKeyStrokeChars = "";
                return;
            }

            // Execution returns from this instance of eatFastKeyStrokes()
            // We schedule another instance of eatFastKeyStrokes() to run again in 1/4 second.
            eatenFastKeyStrokeChars = "";
            window.setTimeout('eatFastKeyStrokes();', 250);
        }

        // Sends the swipe data into the inner page.
        function handleSwipe(swipe) {
            // Do nothing if the origin of the card page (i.e. child page) is unavailable
            var cardPageOrigin = document.getElementById("CardPageOriginHiddenField").value;
            if (cardPageOrigin != "") {
                // Send a message to the card page for swipe data
                var iframe = document.getElementById("CardPageFrame");

                var messageObject = new Object();
                messageObject.type = "msax-cc-swipe";
                messageObject.value = swipe;
                iframe.contentWindow.postMessage(JSON.stringify(messageObject), cardPageOrigin);
            }
        }

        // Sends partial authorization confirmation result into the inner page.
        function sendPartialConfirmation(confirmationResult) {
            // Do nothing if the origin of the card page (i.e. child page) is unavailable
            var cardPageOrigin = document.getElementById("CardPageOriginHiddenField").value;
            if (cardPageOrigin != "") {
                // Send a message to the card page for swipe data
                var iframe = document.getElementById("CardPageFrame");

                var messageObject = new Object();
                messageObject.type = "msax-cc-partialok";
                messageObject.value = confirmationResult;
                iframe.contentWindow.postMessage(JSON.stringify(messageObject), cardPageOrigin);
            }
        }
    </script>
    <title>Payment page</title>
</head>
<body id="PaymentPageBody" onload="bodyOnload();">
    <form id="PaymentForm" runat="server">
    <div>
        <div>
            <h1>Enter your payment card</h1>
        </div>
        <div>
            <asp:Label ID="ErrorMessageLabel" runat="server" ForeColor="Red"></asp:Label>
        </div>

        <!--The iframe containing the tokenization page-->
        <div>
            <div></div>
            <iframe id="CardPageFrame" style="border:1px solid; width: <%= this.PageWidth %>" sandbox="allow-scripts allow-forms allow-same-origin" src="<%= this.PaymentAcceptUrl %>"></iframe>
            <div></div>
        </div>
        
        <!--Additional fields on payment page-->
        <div>
            <div>Received card number prefix</div>
            <div><asp:TextBox ID="CardPrefixTextBox" ReadOnly="true" runat="server"></asp:TextBox></div>
            <br />
            <div>Payment amount</div>
            <div><asp:TextBox ID="AmountTextBox" runat="server"></asp:TextBox></div>
            <br />
            <div><input id="PlaceOrderButton" type="button" value="Place order" onclick="submitCard(); return false;"/></div>
            <div><asp:HiddenField ID="CardPageOriginHiddenField" runat="server" /></div>
            <div><asp:HiddenField ID="ResultAccessCodeHiddenField" runat="server" /></div>
        </div>
    </div>
    </form>
</body>
</html>
