using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Csp.Web.Attributes
{
    public class ValidateAzureSerialNumberAttribute : ValidationAttribute, IClientValidatable
    {
        public override bool IsValid(object value)
        {
            if (value !=null && !string.IsNullOrEmpty(value.ToString()) && value.ToString().Length == 15 && (value.ToString().StartsWith("A") || value.ToString().StartsWith("a")))
                return (bool)value;
            else
                return true;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
                ModelMetadata metadata,
                ControllerContext context)
        {
            yield return new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "validateazureserialnumber"
            };
        }
    }
}