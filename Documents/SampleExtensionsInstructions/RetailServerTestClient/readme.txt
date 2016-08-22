Sample overview:
This simple application is very useful to make either 1) RetailServer calls or 2) offline mode calls through the RetailProxy or 3) both. It acts as a client, similarly as the POS clients, but requires no UI changes and therefore allows for more rapid testing and development.  Use this tool to verify customizations in the areas of Channel database, RTS, CRT, and/or RetailServer before you hand them off to the UI team.


  
Run time steps:
1. Enter the RetailServer url in the text box next to the "Activate New" button and hit it.
2. Enter device and register ids and hit Activate.
3. Enter the AAD credentials that has the registration priviledges and hit Ok.
4. Wait a few seconds.
5. Test client should now show what device is registered.
6. Hit login button and login with worker credentials.
7. Hit Default button. This makes a few simple calls to RetailServer.
8. Add your own (test) code in the "Custom" button handler.
9. Uncheck the Online Mode check box to call RetailProxy for offline mode (you must have implemented offline mode correctly)
10. Hit the "Test Offline Adapters" button to verify that all code has appropirate offline mode implementations. Making sure this is the case, will avoid runtime errors when ModernPOS is in offline mode.

Notes:
1. It is advised that you do these changes on a untouched RetailSdk. Ideally, you would have it under source control (VSO, or similar) and no files are changed so far. This is ideal, as you could revert at any steps without much work.
2. To see a console with errors/logs, use the "Debug" button.
   
