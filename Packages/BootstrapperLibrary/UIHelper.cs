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
    namespace Retail.Deployment.SelfService.BootstrapperLibrary
    {
        using System;
        using Retail.Deployment.PackageSetup;
    
        /// <summary>
        /// Class for UI handlers.
        /// </summary>
        public static class UIHelper
        {
            private static MainView mainView;
    
            /// <summary>
            /// Shows the main UI view for the application.
            /// </summary>
            /// <param name="uiMode">Whether installer is running in UI or silent mode.</param>
            public static void ShowMainView(bool uiMode)
            {
                if (uiMode)
                {
                    if (mainView == null)
                    {
                        InitMainView();
                    }
    
                    mainView.Show();
                }
            }
    
            /// <summary>
            /// Hides the main UI view of the application.
            /// </summary>
            public static void HideMainView()
            {
                if (mainView != null)
                {
                    mainView.Hide();
                }
            }
    
            private static void InitMainView()
            {
                mainView = new MainView();
                mainView.InitializeComponent();
            }
        }
    }
}
