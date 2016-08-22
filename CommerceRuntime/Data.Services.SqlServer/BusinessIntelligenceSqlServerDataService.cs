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
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Xml;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Business Intelligence SQL server data service class.
        /// </summary>
        public class BusinessIntelligenceSqlServerDataService : IRequestHandler
        {
            private const string ParamReportId = "nvc_ReportId";
            private const string ParamLocaleCode = "nvc_LocaleCode";
            private const string ParamChannelId = "bi_ChannelId";
            private const string ParamUserId = "nvc_UserId";
            private const string OutputTableName = "reportdata";
            private const string GetReportConfigSPName = "GETREPORTCONFIGURATION";
            private const string GetReportConfigByIdSPName = "GETREPORTCONFIGBYID";
            private const string GetListOfLocalizedStrings = "GETLOCALIZEDREPORTSTRINGS";
            private const string ColumnReportId = "REPORTID";
            private const string ColumnReportDefinitionXml = "REPORTDEFINITIONXML";
            private const string ColumnRolesAllowed = "ROLEALLOWED";
            private const string ColumnParameters = "PARAMETERS";
            private const string ColumnReportTitle = "REPORTTITLE";
            private const string ColumnCharts = "CHARTS";
            private const string XmlParameterLabel = "Label";
            private const string XmlParameterName = "Name";
            private const string XmlParameterType = "DataType";
            private const string XmlParameterDefaultValue = "DefaultValue";
            private const string XmlDatasourceType = "DataSourceType";
            private const string XmlParameter = "ReportParameter";
            private const string XmlCharts = "ReportCharts";
            private const string XmlXYChart = "ReportXYChart";
            private const string XmlCategories = "Categories";
            private const string XmlSeries = "Series";
            private const string XmlQuery = "Query";
            private const string XmlTitle = "Title";
            private const string XmlDataset = "DataSet";
            private const string XmlContainsTotalRow = "HasTotalRow";
            private const string XmlContainsDisclaimer = "HasDisclaimer";
            private const string XmlParameters = "ReportParameters";
            private const string XmlReport = "RetailReport";
            private const string XmlIsUserBasedReport = "IsUserBasedReport";
            private const string XmlTotalsRow = "TotalsRow";
            private const string XmlColumnNumber = "Column";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(GetReportConfigurationDataRequest),
                    typeof(GetOLTPReportDataRequest),
                    typeof(GetSupportedReportsDataRequest),
                };
                }
            }

            /// <summary>
            /// Gets or sets localized strings for the current locale.
            /// </summary>
            private DataTable LocalizedStrings { get; set; }

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

                if (requestType == typeof(GetReportConfigurationDataRequest))
                {
                    response = this.GetReportConfiguration((GetReportConfigurationDataRequest)request);
                }
                else if (requestType == typeof(GetOLTPReportDataRequest))
                {
                    response = this.GetOLTPReportData((GetOLTPReportDataRequest)request);
                }
                else if (requestType == typeof(GetSupportedReportsDataRequest))
                {
                    response = this.GetSupportedReportData((GetSupportedReportsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Gets localized report strings for this locale.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Data table containing all localized strings.</returns>
            internal DataTable GetLocalizedReportsStrings(RequestContext context, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");

                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                parameters[ParamLocaleCode] = context.LanguageId;

                DataSet reportOutput;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(context, settings))
                {
                    reportOutput = sqlServerDatabaseContext.ExecuteStoredProcedureDataSet(GetListOfLocalizedStrings, parameters);
                }

                if (reportOutput != null && reportOutput.Tables.Count > 0)
                {
                    return reportOutput.Tables[0];
                }

                return null;
            }

            /// <summary>
            /// Converts data table output to report collection.
            /// </summary>
            /// <param name="output">Data table output.</param>
            /// <returns>Report row collection.</returns>
            private Collection<ReportRow> ConvertDataTableToCollection(DataTable output)
            {
                Collection<ReportRow> rows = new Collection<ReportRow>();
                foreach (DataRow dr in output.Rows)
                {
                    var row = new ReportRow();
                    var rowData = new Collection<CommerceProperty>();
                    for (int i = 0; i < output.Columns.Count; i++)
                    {
                        object val = dr[i];

                        // If the current value is of type string, then get the localized string if possible
                        if (output.Columns[i].DataType.Name == "String")
                        {
                            val = this.GetLocalizedLabel((string)val);
                        }

                        rowData.Add(new CommerceProperty(output.Columns[i].ColumnName, val));
                    }

                    row.SetRowData(rowData);
                    rows.Add(row);
                }

                return rows;
            }

            /// <summary>
            /// Gets report configuration.
            /// </summary>
            /// <param name="request">The request with report id, locale and settings.</param>
            /// <returns><see cref="ReportConfiguration"/> object.</returns>
            private SingleEntityDataServiceResponse<ReportConfiguration> GetReportConfiguration(GetReportConfigurationDataRequest request)
            {
                // Get localized strings for the report.
                if (this.LocalizedStrings == null)
                {
                    this.LocalizedStrings = this.GetLocalizedReportsStrings(request.RequestContext, request.QueryResultSettings);
                }

                DataTable reportAttributesTable = this.GetReportsConfiguration(request.RequestContext, request.ReportId, request.QueryResultSettings);
                string query, reportTitle, parameters, type, charts;
                bool hasTotalRow = false;
                bool hasDisclaimer = false;
                bool isUserBasedReport = false;
                ReportConfiguration reportConfig = null;
                DataRow reportRow;
                List<string> rolesAllowed;
                if (reportAttributesTable.Rows.Count > 0)
                {
                    reportConfig = new ReportConfiguration();
                    reportRow = reportAttributesTable.Rows[0];
                    rolesAllowed = new List<string>();
                    reportConfig.ReportId = reportRow[ColumnReportId].ToString();
                    this.GetReportDetailsFromXml(
                        reportRow[ColumnReportDefinitionXml].ToString(),
                        out query,
                        out reportTitle,
                        out parameters,
                        out type,
                        out charts,
                        out hasTotalRow,
                        out hasDisclaimer,
                        out isUserBasedReport);
                    reportConfig.Query = query;
                    reportConfig.DataSourceType = type;
                    reportConfig.IsUserBasedReport = isUserBasedReport;
                    reportConfig.HasTotalRow = hasTotalRow;
                    reportConfig.HasDisclaimer = hasDisclaimer;
                    foreach (DataRow row in reportAttributesTable.Rows)
                    {
                        rolesAllowed.Add(row[ColumnRolesAllowed].ToString());
                    }

                    reportConfig.SetRolesAllowed(rolesAllowed.AsReadOnly());
                }

                return new SingleEntityDataServiceResponse<ReportConfiguration>(reportConfig);
            }

            /// <summary>
            /// Gets OLTP report data.
            /// </summary>
            /// <param name="request">The request with report configuration, locale and settings.</param>
            /// <returns><see cref="ReportDataSet"/> object.</returns>
            private SingleEntityDataServiceResponse<ReportDataSet> GetOLTPReportData(GetOLTPReportDataRequest request)
            {
                ThrowIf.Null(request.ReportConfiguration, "Parameters are empty.");

                ReportConfiguration config = request.ReportConfiguration;
                ParameterSet sqlParameters = new ParameterSet();

                // Add channel id and user id to report parameters.
                if (config.Parameters != null && config.Parameters.Any())
                {
                    foreach (CommerceProperty parameter in config.Parameters)
                    {
                        sqlParameters[parameter.Key] = parameter.Value.GetPropertyValue();
                    }
                }

                sqlParameters[ParamChannelId] = request.RequestContext.GetPrincipal().ChannelId;
                if (config.IsUserBasedReport)
                {
                    sqlParameters[ParamUserId] = request.RequestContext.GetPrincipal().UserId;
                }

                DataSet outputDataSet;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    outputDataSet = sqlServerDatabaseContext.ExecuteQueryDataSet(config.Query, sqlParameters);
                }

                ReportDataSet reportDataSet = null;

                if (outputDataSet != null && outputDataSet.Tables.Count == 1)
                {
                    // Get localized strings for the report.
                    if (this.LocalizedStrings == null)
                    {
                        this.LocalizedStrings = this.GetLocalizedReportsStrings(request.RequestContext, request.QueryResultSettings);
                    }

                    outputDataSet.Tables[0].TableName = OutputTableName;
                    if (outputDataSet.Tables[0].Columns.Count > 0)
                    {
                        foreach (DataColumn column in outputDataSet.Tables[0].Columns)
                        {
                            column.ColumnName = this.GetLocalizedLabel(column.ColumnName);
                        }
                    }

                    reportDataSet = new ReportDataSet
                    {
                        ReportId = config.ReportId,
                        Locale = request.RequestContext.LanguageId,
                        Parameters = config.Parameters,
                        HasTotalRow = config.HasTotalRow,
                        HasDisclaimer = config.HasDisclaimer,
                        Output = this.ConvertDataTableToCollection(outputDataSet.Tables[0])
                    };
                }

                return new SingleEntityDataServiceResponse<ReportDataSet>(reportDataSet);
            }

            /// <summary>
            /// Gets supported reports data.
            /// </summary>
            /// <param name="request">The request with locale and settings.</param>
            /// <returns><see cref="ReportDataSet"/> object.</returns>
            private SingleEntityDataServiceResponse<ReportDataSet> GetSupportedReportData(GetSupportedReportsDataRequest request)
            {
                DataTable reportResults = this.GetReportsConfiguration(request.RequestContext, null, request.QueryResultSettings);

                // Get localized strings for the report.
                if (this.LocalizedStrings == null)
                {
                    this.LocalizedStrings = this.GetLocalizedReportsStrings(request.RequestContext, request.QueryResultSettings);
                }

                // Localize results and convert them to report data set.
                ReportDataSet dataSet = this.CreateReportsListDataSetFromTable(reportResults, request.RequestContext.LanguageId);

                return new SingleEntityDataServiceResponse<ReportDataSet>(dataSet);
            }

            /// <summary>
            /// Creates report list from data table.
            /// </summary>
            /// <param name="reportListTable">Report list data table.</param>        
            /// <param name="locale">Current locale.</param>        
            /// <returns>Report list dataset.</returns>
            private ReportDataSet CreateReportsListDataSetFromTable(DataTable reportListTable, string locale)
            {
                ThrowIf.Null(reportListTable, "Data table output is empty.");

                string query, reportTitle, parameters, type, charts;
                bool hasTotalRow = false;
                bool hasDisclaimer = false;
                bool isUserBasedReport = false;
                using (DataTable listOfReports = new DataTable())
                {
                    listOfReports.Columns.Add(ColumnReportId, typeof(string));
                    listOfReports.Columns.Add(ColumnReportTitle, typeof(string));
                    listOfReports.Columns.Add(ColumnParameters, typeof(string));
                    listOfReports.Columns.Add(ColumnCharts, typeof(string));
                    foreach (DataRow dr in reportListTable.Rows)
                    {
                        DataRow newRow = listOfReports.NewRow();
                        newRow[ColumnReportId] = dr[ColumnReportId].ToString();
                        this.GetReportDetailsFromXml(
                            dr[ColumnReportDefinitionXml].ToString(),
                            out query,
                            out reportTitle,
                            out parameters,
                            out type,
                            out charts,
                            out hasTotalRow,
                            out hasDisclaimer,
                            out isUserBasedReport);
                        newRow[ColumnReportTitle] = reportTitle;
                        newRow[ColumnParameters] = parameters;
                        newRow[ColumnCharts] = charts;
                        listOfReports.Rows.Add(newRow);
                    }

                    ReportDataSet reportListSet = new ReportDataSet()
                    {
                        Output = this.ConvertDataTableToCollection(listOfReports),
                        Locale = locale
                    };

                    return reportListSet;
                }
            }

            /// <summary>
            /// Gets reports configuration. If report identifier is null or empty, it will return configuration of all reports.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="reportId">The report identifier.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Report configuration data table.</returns>
            private DataTable GetReportsConfiguration(RequestContext context, string reportId, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");

                string reportConfigSPName = GetReportConfigSPName;

                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                parameters[ParamUserId] = context.GetPrincipal().UserId;

                if (!string.IsNullOrEmpty(reportId))
                {
                    parameters[ParamReportId] = reportId;
                    reportConfigSPName = GetReportConfigByIdSPName;
                }

                DataSet outputDataSet;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(context, settings))
                {
                    outputDataSet = sqlServerDatabaseContext.ExecuteStoredProcedureDataSet(reportConfigSPName, parameters);
                }

                if (outputDataSet != null && outputDataSet.Tables.Count > 0)
                {
                    outputDataSet.Tables[0].TableName = OutputTableName;
                    return outputDataSet.Tables[0];
                }

                return null;
            }

            /// <summary>
            /// Gets report details from xml.
            /// </summary>
            /// <param name="reportXml">Report definition xml.</param>
            /// <param name="query">Query output.</param>
            /// <param name="reportTitle">Report title label.</param>
            /// <param name="parameters">Report parameters string.</param>
            /// <param name="type">Report type.</param>   
            /// <param name="charts">Report charts.</param> 
            /// <param name="hasTotalRow">Report contains total row attribute.</param> 
            /// <param name="hasDisclaimer">Report contains disclaimer attribute.</param> 
            /// <param name="isUserBasedReport">Report user based attribute.</param> 
            private void GetReportDetailsFromXml(
                string reportXml,
                out string query,
                out string reportTitle,
                out string parameters,
                out string type,
                out string charts,
                out bool hasTotalRow,
                out bool hasDisclaimer,
                out bool isUserBasedReport)
            {
                StringReader stringReader = null;
                string attribute = string.Empty, paramValue = string.Empty;
                parameters = string.Empty;
                query = string.Empty;
                reportTitle = string.Empty;
                charts = string.Empty;
                type = string.Empty;
                hasTotalRow = false;
                hasDisclaimer = false;
                isUserBasedReport = false;
                try
                {
                    stringReader = new StringReader(reportXml);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.XmlResolver = null;
                    using (XmlReader reader = XmlReader.Create(stringReader, settings))
                    {
                        stringReader = null;
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.Name)
                                {
                                    case XmlReport:
                                        break;
                                    case XmlTitle:
                                        reader.Read();
                                        reportTitle = this.GetLocalizedLabel(reader.Value);
                                        break;
                                    case XmlDatasourceType:
                                        reader.Read();
                                        type = reader.Value;
                                        break;
                                    case XmlQuery:
                                        while (reader.Read())
                                        {
                                            if (reader.NodeType == XmlNodeType.CDATA)
                                            {
                                                break;
                                            }
                                        }

                                        query = reader.Value;
                                        break;
                                    case XmlDataset:
                                        bool parsedHasTotalRow = bool.TryParse(reader[XmlContainsTotalRow], out hasTotalRow);
                                        hasTotalRow = parsedHasTotalRow;
                                        bool parsedHasDisclaimer = bool.TryParse(reader[XmlContainsDisclaimer], out hasDisclaimer);
                                        hasDisclaimer = parsedHasDisclaimer;
                                        break;
                                    case XmlIsUserBasedReport:
                                        reader.Read();
                                        bool parsed = bool.TryParse(reader.Value, out isUserBasedReport);
                                        if (!parsed)
                                        {
                                            isUserBasedReport = false;
                                        }

                                        break;
                                    case XmlParameters:
                                        break;
                                    case XmlParameter:
                                        parameters += "|";
                                        attribute = reader[XmlParameterName];
                                        paramValue = string.Empty;
                                        if (attribute != null)
                                        {
                                            paramValue = attribute.Trim();
                                        }

                                        parameters += string.Format(CultureInfo.InvariantCulture, "{0}={1};", XmlParameterName, paramValue);
                                        paramValue = string.Empty;
                                        attribute = reader[XmlParameterType];
                                        if (attribute != null)
                                        {
                                            paramValue = attribute.Trim();
                                        }

                                        parameters += string.Format(CultureInfo.InvariantCulture, "{0}={1};", XmlParameterType, paramValue);
                                        paramValue = string.Empty;
                                        attribute = reader[XmlParameterLabel];
                                        if (attribute != null)
                                        {
                                            paramValue = this.GetLocalizedLabel(attribute.Trim());
                                        }

                                        parameters += string.Format(CultureInfo.InvariantCulture, "{0}={1};", XmlParameterLabel, paramValue);
                                        paramValue = string.Empty;
                                        attribute = reader[XmlParameterDefaultValue];
                                        if (attribute != null)
                                        {
                                            paramValue = attribute.Trim();
                                        }

                                        parameters += string.Format(CultureInfo.InvariantCulture, "{0}={1};", XmlParameterDefaultValue, paramValue);
                                        break;
                                    case XmlCharts:
                                        break;
                                    case XmlXYChart:
                                        charts += "|";
                                        attribute = reader[XmlCategories];
                                        if (attribute != null)
                                        {
                                            charts += string.Format(CultureInfo.InvariantCulture, "{0}={1};", XmlCategories, this.GetLocalizedLabel(attribute.Trim()));
                                        }

                                        break;
                                    case XmlSeries:
                                        reader.Read();
                                        charts += string.Format(CultureInfo.InvariantCulture, "{0}={1};", XmlSeries, this.GetLocalizedLabel(reader.Value));
                                        break;
                                    default:
                                        throw new System.NotImplementedException(string.Format(CultureInfo.InvariantCulture, "This element {0} is not implemented in report definition xml.", reader.Name));
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (stringReader != null)
                    {
                        stringReader.Dispose();
                    }
                }
            }

            /// <summary>
            /// Gets localized label for the passed string.
            /// </summary>
            /// <param name="label">Label Id for the string.</param>
            /// <returns>Localized label.</returns>
            private string GetLocalizedLabel(string label)
            {
                if (this.LocalizedStrings == null)
                {
                    return label;
                }

                DataRow[] localizedLabels = this.LocalizedStrings.Rows.Where(row => ((string)row[0] ?? string.Empty).Equals(label, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (localizedLabels.Length == 1)
                {
                    return localizedLabels[0][1].ToString();
                }
                else
                {
                    // If not found return the current label itself.
                    return label;
                }
            }
        }
    }
}