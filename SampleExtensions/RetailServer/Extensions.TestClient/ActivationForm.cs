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

        internal partial class ActivationForm : Form
        {
            private MainForm mainForm = null;
            private string retailServerUrl = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="ActivationForm"/> class.
            /// </summary>
            public ActivationForm()
            {
                this.InitializeComponent();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ActivationForm"/> class.
            /// </summary>
            /// <param name="retailServerUrl">The retail server url.</param>
            public ActivationForm(string retailServerUrl)
                : this()
            {
                this.retailServerUrl = retailServerUrl;
            }

            public DeviceActivationInformation AppInfo { get; private set; }

            private void ActivationForm_Load(object sender, EventArgs e)
            {
                Helpers.TryGetForm(out this.mainForm);
                this.textBoxRetailServerUrl.Text = this.retailServerUrl;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private async void btnActivate_Click(object sender, EventArgs e)
            {
                try
                {
                    string aadToken = AzureActiveDirectoryHelper.GetAADHeaderWithPrompt();

                    this.textBoxRetailServerUrl.Text = this.retailServerUrl;
                    RetailServerContext context = Helpers.CreateNewRetailServerContext(this.retailServerUrl);
                    ManagerFactory managerFactory = ManagerFactory.Create(context);

                    managerFactory.Context.SetUserToken(new AADToken(aadToken));
                    managerFactory.Context.SetDeviceToken(null);
                    DeviceActivationResult result = null;
                    IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                    result = await storeOperationsManager.ActivateDevice(this.textBoxDeviceId.Text, this.textBoxRegisterId.Text, "testDevice.DeviceId", forceActivate: true, deviceType: 2 /*testDevice.DeviceType*/);
                    this.AppInfo = new DeviceActivationInformation(this.retailServerUrl, result.Device.TerminalId, result.Device.ChannelName, result.Device.Token, result.Device.DeviceNumber, DateTime.Now);
                    this.mainForm.Log("Activation succeeded.");
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