using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.EntityClasses;
using PAN.Common.Core.Persistence;
using Csp.Api.Services.Contracts;


namespace Csp.Infrastructure.Data.Persistence {

	public class SalesOrderRepository : ISalesOrderRepository {
		private readonly IRepository _repository;

		public SalesOrderRepository(IRepository repository) {
			_repository = repository;
		}



		public Salesorder ReadSalesOrder(string sapSalesOrderNumber) {

			try {
				return _repository.Query<Salesorder>().Single(so => so.SapsalesOrderNumber == sapSalesOrderNumber);

			} catch (InvalidOperationException ex) {
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

				return new Salesorder();
			}

		}

		public List<Salesorder> ReadSalesOrders(string salesOrderIds) {

			if (!String.IsNullOrEmpty(salesOrderIds)) {

				var salesOrderIdArray = (salesOrderIds).Split(',').Select<string, long>(long.Parse).ToArray();

				return _repository.Query<Salesorder>().Where(so => salesOrderIdArray.Contains(so.SalesOrderId)).ToList();
			}

			return new List<Salesorder>();
		}

		public List<Device> ReadSalesOrderDevices(string salesOrderIds, Api.Model.DataParameters parameters) {

			if (!String.IsNullOrEmpty(salesOrderIds)) {

				var salesOrderIdArray = (salesOrderIds).Split(',').Select<string, long>(long.Parse).ToArray();

				var devices = _repository.Query<Salesorderitem>()
						.Where(so => salesOrderIdArray.Contains(so.SalesOrderId_Salesorder_.SalesOrderId) && so.DeviceId_Device_.DeviceId != null
								&& so.DeviceId_Device_.SupportAccountId_Supportaccount_.SupportAccountId == null).Select(item => item.DeviceId_Device_).ToList();


				if (devices.Count > 0) {

					if (!String.IsNullOrEmpty(parameters.SearchValue)) {
						switch (parameters.SearchField) {

							case "SerialNumber":
								devices = devices.Where(device => device.SerialNumber.ToLower().Contains(parameters.SearchValue.ToLower())).ToList();
								break;
							case "ModelName":
								devices = devices.Where(device => device.PartMasterId_Partmaster_.PartNumber.ToLower().Contains(parameters.SearchValue.ToLower())).ToList();
								break;
							default: break;

						}
					}

					switch (parameters.SortField) {

						case "SerialNumber":
							devices = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending) ?
									devices.OrderByDescending(device => device.SerialNumber).ToList() :
									devices.OrderBy(device => device.SerialNumber).ToList();

							break;
						case "ModelName":
							devices = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending) ?
									devices.OrderByDescending(device => device.PartMasterId_Partmaster_.PartNumber).ToList() :
									devices.OrderBy(device => device.PartMasterId_Partmaster_.PartNumber).ToList();
							break;
						default:
							devices = devices.OrderBy(device => device.SerialNumber).ToList();
							break;

					}
				}

				parameters.TotalRecordCount = devices.Count;

				if (parameters.PaginationCount == int.MaxValue) {

					return devices;

				} else {

					return devices.Skip(Convert.ToInt32(parameters.PaginationOffset)).Take(parameters.PaginationCount).ToList();

				}
			}

			return new List<Device>();
		}

		public List<License> ReadSalesOrderLicenses(string salesOrderIds, Api.Model.DataParameters parameters) {

			if (!String.IsNullOrEmpty(salesOrderIds)) {

				var salesOrderIdArray = (salesOrderIds).Split(',').Select(x => long.Parse(x)).ToArray();

				//Get all licenses that are not registeed to a device, that belong to the sales order and are not a 4 hour rma or site license
				var licenses = _repository.Query<Salesorderitem>()
						.Where(so => salesOrderIdArray.Contains(so.SalesOrderId_Salesorder_.SalesOrderId) && so.LicenseId_License_.LicenseId != null
								&& so.LicenseId_License_.DeviceId == null && !so.LicenseId_License_.PartMasterId_Partmaster_.PartNumber.Contains("-4HR-")
								&& !so.LicenseId_License_.PartMasterId_Partmaster_.PartNumber.Contains("-SITE-") && !so.LicenseId_License_.PartMasterId_Partmaster_.PartNumber.Contains("-ENT-SUB-")
								 ).Select(item => item.LicenseId_License_).ToList();


				if (licenses.Count > 0) {

					if (!String.IsNullOrEmpty(parameters.SearchValue)) {
						switch (parameters.SearchField) {

							case "AuthorizationCode":
								licenses = licenses.Where(license => license.AuthorizationCode.ToLower().Contains(parameters.SearchValue.ToLower())).ToList();
								break;
							case "ModelName":
								licenses = licenses.Where(license => license.PartMasterId_Partmaster_.PartNumber.ToLower().Contains(parameters.SearchValue.ToLower())).ToList();
								break;
							case "LicenseName":
								licenses = licenses.Where(license => license.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.Name.ToLower().Contains(parameters.SearchValue.ToLower())).ToList();
								break;
							default: break;

						}
					}

					switch (parameters.SortField) {

						case "AuthorizationCode":
							licenses = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending) ?
									licenses.OrderByDescending(license => license.AuthorizationCode).ToList() :
									licenses.OrderBy(license => license.AuthorizationCode).ToList();

							break;
						case "ModelName":
							licenses = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending) ?
									licenses.OrderByDescending(license => license.PartMasterId_Partmaster_.PartNumber).ToList() :
									licenses.OrderBy(license => license.PartMasterId_Partmaster_.PartNumber).ToList();
							break;
						case "LicenseName":
							licenses = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending) ?
									licenses.OrderByDescending(license => license.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.Name).ToList() :
									licenses.OrderBy(license => license.PartMasterId_Partmaster_.PartSubtypeId_Partsubtype_.Name).ToList();
							break;
						default:
							licenses = licenses.OrderBy(license => license.AuthorizationCode).ToList();
							break;

					}
				}

				parameters.TotalRecordCount = licenses.Count;

				if (parameters.PaginationCount == int.MaxValue) {

					return licenses;

				} else {

					return licenses.Skip(Convert.ToInt32(parameters.PaginationOffset)).Take(parameters.PaginationCount).ToList();

				}
			}

			return new List<License>();

		}

	}


}
