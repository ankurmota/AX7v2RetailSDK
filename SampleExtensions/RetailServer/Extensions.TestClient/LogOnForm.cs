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
    namespace RetailServer.TestClient
    {
        using System;
        using System.Windows.Forms;
        using Commerce.RetailProxy;
        using Commerce.RetailProxy.Authentication;

        /// <summary>
        /// The LogOnForm.
        /// </summary>
        internal partial class LogOnForm : Form
        {
            private MainForm mainForm = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="LogOnForm"/> class.
            /// </summary>
            public LogOnForm()
            {
                this.InitializeComponent();
            }

            /// <summary>
            /// Gets or sets the manager factory for RetailServer.
            /// </summary>
            public ManagerFactory ManagerFactory { get; set; }

            /// <summary>
            /// Gets the user token of the worker/staff for RetailServer.
            /// </summary>
            public UserToken UserIdToken { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="LogOnForm"/> class.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The EventArgs.</param>
            private void Login_Load(object sender, EventArgs e)
            {
                Helpers.TryGetForm(out this.mainForm);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private async void btnLogon_Click(object sender, EventArgs e)
            {
                try
                {
                    IAuthenticationManager authenticationManager = ManagerFactory.GetManager<IAuthenticationManager>();
                    CommerceAuthenticationParameters commerceParameters = new CommerceAuthenticationParameters(MainForm.CommerceAuthenticationParametersGrantType, MainForm.CommerceAuthenticationParametersClientId);
                    this.UserIdToken = await authenticationManager.AcquireToken(this.textBoxUserName.Text, this.textBoxPassword.Text, commerceParameters);
                    this.mainForm.Log("Logon succeeded.");
                }
                catch (Exception ex)
                {
                    this.mainForm.Log(ex.ToString());
                }

                this.Close();
            }
        }
    }
}
