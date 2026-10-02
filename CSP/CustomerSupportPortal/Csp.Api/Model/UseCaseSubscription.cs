using Csp.Api.Model.Flex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace Csp.Api.Model
{ 
    public class UseCaseSubscription
    {
        [JsonProperty(PropertyName = "useCases")]
        public List<UseCase> UseCases { get; set; }

        [JsonProperty(PropertyName = "availableSubscriptions")]
        public List<Availablesubscription> AvailableSubscriptions { get; set; }

        [JsonProperty(PropertyName = "availableFeatures")]
        public List<AvailableFeature> AvailableFeatures { get; set; }

        [JsonProperty(PropertyName = "iotSubscriptions")]
        public List<IOTSubscription> IOTSubscription { get; set; }

        [JsonProperty(PropertyName = "grandfatheringEnds")]
        public DateTime? GrandfatheringEnds { get; set; }

        [JsonProperty(PropertyName = "cspIdHasCdlForMigration")]
        public bool CspIdHasCdlForMigration { get; set; }
    }

    public class UseCase
    {
        [JsonProperty(PropertyName = "flexUseCaseId")]
        public int FlexUseCaseId { get; set; }

        [JsonProperty(PropertyName = "flexUseCaseName")]
        public string FlexUseCaseName { get; set; }

        [JsonProperty(PropertyName = "defaultLicIdentifiers")]
        public List<string> DefaultLicIdentifiers { get; set; }
    }

    public class Availablesubscription
    {
        [JsonProperty(PropertyName = "licIdentifier")]
        public string LicIdentifier { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "isLegacy")]
        public bool IsLegacy { get; set; }

        [JsonProperty(PropertyName = "hiddenOnUi")]
        public bool HiddenOnUi { get; set; }
    }

    public class AvailableFeature
    {
        [JsonProperty(PropertyName = "FeatureCode")]
        public string FeatureCode { get; set; }

        [JsonProperty(PropertyName = "FeatureName")]
        public string FeatureName { get; set; }
    }

    public class IOTSubscription
    {
        [JsonProperty(PropertyName = "licIdentifier")]
        public string LicIdentifier { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }
    }
}
