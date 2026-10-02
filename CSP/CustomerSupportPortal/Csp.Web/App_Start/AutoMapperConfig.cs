using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.PartialShipment;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Web.DataTransferObjects;
using Csp.Api.Model.Flex;
using System;

namespace Csp.Web.App_Start {
	public class AutoMapperConfig {

		public static void Configure() {

			Mapper.CreateMap<Api.Logic.BeginDeviceTransferModel, BeginDeviceTransferDto>()
				.ForMember(dest => dest.ActionHelper, opt => opt.Ignore())
				.ForMember(dest => dest.DataParameters, opt => opt.Ignore())
				.ForMember(dest => dest.Notifications, opt => opt.Ignore());

			Mapper.CreateMap<BeginDeviceTransferDto, Api.Logic.BeginDeviceTransferModel>()
				.ForMember(dest => dest.SourceUserAccountId, opt => opt.Ignore());

			Mapper.CreateMap<DataTransferObjects.UserAccountRegistrationDto, Api.HelperClasses.UserAccountRegistrationEmailModel>();

			Mapper.CreateMap<Api.EntityClasses.Salesorder, Api.Model.SalesOrder>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(origin => origin.SalesOrderId))
				.ForMember(dest => dest.SalesOrderNumber, opt => opt.MapFrom(origin => origin.SapsalesOrderNumber))
				.ForMember(dest => dest.SupportAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByUserAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByUserAccountId, opt => opt.Ignore());

			Mapper.CreateMap<Api.EntityClasses.Device, Api.Model.DeviceListItem>()
				.ForMember(dest => dest.ModelName, opt => opt.MapFrom(origin => origin.PartMasterId_Partmaster_.PartNumber))
				.ForMember(dest => dest.CommentsEncoded, opt => opt.Ignore())
				.ForMember(dest => dest.ASCId, opt => opt.Ignore())
				.ForMember(dest => dest.ASCName, opt => opt.Ignore())
				.ForMember(dest => dest.Licenses, opt => opt.Ignore())
				.ForMember(dest => dest.Group, opt => opt.Ignore())
				.ForMember(dest => dest.License, opt => opt.Ignore())
				.ForMember(dest => dest.AuthCode, opt => opt.Ignore())
				.ForMember(dest => dest.ExpirationDate, opt => opt.Ignore())
				.ForMember(dest => dest.DeviceTag, opt => opt.Ignore())
				.ForMember(dest => dest.RegionState, opt => opt.MapFrom(src => src.Region))
				.ForMember(dest => dest.CountryId, opt => opt.Ignore())
				.ForMember(dest => dest.Country, opt => opt.Ignore())
				.ForMember(dest => dest.DeviceTransferMessage, opt => opt.Ignore());

			Mapper.CreateMap<Api.EntityClasses.Device, Api.Model.Device>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(origin => origin.DeviceId))
				.ForMember(dest => dest.CountryId, opt => opt.MapFrom(origin => origin.CountryId_Country_.CountryId))
				.ForMember(dest => dest.IsActive, opt => opt.MapFrom(origin => origin.IsActive == 1))
				.ForMember(dest => dest.ModelName, opt => opt.Ignore())
				.ForMember(dest => dest.Licenses, opt => opt.Ignore())
				.ForMember(dest => dest.SupportProvider, opt => opt.Ignore())
				.ForMember(dest => dest.Customer, opt => opt.Ignore())
				.ForMember(dest => dest.UserAccount, opt => opt.Ignore())
				.ForMember(dest => dest.DefaultRoles, opt => opt.Ignore())
				.ForMember(dest => dest.Members, opt => opt.Ignore())
				.ForMember(dest => dest.DeviceNameEncoded, opt => opt.Ignore())
				.ForMember(dest => dest.CommentsEncoded, opt => opt.Ignore())
				.ForMember(dest => dest.BillToCountryId, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByUserAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByUserAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.CountryName, opt => opt.Ignore())
				.ForMember(dest => dest.DeviceCategoryName, opt => opt.Ignore());

			Mapper.CreateMap<Api.EntityClasses.License, Api.Model.License>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(origin => origin.LicenseId))
				.ForMember(dest => dest.IsActive, opt => opt.MapFrom(origin => origin.IsActive == 1))
				.ForMember(dest => dest.SupportProvider, opt => opt.Ignore())
				.ForMember(dest => dest.Customer, opt => opt.Ignore())
				.ForMember(dest => dest.Entitlement, opt => opt.Ignore())
				.ForMember(dest => dest.BillToCountryId, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByUserAccountId, opt => opt.Ignore())
                .ForMember(dest => dest.CountryId, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByUserAccountId, opt => opt.Ignore());


			Mapper.CreateMap<Api.EntityClasses.License, Api.Model.LicenseListItem>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(origin => origin.LicenseId))
				.ForMember(dest => dest.ModelName, opt => opt.MapFrom(origin => origin.PartMasterId_Partmaster_.PartNumber))
				.ForMember(dest => dest.LicenseName, opt => opt.MapFrom(origin => origin.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.Name))
				.ForMember(dest => dest.LicenseFeaturesName, opt => opt.Ignore())
				.ForMember(dest => dest.HasLicenseKey, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByEmail, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedByUserAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.LastUpdateByUserAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.IsPerpetual, opt => opt.Ignore())
				.ForMember(dest => dest.SupportCenterSupportAccountId, opt => opt.Ignore())
				.ForMember(dest => dest.SupportCenterSupportAccountName, opt => opt.Ignore());


			Mapper.CreateMap<Api.Services.Contracts.PendingDeviceTransfer, Api.Logic.ReviewDeviceTransferModel>()
				.ForMember(dest => dest.DeviceTransferBeginDate, opt => opt.MapFrom(src => src.CreatedOn))
				.ForMember(dest => dest.FromUserAccountId, opt => opt.MapFrom(src => src.CreatedBy))
				.ForMember(dest => dest.FromUserDisplayName, opt => opt.MapFrom(src => src.CreatedByDisplayName))
				.ForMember(dest => dest.DeviceTransferState, opt => opt.UseValue(Api.Logic.DeviceTransferStates.Pending));

            	Mapper.CreateMap<Api.Services.Contracts.PendingDeviceTransfer, PendingDeviceTransferModel>()
				.ForMember(dest => dest.DeviceTransferBeginDate, opt => opt.MapFrom(src => src.CreatedOn))
				.ForMember(dest => dest.FromUserAccountId, opt => opt.MapFrom(src => src.CreatedBy))
				.ForMember(dest => dest.FromUserDisplayName, opt => opt.MapFrom(src => src.CreatedByDisplayName))
				.ForMember(dest => dest.DeviceTransferState, opt => opt.UseValue(Api.Logic.DeviceTransferStates.Pending.ToDescription()));



			Mapper.CreateMap<Infrastructure.Data.DBs.CSP.license, Api.EntityClasses.License>()
						.ForMember(dest => dest.BillToCountryId_Country_, opt => opt.Ignore())
						.ForMember(dest => dest.PartMasterId_Partmaster_, opt => opt.Ignore())
						.ForMember(dest => dest.CreatedByUserAccountId_Useraccount_, opt => opt.Ignore())
						.ForMember(dest => dest.LastUpdateByUserAccountId_Useraccount_, opt => opt.Ignore())
						.ForMember(dest => dest.Grouplicenses, opt => opt.Ignore())
						.ForMember(dest => dest.Salesorderitems, opt => opt.Ignore())
						.ForMember(dest => dest.IsSapsynched, opt => opt.MapFrom(src =>
												 (src.IsSAPSynched.HasValue
												 ? (
													 src.IsSAPSynched.Value
														 ? (short)1
														 : (short)0
													 )
												 : (short?)null)))
						.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ? (short)1 : (short)0));


			Mapper.CreateMap<Csp.Api.Services.Contracts.SelectableAccount, Csp.Api.Services.Contracts.SupportAccountCsv>()
				.ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => ApiCommon.DecodeString(src.CompanyName)));

			Mapper.CreateMap<Csp.Api.Model.MembershipListItem, Csp.Api.Model.MembershipListItemCsv>()
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
				.ForMember(dest => dest.Roles, opt => opt.MapFrom(src => string.Join(", ", src.Roles.Select(r => r.Name))))
				.ForMember(dest => dest.ActivationDate, opt => opt.MapFrom(src => src.ActivationDate.ToShortDateString()))
				.ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate.HasValue ? src.ExpirationDate.Value.ToShortDateString() : string.Empty));

			Mapper.CreateMap<Csp.Api.Model.DeviceListItem, Csp.Api.Model.DeviceListItemSparesCsv>()
				.ForMember(dest => dest.Group, opt => opt.MapFrom(src => ApiCommon.DecodeString(src.Group)))
				.ForMember(dest => dest.DeviceTag, opt => opt.MapFrom(src => ApiCommon.DecodeString(src.DeviceTag)))
				.ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Comments));

			Mapper.CreateMap<Csp.Api.Model.AvailableEp, Csp.Api.Model.AvailableEpCsv>()
				.ForMember(dest => dest.License, opt => opt.MapFrom(src => src.PartSubTypeName))
				.ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity + src.UseCount))
				.ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate.HasValue ? src.ExpirationDate.Value.ToShortDateString() : string.Empty));

			Mapper.CreateMap<Csp.Api.Model.AvailableVm, Csp.Api.Model.AvailableVmCalc>()
				.ForMember(dest => dest.PartDescription, opt => opt.MapFrom(src => src.Description))
				.ForMember(dest => dest.IsNsx, opt => opt.MapFrom(src => src.IsNsx == 1))
				.ForMember(dest => dest.IsPerpetualNsx, opt => opt.MapFrom(src => src.IsPerpetualNsx == 1));

			Mapper.CreateMap<Csp.Api.Model.AvailableVmCalc, Csp.Api.Model.AvailableVmCsv>();

            Mapper.CreateMap<Csp.Api.Model.AvailableSaaSActivations, Csp.Api.Model.AvailableSaasCalc>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AuthorizationCode, opt => opt.MapFrom(src => src.AuthorizationCode))
                .ForMember(dest => dest.AssoicatedDevices, opt => opt.MapFrom(src => src.AssoicatedDevices))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.ModelName))
                .ForMember(dest => dest.Region, opt => opt.MapFrom(src=>src.Region))
                .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate));

            Mapper.CreateMap<Csp.Api.Model.AvailableSaasCalc, Csp.Api.Model.AvailableSaasCsv>();

			Mapper.CreateMap<Csp.Api.Model.MembershipListItem, Csp.Api.Services.Contracts.SupportAccountMembershipModel>();

			Mapper.CreateMap<Csp.Api.Services.Contracts.SupportAccountMembershipModel, Csp.Api.Model.MembershipListItemCsv>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
				.ForMember(dest => dest.Roles, opt => opt.MapFrom(src => string.Join(", ", src.Roles.Select(r => r.Name))))
				.ForMember(dest => dest.ActivationDate, opt => opt.MapFrom(src => src.ActivationDate.ToShortDateString()))
				.ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate.HasValue ? src.ExpirationDate.Value.ToShortDateString() : string.Empty));
		
			#region AscScoreCard
			Mapper.CreateMap<AscScoreCard, AscScoreCardDto>();

			Mapper.CreateMap<PartnerProfile, PartnerProfileDto>();

			Mapper.CreateMap<AscDeviceTheatrePartner, AscDeviceTheatrePartnerDto>();
			Mapper.CreateMap<InternalInformation, InternalInformationDto>();

			Mapper.CreateMap<MetricsData, MetricsDataDto>();
			Mapper.CreateMap<MetricsConsolidated, MetricsConsolidatedDto>();
			Mapper.CreateMap<MetricsChart, MetricsChartDto>();
			Mapper.CreateMap<ComplianceCurrentQtr, ComplianceCurrentQtrDto>();
			Mapper.CreateMap<Comments, CommentsDto>();
			Mapper.CreateMap<SoftwareEngineer, SoftwareEngineerDto>();
            Mapper.CreateMap<DropDownModel, DropDownModelDto>();

		    Mapper.CreateMap<AscDataItemDto, AscDataModel>();
            Mapper.CreateMap<MultipleAscChart, MultipleAscChartDto>();
            Mapper.CreateMap<CsatLineChart, CsatLineChartDto>();

		    #endregion

            Mapper.CreateMap<Csp.Api.Model.CsspProvisionedDevices, Csp.Api.Model.CsspProvisionedDevicesCsv>();

            Mapper.CreateMap<SoftwareUpdateListItem, SoftwareUpdateListItemDto>();
            Mapper.CreateMap<autofocustrialuser, AutoFocusTrialUser>();

            Mapper.CreateMap<RmaDevice, RmaDeviceDto>();

            Mapper.CreateMap<RmaDeviceDto, RmaDeviceDetails>()
                .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => src.SerialNumber))
                .ForMember(dest => dest.TechSupportFileName, opt => opt.MapFrom(src => src.TechSupportFileName))
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.SfdcAccountId))
                .ForMember(dest => dest.ContactId, opt => opt.MapFrom(src => src.SfdcContactId))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ModelName))
                .ForMember(dest => dest.DistributorId, opt => opt.MapFrom(src => src.DistributorId))
                .ForMember(dest => dest.EndCustomerId, opt => opt.MapFrom(src => src.EndCustomerId))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                .ForMember(dest => dest.ShippingCity, opt => opt.MapFrom(src => src.ShippingCity))
                .ForMember(dest => dest.ShippingCountry, opt => opt.MapFrom(src => src.ShippingCountry))
                .ForMember(dest => dest.ShippingPostalCode, opt => opt.MapFrom(src => src.ShippingZipCode))
                .ForMember(dest => dest.ShippingState, opt => opt.MapFrom(src => src.ShippingRegion));

            Mapper.CreateMap<CreditTransferDataModel, CreditTransferRequest>();

            Mapper.CreateMap<CreditDetail, CreditDataModel>()
               .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EffectiveDate) ? DateTimeOffset.Parse(src.EffectiveDate).ToString("MM/dd/yyyy") : string.Empty))
               .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ExpirationDate)? DateTimeOffset.Parse(src.ExpirationDate).ToString("MM/dd/yyyy") : string.Empty))
               .ForMember(dest => dest.CreditType, opt => opt.MapFrom(src => TokenHelper.GetCreditType(src.Sku, src.IsEval)))
               .ForMember(dest => dest.Expiration, opt => opt.MapFrom(src => src.ExpirationDate));

            Mapper.CreateMap<CreditDataModel, CreditDataModelDto>();

            Mapper.CreateMap<CreditDataModel, TokenInfo>().ForMember(dest => dest.CreditName, opt => opt.MapFrom(src => TokenHelper.GetPoolName(src.CreditType)));
            Mapper.CreateMap<DeploymentProfile, DeploymentProfileData>();
            Mapper.CreateMap<DeploymentProfile, DeploymentProfileDto>();
            /*
            Mapper.CreateMap<Csp.Api.Model.AvailableCnSeries, Csp.Api.Model.AvailableCnSeriesCalc>()
                .ForMember(dest => dest.PartDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsNsx, opt => opt.MapFrom(src => src.IsNsx == 1))
                .ForMember(dest => dest.IsPerpetualNsx, opt => opt.MapFrom(src => src.IsPerpetualNsx == 1));
            Mapper.CreateMap<Csp.Api.Model.AvailableCnSeriesCalc, Csp.Api.Model.AvailableCnSeriesCsv>();
            */

            Mapper.CreateMap<ShipmentDetailResponse, ShippingOrderDetail>()
                .ForMember(dest => dest.ItemsShipped, opt => opt.Ignore());
            

            Mapper.CreateMap<Api.LicensingService.DeploymentProfileResponse, DeploymentProfileModel>();

            Mapper.CreateMap<PanoramaProvisionModel, PanoramaProvisionRequest>();
            Mapper.CreateMap<DeviceTransferRequest, Api.Logic.BeginDeviceTransferModel>()
                .ForMember(dest => dest.TransferBy, opt => opt.Ignore());

            // consent request 
           
            Mapper.CreateMap<ConsentDetailList, ConsentDetailListModel>();
            Mapper.CreateMap<ConsentDetailListModel, ConsentDetailList>();
            Mapper.CreateMap<RolesLog, RolesLogModel>();
            Mapper.CreateMap<StatusLog, StatusLogModel>();
            Mapper.CreateMap<RolesLogModel, RolesLog>();
            Mapper.CreateMap<StatusLogModel, StatusLog>();
            Mapper.CreateMap<ConsentDetail, ConsentDetailModel>();
            Mapper.CreateMap<ConsentDetailModel, ConsentDetail>();
            Mapper.CreateMap<ConsentListResponseModel, ConsentListResponse>();
            
            var map = Mapper.CreateMap<ConsentRequest, ConsentRequestCreate>();
                map.ForAllMembers(opt => opt.Ignore());
                map.ForMember(d => d.RequestorSupportAccountId, o => o.MapFrom(s => s.RequestorSupportAccountId));
                map.ForMember(d => d.RequestorUserAccountId, o => o.MapFrom(s => s.RequestorUserAccountId));
                map.ForMember(d => d.DestinationSupportAccountId, o => o.MapFrom(s => s.DestinationSupportAccountId));
                map.ForMember(d => d.DestinationUserAccountId, o => o.MapFrom(s => s.DestinationUserAccountId));
                map.ForMember(d => d.DestinationUserAccountId, o => o.MapFrom(s => s.DestinationUserAccountId));

            
            var mapGetrequest = Mapper.CreateMap<ConsentRequest, ConsentRequestGetDetails>();
            mapGetrequest.ForAllMembers(opt => opt.Ignore());
            mapGetrequest.ForMember(d => d.ConsentList, o => o.MapFrom(s => s.ConsentList));

            var mapGetlinked = Mapper.CreateMap<ConsentRequest, ConsentRequestGetLinkedAccounts>();
            mapGetlinked.ForAllMembers(opt => opt.Ignore());
            mapGetlinked.ForMember(d => d.SupportAccountIdList, o => o.MapFrom(s => s.SupportAccountIdList));

            var mapConsentUpdate = Mapper.CreateMap<ConsentRequest, ConsentRequestUpdate>();
            mapConsentUpdate.ForAllMembers(opt => opt.Ignore());
            mapConsentUpdate.ForMember(d => d.ConsentRequestId, o => o.MapFrom(s => s.ConsentRequestId));
            mapConsentUpdate.ForMember(d => d.ConsentStatus, o => o.MapFrom(s => s.ConsentStatus));
            mapConsentUpdate.ForMember(d => d.RequestorSupportAccountId, o => o.MapFrom(s => s.RequestorSupportAccountId));
            mapConsentUpdate.ForMember(d => d.RequestorUserAccountId, o => o.MapFrom(s => s.RequestorUserAccountId));
            mapConsentUpdate.ForMember(d => d.Roles, o => o.MapFrom(s => s.Roles));

            var mapAlertModel = Mapper.CreateMap<AlertMessagesModel, AlertModel>();
            mapAlertModel.ForAllMembers(opt => opt.Ignore());
            mapAlertModel.ForMember(d => d.Header, o => o.MapFrom(s => s.Header));
            mapAlertModel.ForMember(d => d.Detail, o => o.MapFrom(s => s.Detail));
            mapAlertModel.ForMember(d => d.Severity, o => o.MapFrom(s => s.Severity));
            mapAlertModel.ForMember(d => d.isPortalTac, o => o.MapFrom(s => s.isPortalTac));

            var mapAlertModelList = Mapper.CreateMap<AlertMessagesResponse, AlertModelList>();
            mapAlertModelList.ForAllMembers(opt => opt.Ignore());
            mapAlertModelList.ForMember(d => d.AlertMessages, o => o.MapFrom(s => s.data));   


        }
    }
}
