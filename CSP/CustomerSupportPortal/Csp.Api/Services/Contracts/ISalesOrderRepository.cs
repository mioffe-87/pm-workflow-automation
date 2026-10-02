using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.EntityClasses;

namespace Csp.Api.Services.Contracts {

	public interface ISalesOrderRepository {

		Salesorder ReadSalesOrder(string sapSalesOrderNumber);

		List<Salesorder> ReadSalesOrders(string salesOrderIds);

		List<Device> ReadSalesOrderDevices(string salesOrderIds, Model.DataParameters parameters);

		List<License> ReadSalesOrderLicenses(string salesOrderIds, Model.DataParameters parameters);

	}
}
