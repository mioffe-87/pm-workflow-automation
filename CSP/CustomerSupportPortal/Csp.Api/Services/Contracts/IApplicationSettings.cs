namespace Csp.Api.Services.Contracts {
	public interface IApplicationSettings {
		string StaticResources { get; }
		string LoginServicePrivateKey { get; }
		string LoggerPath { get; }
        string AgentConfigValue { get; }
        string WildfireWipeoutApi { get; }
		string WildfireErrorMailTo { get; }
		string WildfireReplaceNotifyApi { get; }
		string WildfireDeleteNotifyApi { get; }
		string WildfireApiKey { get; }

        string SalesforceUrl { get; }
        string SalesforceUser { get; }
        string SalesforcePassword { get; }
        string SalesforceInstance { get; }
        string SalesforceConsumerKey { get; }
        string SalesforceConsumerSecret { get; }
        string SalesforceTokenRequestEndpointUrl { get; }
        string EaSiteLicense { get; }

        string CoveoOrganizationId { get; }
        string CoveoSearchHub { get; }
        string CoveoSearchPageUrl { get; }
        string CoveoSearchTokenEndpointUrl { get; }
        string CoveoApiKey { get; }
        string CoveoUnauthApiKey { get; }
        string CoveoSearchRestEndpointUrl { get; }
        string CoveoSearchAnonymousUser { get; }
        string CoveoSearchDataSource { get; }
        string CoveoSearchAnonymousDataSource { get; }

        string RssFeedURL { get; }
        string AllowedOrigins { get; }

	    string ProductsUrl { get; }
	    string ProductsUserName { get; }
	    string ProductsPassword { get; }
	    string ProductTrialRequestUrl { get; }
	    string ProductTrialUserName { get; }	   
        
	    string ProductTrialKey { get; }
        string EncryptionProviderPrivateKey { get; }
        string SalesforceDefaultSeAccountId { get; }
        string EmailBlacklistApiUrl { get; }
        string EmailBlacklistApiKey { get; }

        string TsfSanitationUploadUrl { get; }
        string TsfSanitationUrl { get; }
        string TsfSanitationAuthKey { get; }

        string SessionTimeout { get; }

        string PortalBffHost { get; }
        string PortalBffCertThumbPrint { get; }

        string LicenseApi { get; }
        string LicenseApiThumbprint { get; }

        string ReportingBffHost { get; }
        string ReportingBffCertThumbPrint { get; }

        string EntitlementServiceBffHost { get; }
        string EntitlementBffThumbprint { get; }

        string BiApiBffHost { get; }
        string BiApiBffConsumptionServices { get; }
        string BiApiBffHostThumbprint { get; }

        string PiApiBffHost { get; }
        string PiApiBffHostThumbprint { get; }

        string WFAPIBaseURL { get; set; }

        string WFAPICertCnName { get; set; }

        string WfApiAnalyticsHost { get; }
        string WfApiAnalyticsApiKey { get; }
        string WfApiAnalyticsHealthCheckUrl { get; }
        string WfApiAnalyticsHealthCheckApiKey { get; }
        string JwtEncryptionSecretKey { get; }
        string SecurityHelperEncryptionSecretKey { get; }

        string UtilityHost { get; }
        string UtilityHostCertThumbPrint { get; }

        string CspApiBffHost { get; }
        string CspApiBffCertThumbPrint { get; }

        string SupportCasesUrl { get; }
        string CaseCreationUrl { get; }
        string ApplicationUrl { get; }

        string CspPubSubEventsHost { get; }
        string CspPubSubEventsCertThumbPrint { get; }
        string PortalBffUiHost { get; }
        string AppEnv { get; }

        string CspAuthHost { get; }
        string CspAuthCertThumbprint { get; }

    }
}
