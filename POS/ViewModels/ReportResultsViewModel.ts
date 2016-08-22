/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    export class ReportResultsViewModel extends ViewModelBase {
        public ReportTitle: any;
        public ReportID: any;
        public ResultsCaption: any;
        public Charts: any;
        public Locale: any;
        public Output: any;
        public HasTotalRow: boolean;
        public HasDisclaimer: boolean;

        constructor(reportData: any) {
            super();
            this.ReportTitle = reportData.ReportTitle;
            this.ReportID = reportData.ReportID;
            this.ResultsCaption = reportData.ResultsCaption;
            this.Output = reportData.Output;
            this.Charts = reportData.Charts;
            this.HasTotalRow = reportData.HasTotalRow;
            this.HasDisclaimer = reportData.HasDisclaimer;
        }

        public ReturnCommercePropertyValue(value: any) {
            if (value.StringValue != null) {
                return value.StringValue;
            } else if (value.DecimalValue != null) {
                return value.DecimalValue;
            } else if (value.LongValue != null) {
                return value.LongValue;
            } else if (value.IntegerValue != null) {
                return value.IntegerValue;
            } else if (value.BooleanValue != null) {
                return value.BooleanValue;
            } else if (value.DateTimeOffsetValue != null) {
                return value.DateTimeOffsetValue;
            } else if (value.ByteValue != null) {
                return value.ByteValue;
            } else {
                return value.StringValue;
            }
        }
    }
}
