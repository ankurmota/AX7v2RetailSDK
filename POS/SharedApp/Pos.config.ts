/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Core/Converters.ts'/>
///<reference path='Controls/buttonBlock/knockout.buttonBlock.ts'/>
///<reference path='Controls/listView/knockout.listView.ts'/>
///<reference path='Controls/SingleSelectRefinerControl/SingleSelectRefinerControl.ts'/>
///<reference path='Controls/MultiSelectRefinerControl/MultiSelectRefinerControl.ts'/> 
///<reference path='Controls/SliderRefinerControl/SliderRefinerControl.ts'/>
///<reference path='Controls/tileList/knockout.tileList.ts'/>
///<reference path='Views/Affiliation/AffiliationsView.ts'/>
///<reference path='Views/Merchandising/CategoriesView.ts'/>
///<reference path='Views/Merchandising/InventoryLookupView.ts'/>
///<reference path='Views/Merchandising/CompareProductsView.ts'/>
///<reference path='Views/Merchandising/KitDisassemblyView.ts'/>
///<reference path='Views/Merchandising/PickingAndReceivingDetailsView.ts'/>
///<reference path='Views/Merchandising/PriceCheckView.ts'/>
///<reference path='Views/Merchandising/ProductDetailsView.ts'/>
///<reference path='Views/Merchandising/ProductsView.ts'/>
///<reference path='Views/Merchandising/SearchPickingAndReceivingView.ts'/>
///<reference path='Views/Merchandising/SearchReceiptsView.ts'/>
///<reference path='Views/Merchandising/SearchStockCountView.ts'/>
///<reference path='Views/Merchandising/SearchView.ts'/>
///<reference path='Views/Merchandising/StockCountDetailsView.ts'/>
///<reference path='Views/Merchandising/StoreDetailsView.ts'/>
///<reference path='Views/Merchandising/CatalogsView.ts'/>
///<reference path='Views/Merchandising/AllStoresView.ts'/>
///<reference path='Views/Merchandising/ProductRichMediaView.ts'/>
///<reference path='Views/Customer/AddressAddEditView.ts'/>
///<reference path='Views/Customer/CustomerDetailsView.ts'/>
///<reference path='Views/Customer/CustomerAddEditView.ts'/>
///<reference path='Views/Customer/CustomerAddressesView.ts'/>
///<reference path='Views/Customer/CustomerAffiliationsView.ts'/>
///<reference path='Views/Customer/RecentPurchasesView.ts'/>
///<reference path='Views/Cart/CartView.ts'/>
///<reference path='Views/Cart/ResumeCartView.ts'/>
///<reference path='Views/Cart/ShowJournalView.ts'/>
///<reference path='Views/Device/DatabaseConnectionStatusView.ts'/>
///<reference path='Views/Device/HardwareStationView.ts'/>
///<reference path='Views/Home/HomeView.ts'/>
///<reference path='Views/Login/ChangePasswordView.ts'/>
///<reference path='Views/Login/DeviceActivationProcessView.ts'/>
///<reference path='Views/Login/DeviceActivationView.ts'/>
///<reference path='Views/Login/DeviceDeactivation.ts'/>
///<reference path='Views/Login/GetStartedView.ts'/>
///<reference path='Views/Login/GuidedActivationView.ts'/>
///<reference path='Views/Login/ExtendedLogOnView.ts'/>
///<reference path='Views/Login/LockRegister.ts'/>
///<reference path='Views/Login/LoginView.ts'/>
///<reference path='Views/Login/ManagerOverrideView.ts'/>
///<reference path='Views/Login/ResetPasswordView.ts'/>
///<reference path='Views/Order/IssueLoyaltyCardView.ts'/>
///<reference path='Views/Order/PaymentView.ts'/>
///<reference path='Views/Reports/ReportsView.ts'/>
///<reference path='Views/Reports/ReportDetailsView.ts'/>
///<reference path='Views/Reports/ReportResultsView.ts'/>
///<reference path='Views/CustomerOrder/AdvancedSearchOrdersView.ts'/>
///<reference path='Views/CustomerOrder/DepositOverrideView.ts'/>
///<reference path='Views/CustomerOrder/PickUpInStoreView.ts'/>
///<reference path='Views/CustomerOrder/PickUpView.ts'/>
///<reference path='Views/CustomerOrder/SearchOrdersView.ts'/>
///<reference path='Views/CustomerOrder/ShippingMethodsView.ts'/>
///<reference path='Views/CustomerOrder/SalesInvoicesView.ts'/>
///<reference path='Views/CustomerOrder/SalesInvoiceDetailsView.ts'/>
///<reference path='Views/CustomerOrder/PaymentHistoryView.ts'/>
///<reference path='Views/DailyOperations/BlindCloseView.ts'/>
///<reference path='Views/DailyOperations/CashManagementView.ts'/>
///<reference path='Views/DailyOperations/CostAccountView.ts'/>
///<reference path='Views/DailyOperations/ResumeShiftView.ts'/>
///<reference path='Views/DailyOperations/TenderCountingView.ts'/>
///<reference path='Views/DailyOperations/TimeClockView.ts'/>
///<reference path='Views/DailyOperations/TimeClockManagerView.ts'/>
///<reference path='Views/Controls/DeveloperMode.ts'/>
///<reference path='Commerce.Core.d.ts'/>
///<reference path='Commerce.ViewModels.d.ts'/>

module Commerce.Config {
    retailServerUrl = StringExtensions.EMPTY;

    // Following configurations will be populated from Desktop side components
    demoModeDeviceId = StringExtensions.EMPTY;
    demoModeTerminalId = StringExtensions.EMPTY;
    demoModeStaffId = StringExtensions.EMPTY;
    demoModePassword = StringExtensions.EMPTY;
    isDemoMode = false;
    isDebugMode = false;
    onlineDatabase = StringExtensions.EMPTY;
    offlineDatabase = StringExtensions.EMPTY;
    connectionTimeout = 120; // Default 120 seconds
    aadLoginUrl = StringExtensions.EMPTY;
    locatorServiceUrl = StringExtensions.EMPTY; 
    appHardwareId = StringExtensions.EMPTY;
    defaultOfflineDownloadIntervalInMilliseconds = 60000; // Default 1 minute
    defaultOfflineUploadIntervalInMilliseconds = 60000;
    defaultPageSize = 250;
    commerceAuthenticationAudience = "Modern POS";
    sqlCommandTimeout = 3600; // Default 3600 seconds
    // End desktop side configuration

    export var appName: string = ViewModelAdapter.getResourceString("string_0");
    export var viewRoot: string = "Views";
    export var controlRoot: string = viewRoot + "/Controls";
    export var viewPaths: string[] = []; // Used by navigator.
    export var navigation: any = [
        {
            title: "Affiliation View",
            page: "AffiliationsView",
            path: "Affiliation",
            viewController: Commerce.ViewControllers.AffiliationsViewController
        },
        {
            title: "Address Add\Edit View",
            page: "AddressAddEditView",
            path: "Customer",
            viewController: Commerce.ViewControllers.AddressAddEditViewController
        },
        {
            title: "Customer Addresses View",
            page: "CustomerAddressesView",
            path: "Customer",
            viewController: Commerce.ViewControllers.CustomerAddressesViewController
        },
        {
            title: "Customer Affiliations View",
            page: "CustomerAffiliationsView",
            path: "Customer",
            viewController: Commerce.ViewControllers.CustomerAffiliationsViewController,
        }, 
        {
            title: "Cart View",
            page: "CartView",
            path: "Cart",
            viewController: Commerce.ViewControllers.CartViewController,
        },
        {
            title: "Resume Cart View",
            page: "ResumeCartView",
            path: "Cart",
            viewController: Commerce.ViewControllers.ResumeCartViewController,
        },
        {
            title: "Show Journal View",
            page: "ShowJournalView",
            path: "Cart",
            viewController: Commerce.ViewControllers.ShowJournalViewController
        },
        {
            title: "Customer Details",
            page: "CustomerDetailsView",
            path: "Customer",
            viewController: Commerce.ViewControllers.CustomerDetailsViewController,
        },
        {
            title: "Recent Purchases View",
            page: "RecentPurchasesView",
            path: "Customer",
            viewController: Commerce.ViewControllers.RecentPurchasesViewController,
        },
        {
            title: "Categories",
            page: "CategoriesView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.CategoriesViewController,
        },
        {
            title: "Compare products",
            page: "CompareProductsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.CompareProductsViewController,
        },
        {
            title: "Database connection status",
            page: "DatabaseConnectionStatusView",
            path: "Device",
            viewController: Commerce.ViewControllers.DatabaseConnectionStatusViewController
        },
        {
            title: "Deposit Override View",
            page: "DepositOverrideView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.DepositOverrideViewController
        },
        {
            title: "ChangePassword",
            page: "ChangePasswordView",
            path: "Login",
            viewController: Commerce.ViewControllers.ChangePasswordViewController
        },
        {
            title: "ResetPassword",
            page: "ResetPasswordView",
            path: "Login",
            viewController: Commerce.ViewControllers.ResetPasswordViewController
        },
        {
            title: "DeviceActivation",
            page: "DeviceActivationView",
            path: "Login",
            viewController: Commerce.ViewControllers.DeviceActivationViewController
        },
        {
            title: "DeviceActivationProcess",
            page: "DeviceActivationProcessView",
            path: "Login",
            viewController: Commerce.ViewControllers.DeviceActivationProcessViewController
        },
        {
            title: "ExtendedLogOn",
            page: "ExtendedLogOnView",
            path: "Login",
            viewController: Commerce.ViewControllers.ExtendedLogonViewController
        },
        {
            title: "GetStarted",
            page: "GetStartedView",
            path: "Login",
            viewController: Commerce.ViewControllers.GetStartedViewController
        },
        {
            title: "GuidedActivation",
            page: "GuidedActivationView",
            path: "Login",
            viewController: Commerce.ViewControllers.GuidedActivationViewController
        },
        {
            title: "Select Hardware Station",
            page: "HardwareStationView",
            path: "Device",
            viewController: Commerce.ViewControllers.HardwareStationViewController
        },
        {
            title: "Home",
            page: "HomeView",
            path: "Home",
            viewController: Commerce.ViewControllers.HomeViewController
        },
        {
            title: "Customer Add Edit View",
            page: "CustomerAddEditView",
            path: "Customer",
            viewController: Commerce.ViewControllers.CustomerAddEditViewController,
        },
        {
            title: "Issue Loyalty Card View",
            page: "IssueLoyaltyCardView",
            path: "Order",
            viewController: Commerce.ViewControllers.IssueLoyaltyCardViewController,
        },
        {
            title: "LockRegister",
            page: "LockRegister",
            path: "Login",
            viewController: Commerce.ViewControllers.LockRegisterViewController,
        },
        {
            title: "Login",
            page: "LoginView",
            path: "Login",
            viewController: Commerce.ViewControllers.LoginViewController
        },
        {
            title: "Payment",
            page: "PaymentView",
            path: "Order",
            viewController: Commerce.ViewControllers.PaymentViewController
        },
        {
            title: "Products",
            page: "ProductsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.ProductsViewController,
        },
        {
            title: "Products Details",
            page: "ProductDetailsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.ProductDetailsViewController,
        },
        {
            title: "Product Rich Media",
            page: "ProductRichMediaView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.ProductRichMediaViewController,
        },
        {
            title: "Search Receipts View",
            page: "SearchReceiptsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.SearchReceiptsViewController
        },
        {
            title: "Search View",
            page: "SearchView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.SearchViewController
        },
        {
            title: "Reports View",
            page: "ReportsView",
            path: "Reports",
            viewController: Commerce.ViewControllers.ReportsViewController
        },
        {
            title: "Report Details View",
            page: "ReportDetailsView",
            path: "Reports",
            viewController: Commerce.ViewControllers.ReportDetailsViewController
        },
        {
            title: "Report Results View",
            page: "ReportResultsView",
            path: "Reports",
            viewController: Commerce.ViewControllers.ReportResultsViewController
        },
        {
            title: "Shipping Methods View",
            page: "ShippingMethodsView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.ShippingMethodsViewController
        },
        {
            title: "Inventory Lookup View",
            page: "InventoryLookupView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.InventoryLookupViewController
        },
        {
            title: "Pick up in store View",
            page: "PickUpInStoreView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.PickUpInStoreViewController
        },
        {
            title: "Search Orders View",
            page: "SearchOrdersView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.SearchOrdersViewController
        },
        {
            title: "Sales Invoices View",
            page: "SalesInvoicesView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.SalesInvoicesViewController
        },
        {
            title: "Sales Invoice Details View",
            page: "SalesInvoiceDetailsView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.SalesInvoiceDetailsViewController
        },
        {
            title: "Search Stock Count View",
            page: "SearchStockCountView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.SearchStockCountViewController
        },
        {
            title: "Stock Count Details View",
            page: "StockCountDetailsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.StockCountDetailsViewController
        },
        {
            title: "Kit Disassembly View",
            page: "KitDisassemblyView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.KitDisassemblyViewController
        },
        {
            title: "Price Check View",
            page: "PriceCheckView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.PriceCheckViewController
        },
        {
            title: "Search Picking and Receiving View",
            page: "SearchPickingAndReceivingView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.SearchPickingAndReceivingViewController
        },
        {
            title: "Picking and Receiving Details View",
            page: "PickingAndReceivingDetailsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.PickingAndReceivingDetailsViewController
        },
        {
            title: "Store Details View",
            page: "StoreDetailsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.StoreDetailsViewController
        },
        {
            title: "Pick up View",
            page: "PickUpView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.PickUpViewController
        },
        {
            title: "Show Blind Closed Shifts View",
            page: "BlindCloseView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.BlindCloseViewController
        },
        {
            title: "Resume Shift View",
            page: "ResumeShiftView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.ResumeShiftViewController
        },
        {
            title: "Cash Management View",
            page: "CashManagementView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.CashManagementViewController
        },
        {
            title: "Tender Counting View",
            page: "TenderCountingView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.TenderCountingViewController
        },
        {
            title: "Cost Account View",
            page: "CostAccountView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.CostAccountViewController
        },
        {
            title: "Settings",
            page: "SettingsView",
            path: "Device",
            viewController: Commerce.ViewControllers.SettingsViewController
        },
        {
            title: "Time Clock View",
            page: "TimeClockView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.TimeClockViewController
        },
        {
            title: "Time Clock Manager View",
            page: "TimeClockManagerView",
            path: "DailyOperations",
            viewController: Commerce.ViewControllers.TimeClockManagerViewController
        },
        {
            title: "ManagerOverride",
            page: "ManagerOverrideView",
            path: "Login",
            viewController: Commerce.ViewControllers.ManagerOverrideViewController
        },
        {
            title: "Advance search orders",
            page: "AdvancedSearchOrdersView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.AdvancedSearchOrdersViewController
        },
        {
            title: "Catalogs view",
            page: "CatalogsView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.CatalogsViewController
        },
        {
            title: "All stores view",
            page: "AllStoresView",
            path: "Merchandising",
            viewController: Commerce.ViewControllers.AllStoresViewController
        },
        {
            title: "Payment history view",
            page: "PaymentHistoryView",
            path: "CustomerOrder",
            viewController: Commerce.ViewControllers.PaymentHistoryViewController
        }
    ];

    export var taskRecorderPanels: TaskRecorder.IViewDefinitionMap = {
        "MainPanel": {
            viewModelType: TaskRecorder.ViewModel.MainPanelViewModel
        },
        "ControlPanel": {
            viewModelType: TaskRecorder.ViewModel.ControlPanelViewModel
        }
    };

    export var taskRecorderPages: TaskRecorder.IViewDefinitionMap = {
        "Welcome": {
            viewModelType: TaskRecorder.ViewModel.WelcomeViewModel
        },
        "NewRecording": {
            viewModelType: TaskRecorder.ViewModel.NewRecordingViewModel
        },
        "Recording": {
            viewModelType: TaskRecorder.ViewModel.RecordingViewModel
        },
        "NewTask": {
            viewModelType: TaskRecorder.ViewModel.NewTaskViewModel
        },
        "EditStep": {
            viewModelType: TaskRecorder.ViewModel.EditStepViewModel
        },
        "EditTask": {
            viewModelType: TaskRecorder.ViewModel.EditTaskViewModel
        },
        "CompleteRecording": {
            viewModelType: TaskRecorder.ViewModel.CompleteRecordingViewModel
        },
        "Help": {
            viewModelType: TaskRecorder.ViewModel.HelpViewModel
        },
        "StartTaskGuide": {
            viewModelType: TaskRecorder.ViewModel.StartTaskGuideViewModel
        },
    };
}

// in order to avoid reference all the controls in this file, we only define them after all javascript is loaded,
// i.e. after the DOM content is loaded. This way, custom controls defined elsewhere do not have to be referenced here.
document.addEventListener('DOMContentLoaded', function () {
    // load controls defined under Commerce.Controls namespace
    var controlsNamespace = Commerce.Controls;
    var userControlType = Commerce.Controls.UserControl;

    for (var controlName in controlsNamespace) {
        var controlPrototype = controlsNamespace[controlName].prototype;
        if (controlName !== "UserControl" && Commerce.ObjectExtensions.isOfType(controlPrototype, userControlType)) {
            var pagePath: string = null;
            if (controlName.indexOf("DeviceDeactivation") >= 0) {
                pagePath = Commerce.Config.viewRoot + "/Login/" + controlName + ".html";    
            } else {
                pagePath = Commerce.Config.controlRoot + "/" + controlName + ".html";    
            }
            Commerce.ViewModelAdapterWinJS.defineControl(pagePath, controlsNamespace[controlName]);
        }
    }

    // load pages   
    Commerce.Config.navigation.forEach((navigation) => {
        var pagePath = Commerce.Config.viewRoot + "/" + navigation.path + "/" + navigation.page + ".html";
        Commerce.Config.viewPaths[navigation.page] = pagePath; // create collection of all paths.
        Commerce.ViewModelAdapterWinJS.define(pagePath, navigation.page, navigation.viewController);
    });

    // overrides OperationsManager
    Commerce.Operations.OperationsManager.instance.managerOverrideHandler = Commerce.ViewControllers.ManagerOverrideOperationHandler;

    // override currency for en-us
    Globalize.addCultureInfo("en-us", "default", {
        numberFormat: {
            currency: {
                // pattern: [negative pattern, positive pattern]
                pattern: ["($n)", "$n"]
            }
        },
    });
	
    /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
    // Use entensibility helper for extension property
    Commerce.ExtensibilityHelper.extend(Commerce.Model.Entities.CustomerClass, "emailPrefOptIn", "EMAILOPTIN", Commerce.PropertyTypeEnum.IntegerValue);
    END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */
});
