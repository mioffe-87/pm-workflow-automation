using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic
{
    public class PartnerEvalLogic : IPartnerEvalLogic
    {
        private readonly IPartnerEvalRepository _partnerEvalRepository;

        public PartnerEvalLogic(IPartnerEvalRepository partnerEvalRepository)
        {
            _partnerEvalRepository = partnerEvalRepository;
        }


        public List<DeviceDetails> AllInfoReturned(string serialNumber, string partnerLoginId)
        {
            string[] values = serialNumber.Split(',');
            List<DeviceDetails> data = new List<DeviceDetails>();

            var userAccountId = _partnerEvalRepository.DoesUserExistAndGetUserAccountId(partnerLoginId);

            if (userAccountId == null)
            {
                data.Add(new DeviceDetails { SerialNumber = serialNumber, Error = "Email does not correspond with any users." });
                return data;
            }

            data.AddRange(values.Select(val => AllInfo(val, partnerLoginId, userAccountId.Value)));

            return data;
        }


        public DeviceDetails AllInfo(string serialNumber, string partnerLoginId, long userAccountId)
        {
            string deviceTag = String.Empty;

            var device = _partnerEvalRepository.GetDeviceInfo(serialNumber);
            if (device == null)
            {
                return new DeviceDetails { SerialNumber = serialNumber, Error = "Serial number does not exist." };
            }
            if (!device.DeviceType.ToLower().Contains("nfr"))
            {
                return new DeviceDetails { SerialNumber = serialNumber, Error = "Device entered is not an -NFR device." };
            }
            if (device.RegistrationDate == null)
            {
                return new DeviceDetails
                {
                    SerialNumber = serialNumber,
                    DeviceType = device.DeviceType,
                    RegistrationDate = null,
                    Info = "This device is currently unregistered."
                };
            }
            if (device.DeviceCat != null)
            {
                deviceTag = _partnerEvalRepository.GetDeviceTag(device.DeviceCat.Value);
            }

            var memberships = _partnerEvalRepository.GetPartnerMemberships(userAccountId);
            if (!memberships.Contains(device.SupportAccountId.Value))
            {
                return new DeviceDetails { SerialNumber = serialNumber, Error = "User does not have any membership to the Support Account containing this device." };
            }

            //var licenses = _partnerEvalRepository.GetLicenseInfo(serialNumber);
            var supportExpiration = _partnerEvalRepository.GetSupportExpiration(serialNumber);

            return new DeviceDetails
            {
                SerialNumber = serialNumber,
                DeviceType = device.DeviceType,
                DeviceTag = deviceTag,
                RegistrationDate = device.RegistrationDate,
                DeviceExpiration = supportExpiration,
                City = device.City,
                Country = device.Country,
                Error = String.Empty,
                DeviceCat = null,
                SupportAccountId = device.SupportAccountId,
            };
        }
    }
}