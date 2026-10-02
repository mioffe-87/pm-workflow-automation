using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Csp.Web.Attributes {
	public class BooleanRequiredAttribute : ValidationAttribute, IClientValidatable {
		public override bool IsValid(object value) {
			if (value is bool)
				return (bool)value;
			else
				return true;
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
				ModelMetadata metadata,
				ControllerContext context) {
			yield return new ModelClientValidationRule {
				ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
				ValidationType = "booleanrequired"
			};
		}
	}
}