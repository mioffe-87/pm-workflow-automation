using System;
using System.Collections.Generic;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	/// <summary>
	/// Holds sales order information pushed from SAP.					
	/// </summary>
	public class SalesOrder : AuditCspModel {

		#region Public Members

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(SalesOrderFields.SalesOrderId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(SalesOrderFields.SupportAccountId, DataType = DbType.Int64)]
		public long SupportAccountId {
			set;
			get;
		}


		/// <summary>
		/// Sales order date.
		/// </summary>
		[DataAccess(SalesOrderFields.SalesDate, DataType = DbType.DateTime)]
		public DateTime? SalesDate {
			set;
			get;
		}


		/// <summary>
		/// Sales order date.
		/// </summary>
		[DataAccess(SalesOrderFields.SapSalesOrderNumber)]
		public string SalesOrderNumber {
			set;
			get;
		}


		/// <summary>
		/// Sales order date.
		/// </summary>
		[DataAccess(SalesOrderFields.PurchaseOrderNumber)]
		public string PurchaseOrderNumber {
			set;
			get;
		}

		#endregion

	}
}
