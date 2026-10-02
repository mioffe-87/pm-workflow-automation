using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.Services.Contracts;
using Csp.Api.EntityClasses;

namespace Csp.Api.Logic {

	public class SalesOrderLogic : ISalesOrderLogic {

		#region Private Members

		private readonly ISalesOrderRepository salesOrderRepository;


		#endregion

		#region Public Members

		public SalesOrderLogic(ISalesOrderRepository salesOrderRepositoryInterface) {

			salesOrderRepository = salesOrderRepositoryInterface;
		}

		public Salesorder GetSalesOrder(string sapSalesOrderNumber) {

			return salesOrderRepository.ReadSalesOrder(sapSalesOrderNumber);
		}

		public List<Salesorder> GetSalesOrders(string salesOrderIds) {

			return salesOrderRepository.ReadSalesOrders(salesOrderIds);
		}

		public List<Device> GetSalesOrderDevices(string salesOrderIds, Model.DataParameters parameters) {

			return salesOrderRepository.ReadSalesOrderDevices(salesOrderIds, parameters);
		}

		public List<License> GetSalesOrderLicenses(string salesOrderIds, Model.DataParameters parameters) {

			return salesOrderRepository.ReadSalesOrderLicenses(salesOrderIds, parameters);
		}




		#endregion
	}
}
