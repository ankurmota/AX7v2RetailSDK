/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='IMagneticStripeReader.ts'/>

module Commerce.Peripherals {
    "use strict";

    export class MSRKeyboardSwipeParser implements Peripherals.IMagneticStripeReader {
        private static START_TRACK: string = "%";
        private static START_TRACK_2: string = ";";
        private static START_TRACK_3: string = "+";
        private static END_TRACK: string = "?";

        private _swipeBuffer: string = "";
        private _fastCharsEaten: string = "";
        private _captureKeys: boolean = false;
        private _track2Started: boolean = false;
        private _keyPressedSourceElement;
        private _eatFastKeyStrokes: boolean = false;
        private _keystrokeLogTimerID;
        private _cardReaderStreamTimerID;
        private _isSwipeEnabled: boolean = false;
        private _readerMsgEventHandler;

        private _cardNumber: string = "";
        private _expiryMonth: number = 0;
        private _expiryYear: number = 0;
        private _track1: string = "";
        private _track2: string = "";
        private _swipeHandlerPointer: (e: KeyboardEvent) => boolean = this.SwipeParserDocumentKeyPressHandler.bind(this);

        // Switch on for logging
        private _logEnabled: boolean = false;

        /**
         * Enable MSR device for scan.
         *
         * @param {cardInfo: Model.Entities.CardInfo) => void} readerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(readerMsgEventHandler: (cardInfo: Model.Entities.CardInfo) => void): IVoidAsyncResult {
            this.enable(readerMsgEventHandler);
            var result: VoidAsyncResult = new VoidAsyncResult();
            result.resolve();
            return result;
        }

        /**
         * Disable MSR device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            this.disable();
            return VoidAsyncResult.createResolved();
        }

        public disable() {
            if (this._isSwipeEnabled) {
                this._isSwipeEnabled = false;
                KeyboardPressEventAggregator.removeListner(this._swipeHandlerPointer);
            }
        }

        public enable(readerMsgEventHandler?: (cardInfo: Model.Entities.CardInfo) => void) {

            if (ObjectExtensions.isNullOrUndefined(readerMsgEventHandler)) {
                return;
            }

            this._readerMsgEventHandler = readerMsgEventHandler;

            if (!this._isSwipeEnabled) {
                KeyboardPressEventAggregator.addListener(this._swipeHandlerPointer, 5);
                this._isSwipeEnabled = true;
            }
        }

        private SwipeParserDocumentKeyPressHandler(e: KeyboardEvent) {
            var key = e.keyCode;
            var self = this;
            var pressedChar = String.fromCharCode(key);

            if (this._eatFastKeyStrokes) {
                this._fastCharsEaten += pressedChar;

                return false;  //eat char
            }

            this._keyPressedSourceElement = e.srcElement;

            if (this._captureKeys) {
                this._swipeBuffer = this._swipeBuffer + pressedChar;

                //recieved a '?'
                if (pressedChar == MSRKeyboardSwipeParser.END_TRACK) {
                    if (this._track2Started) {
                        //SANITYMUNCH: eat the rest of the fast characters
                        this._eatFastKeyStrokes = true;
                        setTimeout(function () { self.EatFastKeyStrokes(); }, 250);

                        this._captureKeys = false;
                        this._track2Started = false;

                        //clear any open timers
                        this.ClearIsCardReaderStreamTimer();
                        this.ClearKeystrokeLogTimer();

                        this.WriteLog("in onkeypress event handler: got all track data now parsing card...");
                        //parse it here and fill fields
                        if (this.ParseSwipe()) {
                            this.WriteLog("success parsing card data");

                            if (typeof (this._readerMsgEventHandler) == 'function') {
                                this._readerMsgEventHandler.call(this,
                                    <Model.Entities.CardInfo>{
                                        CardNumber: this._cardNumber,
                                        ExpirationMonth: this._expiryMonth,
                                        ExpirationYear: this._expiryYear,
                                        Track1: this._track1 || StringExtensions.EMPTY,
                                        Track2: this._track2 || StringExtensions.EMPTY,
                                        Track3: StringExtensions.EMPTY
                                    });
                            }
                        }
                        else {
                            this.WriteLog("failure parsing card data");
                        }

                        //clear it
                        this._swipeBuffer = '';
                    }
                    else {
                        //everything after this char is track 2 data
                        this._track2Started = true;
                    }
                }

                return false;  //eat char
            }
            else if (pressedChar == MSRKeyboardSwipeParser.START_TRACK || pressedChar == MSRKeyboardSwipeParser.START_TRACK_2) {
                // If we got here and logging is not disabled it is possible that some logtimers are still waiting to
                // run in the background... at this point they're not needed so this is more of a sanity check
                this.ClearIsCardReaderStreamTimer();
                this.ClearKeystrokeLogTimer();

                // If this is a start character (% start for track1 OR ; start for track 2) start checking
                this._captureKeys = true;
                this._swipeBuffer = pressedChar;

                // Set timer to see if this is really a swipe
                this.WriteLog("INFO: starting IsCardReaderStream()");
                this._cardReaderStreamTimerID = setTimeout(function () { self.IsCardReaderStream(); }, 250);

                // the log can only stay open for 7.5 seconds.  all card read input is lost.   this handles the scenario where
                // the card swipe was busted.
                this.WriteLog("INFO: starting KeystrokeLogTimer()");
                this._keystrokeLogTimerID = setTimeout(function () { self.KeystrokeLogTimer(); }, 7500);

                // makes sure the even does not bubble up, nor is consumed by focused inputs
                document.body.focus();
                e.stopImmediatePropagation();

                if (pressedChar == MSRKeyboardSwipeParser.START_TRACK_2) {
                    this._track2Started = true;
                }

                return false;   // eat char
            }
            else {
                // nothing triggered the keystroke capture let the characters pass thru to the form field
                return true;    // pass thru char
            }
        }

        // helper functions for MSR swipe
        private ParseSwipe(): boolean {
            var swipeCopy = this._swipeBuffer;

            //sanity check
            if (swipeCopy == null || swipeCopy.length == 0)
                return true;

            var firstName, lastName, CCNumber;
            var expMonth, expYear;  //unsigned ints

            var reTrackDelimiters = /\?/g;
            var reTrack1FieldDelimiters = /\^|\}/g;
            var reTrack2FieldDelimiters = /\=/g;
            var reLastFirstNameDelimiter = /\//g;
            var reDigits = /^\d+$/;

            var reTrackDelimMatches = new Array();
            var reTrack1FieldDelimMatches = new Array();
            var reTrack2FieldDelimMatches = new Array();
            var reLastFirstNameDelimMatches = new Array();
            var match;

            //flatten the rawdata into one line for simpler parsing...
            //some cardreaders do not use the \r\n when delimiting the trackdata
            swipeCopy = swipeCopy.replace("\n", "").replace("\r", "");

            // get the first delimiter all chars before this represent track1
            // get em' all
            while ((match = reTrackDelimiters.exec(swipeCopy)) != null)
                reTrackDelimMatches.push(match);

            if (reTrackDelimMatches.length < 1) {
                this.WriteLog("ParseSwipe() : error processing track tokens");
                return false;
            }

            var currentTrackIndex = 0;

            if (reTrackDelimMatches.length > 1) {
                var track1EndIdx = reTrackDelimMatches[currentTrackIndex++].index;
                this._track1 = swipeCopy.substring(0, track1EndIdx + 1);

                // Extract name from Track 1
                while ((match = reTrack1FieldDelimiters.exec(this._track1)) != null)
                    reTrack1FieldDelimMatches.push(match);

                if (reTrack1FieldDelimMatches.length < 2) {
                    this.WriteLog("ParseSwipe() : incorrect number of Field delimeter tokens in track1");
                    return false;
                }

                var firstSep = reTrack1FieldDelimMatches[0].index;
                var secondSep = reTrack1FieldDelimMatches[1].index;
                var rawName = this._track1.substring(firstSep + 1, secondSep);

                while ((match = reLastFirstNameDelimiter.exec(rawName)) != null)
                    reLastFirstNameDelimMatches.push(match);

                if (reLastFirstNameDelimMatches.length < 1) {
                    var lastNameStartDelimeter = rawName.replace(/\s+$/, "").lastIndexOf(" ");

                    if (lastNameStartDelimeter == -1) {
                        lastName = rawName;
                        firstName = "";
                    }
                    else {
                        lastName = rawName.substring(lastNameStartDelimeter + 1);
                        firstName = rawName.substring(0, lastNameStartDelimeter);
                    }
                }
                else {
                    var rawNameDelim = reLastFirstNameDelimMatches[0].index;

                    // now parse out the first and last name
                    lastName = rawName.substring(0, rawNameDelim);
                    firstName = rawName.substring(rawNameDelim + 1);
                }
            }

            var track2EndIdx = reTrackDelimMatches[currentTrackIndex++].index;
            // Grab everything after the first trackdelimiter to the next trackdelimiter  this is track2 data
            this._track2 = swipeCopy.substring(track1EndIdx + 1, track2EndIdx + 1);

            while ((match = reTrack2FieldDelimiters.exec(this._track2)) != null)
                reTrack2FieldDelimMatches.push(match);

            if (reTrack2FieldDelimMatches.length != 1) {
                this.WriteLog("ParseSwipe() : incorrect number of Field delimeter tokens in track2");
                return false;
            }

            var firstSep = reTrack2FieldDelimMatches[0].index;

            CCNumber = this._track2.substring(1, firstSep);

            // now we parse out the expiration date  00..99 is valid so the convert should be a good
            // enough validation.  expiration year is located 2 characters after the firstSep.
            expYear = this._track2.substring(firstSep + 1, firstSep + 1 + 2);
            if (!reDigits.test(expYear)) {
                this.WriteLog("ParseSwipe() : error parsing expiration year; contains chars other than digits");
                return false;
            }

            //months are 2 digits as well and are located 3 characters after the secondSep or immediately after the year chars
            expMonth = this._track2.substring(firstSep + 3, firstSep + 3 + 2);
            if (!reDigits.test(expMonth)) {
                this.WriteLog("ParseSwipe() : error parsing expiration month; contains chars other than digits");
                return false;
            }

            var firstDigit = CCNumber.substring(0, 1);

            this._expiryMonth = parseInt(expMonth);
            this._expiryYear = parseInt(expYear.valueOf());
            this._cardNumber = CCNumber;

            return true;
        }

        private WriteLog(str) {
            if (this._logEnabled) {
                RetailLogger.peripheralsMSRKeyboardSwipeParserLog(str + " " + this._swipeBuffer);
            }
        }

        private KeystrokeLogTimer() {
            // notify the main execution that the KeystrokeLogTimer() has executed
            this._keystrokeLogTimerID = -1;

            // a valid log should not remain open for more than N milliseconds.
            this.WriteLog("INFO: KeystrokeLogTimer() => entering");

            if (this._captureKeys) {
                // we are going to clear the keyboard log resulting in a loss of all logged chars
                this._captureKeys = false;
                this._track2Started = false;

                this.WriteLog("ERROR: KeystrokeLogTimer() =>'" + this._swipeBuffer + "' discarded. keystroke stream timeout!");

                this._swipeBuffer = '';
            }
            else {
                this.WriteLog("INFO: KeystrokeLogTimer() => successfull read.");
            }
        }

        private IsCardReaderStream() {
            // notify the main execution that the IsCardReaderStream() has executed
            this._cardReaderStreamTimerID = -1;

            //must get at least 5 chars in 1/4 second to be considered a swipe
            //the IDTech reads 22 chars in 1/4 second.  human typing is max 3 chars in 1/4 second

            //sanity check
            var secondChar = this._swipeBuffer.substring(1, 2);

            if (this._swipeBuffer.length < 5 || secondChar == MSRKeyboardSwipeParser.START_TRACK) {
                //This is not a swipe.  The user manually entered a swipe key character
                //_captureKeys = false will tell the calling code to allow the characters to pass thru.
                this._captureKeys = false;
                this._track2Started = false;

                //NOTE: only pass the text back thru if the merchant selected the swipe button.   they may have
                //		swiped a card prior to selecting the "swipe" button...  we'll be eating this fast input
                if (this._isSwipeEnabled &&
                    (this._keyPressedSourceElement.tagName.toLowerCase() == "input" &&
                        (this._keyPressedSourceElement.type.toLowerCase() == "text" ||
                        this._keyPressedSourceElement.type.toLowerCase() == "password")
                    )
                ) {
                    //BUGBUG: if the calling element is selected we really should do an assignment here and not
                    //a cat.

                    //NOTE: all textfield attributes have the .maxLength property with a default value
                    // of 2^31 (or, 0 if it was set to an improper value) if the value was not programatically set... not sure we can trust this though
                    // so we check the attribute list first to see
                    var maxLength = this._keyPressedSourceElement.maxLength;

                    //check maxlength property of this field before appending the additional chars
                    if (maxLength > 0) {
                        //we have the maxLength attribute so we need to only allow at most the maximum number
                        // of chars specified in the maxLength attribute.
                        this._keyPressedSourceElement.value = (this._keyPressedSourceElement.value + this._swipeBuffer).substring(0, maxLength);
                    }
                    else {
                        // we do not have a maxLength restriction so just append the the data
                        this._keyPressedSourceElement.value += this._swipeBuffer;
                    }
                }
                else {
                    // clear this buffer.
                    this._eatFastKeyStrokes = true;
                    this._swipeBuffer = '';
                    this._captureKeys = false;
                    this._track2Started = false;
                    var self = this;
                    setTimeout(function () { self.EatFastKeyStrokes(); }, 250);
                }

                this.WriteLog("'" + this._swipeBuffer + "' is not valid!");
                this._swipeBuffer = '';
            }
        }

        private EatFastKeyStrokes() {
            //short circuit
            if (this._fastCharsEaten.length < 5) {
                // we must be at the end of the line
                this._eatFastKeyStrokes = false;
                this._fastCharsEaten = "";
                return;
            }

            this.WriteLog("in EatFastKeyStrokes(): ate '" + this._fastCharsEaten + "'");
            this._fastCharsEaten = "";

            // execution returns from this instance of EatFastKeyStrokes() but before we do that we schedule
            // another instance of EatFastKeyStrokes() to run again in 1/4 second.
            var self = this;
            setTimeout(function () { self.EatFastKeyStrokes(); }, 250);
        }

        private ClearIsCardReaderStreamTimer() {
            if (this._cardReaderStreamTimerID != -1) {
                clearInterval(this._cardReaderStreamTimerID);
                this.WriteLog("WARNING: cleared IsCardReaderStreamTimer() before it had a chance to run");
                this._cardReaderStreamTimerID = -1;
            }
        }

        private ClearKeystrokeLogTimer() {
            if (this._keystrokeLogTimerID != -1) {
                clearInterval(this._keystrokeLogTimerID);
                this.WriteLog("WARNING: cleared KeystrokeLogTimer() before it had a chance to run");
                this._keystrokeLogTimerID = -1;
            }
        }
    }
}
