using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence
{
    public class PartnerEvalRepository : IPartnerEvalRepository
    {
        public DeviceDetails GetDeviceInfo(string serialNumber)
        {
            using (var db = new cspEntities())
            {
                var device = (from d in db.devices
                              where d.IsActive && d.SerialNumber == serialNumber
                              select new DeviceDetails
                              {
                                  SerialNumber = serialNumber,
                                  DeviceType = d.partmaster.PartNumber,
                                  RegistrationDate = (DateTime)d.RegistrationDate,
                                  DeviceCat = d.DeviceCategoryId,
                                  City = d.City,
                                  Country = d.country.Name,
                                  SupportAccountId = d.SupportAccountId,
                              }).SingleOrDefault();

                return device;
            }
        }

        //public LicenseDetails[] GetLicenseInfo(string serialNumber)
        //{
        //    using (var db = new cspEntities())
        //    {
        //        return (
        //            from l in db.licenses
        //            join d in db.devices on l.DeviceId equals d.DeviceId
        //            where l.IsActive && d.SerialNumber == serialNumber
        //            select new LicenseDetails
        //            {
        //                AuthCode = l.AuthorizationCode,
        //                LicenseType = l.partmaster.PartNumber,
        //                RegistrationDate = (DateTime)l.RegistrationDate,
        //                ExpirationDate = (DateTime)l.ExpirationDate,
        //                Description = l.partmaster.Description,
        //            }).ToArray();
        //    }
        //}

        public List<long> GetPartnerMemberships(long userAccountId)
        {
            using (var db = new cspEntities())
            {
                var direct = (from m in db.memberships
                              where m.UserAccountId == userAccountId && m.SupportAccountId != null
                              select m.SupportAccountId.Value
                    ).ToList();
                var group = (from m in db.memberships
                             join sag in db.supportaccountgroups on m.GroupId equals sag.GroupId
                             where m.UserAccountId == userAccountId
                             select sag.SupportAccountId).ToList();
                direct.AddRange(@group);

                return direct;
            }
        }

        public long? DoesUserExistAndGetUserAccountId(string partnerLoginId)
        {
            using (var db = new cspEntities())
            {
                return (from m in db.memberships
                        where m.SalesforceLogInId == partnerLoginId && m.SupportAccountId != null
                        select m.UserAccountId).SingleOrDefault();
            }
        }

        public string GetDeviceTag(long deviceCat)
        {
            using (var db = new cspEntities())
            {
                return (from dc in db.devicecategories
                        where dc.DeviceCategoryId == deviceCat
                        select dc.DeviceCategoryName).SingleOrDefault();
            }
        }

        public DateTime? GetSupportExpiration(string serialNumber)
        {
            using (var db = new cspEntities())
            {
                return (from d in db.devices
                        join l in db.licenses on d.DeviceId equals l.DeviceId
                        join p in db.partmasters on l.PartMasterId equals p.PartMasterId
                        where d.SerialNumber == serialNumber && l.IsActive && p.PartNumber.ToLower().Contains("svc")
                        select l.ExpirationDate).SingleOrDefault();
            }
        }
    }
}