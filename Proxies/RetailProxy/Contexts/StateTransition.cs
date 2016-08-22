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
    namespace Commerce.RetailProxy
    {
        using Microsoft.Dynamics.Commerce.Runtime;

        internal class StateTransition
        {
            public StateTransition(ChainedContextState start, ChainedContextState stop, StateTransitionEvent allowedEvents, StateTransitionAction action)
            {
                this.Start = start;
                this.Stop = stop;
                this.AllowedEvents = allowedEvents;
                this.Action = action;
            }
    
            public ChainedContextState Start { get; private set; }
    
            public ChainedContextState Stop { get; private set; }
    
            public StateTransitionEvent AllowedEvents { get; private set; }
    
            public StateTransitionAction Action { get; private set; }
        }
    }
}
