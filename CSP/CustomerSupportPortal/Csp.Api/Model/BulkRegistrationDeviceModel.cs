using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model
{
    public class BulkRegistrationDeviceModel
    {
        public long Id { get; set; }
        public long BulkRegistrationId { get; set; }
        public long DeviceId { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public long? VmSeriesLicenseId { get; set; }
        public bool? IsAlreadyRegistered { get; set; }
        public string CustomValueDeviceName { get; set; }
        public NameValue CustomValueDeviceTag { get; set; }
        public string CustomValueAddress1 { get; set; }
        public string CustomValueAddress2 { get; set; }
        public string CustomValueCity { get; set; }
        public Country CustomValueCountry { get; set; }
        public Region CustomValueRegion { get; set; }
        public string CustomValueOtherRegion { get; set; }
        public string CustomValuePostalCode { get; set; }
        public string CustomValueRemarks { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool? IsRegistrationSuccessful { get; set; }
        public string RegistrationNotes { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public DateTime? ShipDate { get; set; }

        public IEnumerable<BulkRegistrationLicenseModel> BulkRegistrationLicenses { get; set; }
    }

    public class BulkRegistrationDeviceModelValidator : AbstractValidator<BulkRegistrationDeviceModel>
    {
        public BulkRegistrationDeviceModelValidator()
        {
            RuleFor(dataItem => dataItem.CustomValueCity).NotEmpty().WithMessage("City is required");

            RuleFor(dataItem => dataItem.CustomValueCountry).Must(IsValidCustomValueCountry).WithMessage("Country is required");

            RuleFor(dataItem => dataItem.CustomValueRegion).Must(IsValidCustomValueRegion).When(
                x =>
                    (x.CustomValueCountry.Code == CountryCode.US.ToDescription() ||
                     x.CustomValueCountry.Code == CountryCode.CA.ToDescription()))
                .WithMessage("Region is required");

            RuleFor(dataItem => dataItem.CustomValueOtherRegion).NotEmpty().When(
                x =>
                    (x.CustomValueCountry.Code != CountryCode.US.ToDescription() &&
                     x.CustomValueCountry.Code != CountryCode.CA.ToDescription()))
                .WithMessage("Other Region is required");

            RuleFor(dataItem => dataItem.CustomValuePostalCode).NotEmpty().WithMessage("Zip Code is required");
        }

        private bool IsValidCustomValueCountry(Country customValueCountry)
        {
            return (customValueCountry.Id > 0);
        }

        private bool IsValidCustomValueRegion(Region customValueRegion)
        {
            return (customValueRegion.Id > 0);
        }
    }
}
