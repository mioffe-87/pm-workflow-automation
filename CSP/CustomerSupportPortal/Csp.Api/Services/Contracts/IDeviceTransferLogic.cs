using System.Collections.Generic;
using System.Threading.Tasks;
using Csp.Api.Logic;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {

	public interface IDeviceTransferLogic {

		bool ShowBeginDeviceTransferButton(UserAccount userAccount, long supportAccountId, Device deviceData , bool showBootstrapContainerButton );
		bool ShowReviewPendingTransfersButton(UserAccount userAccount, long supportAccountId);
		BeginDeviceTransferModel GetBeginTransferData(long deviceId, long restrictedByUserAccountId, long supportAccountId = 0);
		Task<string> BeginTransfer(BeginDeviceTransferModel model, long userAccountId = 0, long supportAccountId = 0);
		IEnumerable<PendingDeviceTransfer> GetReviewTransferData(UserAccount userAccount, long supportAccountId,long deviceTransferId=0);

		ReviewTransferResult ReviewTransfer(long deviceTransferId, bool isApproved, UserAccount userAccount,
																				long supportAccountId);
        bool HasAscTransferBetaAccess(long? supportAccountId);
	    void ProcessBulkDeviceTransfer();
        bool ShowPanoramaDeviceTransferButton(long supportAccountId);
    }
}
