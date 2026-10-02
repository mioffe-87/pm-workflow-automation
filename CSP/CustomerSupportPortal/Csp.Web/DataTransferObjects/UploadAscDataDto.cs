using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Web.Attributes;
using FluentValidation;
using System.Globalization;
using Csp.Web.HelperClasses;

namespace Csp.Web.DataTransferObjects
{
    public class UploadAscDataDto : DefaultDto
    {
        [Required]
        [File(FileSize = 100 * 1024 * 1024, FileTypes = new[] { ".csv" })]
        public HttpPostedFileBase Attachment { get; set; }
        public DateTime? LastUploadedOn { get; set; }
    }

    public class AscDataItemDto : DefaultDto
    {
        public long Id { get; set; }
        public long AscId { get; set; }
        public string SalesforceAccountId { get; set; }
        public string CreatedOnDateString { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ClosedOnDateString { get; set; }
        public DateTime? ClosedOn { get; set; }
        public string CaseNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ProcessedMonth { get; set; }
    }

    public class AscDataValidator : AbstractValidator<AscDataItemDto> {

        public AscDataValidator()
        {
            RuleFor(dataItem => dataItem.AscId);

            RuleFor(dataItem => dataItem.SalesforceAccountId).NotEmpty().WithMessage("Error occurred during upload ASC Program team has been notified");

            RuleFor(dataItem => dataItem.CaseNumber).NotEmpty().WithMessage("CaseNumber is required");

            RuleFor(dataItem => dataItem.CreatedOnDateString).Must(IsValidDate).WithMessage("CreationDate is invalid");

            RuleFor(dataItem => dataItem.CreatedOn).LessThan(DateTime.Today).WithMessage("CreationDate cannot be a future date");

            RuleFor(dataItem => dataItem.ClosedOnDateString).Must(IsValidClosedOnDate).WithMessage("ClosedDate is invalid");
        }

        private bool IsValidDate(string dateString)
        {
            DateTime timestamp;

            return DateTime.TryParseExact(dateString, SiteHelper.ValidAscScorecardUploadDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out timestamp);
        }

        private bool IsValidClosedOnDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return true;
            }

            return IsValidDate(dateString);
        }
    }
}