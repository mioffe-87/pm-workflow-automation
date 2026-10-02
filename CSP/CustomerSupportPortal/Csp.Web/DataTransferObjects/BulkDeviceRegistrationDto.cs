using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using System.ComponentModel.DataAnnotations;

namespace Csp.Web.DataTransferObjects {


	public class BulkDeviceRegistrationDto : DefaultDto {


		#region Public properties

		public Device Device { get; set; }

		/// <summary>
		/// Used to store between requests if all licenses are selected in the list
		/// </summary>
		public bool CheckAllLicenses {
			get;
			set;
		}


		/// <summary>
		/// Used to store between requests if all devices are selected in the list
		/// </summary>
		public bool CheckAllDevices {
			get;
			set;
		}

		/// <summary>
		/// Flag used to determine if registration is started as the form as multiple submits, one to get sales orders, devices and licenses and one to post
		/// </summary>
		public bool StartRegistration {
			get;
			set;
		}



		/// <summary>
		/// The support account id that the devices are to be registered for.
		/// </summary>
		public LicensesDto AuthorizationCodes {

			get;
			set;

		}

		/// <summary>
		/// The support account id that the devices are to be registered for.
		/// </summary>
		public long SupportAccountId {

			get;
			set;

		}


		/// <summary>
		/// The Sales order number used to search for a specific sales order to add to the selected Sales Orders property
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters")]
		public string SalesOrderNumberSearchValue {

			get;
			set;

		}


		/// <summary>
		/// The list of sales orders that are to be processed in the bulk registration
		/// </summary>
		public List<SalesOrder> SalesOrders {

			get;
			set;

		}

		/// <summary>
		/// The list of sales order IDs that are selected
		/// </summary>
		public string SelectedSalesOrders {

			get;
			set;

		}

		/// <summary>
		/// The list of devices that are to be bulk registered. 
		/// </summary>
		public List<DeviceListItem> Devices {

			get;
			set;
		}

		/// <summary>
		/// List of authorization codes used to be processed. 
		/// </summary>
		public string SelectedAuthorizationCodes {
			get;
			set;
		}

		/// <summary>
		/// A string that holds a comma delimited list of selected devices
		/// </summary>
		/// 
		[Required(ErrorMessage = "You must select a device.")]
		public string SelectedDevices {

			get;
			set;
		}


		#endregion

	}
}