/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
USE msdb

-- Create a job for history purge
EXEC [dbo].[sp_add_job]
    @job_name = N'cardpaymentaccept_job_purge_history',
	@owner_login_name = N'sa';
GO

-- Create a job step to call a stored procedure to purge history
-- In the example, the entry life is set to 1440 minutes (24 hours).  
-- Change entry life as needed. Do not make it shorter than the expiration period.
EXEC [dbo].[sp_add_jobstep]
    @job_name = N'cardpaymentaccept_job_purge_history',
    @step_name = N'cardpaymentaccept_jobstep_run_sp_purgecardpaymenthistory',
    @subsystem = N'TSQL',
    @command = N'EXEC [CardPaymentAccept].[dbo].[PURGECARDPAYMENTHISTORY] @i_EntryLifeInMinutes = 1440;', 
    @retry_attempts = 3,
    @retry_interval = 5;
GO

-- Create a schedule to run nightly at 1AM
EXEC [dbo].[sp_add_schedule]
    @schedule_name = N'cardpaymentaccept_schedule_nightly',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 010000,
	@owner_login_name = N'sa';
GO

-- Attach the schedule to the job
EXEC [dbo].[sp_attach_schedule]
   @job_name = N'cardpaymentaccept_job_purge_history',
   @schedule_name = N'cardpaymentaccept_schedule_nightly';
GO

-- Target the job to the local server
EXEC [dbo].[sp_add_jobserver]
    @job_name = N'cardpaymentaccept_job_purge_history';
GO