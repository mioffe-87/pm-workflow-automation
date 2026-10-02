using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atlas;
using CspScheduledTasks.Jobs;
using Quartz;
using System.IO;

namespace CspScheduledTasks
{
    public class CspSchedulerService : IAmAHostedProcess
    {
        public IScheduler Scheduler { get; set; }
        public IJobListener JobListener { get; set; }

        public void Start()
        {
           
            var evalAccountCreationJobCronSchedule = ConfigurationManager.AppSettings["EvalAccountCreationJobCronSchedule"];        
            var sfdcAddUpdateContactJobCronSchedule = ConfigurationManager.AppSettings["SfdcAddUpdateContactJobCronSchedule"];
            var bulkRegistrationJobCronSchedule = ConfigurationManager.AppSettings["BulkRegistrationJobCronSchedule"];
            var autofocusTrialExpJobCronSchedule = ConfigurationManager.AppSettings["AutofocusTrialExpJobCronSchedule"];
            var mfaRegistrationCronSchedule = ConfigurationManager.AppSettings["MfaRegistrationCronSchedule"];
            var bulkDeviceTransferCronSchedule = ConfigurationManager.AppSettings["bulkDeviceTransferCronSchedule"];
            var emailNotificationsCronSchedule = ConfigurationManager.AppSettings["emailNotificationsCronSchedule"];
            var licensingApiNotificationJobCronSchedule = ConfigurationManager.AppSettings["LicensingApiNotificationJobCronSchedule"];
            var vmElaTokenNotificationJobCronSchedule = ConfigurationManager.AppSettings["VmElaTokenNotificationJobCronSchedule"];
            var cloudServiceUserTransferCronSchedule = ConfigurationManager.AppSettings["CloudServiceUserTransferCronSchedule"];
            var bulkOtpRequestCronSchedule = ConfigurationManager.AppSettings["BulkOtpRequestCronSchedule"];    
            var cbhUsageCronSchedule = ConfigurationManager.AppSettings["CbhUsageCronSchedule"];

            // Eval Account Creation Job
            var evalAccountCreationJob = JobBuilder.Create<EvalAccountCreationJob>()
                .WithIdentity("EvalAccountCreationJob")
                .Build();

            var evalAccountCreationTrigger = TriggerBuilder.Create()
                .WithIdentity("EvalAccountCreationTrigger")
                .WithCronSchedule(evalAccountCreationJobCronSchedule)
                .StartAt(DateTime.Now)
                .WithPriority(1)
                .ForJob("EvalAccountCreationJob")
                .Build();


            // Sfdc Add/Update Contact Sync Job
            var sfdcAddUpdateContactJob = JobBuilder.Create<SfdcAddUpdateContactJob>()
                .WithIdentity("SfdcAddUpdateContactJob")
                .Build();

            var sfdcAddUpdateContactTrigger = TriggerBuilder.Create()
                .WithIdentity("SfdcAddUpdateContactTrigger")
                .WithCronSchedule(sfdcAddUpdateContactJobCronSchedule)
                .StartAt(DateTime.Now)
                .ForJob("SfdcAddUpdateContactJob")
                .Build();

            // Bulk Registration Sync Job
            var bulkRegistrationJob = JobBuilder.Create<BulkRegistrationJob>()
                .WithIdentity("BulkRegistrationJob")
                .Build();

            var bulkRegistrationTrigger = TriggerBuilder.Create()
                .WithIdentity("BulkRegistrationTrigger")
                .WithCronSchedule(bulkRegistrationJobCronSchedule)
                .StartAt(DateTime.Now)
                .ForJob("BulkRegistrationJob")
                .Build();

            // Autofocus Trial role Expiration Job - runs daily
            //var autofocusTrialExpirationJob = JobBuilder.Create<AutofocusTrialExpirationJob>()
            //    .WithIdentity("AutofocusTrialExpirationJob")
            //    .Build();

            //var autofocusTrialExpirationTrigger = TriggerBuilder.Create()
            //    .WithIdentity("autofocusTrialExpirationTrigger")
            //    .WithCronSchedule(autofocusTrialExpJobCronSchedule)
            //    .StartAt(DateTime.Now)
            //    .ForJob("AutofocusTrialExpirationJob")
            //    .Build();

            //Mfa User Registrations
            //var mfaRegistrationJob = JobBuilder.Create<MfaRegistrationJob>()
            //    .WithIdentity("MfaRegistrationJob")
            //    .Build();

            //var mfaRegistrationTrigger = TriggerBuilder.Create()
            //    .WithIdentity("mfaRegistrationTrigger")
            //    .WithCronSchedule(mfaRegistrationCronSchedule)
            //    .StartAt(DateTime.Now)
            //    .ForJob("MfaRegistrationJob")
            //    .Build();

            // Bulk Device transfer Sync Job
            var bulkDevicetransferJob = JobBuilder.Create<BulkDeviceTransferJob>()
                .WithIdentity("BulkDevicetransferJob")
                .Build();

            var bulkDeviceTransferTrigger = TriggerBuilder.Create()
                .WithIdentity("BulkDeviceTransferTrigger")
                .WithCronSchedule(bulkRegistrationJobCronSchedule)
                .StartAt(DateTime.Now)
                .ForJob("BulkDevicetransferJob")
                .Build();

            //Process Pending Email Notifications's Status Update
            var emailNotificationsJob = JobBuilder.Create<EmailNotificationsJob>()
                .WithIdentity("EmailNotificationsJob")
                .Build();

            var emailNotficationsTrigger = TriggerBuilder.Create()
                .WithIdentity("emailNotficationsTrigger")
                .WithCronSchedule(emailNotificationsCronSchedule)
                .StartAt(DateTime.Now)
                .ForJob("EmailNotificationsJob")
                .Build();


            //Notify superusers if the licensingapi key is expiring in 15 days
            //var licensingApiNotificationJob = JobBuilder.Create<LicensingApiNotificationJob>()
            //    .WithIdentity("LicensingApiNotificationJob")
            //    .Build();

            //var licensingApiNotificationTrigger = TriggerBuilder.Create()
            //    .WithIdentity("licensingApiNotificationTrigger")
            //    .WithCronSchedule(licensingApiNotificationJobCronSchedule)
            //    .StartAt(DateTime.Now)
            //    .ForJob("LicensingApiNotificationJob")
            //    .Build();

            //VM ELA Token Notifications
            var vmElaTokenNotificationJob = JobBuilder.Create<VmElaTokenNotificationJob>()
                .WithIdentity("VmElaTokenNotificationJob")
                .Build();

            var vmElaTokenNotificationTrigger = TriggerBuilder.Create()
                .WithIdentity("vmElaTokenNotificationTrigger")
                .WithCronSchedule(vmElaTokenNotificationJobCronSchedule)
                .StartAt(DateTime.Now)
                .ForJob("VmElaTokenNotificationJob")
                .Build();

            //Cloud Service User Transfer
            //var cloudServiceUserTransferJob = JobBuilder.Create<CloudServiceUserTransferJob>()
            //    .WithIdentity("CloudServiceUserTransferJob")
            //    .Build();

            //var cloudServiceUserTransferTrigger = TriggerBuilder.Create()
            //    .WithIdentity("CloudServiceUserTransferTrigger")
            //    .WithCronSchedule(cloudServiceUserTransferCronSchedule)
            //    .StartAt(DateTime.Now)
            //    .ForJob("CloudServiceUserTransferJob")
            //    .Build();

            //Bulk OTP Processing
            var bulkOtpRequestJob = JobBuilder.Create<BulkOtpRequestJob>()
                .WithIdentity("BulkOtpRequestJob")
                .Build();

            var bulkOtpRequestTrigger = TriggerBuilder.Create()
                .WithIdentity("BulkOtpRequestTrigger")
                .WithCronSchedule(bulkOtpRequestCronSchedule)
                .StartAt(DateTime.Now)
                .ForJob("BulkOtpRequestJob")
                .Build();
            // Removed PIN Expiration Job as it is moved to Java 02/06/2025
            //PIN Expiration Processing
           
            // Removed Otp Expiration Job as it is moved to Java 12/12/24
            // Comments By LK On 11/23/15
            // M. La Voie to check whether these jobs are needed           
            Scheduler.ScheduleJob(evalAccountCreationJob, evalAccountCreationTrigger);
            Scheduler.ScheduleJob(sfdcAddUpdateContactJob, sfdcAddUpdateContactTrigger);
            Scheduler.ScheduleJob(bulkRegistrationJob, bulkRegistrationTrigger);
            // Scheduler.ScheduleJob(autofocusTrialExpirationJob, autofocusTrialExpirationTrigger);
            //Scheduler.ScheduleJob(mfaRegistrationJob, mfaRegistrationTrigger);
            Scheduler.ScheduleJob(bulkDevicetransferJob, bulkDeviceTransferTrigger);
            Scheduler.ScheduleJob(emailNotificationsJob, emailNotficationsTrigger);
            //  Scheduler.ScheduleJob(licensingApiNotificationJob,licensingApiNotificationTrigger);
            //  Scheduler.ScheduleJob(cloudServiceUserTransferJob, cloudServiceUserTransferTrigger);
            Scheduler.ScheduleJob(vmElaTokenNotificationJob, vmElaTokenNotificationTrigger);
            Scheduler.ScheduleJob(bulkOtpRequestJob, bulkOtpRequestTrigger);     
            Scheduler.ListenerManager.AddJobListener(JobListener);           
            Scheduler.Start();
          
        }

        public void Stop()
        {
            Scheduler.Shutdown();
        }

        public void Resume()
        {
            Scheduler.ResumeAll();
        }

        public void Pause()
        {
            Scheduler.PauseAll();
        }
    }
}
