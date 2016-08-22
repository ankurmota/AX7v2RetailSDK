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
    namespace Commerce.Runtime.DataServices.Sqlite.DataServices.SalesTransaction
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for inserting sales transactions.
        /// </summary>
        internal sealed class InsertSalesTransactionTablesProcedure
        {
            private InsertSalesTransactionTablesDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="InsertSalesTransactionTablesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public InsertSalesTransactionTablesProcedure(InsertSalesTransactionTablesDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            public void Execute()
            {
                var allTables = new DataTable[]
                {
                    this.request.TransactionTable,
                    this.request.LinesTable,
                    this.request.TaxTable,
                    this.request.RewardPointTable,
                    this.request.ReasonCodeTable,
                    this.request.PropertiesTable,
                    this.request.PaymentTable,
                    this.request.MarkupTable,
                    this.request.InvoiceTable,
                    this.request.InvoiceTable,
                    this.request.IncomeExpenseTable,
                    this.request.DiscountTable,
                    this.request.CustomerOrderTable,
                    this.request.AttributeTable,
                    this.request.AffiliationsTable,
                    this.request.AddressTable,
                    this.request.CustomerAccountDepositTable
                };
    
                // change table name to match table name in the database
                this.request.TransactionTable.TableName                 = "ax.RETAILTRANSACTIONTABLE";
                this.request.PaymentTable.TableName                     = "ax.RETAILTRANSACTIONPAYMENTTRANS";
                this.request.LinesTable.TableName                       = "ax.RETAILTRANSACTIONSALESTRANS";
                this.request.IncomeExpenseTable.TableName               = "ax.RETAILTRANSACTIONINCOMEEXPENSETRANS";
                this.request.MarkupTable.TableName                      = "ax.RETAILTRANSACTIONMARKUPTRANS";
                this.request.TaxTable.TableName                         = "ax.RETAILTRANSACTIONTAXTRANS";
                this.request.AttributeTable.TableName                   = "ax.RETAILTRANSACTIONATTRIBUTETRANS";
                this.request.AddressTable.TableName                     = "ax.RETAILTRANSACTIONADDRESSTRANS";
                this.request.DiscountTable.TableName                    = "ax.RETAILTRANSACTIONDISCOUNTTRANS";
                this.request.ReasonCodeTable.TableName                  = "ax.RETAILTRANSACTIONINFOCODETRANS";
                this.request.PropertiesTable.TableName                  = "crt.RETAILTRANSACTIONPROPERTIES";
                this.request.AffiliationsTable.TableName                = "ax.RETAILTRANSACTIONAFFILIATIONTRANS";
                this.request.RewardPointTable.TableName                 = "ax.RETAILTRANSACTIONLOYALTYREWARDPOINTTRANS";
                this.request.CustomerOrderTable.TableName               = "crt.CUSTOMERORDERTRANSACTION";
                this.request.InvoiceTable.TableName                     = "ax.RETAILTRANSACTIONORDERINVOICETRANS";
                this.request.CustomerAccountDepositTable.TableName      = "ax.RETAILTRANSACTIONCUSTOMERACCOUNTDEPOSITTRANS";
    
                this.AdaptTransactionTable(this.request.TransactionTable);
                this.AdaptPaymentTable(this.request.PaymentTable);
                this.AdaptInvoiceTable(this.request.InvoiceTable);
    
                this.AddChannelToTable(this.request.PaymentTable);
                this.AddChannelToTable(this.request.TransactionTable);
                this.AddChannelToTable(this.request.LinesTable);
                this.AddChannelToTable(this.request.MarkupTable);
                this.AddChannelToTable(this.request.TaxTable);
                this.AddChannelToTable(this.request.AttributeTable);
                this.AddChannelToTable(this.request.AddressTable);
                this.AddChannelToTable(this.request.DiscountTable);
                this.AddChannelToTable(this.request.ReasonCodeTable);
                this.AddChannelToTable(this.request.PropertiesTable);
                this.AddChannelToTable(this.request.RewardPointTable);
                this.AddChannelToTable(this.request.AffiliationsTable);
                this.AddChannelToTable(this.request.CustomerOrderTable);
                this.AddChannelToTable(this.request.InvoiceTable);
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                using (var transaction = databaseContext.BeginTransaction())
                {
                    foreach (DataTable dataTable in allTables)
                    {
                        databaseContext.SaveTable(dataTable);
                    }
    
                    transaction.Commit();
                }
            }
    
            private void AdaptPaymentTable(DataTable table)
            {
                table.Columns.Add(RetailTransactionPaymentSchema.QuantityColumn, typeof(decimal));
    
                foreach (DataRow row in table.Rows)
                {
                    row[RetailTransactionPaymentSchema.ForeignCurrencyExchangeRateTableColumn] = (decimal)row[RetailTransactionPaymentSchema.ForeignCurrencyExchangeRateTableColumn] * 100M;
                    row[RetailTransactionPaymentSchema.CompanyCurrencyExchangeRateColumn] = (decimal)row[RetailTransactionPaymentSchema.CompanyCurrencyExchangeRateColumn] * 100M;
                    row[RetailTransactionPaymentSchema.QuantityColumn] = 1M;
                }
            }
    
            private void AdaptInvoiceTable(DataTable table)
            {
                table.Columns.Add("SALESORDERINVOICETYPE", typeof(int));
    
                foreach (DataRow row in table.Rows)
                {
                    row["SALESORDERINVOICETYPE"] = 1; // OrderInvoiceType::Invoice == 1
                }
            }
    
            private void AdaptTransactionTable(DataTable transactionTable)
            {
                transactionTable.Columns.Add("FISCALDOCUMENTID", typeof(string));
                transactionTable.Columns.Add("FISCALSERIALID", typeof(string));
    
                foreach (DataRow row in transactionTable.Rows)
                {
                    row["EXCHRATE"] = (decimal)row["EXCHRATE"] * 100M;
                    row["FISCALDOCUMENTID"] = string.Empty;
                    row["FISCALSERIALID"] = string.Empty;
                }
            }
    
            private void AddChannelToTable(DataTable table)
            {
                table.Columns.Add("CHANNEL", typeof(long));
    
                foreach (DataRow row in table.Rows)
                {
                    row["CHANNEL"] = this.request.RequestContext.GetPrincipal().ChannelId;
                }
            }
        }
    }
}
