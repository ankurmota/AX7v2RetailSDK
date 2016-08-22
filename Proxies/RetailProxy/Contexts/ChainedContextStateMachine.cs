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
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Encapsulates the state machine of chained context.
        /// </summary>
        internal class ChainedContextStateMachine
        {
            /// <summary>
            /// Actions to be handled specially.
            /// </summary>
            private const string ActivateDeviceActionName = "ActivateDevice";
            private const string DeactivateDeviceActionName = "DeactivateDevice";
    
            /// <summary>
            /// The defined actions which require special handling.
            /// </summary>
            private static readonly IDictionary<string, ActionExecutionPattern> DefinedActionExcutions =
                GetSpecialActionExectionPatterns();
    
            private readonly TimeSpan reconnectionInterval;
            private readonly Func<Type, Task<object>> getCachedEntityFuncAsync;
            private ChainedContextState contextState;
            private DateTime lastOnlineConnectionDateTime;
    
            /// <summary>
            /// The special flag indicating whether the current context is forced in offline mode.
            /// </summary>
            private bool forcedToOffline;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ChainedContextStateMachine"/> class.
            /// </summary>
            /// <param name="onlineContext">The online context.</param>
            /// <param name="offlineContext">The offline context.</param>
            /// <param name="getCachedEntityFuncAsync">The function delegate to get cached entities.</param>
            /// <param name="reconnectionInterval">The time interval for the next connection attempt.</param>
            public ChainedContextStateMachine(IContext onlineContext, IContext offlineContext, Func<Type, Task<object>> getCachedEntityFuncAsync, TimeSpan reconnectionInterval)
                : this(onlineContext, offlineContext, getCachedEntityFuncAsync, ChainedContextState.OnlineOnly, reconnectionInterval)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ChainedContextStateMachine"/> class.
            /// </summary>
            /// <param name="onlineContext">The online context.</param>
            /// <param name="offlineContext">The offline context.</param>
            /// <param name="getCachedEntityFuncAsync">The function delegate to get cached entities.</param>
            /// <param name="contextState">The initial context state.</param>
            /// <param name="reconnectionInterval">The time interval for the next connection attempt.</param>
            public ChainedContextStateMachine(IContext onlineContext, IContext offlineContext, Func<Type, Task<object>> getCachedEntityFuncAsync, ChainedContextState contextState, TimeSpan reconnectionInterval)
            {
                if (getCachedEntityFuncAsync == null)
                {
                    throw new ArgumentNullException("getCachedEntityFuncAsync");
                }
    
                if (onlineContext == null && (contextState != ChainedContextState.OfflineOnly))
                {
                    throw new InvalidOperationException("Online context can't be null when chained context is initialized in online mode.");
                }
    
                if (offlineContext == null && contextState == ChainedContextState.OfflineOnly)
                {
                    throw new InvalidOperationException("Offline context can't be null when chained context is initialized in offline mode.");
                }
    
                this.reconnectionInterval = reconnectionInterval;
                this.OnlineContext = onlineContext;
                this.OfflineContext = offlineContext;
                this.getCachedEntityFuncAsync = getCachedEntityFuncAsync;
                this.forcedToOffline = false;
    
                this.contextState = contextState;
                this.InitTransitionMap();
            }
    
            /// <summary>
            /// Gets or sets the online context for this ChainedContext.
            /// </summary>
            public IContext OnlineContext { get; set; }
    
            /// <summary>
            /// Gets or sets the offline context for this ChainedContext.
            /// </summary>
            public IContext OfflineContext { get; set; }
    
            /// <summary>
            /// Gets the current state.
            /// </summary>
            public ChainedContextState CurrentState
            {
                get { return this.contextState; }
            }
    
            /// <summary>
            /// Gets a value indicating whether the context is online or offline.
            /// </summary>
            public bool IsOffline
            {
                get { return this.CurrentState == ChainedContextState.OfflineOnly; }
            }
    
            /// <summary>
            /// Gets the last online connection UTC date/time.
            /// </summary>
            public DateTime LastOnlineConnectionUtcDateTime
            {
                get { return this.lastOnlineConnectionDateTime; }
            }
    
            /// <summary>
            /// Gets the collection of state transition objects.
            /// </summary>
            internal IList<StateTransition> StateTransitions { get; private set; }
    
            /// <summary>
            /// Gets or sets the locale for the contexts in current state machine instance.
            /// </summary>
            internal string Locale
            {
                get { return this.OnlineContext.Locale; }
                set { this.OnlineContext.Locale = this.OfflineContext.Locale = value; }
            }
    
            /// <summary>
            /// Switches to online mode asynchronously.
            /// </summary>
            /// <returns>No return.</returns>
            public async Task SwitchToOnlineAsync()
            {
                if (this.OnlineContext == null)
                {
                    throw new InvalidOperationException("Can't switch to online mode when online context is null.");
                }
    
                if (this.IsOffline)
                {
                    Cart cachedCart = await this.GetCachedCartAsync();
    
                    if (cachedCart != null)
                    {
                        throw new NotSupportedException("Can't switch to online mode when offline cart is unfinished.");
                    }
    
                    await this.MoveNextAsync(StateTransitionEvent.SwitchToOnlineManually);
                }
    
                this.forcedToOffline = false;
            }
    
            /// <summary>
            /// Switches to offline mode asynchronously.
            /// </summary>
            /// <returns>No return.</returns>
            public async Task SwitchToOfflineAsync()
            {
                if (this.OfflineContext == null)
                {
                    throw new InvalidOperationException("Can't switch to offline mode when offline context is null.");
                }
    
                if (!this.IsOffline)
                {
                    await this.MoveNextAsync(StateTransitionEvent.SwitchToOfflineManually);
                }
    
                this.forcedToOffline = true;
            }
    
            /// <summary>
            /// Moves to the next state asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="entitySet">The target entity set.</param>
            /// <param name="action">The action name.</param>
            /// <param name="method">The method to be executed.</param>
            /// <returns>The result object.</returns>
            public async Task<TResult> MoveNextAsync<TResult>(string entitySet, string action, Func<IContext, Task<TResult>> method)
            {
                if (this.OnlineContext != null && !this.IsOffline)
                {
                    this.lastOnlineConnectionDateTime = DateTime.UtcNow;
                    return await this.MoveNextAsync(entitySet, action, method: method, isRequestOffline: false);
                }
    
                if (this.OnlineContext != null &&
                    !this.forcedToOffline &&
                    this.GetDefinedActionExctionPattern(action) != ActionExecutionPattern.Offline &&
                    DateTime.UtcNow > this.lastOnlineConnectionDateTime.Add(this.reconnectionInterval))
                {
                    var cachedCart = await this.GetCachedCartAsync();
    
                    if (cachedCart == null)
                    {
                        this.lastOnlineConnectionDateTime = DateTime.UtcNow;
                        return await this.MoveNextAsync(entitySet, action, method: method, isRequestOffline: false);
                    }
                }
    
                return await this.MoveNextAsync(entitySet, action, method: method, isRequestOffline: true);
            }
    
            /// <summary>
            /// Checks if the current request should be sent in offline mode when exception happens.
            /// </summary>
            /// <param name="communicationException">The communication exception.</param>
            /// <returns>Whether the request should be tried offline.</returns>
            internal static bool ShouldTryOfflineOnError(CommunicationException communicationException)
            {
                return communicationException != null && (communicationException.ErrorResourceId == ClientCommunicationErrors.NotFound || communicationException.ErrorResourceId == ClientCommunicationErrors.RequestTimeout);
            }
    
            /// <summary>
            /// Switches seamlessly to offline mode.
            /// </summary>
            /// <returns>No return.</returns>
            internal async Task SeamlessSwitchToOfflineAsync()
            {
                if (this.OfflineContext == null)
                {
                    throw new InvalidOperationException("Cannot switch to offline mode when offline context is null.");
                }
    
                if (!this.IsOffline)
                {
                    await this.MoveNextAsync(StateTransitionEvent.SwitchToOfflineManually);
                }
            }
    
            /// <summary>
            /// Sets the operating unit number.
            /// </summary>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            internal void SetOperatingUnitNumber(string operatingUnitNumber)
            {
                this.OnlineContext.SetOperatingUnitNumber(operatingUnitNumber);
            }
    
            /// <summary>
            /// Gets the operating unit number.
            /// </summary>
            /// <returns>The context operating unit number.</returns>
            internal string GetOperatingUnitNumber()
            {
                return this.OnlineContext.GetOperatingUnitNumber();
            }
    
            /// <summary>
            /// Sets the device token.
            /// </summary>
            /// <param name="deviceToken">The device token.</param>
            internal void SetDeviceToken(string deviceToken)
            {
                this.OnlineContext.SetDeviceToken(deviceToken);
                this.OfflineContext.SetDeviceToken(deviceToken);
            }
    
            /// <summary>
            /// Gets the device token.
            /// </summary>
            /// <returns>The context device token.</returns>
            internal string GetDeviceToken()
            {
                return this.OnlineContext.GetDeviceToken();
            }
    
            /// <summary>
            /// Sets the user token.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            internal void SetUserToken(UserToken userToken)
            {
                this.OnlineContext.SetUserToken(userToken);

                if (userToken == null)
                {
                    this.OfflineContext.SetUserToken(userToken);
                }
                else
                {
                    CommerceUserToken commerceUserToken = userToken as CommerceUserToken;

                    if (commerceUserToken != null)
                    {
                        this.OfflineContext.SetUserToken(commerceUserToken.CommerceRuntimeToken);
                    }
                }
            }
    
            /// <summary>
            /// Gets the user token.
            /// </summary>
            /// <returns>The user token.</returns>
            internal UserToken GetUserToken()
            {
                UserToken onlineUserToken = this.OnlineContext.GetUserToken();
                CommerceRuntimeUserToken offlineToken = this.OfflineContext.GetUserToken() as CommerceRuntimeUserToken;

                if (onlineUserToken == null)
                {
                    return offlineToken;
                }
                else if (onlineUserToken is UserIdToken)
                {
                    CommerceUserToken commerceUserToken = new CommerceUserToken(onlineUserToken as UserIdToken);
                    commerceUserToken.CommerceRuntimeToken = offlineToken;
                    return commerceUserToken;
                }
                else
                {
                    return onlineUserToken;
                }
            }
    
            /// <summary>
            /// Gets the actions which need to be specially handled. By default, the action need to be executed in online mode first, if failed, switch to offline mode.
            /// </summary>
            /// <returns>The action execution pattern dictionary.</returns>
            private static IDictionary<string, ActionExecutionPattern> GetSpecialActionExectionPatterns()
            {
                var actionExectuionPatterns = new Dictionary<string, ActionExecutionPattern>();
    
                actionExectuionPatterns[CommerceAuthenticationRetailServerProvider.AcquireTokenActionName] = ActionExecutionPattern.OnlineAndOffline;
    
                actionExectuionPatterns[ActivateDeviceActionName] = ActionExecutionPattern.Online;
                actionExectuionPatterns[DeactivateDeviceActionName] = ActionExecutionPattern.Online;
    
                return actionExectuionPatterns;
            }
    
            /// <summary>
            /// Gets the cached device entity.
            /// </summary>
            /// <returns>The device object.</returns>
            private async Task<Device> GetCachedDeviceAsync()
            {
                return await this.getCachedEntityFuncAsync(typeof(Device)) as Device;
            }
    
            /// <summary>
            /// Gets the cached shift entity.
            /// </summary>
            /// <returns>The shift object.</returns>
            private async Task<Shift> GetCachedShiftAsync()
            {
                return await this.getCachedEntityFuncAsync(typeof(Shift)) as Shift;
            }
    
            /// <summary>
            /// Gets the cached cart entity.
            /// </summary>
            /// <returns>The cart object.</returns>
            private async Task<Cart> GetCachedCartAsync()
            {
                return await this.getCachedEntityFuncAsync(typeof(Cart)) as Cart;
            }
    
            /// <summary>
            /// Initializes the state transition map.
            /// </summary>
            private void InitTransitionMap()
            {
                this.StateTransitions = new List<StateTransition>();
    
                // Starting state = 'OnlineOnly'.
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.OnlineOnly,
                    ChainedContextState.Dual,
                    StateTransitionEvent.ExecuteOnlineAndOfflineConcurrently,
                    StateTransitionAction.ExecuteOnlineAndOffline));
    
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.OnlineOnly,
                    ChainedContextState.OnlineOnly,
                    StateTransitionEvent.ExecuteOnlineOnly | StateTransitionEvent.ExecuteOnlineFirstIfFailedSwitchToOffline,
                    StateTransitionAction.ExecuteOnlineOnly));
    
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.OnlineOnly,
                    ChainedContextState.OfflineOnly,
                    StateTransitionEvent.SwitchToOfflineManually | StateTransitionEvent.ExecuteOfflineOnly,
                    StateTransitionAction.SwitchToOffline));
    
                this.StateTransitions.Add(new StateTransition(
                   ChainedContextState.OnlineOnly,
                   ChainedContextState.OnlineOnly,
                   StateTransitionEvent.OfflineAuthenticationFailed,
                   StateTransitionAction.ChangeState));
    
                // Starting state = 'Dual'.
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.Dual,
                    ChainedContextState.Dual,
                    StateTransitionEvent.ExecuteOnlineAndOfflineConcurrently,
                    StateTransitionAction.ExecuteOnlineAndOffline));
    
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.Dual,
                    ChainedContextState.Dual,
                    StateTransitionEvent.ExecuteOnlineOnly,
                    StateTransitionAction.ExecuteOnlineOnly));
    
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.Dual,
                    ChainedContextState.Dual,
                    StateTransitionEvent.ExecuteOnlineFirstIfFailedSwitchToOffline,
                    StateTransitionAction.ExecuteOnlineFirstIfFailedSwitchToOffline));
    
                this.StateTransitions.Add(new StateTransition(
                   ChainedContextState.Dual,
                   ChainedContextState.OnlineOnly,
                   StateTransitionEvent.OfflineAuthenticationFailed,
                   StateTransitionAction.ChangeState));
    
                this.StateTransitions.Add(new StateTransition(
                   ChainedContextState.Dual,
                   ChainedContextState.OfflineOnly,
                   StateTransitionEvent.SwitchToOfflineManually | StateTransitionEvent.ExecuteOfflineOnly,
                   StateTransitionAction.SwitchToOffline));
    
                // Starting state = 'OfflineOnly'.
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.OfflineOnly,
                    ChainedContextState.OfflineOnly,
                    StateTransitionEvent.ExecuteOfflineOnly,
                    StateTransitionAction.ExecuteOfflineOnly));
    
                this.StateTransitions.Add(new StateTransition(
                    ChainedContextState.OfflineOnly,
                    ChainedContextState.Dual,
                    StateTransitionEvent.SwitchToOnlineManually | StateTransitionEvent.ExecuteOnlineOnly | StateTransitionEvent.ExecuteOnlineAndOfflineConcurrently | StateTransitionEvent.ExecuteOnlineFirstIfFailedSwitchToOffline,
                    StateTransitionAction.SwitchToOnline));
            }
    
            private ActionExecutionPattern GetDefinedActionExctionPattern(string action)
            {
                if (DefinedActionExcutions.ContainsKey(action))
                {
                    return DefinedActionExcutions[action];
                }
    
                return ActionExecutionPattern.SeamlessOnlineOffline;
            }
    
            /// <summary>
            /// Gets the transition event for a given request.
            /// </summary>
            /// <param name="action">The action name.</param>
            /// <param name="isOfflineRequest">The flag indicting whether the requests is sent to offline.</param>
            /// <returns>The matched state transition event.</returns>
            private async Task<StateTransitionEvent> GetEventForRequestAsync(string action, bool isOfflineRequest)
            {
                var actionExecutionPattern = this.GetDefinedActionExctionPattern(action);
    
                if (!isOfflineRequest)
                {
                    Device cachedDevice = await this.GetCachedDeviceAsync();
    
                    // Before device activation is done, all requests can only be executed in online mode.
                    if (cachedDevice == null)
                    {
                        switch (actionExecutionPattern)
                        {
                            case ActionExecutionPattern.OnlineAndOffline:
                            case ActionExecutionPattern.SeamlessOnlineOffline:
                            case ActionExecutionPattern.Online:
                                return StateTransitionEvent.ExecuteOnlineOnly;
                        }
                    }
    
                    if (this.CurrentState == ChainedContextState.OnlineOnly)
                    {
                        switch (actionExecutionPattern)
                        {
                            case ActionExecutionPattern.OnlineAndOffline:
                                return StateTransitionEvent.ExecuteOnlineAndOfflineConcurrently;
    
                            case ActionExecutionPattern.SeamlessOnlineOffline:
                            case ActionExecutionPattern.Online:
                                return StateTransitionEvent.ExecuteOnlineOnly;
                        }
                    }
    
                    switch (actionExecutionPattern)
                    {
                        case ActionExecutionPattern.OnlineAndOffline:
                            return StateTransitionEvent.ExecuteOnlineAndOfflineConcurrently;
    
                        case ActionExecutionPattern.SeamlessOnlineOffline:
                            return StateTransitionEvent.ExecuteOnlineFirstIfFailedSwitchToOffline;
    
                        case ActionExecutionPattern.Online:
                            return StateTransitionEvent.ExecuteOnlineOnly;
                    }
                }
    
                if (isOfflineRequest)
                {
                    switch (actionExecutionPattern)
                    {
                        case ActionExecutionPattern.OnlineAndOffline:
                        case ActionExecutionPattern.SeamlessOnlineOffline:
                        case ActionExecutionPattern.Offline:
                            return StateTransitionEvent.ExecuteOfflineOnly;
                    }
                }
    
                throw new NotSupportedException(string.Format("Invalid request. CurrentState: {0}, action: {1}, isofflinerequest: {2}.", this.CurrentState, action, isOfflineRequest));
            }
    
            /// <summary>
            /// Moves to the next state asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="entitySet">The target entity set.</param>
            /// <param name="action">The action name.</param>
            /// <param name="method">The method to be executed.</param>
            /// <param name="isRequestOffline">The flag indicating whether the current request is offline request.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> MoveNextAsync<TResult>(string entitySet, string action, Func<IContext, Task<TResult>> method, bool isRequestOffline)
            {
                StateTransitionEvent transitionEvent = await this.GetEventForRequestAsync(action, isRequestOffline);
    
                var transition = this.GetValidTransition(transitionEvent);
    
                switch (transition.Action)
                {
                    case StateTransitionAction.ExecuteOnlineFirstIfFailedSwitchToOffline:
                        return await ExecuteOnlineIfFailedTriggerOfflineRequestAsync(entitySet, action, method, transition.Stop);
    
                    case StateTransitionAction.ExecuteOnlineAndOffline:
                        return await ExecuteBothOnlineAndOfflineAsync(method, transition.Stop);
    
                    case StateTransitionAction.ExecuteOnlineOnly:
                        return await ExecuteOnlineAsync(method, transition.Stop);
    
                    case StateTransitionAction.ExecuteOfflineOnly:
                        return await ExecuteOfflineAsync(method, transition.Stop);
    
                    case StateTransitionAction.SwitchToOffline:
                        return await SwitchToOfflineAndExecuteAsync(action, method, transition.Stop);
    
                    case StateTransitionAction.SwitchToOnline:
                        return await SwitchToOnlineAndExecuteIfFailedTriggerOfflineRequestAsync(entitySet, action, method, transition.Stop);
    
                    case StateTransitionAction.ChangeState:
                        return await Task.Run<TResult>(() =>
                        {
                            SetState(transition.Stop);
                            return null;
                        });
    
                    default:
                        throw new NotSupportedException("Unsupported transition action.");
                }
            }
    
            /// <summary>
            /// Moves to the next state asynchronously.
            /// </summary>
            /// <param name="transitionEvent">The transition event.</param>
            /// <returns>No return.</returns>
            private async Task MoveNextAsync(StateTransitionEvent transitionEvent)
            {
                var transition = this.GetValidTransition(transitionEvent);
                switch (transition.Action)
                {
                    case StateTransitionAction.SwitchToOffline:
                        await this.SwitchToOfflineAsync(transition.Stop);
                        break;
    
                    case StateTransitionAction.SwitchToOnline:
                        await this.SwitchToOnlineAsync(transition.Stop);
                        break;
    
                    case StateTransitionAction.ChangeState:
                        this.SetState(transition.Stop);
                        break;
    
                    default:
                        throw new NotSupportedException("Unsupported transition action.");
                }
            }
    
            /// <summary>
            /// Gets a valid transition based on the event.
            /// </summary>
            /// <param name="transitionEvent">The transition event.</param>
            /// <returns>The transition object.</returns>
            private StateTransition GetValidTransition(StateTransitionEvent transitionEvent)
            {
                var transitions = this.StateTransitions.Where(t => t.Start == this.CurrentState && t.AllowedEvents.HasFlag(transitionEvent)).ToList();
    
                if (transitions == null || transitions.Count() != 1)
                {
                    throw new NotSupportedException(string.Format("The requested (current state = {0}, event = {1}) operation is not supported.", this.CurrentState, transitionEvent));
                }
    
                var transition = transitions[0];
                return transition;
            }
    
            /// <summary>
            /// Executes the method in online mode asynchronously. If failed, resubmit it as offline.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="entitySet">The target entity set.</param>
            /// <param name="action">The action name.</param>
            /// <param name="method">The method to be executed.</param>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> ExecuteOnlineIfFailedTriggerOfflineRequestAsync<TResult>(string entitySet, string action, Func<IContext, Task<TResult>> method, ChainedContextState targetState)
            {
                try
                {
                    var result = await method(this.OnlineContext);
                    this.SetState(targetState);
                    return result;
                }
                catch (CommunicationException ex)
                {
                    if (!this.ShouldTryOffline(ex))
                    {
                        throw;
                    }
                }
    
                return await this.MoveNextAsync(entitySet, action, method: method, isRequestOffline: true);
            }
    
            /// <summary>
            /// Executes the method in both online and offline mode asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="method">The method to be executed.</param>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> ExecuteBothOnlineAndOfflineAsync<TResult>(Func<IContext, Task<TResult>> method, ChainedContextState targetState)
            {
                var tasks = new Task<TResult>[2];
                Exception exceptionThrownInOnlineCall = null, exceptionThrownInOfflineCall = null;
    
                // Online task.
                tasks[0] = Task.Run(async () =>
                {
                    try
                    {
                        return await method(this.OnlineContext);
                    }
                    catch (Exception e)
                    {
                        exceptionThrownInOnlineCall = e;
                    }
    
                    return default(TResult);
                });
    
                // Offline task.
                tasks[1] = Task.Run(async () =>
                {
                    try
                    {
                        return await method(this.OfflineContext);
                    }
                    catch (Exception e)
                    {
                        exceptionThrownInOfflineCall = e;
                    }
    
                    return default(TResult);
                });
    
                await Task.WhenAll(tasks);
    
                if (exceptionThrownInOnlineCall != null)
                {
                    // Return offline result if online failed but offline succeeded.
                    if (exceptionThrownInOfflineCall == null)
                    {
                        await this.SwitchToOfflineAsync();
    
                        // Return offline result.
                        return await tasks[1];
                    }
                    else
                    {
                        // Throw online exception if both online and offline failed.
                        throw exceptionThrownInOnlineCall;
                    }
                }
    
                var onlineResult = await tasks[0];
                if (exceptionThrownInOfflineCall != null)
                {
                    // Handle the offline request failure silently.
                    await this.MoveNextAsync(StateTransitionEvent.OfflineAuthenticationFailed);
                    return onlineResult;
                }
    
                // Both online and offline succeed.
                this.SetState(targetState);
                return onlineResult;
            }
    
            /// <summary>
            /// Executes the method in online mode asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="method">The method to be executed.</param>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> ExecuteOnlineAsync<TResult>(Func<IContext, Task<TResult>> method, ChainedContextState targetState)
            {
                var result = await method(this.OnlineContext);
                this.SetState(targetState);
    
                return result;
            }
    
            /// <summary>
            /// Executes the method in offline mode asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="method">The method to be executed.</param>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> ExecuteOfflineAsync<TResult>(Func<IContext, Task<TResult>> method, ChainedContextState targetState)
            {
                var result = await method(this.OfflineContext);
                this.SetState(targetState);
    
                return result;
            }
    
            /// <summary>
            /// Switches to offline mode and executes the method asynchronously.
            /// </summary>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="action">The action name.</param>
            /// <param name="method">The method to be executed.</param>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> SwitchToOfflineAndExecuteAsync<TResult>(string action, Func<IContext, Task<TResult>> method, ChainedContextState targetState)
            {
                this.SetState(targetState);
    
                TResult result;
    
                // For logon request, transfer request can only be done after it succeeds.
                if (action.Equals(CommerceAuthenticationProvider.AcquireTokenActionName, StringComparison.OrdinalIgnoreCase))
                {
                    result = await method(this.OfflineContext);
                    await this.TransferShiftToOfflineAsync();
                    await this.TransferCartToOfflineAsync();
                }
                else
                {
                    await this.TransferShiftToOfflineAsync();
                    await this.TransferCartToOfflineAsync();
                    result = await method(this.OfflineContext);
                }
    
                return result;
            }
    
            /// <summary>
            /// Switches to offline mode asynchronously.
            /// </summary>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task SwitchToOfflineAsync(ChainedContextState targetState)
            {
                await this.TransferShiftToOfflineAsync();
                await this.TransferCartToOfflineAsync();
    
                // Change the transition state since the shift and cart have been transfered.
                this.SetState(targetState);
            }
    
            /// <summary>
            /// Switches to online mode and executes the method asynchronously. If failed, resubmit the request in offline mode.
            /// </summary>
            /// <typeparam name="TResult">The type of result object.</typeparam>
            /// <param name="entitySet">The target entity set.</param>
            /// <param name="action">The action name.</param>
            /// <param name="method">The method to be executed.</param>
            /// <param name="targetState">The target state.</param>
            /// <returns>The result object.</returns>
            private async Task<TResult> SwitchToOnlineAndExecuteIfFailedTriggerOfflineRequestAsync<TResult>(string entitySet, string action, Func<IContext, Task<TResult>> method, ChainedContextState targetState)
            {
                try
                {
                    TResult result;
    
                    // For logon request, transfer request can only be done after it succeeds.
                    if (action.Equals(CommerceAuthenticationProvider.AcquireTokenActionName, StringComparison.OrdinalIgnoreCase))
                    {
                        result = await method(this.OnlineContext);
                        await this.TransferShiftToOnlineAsync();
                    }
                    else
                    {
                        await this.TransferShiftToOnlineAsync();
                        result = await method(this.OnlineContext);
                    }
    
                    this.SetState(targetState);
    
                    // Delete the offline shift after transition is done.
                    await this.DeleteOfflineShiftAsync();
                    return result;
                }
                catch (CommunicationException ex)
                {
                    if (!this.ShouldTryOffline(ex))
                    {
                        throw;
                    }
                }
    
                return await this.MoveNextAsync(entitySet, action, method: method, isRequestOffline: true);
            }
    
            /// <summary>
            /// Switches to online mode asynchronously.
            /// </summary>
            /// <param name="targetState">The target state.</param>
            /// <returns>No return.</returns>
            private async Task SwitchToOnlineAsync(ChainedContextState targetState)
            {
                await this.TransferShiftToOnlineAsync();
    
                this.SetState(targetState);
    
                // Delete the offline shift after transition is done.
                await this.DeleteOfflineShiftAsync();
            }
    
            private async Task DeleteOfflineShiftAsync()
            {
                if (await this.GetCachedShiftAsync() != null)
                {
                    try
                    {
                        var shiftManager = ManagerFactory.Create(this.OfflineContext).GetManager<IShiftManager>();
                        await shiftManager.Delete(await this.GetCachedShiftAsync());
                    }
                    catch
                    {
                        // Do nothing if shift can't be deleted.
                    }
                }
            }
    
            private async Task TransferCartToOfflineAsync()
            {
                if (await this.GetCachedCartAsync() != null)
                {
                    var cartManager = ManagerFactory.Create(this.OfflineContext).GetManager<ICartManager>();
                    await cartManager.Create(await this.GetCachedCartAsync());
                }
            }
    
            private async Task TransferShiftToOnlineAsync()
            {
                Shift shift = await this.GetCachedShiftAsync();
                if (shift != null)
                {
                    var shiftManager = ManagerFactory.Create(this.OnlineContext).GetManager<IShiftManager>();
                    await shiftManager.Create(await this.GetCachedShiftAsync());
                }
            }
    
            private async Task TransferShiftToOfflineAsync()
            {
                if (await this.GetCachedShiftAsync() != null)
                {
                    var shiftManager = ManagerFactory.Create(this.OfflineContext).GetManager<IShiftManager>();
                    await shiftManager.Create(await this.GetCachedShiftAsync());
                }
            }
    
            private void SetState(ChainedContextState state)
            {
                this.contextState = state;
            }
    
            /// <summary>
            /// Checks if the current request should be sent in offline mode when exception happens.
            /// </summary>
            /// <param name="ex">The exception.</param>
            /// <returns>The flag indicating whether the current request need to be sent in offline mode.</returns>
            private bool ShouldTryOffline(CommunicationException ex)
            {
                return this.OfflineContext != null && ex != null && ShouldTryOfflineOnError(ex);
            }
        }
    }
}
