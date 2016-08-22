/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder {
    "use strict";

    TaskRecorderDownloader.downloadFile = (url: string): IAsyncResult<boolean> => {
        var result: AsyncResult<boolean> = new AsyncResult<boolean>();

        var fileName: string = UrlHelper.extractFileName(url);

        TaskRecorderDialogHelper.showSaveDialog(fileName)
            .done((file: Windows.Storage.StorageFile) => {
                if (ObjectExtensions.isNullOrUndefined(file)) {
                    result.resolve(true);
                    return;
                }

                TaskRecorderDownloadHelper.downloadFile(url, file)
                    .done(() => {
                        result.resolve(false);
                    })
                    .fail((errors: Proxy.Entities.Error[]) => {
                        result.reject(errors);
                    });
            })
            .fail((errors: Proxy.Entities.Error[]) => {
                result.reject(errors);
            });

        return result;
    };
}