using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class DeviceLicensesDto : DefaultDto {

		public long DeviceId { get; set; }

		public Device Device { get; set; }
		public ActivateLicense ActivateLicense { get; set; }
		public LicensesDto Licenses { get; set; }

		public bool IsPostback { get; set; }
        //Decryption Port Mirror License
		public bool IsEligibleForDpmLicense { get; set; }
        //Network Packet Broker License
        public bool IsEligibleForDpbLicense { get; set; }
        //Decryption Port Mirror License or Network Packet Broker License
        public bool IsEligibleForFeatureLicense { get; set; }
        //Panorama
        public bool IsEligibleForPanoramaLicense { get; set; }
        // Check if cloud licenses can be transfered from eval panorama
        // should be eval panorama
        // should have Cloud Licenses
        public bool IsEligibleForPanoramaCloudLicenseTransfer { get; set; }
        public bool IsHAPanorama { get; set; }
        public RegisterVmDto VMRegistrationDto { get; set; }
        public PanoramaSerialNumber CloudConvertPanorama { get; set; }
		public DeviceLicenseMode DeviceLicenseMode { get; set; }
		public List<object> AvailableLicenses { get; set; }
		public List<SoftwareKey> WF500Keys { get; set; }
        public EulaModel Eula { get; set; }
        public long SupportAccountId { get; set; }
        public bool IsK2Device { get; set; }
	}

	public enum DeviceLicenseMode {
		None,
		Trial,
		Eval,
	}
}