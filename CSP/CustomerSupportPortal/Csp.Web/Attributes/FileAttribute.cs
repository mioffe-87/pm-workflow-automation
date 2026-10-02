using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Csp.Web.Attributes {
	public class FileAttribute : ValidationAttribute {
		public int FileSize = int.MaxValue;
		public string[] FileTypes;

		public override bool IsValid(object value) {
			var files = (value is IEnumerable)
											? value as IEnumerable<HttpPostedFileBase>
											: new[] { value as HttpPostedFileBase };

			if (files != null) {
				foreach (var file in files) {
					if (file == null)
						return true;

					if (file.ContentLength > FileSize) {
						ErrorMessage = string.Format("The file size should not exceed {0} MB", FileSize / 1024 * 1024);
						return false;
					}

					if (FileTypes != null) {
						if (!FileTypes.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.')))) {
							ErrorMessage =
									string.Format("Invalid file type. Only the following types {0} are supported.",
																string.Join(", ", FileTypes));
							return false;
						}
					}
				}
			}

			return true;
		}
	}
}