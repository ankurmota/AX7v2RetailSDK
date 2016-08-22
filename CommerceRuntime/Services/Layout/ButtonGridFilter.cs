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
    namespace Commerce.Runtime.Services.Layout
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Class to filter button grid results based on a collection of initial button grid identifiers.
        /// </summary>
        internal class ButtonGridFilter
        {
            private const int ButtonGridActionSubMenu = 401;
            private IDictionary<string, ButtonGrid> buttonGridById;
            private IEnumerable<string> topLevelButtonGridIds;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ButtonGridFilter"/> class.
            /// </summary>
            /// <param name="topLevelButtonGridIds">The identifiers associated to the button grids defined as top level by the layout.</param>
            /// <param name="buttonGrids">All button grids available for search.</param>
            public ButtonGridFilter(IEnumerable<string> topLevelButtonGridIds, IEnumerable<ButtonGrid> buttonGrids)
            {
                this.buttonGridById = buttonGrids.ToDictionary(buttonGrid => buttonGrid.Id, StringComparer.OrdinalIgnoreCase);
                this.topLevelButtonGridIds = topLevelButtonGridIds;
            }
    
            /// <summary>
            /// Get button grids based on the filter criteria in this filter object.
            /// </summary>
            /// <returns>The button grids filtered by this object.</returns>
            public ReadOnlyCollection<ButtonGrid> GetButtonGrids()
            {
                /*
                 * Button grids form a (potentially cyclic) graph, in which, nodes are the button grids and edges reference other button grids
                 * that a user could navigate to from a specific button grid / node.
                 *
                 * The filter logic starts with the top level button grids defined by the layout (topLevelButtonGridIds) and drill down the graph performing a depth-first search.
                 */
                Queue<string> buttonGridIdProcessList = new Queue<string>(this.topLevelButtonGridIds);
                List<ButtonGrid> buttonGridResultList = new List<ButtonGrid>();
    
                while (buttonGridIdProcessList.Any())
                {
                    string buttonGridId = buttonGridIdProcessList.Dequeue();
    
                    ButtonGrid buttonGrid;
                    if (!this.buttonGridById.TryGetValue(buttonGridId, out buttonGrid))
                    {
                        RetailLogger.Instance.CrtServicesLayoutServiceButtonGridNotFound(buttonGridId);
                        continue;
                    }
    
                    // if button grid has already been processed, then we don't need to look at it again
                    // this is required to handle the potential cycles in the graph
                    if (buttonGrid.IsProcessed)
                    {
                        continue;
                    }

                    // mark button grid as processed
                    buttonGrid.IsProcessed = true;
    
                    // process button grid:
                    // Part 1: add it to the result collection
                    buttonGridResultList.Add(buttonGrid);
    
                    // Part 2: look at all the edges (button grid buttons) pointing to other button grids and add them to the processing queue, so we can process them latter
                    foreach (ButtonGridButton button in buttonGrid.Buttons)
                    {
                        if (button.Action == ButtonGridActionSubMenu && !string.IsNullOrWhiteSpace(button.ActionProperty))
                        {
                            string subMenuButtonGridId = button.ActionProperty;
                            buttonGridIdProcessList.Enqueue(subMenuButtonGridId);
                        }
                    }
                }
    
                return buttonGridResultList.AsReadOnly();
            }
        }
    }
}
