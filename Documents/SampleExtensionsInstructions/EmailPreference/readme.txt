Sample overview:
The sample shows the usage of extension properties to extend an entity. Entity is extended in AX, persisted in both AX and Channel databases, and POS UI allows to access the value.  Additionally, the new value is written via the RetailRealtimeTransaction service synchronously to AX.  No customizations are needed in the areas of CommerceRuntime or RetailServer (Extension properties flow automatically).

Changes are in AX forms, AX tables, AX RTS client, CDX, Channel DB, Point of Sale (both Modern POS and Cloud POS). Offline mode is not supported for this sample. 

Setup steps:

0. It is advised that you do these changes on a untouched RetailSdk. Ideally, you would have it under source control (VSO, or similar) and no files are changed so far. This is ideal, as you could revert at any steps
   without much work.



   
1. AX customization: 
	- import project file EmailPreferenceAXChanges.axpp, compile and run job (enable Database syncronization in the project)

	OR do these manual steps

	- Create a new Table called RetailCustPreference referencing CustTable
		- Launch Visual Studio 2013
		- Create a new model and project, go to Dynamics AX7 > Model Management > Create Model...
		- Create a new model in USR layer, then click next
		- Add to the existing package ApplicationSuite, click Next and then Finish
		- Give the project a name, click ok
		- Right click your project and go to Add > New Item. Add a new Table called RetailCustPreference
		- Double click RetailCustPreference in your project to open the table designer
		- Create a new enum field EmailOptIn and set Enum Type property equal to NoYes
		- Create a new string field AccountNum and set extended data type to CustAccount and Mandatory to Yes
		- Create a new relation with CustTable and add a new Normal relation between RetailCustPreference.AccountNum and CustTable.AccountNum
		- Save the changes and in your project properties set Synchronize database on build to True
		- Right click on your project to Build the new code and synchronize the database
		- Once this completes successfully, go back to your project properties and uncheck the option to synchronize database on build.

	- Update the CustTable form
		- Go to Application Explorer, User Interface, Forms, CustTable, right click and Create Extension
		- A new element CustTable.Extension is now in your project, double click it to open the form designer
		- Add a new datasource, RetailCustPreference with Link Type equal to OuterJoin
		- Add a new Group named CustomerPreference with Caption as Customer Preference to CustTable form under the Retail Tab page
		- Add a new Check Box field EmailOptIn from table RetailCustPreference within the CustomerPreference group
		- Add a new class CustTable_Extension and in the code editor add code as below:

			 public static class CustTable_Extension
			{
				[FormDataSourceEventHandler(formDataSourceStr(CustTable, CustTable), FormDataSourceEventType::ValidatingWrite)]
				public static void foo(FormDataSource fds, FormDataSourceEventArgs e)
				{
					FormRun                                 form = fds.formRun();
					CustTable                               custTable = fds.cursor() as CustTable;
					RetailCustPreference                    retailCustPreference = form.dataSource(formDataSourceStr(CustTable, RetailCustPreference)).cursor() as RetailCustPreference;

					retailCustPreference.AccountNum = custTable.AccountNum;
					form.dataSource(formDataSourceStr(CustTable, RetailCustPreference)).write();
				}
			}

		- Save all and build your project again (right click project > build)
		- Do an iisreset
		- Go to  Accounts receivable > Common > Customers > All customers, edit a customer record, go to Retail fasttab, check Email Opt In checkbox, and save.
		- In SQL Server Management Studio, verify that the RetailCustPreference table is being saved to correctly.

		
		
2. Configure CDX to sync the new table:
	1.	In AX, go to Retail -> Setup -> Retail Scheduler -> Retail Channel schema and edit the channel schema by adding a new table (channel tables, new): 
		a.	ax.RetailCustPreference, Save
		b.	add fields ACCOUNTNUM, DATAREAID, EMAILOPTIN, RECID 
		c.	hit Ok
 
	2.	Create a new subjob (Retail -> Setup -> Retail Scheduler -> scheduler subjobs):
		a.	Name: RetailCustPreference, Description: RetailCustPreference,
		b.	Channel table name: ax.RetailCustPreference, 
		c.	AX table RetailCustPreference,
		d.	Hit “Match fields”, 
		e.	Save

	3.	Add the new sub job to job “Customers” - 1010 (Retail -> Setup -> Retail Scheduler -> scheduler subjobs) and Save it
	
	4.	Edit the CDX table distribution xml (Retail -> Setup -> Retail Scheduler -> Retail channel schema, export, edit, save with new name, import), add this XML fragment inside both CustTable nodes.
	    Inside and before the closing tag (/Table)

			<Table name="RetailCustPreference">
				<LinkGroup>
					<Link type="FieldMatch" fieldName="accountNum" parentFieldName="AccountNum" />
				</LinkGroup>
			</Table>
	
	By adding this, we explicitely instruct to include changes in this table when synced with the channels

	5. On the “Retail channel schema” form, select “AX7” for the schema name and then click “Generate queries”. 


	
	
3. Channel db (manual, just for development, for official change, see deployment below):
	- apply schema change from ChannelDBUpgrade.sql to correct channel database. This will add the new table
	- change [crt].[CUSTOMERSVIEW]:
		- add ", isnull(rcp.EMAILOPTIN, 0) as EMAILOPTIN" to the end of list of columns to be selected before the UNION All
		- add "LEFT OUTER JOIN [ax].RETAILCUSTPREFERENCE rcp ON ct.ACCOUNTNUM = rcp.ACCOUNTNUM AND ct.DATAAREAID = rcp.DATAAREAID" before the UNION All
		= add ", 0  as EMAILOPTIN" to the end of list of columns to be selected after the UNION ALL
	- change sproc [crt].[CREATEUPDATECUSTOMER]:
		- add the following just before line "MERGE INTO [ax].DIRADDRESSBOOKPARTY":
		
			MERGE INTO [ax].RETAILCUSTPREFERENCE
			USING (SELECT DISTINCT 
			tp.PARENTRECID, tp.PROPERTYVALUE as [EMAILOPTIN], ct.ACCOUNTNUM, ct.DATAAREAID 
			FROM @TVP_EXTENSIONPROPERTIESTABLETYPE tp
			JOIN [ax].CUSTTABLE ct on ct.RECID = tp.PARENTRECID
			WHERE tp.PARENTRECID <> 0 and tp.PROPERTYNAME = 'EMAILOPTIN') AS SOURCE
			ON [ax].RETAILCUSTPREFERENCE.RECID = SOURCE.PARENTRECID
			and [ax].RETAILCUSTPREFERENCE.DATAAREAID = SOURCE.DATAAREAID 
			and [ax].RETAILCUSTPREFERENCE.ACCOUNTNUM = SOURCE.ACCOUNTNUM 
			WHEN MATCHED THEN 
				UPDATE SET [EMAILOPTIN] = source.[EMAILOPTIN]
			WHEN NOT MATCHED THEN
				INSERT
				(
					 RECID
					,DATAAREAID
					,EMAILOPTIN
					,ACCOUNTNUM
				)
				VALUES
				(
					SOURCE.PARENTRECID
					,SOURCE.DATAAREAID
					,SOURCE.EMAILOPTIN
					,SOURCE.ACCOUNTNUM
				);

			SELECT @i_Error = @@ERROR;
			IF @i_Error <> 0
			BEGIN
				SET @i_ReturnCode = @i_Error;
				GOTO exit_label;
			END;


			
4. Verify CDX:
	- run 1010 job full sync (channel data group)
	- check Download sessions and channel db that the data arrived (should show as "Applied")

Note: No customizations are needed in the areas of CommerceRuntime or RetailServer (Extension properties flow automatically).

	
	
5. Test the customization's business logic with the RetailServer test client:
	- open project at RetailSdk\SampleExtensions\RetailServer\Extensions.TestClient, compile and run it
	- Enter the RetailServer url in the text box next to the "Activate New" button and hit it.
	- Enter device and register ids and hit Activate.
	- Enter the AAD credentials that has the registration priviledges and hit Ok.
	- Wait a few seconds.
	- Test client should now show what device is registered.
	- Hit login button and login with worker credentials.
	- Hit "Sdk Tests"  button. This will call the new functionality that saves a customer with the EmailOptIn extension property applied.
	- Verify in AX or database that the customer's EmailOptIn value is stored correctly
	Notes:
	- To see a console with errors/logs, use the "Debug" button.
	
6. Extend Modern POS
	- Open the ModernPos.sln solution
	- do a global search for "BEGIN SDKSAMPLE_CUSTOMERPREFERENCES" in the whole solution.
	- enable the code at all places you found (only one resources file is needed, pick your needed locale) and recompile
	- run Modern POS and verify creating a new customer, updating existing customer updates the flag correctly in both Channel db and AX db
	
	
7. Official Deployment
	- add the channel DB change file to the database folder and register it in customization.settings
	- run msbuild for the whole RetailSdk
	- all packages will have all appropriate changes
	- deploy packages via LCS or manual
