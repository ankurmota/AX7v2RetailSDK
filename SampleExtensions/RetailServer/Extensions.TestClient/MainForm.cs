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
        using System.Collections.Generic;
        using System.Configuration;
        using System.Linq;
        using System.Text;
        using System.Windows.Forms;
        using Commerce.RetailProxy;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Configuration;
        using RetailServer.TestClient;

        /// <summary>
        /// The MainForm.
        /// </summary>
        internal partial class MainForm : Form
        {
            /// <summary>
            /// The special string that designates a authentication parameter for password authentication.
            /// </summary>
            internal const string CommerceAuthenticationParametersGrantType = "password";

            /// <summary>
            /// The special string that designates a authentication parameter for Modern Pos client.
            /// </summary>
            internal const string CommerceAuthenticationParametersClientId = "Modern POS";

            /// <summary>
            /// File name of app storage xml file.
            /// </summary>
            internal const string AppStorageXmlFilePath = "TestClientStorage.xml";
            private const string DefaultCustomerAccountNumber = "100001";

            private ApplicationStorage appStorage = null;
            private DeviceActivationInformation currentDeviceActivationInfo = null;
            private ManagerFactory currentManagerFactory = null;
            private UserToken currentUserToken = null;
            private StringBuilder stringBuilderDebug = new StringBuilder();
            private Random random = new Random();

            /// <summary>
            /// Initializes a new instance of the <see cref="MainForm"/> class.
            /// </summary>
            public MainForm()
            {
                this.InitializeComponent();
            }

            /// <summary>
            /// Gets a value indicating whether a device is activated.
            /// </summary>
            public bool IsActivated
            {
                get
                {
                    return this.currentManagerFactory != null && !string.IsNullOrEmpty(this.currentManagerFactory.Context.GetDeviceToken());
                }
            }

            /// <summary>
            /// Gets a value indicating whether a user is currently logged on.
            /// </summary>
            public bool IsLoggedOn
            {
                get
                {
                    return this.IsActivated && this.currentManagerFactory.Context.GetUserToken() != null;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the device context should be in API calls to RetailServer.
            /// </summary>
            public bool ShouldUseDeviceContext
            {
                get
                {
                    return this.chkCallWithDeviceToken.Checked;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the user context should be in API calls to RetailServer.
            /// </summary>
            public bool ShouldUseUserContext
            {
                get
                {
                    return this.chkCallWithUserToken.Checked;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the Operating unit number should be in API calls to RetailServer.
            /// </summary>
            public bool ShouldUseOperatingUnitNumber
            {
                get
                {
                    return this.chkCallWithOperatingUnitNumber.Checked;
                }
            }

            /// <summary>
            /// Gets a value indicating whether to use Online mode (call RetailServer).
            /// </summary>
            public bool ShouldUseOnlineMode
            {
                get
                {
                    return this.chkUseOnlineMode.Checked;
                }
            }

            /// <summary>
            /// Gets a value with the operating unit number for the API calls to RetailServer.
            /// </summary>
            public string OperatingUnitNumber
            {
                get
                {
                    return this.txtOperatingUnitNumber.Text;
                }
            }

            internal static void RefreshLogForm(string log, bool openForm)
            {
                DebugForm debugForm = null;
                if (Helpers.TryGetForm(out debugForm))
                {
                    debugForm.TextBoxDebug.Text = log;
                    debugForm.TextBoxDebug.SelectionStart = debugForm.TextBoxDebug.Text.Length;
                    debugForm.TextBoxDebug.ScrollToCaret();
                    debugForm.TextBoxDebug.Refresh();
                    debugForm.Activate();
                }
                else if (openForm)
                {
                    debugForm = new DebugForm();
                    debugForm.TextBoxDebug.Text = log;
                    debugForm.TextBoxDebug.SelectionStart = debugForm.TextBoxDebug.Text.Length;
                    debugForm.TextBoxDebug.ScrollToCaret();
                    debugForm.TextBoxDebug.Refresh();
                    debugForm.Show();
                }
            }

            internal void Log(string s)
            {
                this.stringBuilderDebug.AppendLine(DateTime.Now.ToString());
                this.stringBuilderDebug.Append(s).AppendLine();

                MainForm.RefreshLogForm(this.stringBuilderDebug.ToString(), false);
            }

            internal void Log(string format, params object[] args)
            {
                this.stringBuilderDebug.AppendLine(DateTime.Now.ToString());
                this.stringBuilderDebug.AppendFormat(format, args).AppendLine();

                MainForm.RefreshLogForm(this.stringBuilderDebug.ToString(), false);
            }

            private void MainForm_Load(object sender, EventArgs e)
            {
                this.appStorage = ApplicationStorage.Initialize();

                this.chkCallWithDeviceToken.Checked = true;
                this.chkCallWithUserToken.Checked = true;
                this.chkCallWithOperatingUnitNumber.Checked = false;
                this.chkUseOnlineMode.Checked = true;
                this.RefreshActivationStorageInformation();
                this.RefreshLogOnInformation();
                this.RefreshUI();
            }

            private void RefreshActivationStorageInformation()
            {
                this.comboRetailServerUrl.Items.Clear();
                foreach (DeviceActivationInformation x in this.appStorage.ActivationInformation)
                {
                    this.comboRetailServerUrl.Items.Add(x);
                }

                if (this.currentDeviceActivationInfo != null)
                {
                    this.comboRetailServerUrl.SelectedItem = this.currentDeviceActivationInfo;
                    this.txtNewRetailServerUrl.Text = this.currentDeviceActivationInfo.RetailServerUrl;
                }
                else if (this.appStorage.ActivationInformation.Length > 0)
                {
                    this.comboRetailServerUrl.SelectedIndex = 0;
                }

                if (this.appStorage.ActivationInformation.Length == 0)
                {
                    this.btnSelectActivated.Enabled = false;
                }

                this.listViewActivationDetails.Items.Clear();
                string retailServerUrl = null;
                string deviceToken = null;
                if (this.currentDeviceActivationInfo != null)
                {
                    foreach (string line in this.currentDeviceActivationInfo.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        this.listViewActivationDetails.Items.Add(line);
                    }

                    retailServerUrl = this.currentDeviceActivationInfo.RetailServerUrl;
                    deviceToken = this.currentDeviceActivationInfo.DeviceToken;
                }
                else
                {
                    retailServerUrl = this.txtNewRetailServerUrl.Text;
                }

                IContext context = null;

                if (this.ShouldUseOnlineMode)
                {
                    if (!string.IsNullOrEmpty(retailServerUrl))
                    {
                        context = Helpers.CreateNewRetailServerContext(retailServerUrl);
                        this.currentManagerFactory = ManagerFactory.Create(context);

                        if (!string.IsNullOrEmpty(deviceToken))
                        {
                            this.currentManagerFactory.Context.SetDeviceToken(deviceToken);
                        }
                    }
                }
                else
                {
                    string offlineConnectionString = ConfigurationManager.ConnectionStrings["OfflineDatabase"].ConnectionString;
                    ICommerceRuntimeSection commerceRuntimeSection = (ICommerceRuntimeSection)ConfigurationManager.GetSection("commerceRuntime");
                    var commerceRuntimeConfiguration = new CommerceRuntimeConfiguration(
                        commerceRuntimeSection,
                        offlineConnectionString,
                        storageLookupConnectionStrings: null,
                        isOverride: true,
                        isMasterDatabaseConnectionString: false);

                    CommerceAuthenticationRuntimeProvider authenticationProvider = new CommerceAuthenticationRuntimeProvider();

                    context = new CommerceRuntimeContext(_ => commerceRuntimeConfiguration, authenticationProvider);
                    this.currentManagerFactory = ManagerFactory.Create(context);
                    context.SetDeviceToken(deviceToken);
                }
            }

            private void RefreshLogOnInformation()
            {
                if (this.IsActivated && this.currentManagerFactory.Context.GetUserToken() != this.currentUserToken)
                {
                    this.currentManagerFactory.Context.SetUserToken(this.currentUserToken);
                }
            }

            private void DisableUI()
            {
                this.btnActivateNew.Enabled = false;
                this.btnDeactivate.Enabled = false;
                this.btnLogin.Enabled = false;
                this.btnLogOff.Enabled = false;
            }

            private async void RefreshUI()
            {
                this.btnActivateNew.Enabled = true; // want to be able to force activate it...
                this.btnDeactivate.Enabled = this.IsActivated;
                this.btnLogin.Enabled = this.IsActivated && !this.IsLoggedOn;
                this.btnLogOff.Enabled = this.IsActivated && this.IsLoggedOn;
                this.chkCallWithDeviceToken.Enabled = this.IsActivated;
                this.chkCallWithUserToken.Enabled = this.IsLoggedOn;

                this.listViewLogonDetails.Items.Clear();
                if (this.IsLoggedOn)
                {
                    IEmployeeManager x = this.currentManagerFactory.GetManager<IEmployeeManager>();
                    Employee employee = await x.GetCurrentEmployee();
                    this.listViewLogonDetails.Items.Add(string.Format("Logged on as {0}, {1}", employee.StaffId, employee.Name));
                }
                else
                {
                    this.listViewLogonDetails.Items.Add("Not logged on");
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnActivate_Click(object sender, EventArgs e)
            {
                this.DisableUI();
                try
                {
                    using (ActivationForm f = new ActivationForm(this.txtNewRetailServerUrl.Text))
                    {
                        f.ShowDialog();
                        if (f.AppInfo != null)
                        {
                            this.currentDeviceActivationInfo = this.appStorage.Add(f.AppInfo);
                        }
                    }
                }
                finally
                {
                    this.RefreshActivationStorageInformation();
                    this.RefreshLogOnInformation();
                    this.RefreshUI();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnLogin_Click(object sender, EventArgs e)
            {
                this.DisableUI();
                try
                {
                    using (LogOnForm f = new LogOnForm())
                    {
                        f.ManagerFactory = this.currentManagerFactory;
                        f.ShowDialog();
                        this.currentUserToken = f.UserIdToken;
                    }
                }
                finally
                {
                    this.RefreshActivationStorageInformation();
                    this.RefreshLogOnInformation();
                    this.RefreshUI();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnLogOff_Click(object sender, EventArgs e)
            {
                this.DisableUI();
                try
                {
                    this.currentUserToken = null;
                    this.Log("LogOff succeeded.");
                    this.RefreshLogOnInformation();
                }
                finally
                {
                    this.RefreshUI();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private async void btnDeactivate_Click(object sender, EventArgs e)
            {
                if (!this.IsLoggedOn)
                {
                    MessageBox.Show("You have to be logged on in order to deactivate the device.");
                    return;
                }

                this.DisableUI();
                try
                {
                    IStoreOperationsManager storeOperationsManager = this.currentManagerFactory.GetManager<IStoreOperationsManager>();
                    await storeOperationsManager.DeactivateDevice("testDevice.DeviceId");
                    this.appStorage.Remove(this.currentDeviceActivationInfo);
                    this.currentDeviceActivationInfo = null;
                    this.Log("Deactivation succeeded.");
                }
                catch (Exception ex)
                {
                    this.Log(ex.ToString());
                }
                finally
                {
                    this.RefreshActivationStorageInformation();
                    this.RefreshLogOnInformation();
                    this.RefreshUI();
                    this.btnLogOff_Click(sender, e);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnShowDebugWindow_Click(object sender, EventArgs e)
            {
                RefreshLogForm(this.stringBuilderDebug.ToString(), true);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnClearDebug_Click(object sender, EventArgs e)
            {
                this.stringBuilderDebug.Clear();
                RefreshLogForm(string.Empty, false);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void comboRetailServerUrl_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (this.comboRetailServerUrl.SelectedItem != null)
                {
                    this.btnSelectActivated.Enabled = true;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnSelectActivated_Click(object sender, EventArgs e)
            {
                if (this.comboRetailServerUrl.SelectedItem != null)
                {
                    this.btnLogOff_Click(sender, e);
                    this.currentDeviceActivationInfo = (DeviceActivationInformation)this.comboRetailServerUrl.SelectedItem;
                    this.RefreshActivationStorageInformation();
                    this.RefreshLogOnInformation();
                    this.RefreshUI();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnDefaultTests_Click(object sender, EventArgs e)
            {
                using (RetailServerCallingContext apiContext = RetailServerCallingContext.TransitionFromUI(this))
                {
                    this.RunDefaultTests();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnSdkSampleTests_Click(object sender, EventArgs e)
            {
                using (RetailServerCallingContext apiContext = RetailServerCallingContext.TransitionFromUI(this))
                {
                    this.RunSdkSampleTests();
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnExtensionTests_Click(object sender, EventArgs e)
            {
                using (RetailServerCallingContext apiContext = RetailServerCallingContext.TransitionFromUI(this))
                {
                    this.RunExtensionTests();
                }
            }
            
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void btnTestOfflineAdapters_Click(object sender, EventArgs e)
            {
                this.Log(Helpers.TestRetailProxyIsFullyImplemented());
            }

            private async void RunDefaultTests()
            {
                try
                {
                    QueryResultSettings queryResultSettings = new QueryResultSettings();
                    PagingInfo pagingInfo = new PagingInfo();
                    pagingInfo.Top = 10;
                    pagingInfo.Skip = 0;
                    queryResultSettings.Paging = pagingInfo;

                    // query for product availability
                    IProductManager productManager = this.currentManagerFactory.GetManager<IProductManager>();
                    var result = await productManager.GetProductAvailabilities(new long[] { 22565421963 }, 5637144592, queryResultSettings);
                    IEnumerable<ProductAvailableQuantity> response = result.Results;
                    this.Log("Found {0} product availabilities.", response.Count());

                    // read a customer
                    ICustomerManager customerManager = this.currentManagerFactory.GetManager<ICustomerManager>();
                    Customer customer = await customerManager.Read(DefaultCustomerAccountNumber);
                    this.Log("Fetched customer with name '{0}'.", customer.Name);

                    // create cart with known customer and checkout
                }
                catch (Exception ex)
                {
                    this.Log(ex.ToString());
                }
            }

            private /* async */ void RunSdkSampleTests()
            {
                try
                {
                    /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
                    QueryResultSettings queryResultSettings = new QueryResultSettings();
                    PagingInfo pagingInfo = new PagingInfo();
                    pagingInfo.Top = 10;
                    pagingInfo.Skip = 0;
                    queryResultSettings.Paging = pagingInfo;

                    // query for customers 
                    ICustomerManager customerManager = this.currentManagerFactory.GetManager<ICustomerManager>();
                    PagedResult<Customer> customers = await customerManager.ReadAll(queryResultSettings);
                    this.Log("Found {0} customers.", customers.Count());

                    foreach (Customer c in customers)
                    {
                        CommerceProperty emailOptInProperty = new CommerceProperty();
                        emailOptInProperty.Key = "EMAILOPTIN";
                        emailOptInProperty.Value = new CommercePropertyValue();
                        int emailOptInPropertyValue = this.random.Next(0, 2); 
                        emailOptInProperty.Value.IntegerValue = emailOptInPropertyValue;
                        c.ExtensionProperties.Add(emailOptInProperty);

                        // this call goes from RS/CRT all the way to AX via RTS. If AX was customized correctly, the extension property will be processed and saved.
                        Customer updatedCustomer = await customerManager.Update(c);
                        this.Log("Customer '{0}' updated with ExtensionProperty EMAILOPTIN={1}.", c.AccountNumber, emailOptInPropertyValue);
                    }
                    // END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

                    /* BEGIN SDKSAMPLE_CROSSLOYALTY (do not remove this)
                    // the following code can only be built after the CRT and RetailServer sample customizations have been carried out. 
                    // See the instructions for details.
                    ICustomerManager customerManager = this.currentManagerFactory.GetManager<ICustomerManager>();
                    decimal discount = await customerManager.GetCrossLoyaltyCardDiscountAction("425-999-4444");
                    this.Log("GetCrossLoyaltyCardDiscountAction returned discount of {0}.", discount);
                    // END SDKSAMPLE_CROSSLOYALTY (do not remove this) */

                    /* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
                    QueryResultSettings queryResultSettings = new QueryResultSettings();
                    PagingInfo pagingInfo = new PagingInfo();
                    pagingInfo.Top = 10;
                    pagingInfo.Skip = 0;
                    queryResultSettings.Paging = pagingInfo;

                    IStoreDayHoursManager storeDayHoursManager = this.currentManagerFactory.GetManager<IStoreDayHoursManager>();
                    var result = await storeDayHoursManager.GetStoreDaysByStore("HOUSTON", queryResultSettings);
                    IEnumerable<StoreDayHours> storeDayHours = result.Results;
                    this.Log("GetStoreDaysByStore returned {0} StoreDayHours.", storeDayHours.Count());
                    // END SDKSAMPLE_STOREHOURS (do not remove this) */
                }
                catch (Exception ex)
                {
                    this.Log(ex.ToString());
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code.")]
            private void RunExtensionTests()
            {
                try
                {
                    MessageBox.Show("No extension tests available. This is the place where you can add your code...");

                    // add code here...
                }
                catch (Exception ex)
                {
                    this.Log(ex.ToString());
                }
            }

            private void CallWithToken_Changed(object sender, EventArgs e)
            {
                if (sender == this.chkCallWithDeviceToken && this.chkCallWithDeviceToken.Checked)
                {
                    this.chkCallWithOperatingUnitNumber.Checked = false;
                }

                if (sender == this.chkCallWithOperatingUnitNumber && this.chkCallWithOperatingUnitNumber.Checked)
                {
                    this.chkCallWithDeviceToken.Checked = false;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Auto generated name")]
            private void chkUseOnlineMode_CheckedChanged(object sender, EventArgs e)
            {
                this.btnLogOff_Click(sender, e);
                this.RefreshActivationStorageInformation();
                this.RefreshLogOnInformation();
                this.RefreshUI();
            }

            internal sealed class RetailServerCallingContext : IDisposable
            {
                private MainForm mainForm;
                private string oldDeviceToken;
                private UserToken oldUserToken;
                private string oldOperatingUnitNumber;

                private RetailServerCallingContext(MainForm mainForm)
                {
                    // backing up some things
                    this.mainForm = mainForm;

                    mainForm.DisableUI();

                    if (this.mainForm.currentManagerFactory != null)
                    {
                        this.oldDeviceToken = mainForm.currentManagerFactory.Context.GetDeviceToken();
                        this.oldUserToken = mainForm.currentManagerFactory.Context.GetUserToken();
                        if (mainForm.ShouldUseOnlineMode)
                        {
                            // operatingUnitNumber is not supported in offline context
                            this.oldOperatingUnitNumber = mainForm.currentManagerFactory.Context.GetOperatingUnitNumber();
                        }

                        // switching context
                        if (!this.mainForm.ShouldUseDeviceContext)
                        {
                            this.mainForm.currentManagerFactory.Context.SetDeviceToken(null);
                        }

                        if (!this.mainForm.ShouldUseUserContext)
                        {
                            this.mainForm.currentManagerFactory.Context.SetUserToken(null);
                        }

                        if (mainForm.ShouldUseOnlineMode)
                        {
                            // operatingUnitNumber is not supported in offline context
                            if (this.mainForm.ShouldUseOperatingUnitNumber)
                            {
                                this.mainForm.currentManagerFactory.Context.SetOperatingUnitNumber(this.mainForm.OperatingUnitNumber);
                            }
                        }
                    }
                }

                public void Dispose()
                {
                    GC.SuppressFinalize(this);

                    if (this.mainForm.currentManagerFactory != null)
                    {
                        // restoring original context
                        this.mainForm.currentManagerFactory.Context.SetDeviceToken(this.oldDeviceToken);
                        this.mainForm.currentManagerFactory.Context.SetUserToken(this.oldUserToken);

                        if (this.mainForm.ShouldUseOnlineMode)
                        {
                            // operatingUnitNumber is not supported in offline context
                            this.mainForm.currentManagerFactory.Context.SetOperatingUnitNumber(this.oldOperatingUnitNumber);
                        }
                    }

                    this.mainForm.RefreshUI();
                }

                internal static RetailServerCallingContext TransitionFromUI(MainForm form)
                {
                    return new RetailServerCallingContext(form);
                }
            }
        }
    }
}
