using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface IAscScoreCardLogic {
        AscScoreCard GetAscScoreCardQtrData(string sfdcId, bool isInternal, string theatre, string includeCurrentPartial);
        AscScoreCard GetAscScoreCardLastFiveMonthData(string sfdcId, bool isInternal, string theatre, string includeCurrentPartial);
        DataSet GetFrontLineMetricsDetail(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial);
        DataSet GetCustomerComments(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial);

	    DataSet GetHighScoreCustomerComments(string sfdcId, bool isInternal, string theatre, bool isFiveQuater,
	        string isIncludeCurrentPartial);

		List<SoftwareEngineer> GetAscSoftwareEngineer(string sfdcId, bool isInternal, string theatre, bool isFiveQuater,string isIncludeCurrentPartial,
		                                                     int currentPage, int pageSize, string sortExpression,
		                                                     string sortDirection);

        int GetAscSoftwareEngineerTotalCount(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial);

        DataSet GetResolutionMetricsExport(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial);
        DataSet ExportAscEngineer(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial);

        List<DropDownModel> GetAscUserList(string partnerName, string theatre);
        List<AscPartner> GetAllAscPartnerList();
	    List<AscPartner> GetGlobalAscPartnerList();
	    DateTime? GetAscDataLastUploadedOn(long ascId);
        void InsertAscData(List<AscDataModel> ascData);

        bool betaAccess(long supportAccountId);

	    void AscReminderEmail();

	    string GetLastRefreshDate();

	    DataSet ExportDeviceInformation(string sfdcId, bool isInternal, string theatre, bool isFiveQuater,
	        string isIncludeCurrentPartial);
	}
}
