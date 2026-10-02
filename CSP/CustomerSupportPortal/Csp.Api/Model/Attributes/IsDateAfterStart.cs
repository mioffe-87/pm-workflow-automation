using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


using System.Collections.Generic;

namespace Csp.Api.Model.Attributes {

	/// <summary>
	/// This custom attribute is used to determine if an date used as an ending date is less than or equal to 
	/// the starting date
	/// </summary>
	public sealed class IsDateAfterStartAttribute : ValidationAttribute, IClientValidatable {


		private readonly string startPropertyName;
		private readonly bool allowEqualDates;

		/// <summary>
		/// This attribute should be placed only on the end date of the property in the check. 
		/// This Constructor takes the name of the model property that holds the start date and 
		/// if the dates can be equal in the check.
		/// </summary>
		/// <param name="startPropertyName">Name of the property that holds the start date</param>
		/// <param name="allowEqualDates">Default is false. Used to determine if the end date can be
		///  equal to the start date</param>
		public IsDateAfterStartAttribute(string startPropertyName, bool allowEqualDates = true) {

			this.startPropertyName = startPropertyName;
			this.allowEqualDates = allowEqualDates;
		}

		/// <summary>
		/// This is overriden to check if the dates are valid (end date is greater than start date, or 
		/// equal to start date if allowEqualDates is true.)
		/// </summary>
		/// <param name="value">The value being validated</param>
		/// <param name="validationContext">The validation context object</param>
		/// <returns>The validation result</returns>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext) {

			//Get the property of of the start date
			var propertyTestedInfo = validationContext.ObjectType.GetProperty(startPropertyName);

			//If no property is not found then return the information as an error in validation.
			if (propertyTestedInfo == null) {
				return new ValidationResult(string.Format("unknown property {0}", startPropertyName));
			}

			//Get the value of the start date
			var propertyTestedValue = propertyTestedInfo.GetValue(validationContext.ObjectInstance, null);

			//If the value is null or is not a date then return success
			if (value == null || !(value is DateTime)) {
				return ValidationResult.Success;
			}

			//If the value is null or is not a date then return success
			if (propertyTestedValue == null || !(propertyTestedValue is DateTime)) {
				return ValidationResult.Success;
			}

			//If the end date is greater than the start date, or if allowEqualDates is true they are equal to eachother return sucess
			if ((DateTime)value >= (DateTime)propertyTestedValue) {
				if (allowEqualDates) {
					return ValidationResult.Success;
				}
				//Otherwise if the end date is greater than the start date return sucess
				if ((DateTime)value > (DateTime)propertyTestedValue) {
					return ValidationResult.Success;
				}
			}
			//otherwise if start date is greater than end date return validation error
			return new ValidationResult("The date must be greater than the starting range.");
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context) {
			//Adds validation rules for client

			//Create a new rule.
			var rule = new ModelClientValidationRule {
				ErrorMessage = ErrorMessageString,
				ValidationType = "isdateafterstart"
			};

			//Add the validation parameters
			rule.ValidationParameters["startpropertyname"] = startPropertyName;
			rule.ValidationParameters["allowequaldates"] = allowEqualDates;

			//return the rule one at a time
			yield return rule;
		}
	}
}
