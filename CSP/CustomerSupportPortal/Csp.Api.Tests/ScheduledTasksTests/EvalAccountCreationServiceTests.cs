using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Api.Tests.ScheduledTasksTests {
	[TestClass]
	public class EvalAccountCreationServiceTests {

		private readonly Mock<IEvalAccountCreationRepository> _evalAccountCreationRepository = new Mock<IEvalAccountCreationRepository>();
		private readonly Mock<ISalesforceService> _salesforceService = new Mock<ISalesforceService>();

		[TestMethod]
		public void TestEvalCode() {

			_evalAccountCreationRepository.Setup(x => x.GetDevicesCodesToProcess(It.IsAny<LatestIdsModel>())).Returns(new[]
				{
					new EvalDeviceModel{ EvalId = "E34"},
					new EvalDeviceModel{ EvalId = "EVAL-12345"},
					new EvalDeviceModel{ EvalId = "EVAL-12345 is not valid"},
					new EvalDeviceModel{ EvalId = "evaluationblahblahbla"},
					new EvalDeviceModel{ EvalId = "E3"},
					new EvalDeviceModel{ EvalId = "E323423423"},
					new EvalDeviceModel{ EvalId = "E"},
					new EvalDeviceModel{ EvalId = "EVAL-1"},
					new EvalDeviceModel{ EvalId = "EVAL-"},
				});

			var evalAccountCreationService = new EvalAccountCreationService(_evalAccountCreationRepository.Object, _salesforceService.Object);
			evalAccountCreationService.CreateEvalAccountsForVmEvals();

			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("E34"), Times.Once);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("EVAL-12345"), Times.Once);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("EVAL-12345 is not valid"), Times.Never);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("evaluationblahblahbla"), Times.Never);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("E3"), Times.Once);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("E323423423"), Times.Once);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("E"), Times.Never);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("EVAL-1"), Times.Once);
			_salesforceService.Verify(x => x.GetSalesforceEvalDataByEvalId("EVAL-"), Times.Never);
		}
	}
}
