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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using System.Text;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class to get channel category attributes.
        /// </summary>
        internal sealed class GetChannelCategoryAttributesProcedure
        {
            private const string ChannelCategoryAttributeViewName = "CHANNELCATEGORYATTRIBUTEVIEW";
            private const string SqlParamChannelId = "@channelId";
            private readonly GetChannelCategoryAttributesDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetChannelCategoryAttributesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request message.</param>
            public GetChannelCategoryAttributesProcedure(GetChannelCategoryAttributesDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            /// <returns>The collection of channel category attributes.</returns>
            public PagedResult<ChannelCategoryAttribute> Execute()
            {
                PagedResult<ChannelCategoryAttribute> channelCategoryAttributes;
    
                var query = new SqlPagedQuery(this.request.QueryResultSettings)
                {
                    From = ChannelCategoryAttributeViewName,
                    Where = "HOSTCHANNEL = " + SqlParamChannelId
                };
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.request.RequestContext))
                using (RecordIdTableType categoryRecordIds = new RecordIdTableType(this.request.CategoryIds, "CATEGORY"))
                {
                    query.Parameters[SqlParamChannelId] = this.request.ChannelId;
                    query.Parameters["@TVP_RECIDTABLETYPE"] = categoryRecordIds;
    
                    channelCategoryAttributes = context.ReadEntity<ChannelCategoryAttribute>(query);
    
                    PopulateCategoryPath(context, channelCategoryAttributes.Results, this.request.ChannelId);
                }
    
                return channelCategoryAttributes;
            }
    
            private static void PopulateCategoryPath(SqliteDatabaseContext context, ReadOnlyCollection<ChannelCategoryAttribute> channelCategoryAttributes, long channelId)
            {
                // this query returns all parent category names ordered from root to node for each childCategoryId
                const string GetCategoryParentNameQuery = @"
                    SELECT
    	                RPERC.ORIGINID      AS CATEGORY,
    	                ERC.NAME            AS NAME
                    FROM [ax].RETAILPUBECORESCATEGORY RPERC
                    INNER JOIN [ax].ECORESCATEGORY ERC ON ERC.CATEGORYHIERARCHY = RPERC.CATEGORYHIERARCHY AND ERC.NESTEDSETLEFT <= RPERC.NESTEDSETLEFT AND ERC.NESTEDSETRIGHT >= RPERC.NESTEDSETRIGHT
                    WHERE
    	                RPERC.CHANNEL = @bi_ChannelId
    	                AND RPERC.ORIGINID IN (SELECT RECID FROM {0})
                    ORDER BY
    	                ERC.NESTEDSETLEFT   ASC";
    
                ReadOnlyCollection<CategoryName> categoryNames;
                IEnumerable<long> categoryIds = channelCategoryAttributes.Select(category => category.Category).Distinct();
    
                using (TempTable categoryIdsTempTable = TempTableHelper.CreateScalarTempTable<long>(context, "RECID", categoryIds))
                {
                    SqlQuery query = new SqlQuery(GetCategoryParentNameQuery, categoryIdsTempTable.TableName);
                    query.Parameters["@bi_ChannelId"] = channelId;
                    categoryNames = context.ReadEntity<CategoryName>(query).Results;
                }
    
                ILookup<long, ChannelCategoryAttribute> categoryAttributesByCategoryIds = channelCategoryAttributes.ToLookup(category => category.Category);
                StringBuilder builder = new StringBuilder();
    
                foreach (var group in categoryNames.GroupBy(categoryName => categoryName.Category))
                {
                    long categoryId = group.Key;
    
                    builder.Clear();
                    group.Aggregate(builder, AggregateCategoryName);
    
                    string fullCategoryPath = builder.ToString();
    
                    foreach (ChannelCategoryAttribute categoryAttributes in categoryAttributesByCategoryIds[categoryId])
                    {
                        categoryAttributes.CategoryPath = fullCategoryPath;
                    }
                }
            }
    
            private static StringBuilder AggregateCategoryName(StringBuilder builder, CategoryName categoryName)
            {
                const string CategoryPathSeparator = "/";
                return builder.Append(CategoryPathSeparator).Append(categoryName.Name);
            }
    
            [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "False positive. This class is used to determine the shape of the data set and does not need to be instantiated.")]
            [SuppressMessage("Maintainability", "DR1717:AssertTypesAreInExpectedNamespace", Justification = "Private class.")]
            private class CategoryName : CommerceEntity
            {
                private const string NameColumn = "NAME";
                private const string CategoryColumn = "CATEGORY";
    
                public CategoryName() : base("CategoryName")
                {
                }
    
                /// <summary>
                /// Gets the name of the category.
                /// </summary>
                [Column(NameColumn)]
                public string Name
                {
                    get { return (string)this[NameColumn]; }
                }
    
                /// <summary>
                /// Gets the record identifier for the category.
                /// </summary>
                [Column(CategoryColumn)]
                public long Category
                {
                    get { return (long)(this[CategoryColumn] ?? 0L); }
                }
            }
        }
    }
}
