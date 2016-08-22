Sample overview:
The sample shows these customization strategies for the CommerceRuntime: Extension property for service, entity, request, and response. Triggers, Notifications and Notification Handlers.

Setup:
add these to the Commerceruntime.config:
    <add source="type" value="Microsoft.Dynamics.Commerce.Runtime.Sample.ExtensionProperties.CustomNotificationHandler, Microsoft.Dynamics.Commerce.Runtime.Sample" />
    <add source="type" value="Microsoft.Dynamics.Commerce.Runtime.Sample.ExtensionProperties.ExtensionPropertiesService, Microsoft.Dynamics.Commerce.Runtime.Sample" />
    <add source="type" value="Microsoft.Dynamics.Commerce.Runtime.Sample.ExtensionProperties.ExtensionPropertiesTriggers, Microsoft.Dynamics.Commerce.Runtime.Sample" />


1. Use of extension property bag for request, response and entities
A simple service is added to demonstrate how extension properties can be used to add custom data and round-trip it non-persisted.

2. Use of a notification and notification handler
Learn how notifications can be used to signal some event to the system. The concept is very similar to .NET delegates.


