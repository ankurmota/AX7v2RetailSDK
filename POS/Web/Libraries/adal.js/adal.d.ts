/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

interface AuthenticationContextOptions {
    clientId: string;        
    redirectUri: string;    
    instance: string;
    tenant?: string;
    cacheLocation?: string;
    loginResource?: string;
    postLogoutRedirectUri?: string;
    endpoints?: Object;    
}

declare class AuthenticationContext {
    constructor(options: AuthenticationContextOptions);
    acquireToken(resource: string, callback: (error: string, token: string) => void): void;
    handleWindowCallback(): void;
    getUser(callback: (error: string, user: any) => void);
    getCachedToken(resource: string): string;
    clearCache(): void;
    login(): void;
    logOut(): void;
    promptUser(navigationUrl: string): void;
}