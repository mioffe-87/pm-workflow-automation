using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Csp.Api.Model
{
    public class EmailNotification
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("content")]
        public List<Content> Content { get; set; }

        [JsonProperty("from")]
        public From From { get; set; }

        [JsonProperty("personalizations")]
        public List<Personalization> Personalizations { get; set; }

        [JsonProperty("notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty("batchCount")]
        public long TotalBatchCount { get; set; }

        [JsonProperty("queuedBatchCount")]
        public long QueuedBatchCount { get; set; }

        [JsonProperty("processed_batch_count")]
        public long ProcessedBatchCount { get; set; }

        [JsonProperty("completedBatchCount")]
        public long CompletedBatchCount { get; set; }

        [JsonProperty("recipientCount")]
        public long TotalRecipientCount { get; set; }

        [JsonProperty("completedRecipientCount")]
        public long CompletedRecipientCount { get; set; }

        [JsonProperty("isBlacklistEmailFilterOn")]
        public bool IsBlacklistEmailFilterOn { get; set; }
    }
    public class Content
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public class From
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class Personalization
    {
        [JsonProperty("to")]
        public List<To> To { get; set; }

        [JsonProperty("cc")]
        public List<Cc> Cc { get; set; }

        [JsonProperty("bcc")]
        public List<Bcc> Bcc { get; set; }
    }
    public class To
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
    public class Cc
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
    public class Bcc
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class LogEmailNotification
    {
        public string Status { get; set; }
        public string Addressing { get; set; }
        public long CreatedByUserAccountId { get; set; }
        public long CountToBeSent { get; set; }
        public long CountSent { get; set; }
        public string AwsEmailNotificationId { get; set; }
        public string ApplicationName { get; set; }
        public string FeatureName { get; set; }
    }

    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string Content { get; set; }
    }

    public enum TemplateId
    {
        None = 0,
        //New User Settings
        RequireSuperUserApproval = 1,

        //Licensing Api Key Notification Settings
        Enable = 2,
        Disable = 3,
        Extend = 4,
        Regenerate = 5,
        NotifyExpiryIn14Days = 6,
        NotifyExpiryIn7Days = 7,
        NotifyExpiryIn2Days = 8,

        //VM ELA Grant and Accept Tokens
        GrantToken = 9,
        AcceptToken = 10,
        TokenUsage = 12,
        DayOneConfig = 13,
        NewUserTransfer = 16,
        ExistingUserTransfer = 17,

        //Membership approval
        MembershipDecision = 14,
        MembershipPendingDecision = 15,

        //ThirdParty Idp
        SetPassword = 25,
        //Consent Requests
        ConsentRequest = 22,
        ConsentApproval = 23,
        ConsentRejected = 24,
        ConsentUnlinked = 26,
        ConsentRequestMetrics = 28

    }

    public class EmailOptOutListItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("blacklisted")]
        public  bool BlackListed { get; set; }
    }

    public class EmailNotificationResponse
    {
        [JsonProperty("data")]
        public EmailNotification EmailNotification { get; set; }
    }
}
