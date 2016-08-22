1. Each test method should have its test data XML.
2. In the XML, each test case must include the following XML elements:
ConnectorType: e.g. Portable, Destktop
ConnectorName: e.g. TestConnector
3. When a test method requires the merchant account, the test data XML must include the following XML 
element which points to the merchant account XML file under "TestData" folder.
MerchantAccountXmlPath: e.g. MerchantAccount_TestConnectorPortable.xml