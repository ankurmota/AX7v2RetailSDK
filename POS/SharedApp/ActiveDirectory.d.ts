/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

declare module Microsoft.IdentityModel.Clients.ActiveDirectory {

    //The status of the authentication request.
    enum AuthenticationStatus {
        ClientError = -1,
        ServiceError = -2,
        Success = 0
    }

    interface IUserInfoClass {
        displayableId: string;
        familyName: string;
        givenName: string;
        identityProvider: string;
        passwordChangeUrl: Windows.Foundation.Uri;
        passwordExpiresOn: Windows.Foundation.IReference<Windows.Foundation.DateTime>;
        uniqueId: string;
    }

    //Contains information of a single user. This information is used for token cache lookup.
    class UserInfo implements IUserInfoClass {
        displayableId: string;
        familyName: string;
        givenName: string;
        identityProvider: string;
        passwordChangeUrl: Windows.Foundation.Uri;
        passwordExpiresOn: Windows.Foundation.IReference<Windows.Foundation.DateTime>;
        uniqueId: string;
    }

    //A class that represents the result of authentication with the target identity provider.
    class AuthenticationResult {
        public accessToken: string;
        public accessTokenType: string;
        error: string;
        errorDescription: string;
        expiresOn: Windows.Foundation.DateTime;
        idToken: string;
        isMultipleResourceRefreshToken: boolean;
        refreshToken: string;
        status: AuthenticationStatus;
        statusCode: number;
        tenantId: string;
        userInfo: UserInfo;
        public static deserialize(serializedObject: string): AuthenticationResult;
        public serialize(): string;
    }

    interface ITokenCacheItemClass {
        accessToken: string;
        authority: string;
        clientId: string;
        displayableId: string;
        expiresOn: Windows.Foundation.DateTime;
        familyName: string;
        givenName: string;
        identityProvider: string;
        idToken: string;
        isMultipleResourceRefreshToken: string;
        refreshToken: string;
        resource: string;
        tenantId: string;
        uniqueId: string;
    }


    class TokenCacheItem implements ITokenCacheItemClass {
        accessToken: string;
        authority: string;
        clientId: string;
        displayableId: string;
        expiresOn: Windows.Foundation.DateTime;
        familyName: string;
        givenName: string;
        identityProvider: string;
        idToken: string;
        isMultipleResourceRefreshToken: string;
        refreshToken: string;
        resource: string;
        tenantId: string;
        uniqueId: string;
    }

    interface ITokenCacheNotificationArgsClass {
        clientId: string;
        displayableId: string;
        resource: string;
        tokenCache: TokenCache;
        uniqueId: string;
    }

    class TokenCacheNotificationArgs implements ITokenCacheNotificationArgsClass {
        clientId: string;
        displayableId: string;
        resource: string;
        tokenCache: TokenCache;
        uniqueId: string;
        TokenCacheNotificationArgs();
    }

    interface TokenCacheNotification {
        (args: TokenCacheNotificationArgs): void;
    }

    interface ITokenCacheClass {
        afterAccess: TokenCacheNotification;
        beforeAccess: TokenCacheNotification;
        beforeWrite: TokenCacheNotification;
        count: number;
        hasStateChanged: boolean;
        clear(): void;
        deleteItem(item: TokenCacheItem): void;
        deserialize(state: any[]): void;
        readItems(): Windows.Foundation.Collections.IIterable<TokenCacheItem>;
        serialize(): any[];
    }


    class TokenCache implements ITokenCacheClass {
        afterAccess: TokenCacheNotification;
        beforeAccess: TokenCacheNotification;
        beforeWrite: TokenCacheNotification;
        count: number;
        defaultShared: TokenCache;
        hasStateChanged: boolean;
        TokenCache();
        clear(): void;
        deleteItem(item: TokenCacheItem): void;
        deserialize(state: any[]): void;
        readItems(): Windows.Foundation.Collections.IIterable<TokenCacheItem>;
        serialize(): any[];
    }

    interface IAuthenticationContextClass {
        authority: string;
        correlationId: string;
        tokenCache: TokenCache;
        useCorporateNetwork: boolean;
        validateAuthority: boolean;
		acquireTokenAsync(resource: string, clientId: string, redirectUri: Windows.Foundation.Uri, promptBehavior : PromptBehavior, userIdentifier : UserIdentifier, extraQueryParameters : string): Windows.Foundation.IAsyncOperation<AuthenticationResult>;
    }

    enum PromptBehavior {
        Auto,
        Always,
        Never,
        RefreshSession
    }

	enum UserIdentifierType {
		UniqueId,
		OptionalDisplayableId,
		RequiredDisplayableId
	}

	interface IUserIdentifier {
		id : string;
		userIdentifierType : UserIdentifierType;
	}

	class UserIdentifier implements IUserIdentifier {
		id : string;
		userIdentifierType : UserIdentifierType;
		static anyUser : UserIdentifier;
	}

    //The main class representing the token issuing authority for resources. 
    class AuthenticationContext implements IAuthenticationContextClass {
        authority: string;
        correlationId: string;
        tokenCache: TokenCache;
        useCorporateNetwork: boolean;
        validateAuthority: boolean;
        constructor();
        constructor(authority: string);
        constructor(authority: string, validateAuthority: boolean);
        constructor(string, boolean, TokenCache);
		acquireTokenAsync(resource: string, clientId: string, redirectUri: Windows.Foundation.Uri, promptBehavior : PromptBehavior, userIdentifier : UserIdentifier, extraQueryParameters : string): Windows.Foundation.IAsyncOperation<AuthenticationResult>;
    }
}
