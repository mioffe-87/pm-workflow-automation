using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using System.Configuration; 

namespace Csp.Infrastructure.Data.Persistence
{
    public class SaasRegistrationRepository:ISaasRegistrationRepository
    {
        public string BetaStubPartNumbers = ConfigurationManager.AppSettings["SaasBetaStubPartNumbers"];
        public string GAStubPartNumbers = ConfigurationManager.AppSettings["SaasGAStubPartNumbers"];
        private string SaasStubIdentification = ConfigurationManager.AppSettings["SaasStubIdentification"];
        private const string SaasPartMaster = "aas";
        private const string SaasSupport = "-SVC-";
        private const string WarrantySupport = "WRNT";
        List<string> CompatiblePanoramaModels = ConfigurationManager.AppSettings["IncludeSaasCompatiblePanoramaModels"].Split(',').ToList();
        private int OTPGraceMinutes = Int32.Parse(ConfigurationManager.AppSettings["OTPGraceTime"]);
        private bool SaasPanoramasOnly = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSaasPanoramasOnly"]);
        private string SaasBetaAccessFeature = ConfigurationManager.AppSettings["SaasBetaFeatureName"];
        private string MagnifierBetaPartNumber = ConfigurationManager.AppSettings["MagnifierBetaPartNumber"];
        public List<string> StubPartNumbers;
            
        /// <summary>
        /// Get Panorama Serial Numbers related to given support Account Id
        /// </summary>
        /// <param name="SupportAccountId">Current Support Account ID</param>
        public IEnumerable<PanoramaSerialNumber> GetPanoramaSerialNumbers(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var ShowSaasOnly = SaasPanoramasOnly ? SaasPartMaster : String.Empty;
            var panoramaSerialNumbers = from dev in db.devices
                                                 join pm in db.partmasters on dev.PartMasterId equals pm.PartMasterId
                                                 join ps in db.partsubtypes on pm.PartSubtypeId equals ps.PartSubtypeId into tempPs
                                                 from leftPS in tempPs.DefaultIfEmpty()
                                                 where dev.SupportAccountId == supportAccountId
                                                  && pm.PartTypeId == 3 // Devices only 
                                                  && CompatiblePanoramaModels.Contains(pm.ModelCompatibility)
                                                  && pm.PartNumber.Contains(ShowSaasOnly)
                                                  && leftPS.PanDbIdentification != SaasStubIdentification
                                                 select new PanoramaSerialNumber
                                                 {
                                                     SerialNumber = dev.SerialNumber
                                                 };

            var panoramasWithSupport = from pn in panoramaSerialNumbers
                                  join dev in db.devices on pn.SerialNumber equals dev.SerialNumber
                                  join lic in db.licenses on dev.DeviceId equals lic.LicenseId
                                  join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                                  where pm.PartNumber.Contains(SaasSupport)
                                  && ( pm.PartSubtypeId == 113 || pm.PartSubtypeId == null)
                                  select new PanoramaSerialNumber { SerialNumber = pn.SerialNumber };

            return panoramasWithSupport.OrderBy(x => x.SerialNumber).ToList();
        }

        public IEnumerable<LoggingServiceDevice> GetLoggingServiceDevices(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            if (HasSaasBetaAccess(supportAccountId))
                StubPartNumbers = BetaStubPartNumbers.Split(',').ToList();
            else
                StubPartNumbers = GAStubPartNumbers.Split(',').ToList();
            var loggingServiceDevices = (from lic in db.licenses 
                           join pl in db.partmasters on lic.PartMasterId equals pl.PartMasterId                           
                           join dev in db.devices on lic.VMStubDeviceId equals dev.DeviceId 
                           join pd in db.partmasters on dev.PartMasterId equals pd.PartMasterId
                           where dev.SupportAccountId == supportAccountId
                           && StubPartNumbers.Contains(pd.PartNumber)
                           && pl.PartNumber == "PAN-LGS-MGFR-BETA"
                           &&(lic.VMParentLicenseId ==0 || lic.VMParentLicenseId == null)
                           select new LoggingServiceDevice
                           {
                               SerialNumber = dev.SerialNumber
                           });

            return loggingServiceDevices;
        }

        /// <summary> 
        /// Check if Support Account has Access to Secaas! 
        /// </summary> 
        /// <param name="supportAccountId"></param> 
        /// <returns></returns> 
        public bool HasSaasBetaAccess(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var hasAccess = from ba in db.betaaccesses
                            where ba.SupportAccountId == supportAccountId
                            && ba.FeatureName == SaasBetaAccessFeature
                            select ba.BetaAccessId;

            return hasAccess.Any();
        } 

        public long InsertSaasOTP(string serialNumber,long supportAccountId, string lookUp ,
            string createdBy)
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var saasOTP = new secaasotp()
                {
                    SerialNumber = serialNumber,
                    SupportAccountId = supportAccountId,
                    Lookup = lookUp,
                    SequenceNumber = 0,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = createdBy,
                    UpdatedDate = DateTime.Now,
                    IsActive = false
                };
                db.secaasotps.Add(saasOTP);
                db.SaveChanges();
                return saasOTP.SecaasOtpId;
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);                
            }
            return 0; 
        }

        public bool UpdateSaasOTP(long secaasOTPid, string updatedBy, string OTP, bool isActive)
        {
            try
            {

                var db = SessionDbContext.CspObjectContext;
                var saasOTP = db.secaasotps.Where(x => x.SecaasOtpId == secaasOTPid).FirstOrDefault();
                saasOTP.OTP = OTP;
                saasOTP.IsActive = isActive;
                saasOTP.UpdatedBy = updatedBy;
                saasOTP.UpdatedDate = DateTime.Now;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public OTPOutput GetSaasOTP(long? supportAccountID, string serialNumber)
        {
            OTPOutput otpOp = new OTPOutput();
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var otp = (from so in db.secaasotps
                          where so.SupportAccountId == supportAccountID
                          && so.SerialNumber == serialNumber
                          orderby so.CreatedDate descending
                          select new {so.OTP, so.UpdatedDate}).FirstOrDefault();

                if (otp != null)
                {
                    TimeSpan ts = DateTime.Now.Subtract(otp.UpdatedDate ?? new DateTime());
                    if (ts.TotalMinutes <= 10 && otp.OTP != null && otp.OTP != "EXPIRED!")
                    {
                        otpOp.OneTimePassword = otp.OTP;
                        otpOp.ExpiryDate = DateTime.Parse(otp.UpdatedDate.ToString()).AddMinutes(10).ToString();
                        return otpOp;
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);              
            }
            return otpOp;
        }


        public long GetPanoramaTenantId(long supId, string serialNumber)
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var isSecondarySerialNumber = db.secaaspanoramaregistrations
                    .Where(x => x.IsActive && x.SecondarySerialNumber == serialNumber && x.SupportAccountId == supId)
                    .Any();
				long tenantId = 0;
                string strTenantId;
                if (isSecondarySerialNumber)
                {
					strTenantId = db.secaaspanoramaregistrations.Where(
                    x => x.IsActive && x.SupportAccountId == supId && x.SecondarySerialNumber == serialNumber)
                    .OrderByDescending(y => y.CreatedDate)
                    .Select(x => x.TenantId)
                    .FirstOrDefault();
                }
                else
                {
					strTenantId = db.secaaspanoramaregistrations.Where(x => x.IsActive && x.SupportAccountId == supId && x.PanoramaSerialNumber == serialNumber)
                    .OrderByDescending(y => y.CreatedDate)
                    .Select(x => x.TenantId)
                    .FirstOrDefault();
                }

				if (!string.IsNullOrWhiteSpace(strTenantId)) long.TryParse(strTenantId, out tenantId);
                 
                return tenantId;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return 0;
        }


        public bool HasLcaasLicense(string panoramaSerialNumber) 
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var result = (from dev in db.devices
                              join lic in db.licenses on dev.DeviceId equals lic.DeviceId
                              join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                              join ps in db.partsubtypes on pm.PartSubtypeId equals ps.PartSubtypeId
                              where dev.SerialNumber == panoramaSerialNumber
                              && ps.PanDbIdentification.Contains("LCAAS")
                              && lic.IsActive
                              && (lic.ExpirationDate ?? DateTime.MaxValue) >= DateTime.Now
                              select dev.DeviceId).Any();
                return result;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }

        public bool ContainsSaasAssets(long deviceID, long supportAccountID)
        {
            try
            {
                var db = SessionDbContext.CspObjectContext;
                var result = (from dev in db.devices
                              join lic in db.licenses on dev.DeviceId equals lic.DeviceId
                              join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                              join ps in db.partsubtypes on pm.PartSubtypeId equals ps.PartSubtypeId
                              where dev.DeviceId == deviceID
                              && (ps.PanDbIdentification.ToUpper().Contains(SaasPartMaster.ToUpper())
                              || ps.PanDbIdentification.ToUpper() == "MGFRSC"
                              || ps.PanDbIdentification.ToUpper() == "EVALMGFR")
                              select dev).Any();
                return result;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }
        

        public bool CheckIfPreviouslyActivated(string authCode, string panoramaSerialNumber)
        {
            var db = SessionDbContext.CspObjectContext;
            var partSubTypeId = (from lic in db.licenses
                                 join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                                 where lic.AuthorizationCode == authCode
                                 select pm.PartSubtypeId).FirstOrDefault();
            var result = (from dev in db.devices
                          join lic in db.licenses on dev.DeviceId equals lic.DeviceId
                          join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                          where dev.SerialNumber == panoramaSerialNumber
                          && pm.PartSubtypeId == partSubTypeId
                          select dev).Any();
            return result;
        }

        public bool IsTacUser(long supportAccountId, string email)
        {
            var db = SessionDbContext.CspObjectContext;
            var userAccountId = db.useraccounts.Where(e => e.Email == email).Select(e => e.UserAccountId).FirstOrDefault();
            var tacUserRoles =
                    (from mr in db.membershiproles
                     where
                         mr.membership.UserAccountId == userAccountId
                        // && mr.membership.SupportAccountId == supportAccountId
                         && mr.membership.ActivationDate <= DateTime.Now
                         && (
                                    mr.membership.ExpirationDate >= DateTime.Now
                                    || mr.membership.ExpirationDate == null
                                )
                                && mr.role.RoleId == (long)RoleType.TACUser
                     select mr.MembershipId).ToArray();
            return tacUserRoles.Any();
        }

        public bool IsTacUserOrAdmin(long supportAccountId, string email) {
	        var db = SessionDbContext.CspObjectContext;
	        var userAccountId = db.useraccounts.Where(e => e.Email == email).Select(e => e.UserAccountId).FirstOrDefault();
	        var tacUserRoles =
		        (from mr in db.membershiproles
			        where
				        mr.membership.UserAccountId == userAccountId
				        && mr.membership.ActivationDate <= DateTime.Now
				        && (
					        mr.membership.ExpirationDate >= DateTime.Now
					        || mr.membership.ExpirationDate == null
				        )
				        && (
					        mr.role.RoleId == (long)RoleType.TACUser
					        || mr.role.RoleId == (long)RoleType.TACAdmin || mr.role.RoleId == (long)RoleType.TACSuperUser)
			        select mr.MembershipId).ToArray();
	        return tacUserRoles.Any();
        }

        public bool VerifyAuthorizationBySupportAccount(long supportAccountId, string email)
        {
            var db = SessionDbContext.CspObjectContext;
            var isValid = (from ua in db.useraccounts 
                           join m in db.memberships on ua.UserAccountId equals m.UserAccountId
                           where m.SupportAccountId == supportAccountId
                           && ua.Email == email
                               select m.MembershipId).Any();
            return isValid;
        }

        public bool VerifyAuthorizationBySerailNumber(string serialNumber, string email)
        {
            var db = SessionDbContext.CspObjectContext;
            var isValid = (from dev in db.devices                           
                           join m in db.memberships on dev.SupportAccountId equals m.SupportAccountId
                           join ua in db.useraccounts on m.UserAccountId equals ua.UserAccountId
                           where dev.SerialNumber == serialNumber
                            && m.ActivationDate <= DateTime.Now
                            && (
                                m.ExpirationDate >= DateTime.Now
                                || m.ExpirationDate == null
                                )
                           && ua.Email == email
                           select m.MembershipId).Any();
            return isValid;
        }
        public string GetAuthcodeType(string authCode)
        {
            var db = SessionDbContext.CspObjectContext;
            var authCodeType = (from lic in db.licenses
                           join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                           join ps in db.partsubtypes on pm.PartSubtypeId equals ps.PartSubtypeId
                           where lic.AuthorizationCode == authCode
                           select new {  ps.LicenseFeaturesName, pm.PartNumber})
                           .FirstOrDefault();

            if(authCodeType != null && authCodeType.PartNumber != null && authCodeType.PartNumber.Contains(MagnifierBetaPartNumber))
                return "MGFRBETA";
            else
                return authCodeType.LicenseFeaturesName;
        }
    }
}
