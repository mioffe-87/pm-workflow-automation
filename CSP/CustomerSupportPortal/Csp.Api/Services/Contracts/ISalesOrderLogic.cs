using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.EntityClasses;


namespace Csp.Api.Services.Contracts {

	public interface ISalesOrderLogic {

		Salesorder GetSalesOrder(string sapSalesOrderNumber);
		List<Salesorder> GetSalesOrders(string salesOrderIds);
		List<Device> GetSalesOrderDevices(string salesOrderIds, Model.DataParameters parameters);
		List<License> GetSalesOrderLicenses(string salesOrderIds, Model.DataParameters parameters);

	}
}
