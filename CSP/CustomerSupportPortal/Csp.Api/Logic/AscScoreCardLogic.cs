using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Sap.Data.Hana;

namespace Csp.Api.Logic
{

    public static class AscConstants
    {

        #region Five Month Queries

        public const string AscDeviceTheatreLastFiveMonthQuery =
            "SELECT  Theatre,  FISCAL_MONTH As Category, year_month As FiscalCode, " +
            " SUM(DEVICE_COUNT) As DeviceCount, SUM(CUSTOMER_COUNT) As CustomerCount " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_DEVICE_CUSTOMER_LASTFIVEMONTH\" ";

        public const string FrontLineMetricsFiveMonthQuery =
            "SELECT  WORLDWIDE_TAGGED_FRONTLINE As Theatre,'' As PartnerName, FISCAL_MONTH As Category, year_month As FiscalCode, FRONTLINE_CASE_COUNT As CaseCount, " +
            " TOTAL_CASE_COUNT As TotalCount, TO_DOUBLE(PERCENTAGE_TAGGED_FRONTLINE) As PercentageTagged,'' AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_FRONT_LINE_METRICES_LASTFIVEMNTH\" ";

        public const string FrontLineMetricsPartnerFiveMonthQuery =
            "SELECT THEATRE As Theatre,PARTNER_Name As PartnerName, FISCAL_MONTH As Category,year_month As FiscalCode, FRONTLINE_CASE_COUNT As CaseCount, " +
            " TOTAL_CASE_COUNT As TotalCount, TO_DOUBLE(PERCENTAGE_TAGGED_FRONTLINE) As PercentageTagged,FLAGGED_NONCOMPLIANCE_ALERT AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_FRONT_LINE_METRICES_PARTNER_LASTFIVEMNTH\" ";

        public const string FrontLineMetricsDetailFiveMonthQuery = "SELECT THEATRE,GLOBAL_THEATRE,CASENUMBER,PARTNER_NAME,SUPPORT_TYPE,"+
            "CASE_OPEN_DATE,CASE_CLOSE_DATE,REASON,AGE,CUSTOMER_SELECTED_SERIAL_NUMBER,IS_FRONTLINE,CASE_ORIGIN,SUBJECT,"+
            "PRIORITY,REGISTERED_COMPANY,CASE_PRODUCT,VENDOR_CASE_NUMBER,PART_ID,SUPPORTACCOUNTID,CASE_CATEGORY,CONTACT_EMAIL,FISCAL_YEAR,FISCAL_QTR_CODE,SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH " + 
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_FRONT_LINE_METRICES_DETAIL_LASTFIVEMNTH\" ";

        public const string ResolutionMetricsFiveMonthQuery =
            "SELECT  THEATRE As Theatre,'' As PartnerName, FISCAL_MONTH As Category, year_month As FiscalCode, 0 As CaseCount, " +
            " TOTAL_CASES As TotalCount, TO_DOUBLE(RES_RATE) As PercentageTagged,'' AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_RATE_LASTFIVEMONTH\" ";

        public const string ResolutionMetricsPartnerFiveMonthQuery =
            "SELECT THEATRE As Theatre,PARTNER_Name As PartnerName, FISCAL_MONTH As Category,year_month As FiscalCode, 0 As CaseCount, " +
            " TOTAL_CASES As TotalCount, TO_DOUBLE(RES_RATE) As PercentageTagged,non_compliance_flag AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_RATE_PARTNER_LASTFIVEMONTH\" ";

        public const string ResolutionMetricsPartnerFiveMonthExportT1Query =
            "SELECT ASC_ID,ACCOUNT_NAME,THEATRE,GLOBAL_THEATRE,CASE_NUMBER,CASE_CREATED_DATE,CASE_CLOSED_DATE," +
             "FISCAL_QUARTER,SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH,FISCAL_YEAR,END_CUSTOMER_NAME,END_CUSTOMER_EMAIL " +
            " FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_T12_CASE_DETAIL_LASTFIVEMONTH\" ";

        public const string ResolutionMetricsPartnerFiveMonthExportT3Query =
            "SELECT ACCOUNT_NAME,THEATRE,GLOBAL_THEATRE,CASE_NUMBER,CASE_REASON,CASE_CLOSED_DATE,CASE_CREATED_DATE," +
             "FISCAL_QUARTER,SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH,FISCAL_YEAR,CONTACT_NAME,REGISTERED_COMPANY," +
             "CASE_STATUS,ASC_CASE_NUMBER,CASE_ORIGIN,CASE_SUBJECT " +
            " FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_T3_CASE_DETAIL_LASTFIVEMONTH\" ";

        public const string Top3FiveMonthQuery =
        "SELECT  Theatre As Theatre, FISCAL_MONTH As Category, year_month As FiscalCode, " +
        "  To_Double(Top3_Percent) As Value " +
        "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_TOP3_LASTFIVEMONTH\" ";

        public const string CsatResponseFiveMonthQuery =
            "SELECT  Theatre As Theatre, FISCAL_MONTH As Category, year_month As FiscalCode, " +
            " To_Double(Responses) As Value " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_RESPONSE_LASTFIVEMONTH\" ";

        public const string CsatCommentsFiveMonthQuery =
            "SELECT  response_date As ResponseDate, casenumberID As CaseNumber, survey_response As SurveyResponse " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_LOW_SCORE_LASTFIVEMONTH\" ";

        public const string CsatCommentsHighScoreFiveMonthQuery =
           "SELECT  response_date As ResponseDate, casenumberID As CaseNumber, survey_response As SurveyResponse " +
           "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_HIGH_SCORE_LASTFIVEMONTH\" ";

        public const string CsatCommentsExportFiveMonthQuery =
            "SELECT  CAST(response_date As NVARCHAR)  Response_Date, CaseNumberID As Case_Number,NAME AS Account_Name ,Theatre,FISCAL_YEAR,FISCAL_QTR_CODE," +
            "SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH, Survey_Response,GlobalTheatre  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_LOW_SCORE_LASTFIVEMONTH\" ";

        public const string CsatCommentsHighScoreExportFiveMonthQuery =
            "SELECT  CAST(response_date As NVARCHAR)  Response_Date, CaseNumberID As Case_Number,NAME AS Account_Name ,Theatre,FISCAL_YEAR,FISCAL_QTR_CODE," +
            "SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH, Survey_Response,GlobalTheatre  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_HIGH_SCORE_LASTFIVEMONTH\" ";

        public const string SupportEngineerFiveMonthQuery =
            "SELECT  Account_Name As AccountName,Contact_Name As Name,Contact_Email As Email,Theatre, " +
            " Total_Case_Count As TotalCaseCount, Frontline_Case_Count As FrontlineCaseCount ,To_Double(Percentage_Tagged_Frontline)  As PercentFrontline, " +
            " PCNSE As Pcnse, PCNSA As Pcnsa, ACE As Ace, GlobalTheatre " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_SUPPORT_ENGINEERS_LASTFIVEMNTH\" ";

        public const string SupportEngineerFiveMonthTotalCountQuery =
            "SELECT  Count(*) " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_SUPPORT_ENGINEERS_LASTFIVEMNTH\" ";

        public const string SupportEngineerFiveMonthSp =
        "CALL \"SFDC\".\"SP_GCSP_SDFC_SUPPORT_ENGINEERS_PERPAGE_LASTFIVEMNTH\" ( '{0}' ,'{1}' ,'{2}','{3}',{4}, {5} ,'{6}','{7}' )";

        #endregion


        //public const string MetricsFilter = " WITH PARAMETERS (" +
        //                                                                     "'PLACEHOLDER' = ('$$IP_ACCOUNT_ID$$','{0}') , " +
        //                                                                     "'PLACEHOLDER' = ('$$IP_INTERNAL$$','{1}')," +
        //                                                                     "'PLACEHOLDER' = ('$$IP_THEATRE$$','{2}'))";


        public const string MetricsFilter = " WITH PARAMETERS (" +
                                                                            "'PLACEHOLDER' = ('$$IP_ACCOUNT_ID$$','{0}') , " +
                                                                            "'PLACEHOLDER' = ('$$IP_INTERNAL$$','{1}')," +
                                                                            "'PLACEHOLDER' = ('$$IP_THEATRE$$','{2}'), 'PLACEHOLDER' = ('$$IP_CURRENT_PERIOD$$','{3}') )";

        public const string DeviceFilter = " (" +
                                                                            "'PLACEHOLDER' = ('$$IP_ACCOUNT_ID$$','{0}') , " +
                                                                            "'PLACEHOLDER' = ('$$IP_INTERNAL$$','{1}')," +
                                                                            "'PLACEHOLDER' = ('$$IP_THEATRE$$','{2}'), 'PLACEHOLDER' = ('$$IP_CURRENT_PERIOD$$','{3}') )";

        
        public const string CsatFilter = " WITH PARAMETERS (" + "'PLACEHOLDER' = ('$$IP_ACCOUNT_ID$$','{0}'),'PLACEHOLDER' = ('$$IP_CURRENT_PERIOD$$','{1}') ) ";



        public const string PartnerProfileFilter = " WITH PARAMETERS (" + "'PLACEHOLDER' = ('$$ACCOUNT_ID$$','{0}') ) ";

        #region Five Quaters Queries

        public const string AscDeviceTheatreLastFiveQuaterQuery =
            "SELECT Theatre As Theatre, FISCAL_QTR_CODE As Category,  FISCAL_QTR_CODE As FiscalCode, " +
            " SUM(DEVICE_COUNT) As DeviceCount, SUM(CUSTOMER_COUNT) As CustomerCount " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_DEVICE_CUSTOMER_LASTFIVEQTR\" ";


        public const string FrontLineMetricsFiveQuaterQuery =
            "SELECT  WORLDWIDE_TAGGED_FRONTLINE As Theatre,'' As PartnerName, FISCAL_QTR_CODE As Category,FISCAL_QTR_CODE As FiscalCode, FRONTLINE_CASE_COUNT As CaseCount, " +
            " TOTAL_CASE_COUNT As TotalCount, TO_DOUBLE(PERCENTAGE_TAGGED_FRONTLINE) As PercentageTagged,'' AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_FRONT_LINE_METRICES_LASTFIVEQTR\" ";


        public const string FrontLineMetricsPartnerFiveQuaterQuery =
            "SELECT THEATRE As Theatre,PARTNER_Name As PartnerName, FISCAL_QTR_CODE As Category,FISCAL_QTR_CODE As FiscalCode, FRONTLINE_CASE_COUNT As CaseCount, " +
            " TOTAL_CASE_COUNT As TotalCount, TO_DOUBLE(PERCENTAGE_TAGGED_FRONTLINE) As PercentageTagged,FLAGGED_NONCOMPLIANCE_ALERT AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_FRONT_LINE_METRICES_PARTNER_LASTFIVEQTR\" ";


        public const string FrontLineMetricsDetailFiveQuaterQuery = "SELECT  THEATRE,GLOBAL_THEATRE,CASENUMBER,PARTNER_NAME,SUPPORT_TYPE,"+
            "CASE_OPEN_DATE,CASE_CLOSE_DATE,REASON,AGE,CUSTOMER_SELECTED_SERIAL_NUMBER,IS_FRONTLINE,CASE_ORIGIN,SUBJECT,"+
            "PRIORITY,REGISTERED_COMPANY,CASE_PRODUCT,VENDOR_CASE_NUMBER,PART_ID,SUPPORTACCOUNTID,CASE_CATEGORY,CONTACT_EMAIL,FISCAL_YEAR,FISCAL_QTR_CODE,SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH " +
                     "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_FRONT_LINE_METRICES_DETAIL_LASTFIVEQTR\" ";

        public const string ResolutionMetricsFiveQuaterQuery =
            "SELECT  Theatre As Theatre,'' As PartnerName, FISCAL_QTR_CODE As Category, FISCAL_QTR_CODE As FiscalCode, 0 As CaseCount, " +
            " TOTAL_CASES As TotalCount, TO_DOUBLE(RES_RATE) As PercentageTagged,'' AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_RATE_LASTFIVEQTR\" ";


        public const string ResolutionMetricsPartnerFiveQuaterQuery =
            "SELECT THEATRE As Theatre,PARTNER_Name As PartnerName, FISCAL_QTR_CODE As Category, FISCAL_QTR_CODE As FiscalCode, 0 As CaseCount, " +
            " TOTAL_CASES As TotalCount, TO_DOUBLE(RES_RATE) As PercentageTagged,Non_Compliance_Flag AS NonComplianceAlert " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_RATE_PARTNER_LASTFIVEQTR\" ";


        public const string ResolutionMetricsPartnerFiveQuaterExportT1Query =
            "SELECT ASC_ID,ACCOUNT_NAME,THEATRE,GLOBAL_THEATRE,CASE_NUMBER,CASE_CREATED_DATE,CASE_CLOSED_DATE,"+
             "FISCAL_QUARTER,SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH,FISCAL_YEAR,END_CUSTOMER_NAME,END_CUSTOMER_EMAIL " +
            " FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_T12_CASE_DETAIL_LASTFIVEQTR\" ";

        public const string ResolutionMetricsPartnerFiveQuaterExportT3Query =
            "SELECT ACCOUNT_NAME,THEATRE,GLOBAL_THEATRE,CASE_NUMBER,CASE_REASON,CASE_CLOSED_DATE,CASE_CREATED_DATE,"+
             "FISCAL_QUARTER,SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH,FISCAL_YEAR,CONTACT_NAME,REGISTERED_COMPANY,"+
             "CASE_STATUS,ASC_CASE_NUMBER,CASE_ORIGIN,CASE_SUBJECT " +
            " FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_RESOLUTION_T3_CASE_DETAIL_LASTFIVEQTR\" ";

        public const string Top3FiveQuaterQuery =
            "SELECT  Theatre As Theatre, FISCAL_QTR_CODE As Category, FISCAL_QTR_CODE As FiscalCode, " +
            "  To_Double(Top3_Percent) As Value " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_TOP3_LASTFIVEQTR\" ";

        public const string CsatResponseFiveQuaterQuery =
            "SELECT  Theatre As Theatre, FISCAL_QTR_CODE As Category, FISCAL_QTR_CODE As FiscalCode, " +
            " To_Double(Responses) As Value " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_RESPONSE_LASTFIVEQTR\" ";

        public const string CsatCommentsFiveQuaterQuery =
            "SELECT  response_date As ResponseDate, casenumberID As CaseNumber, survey_response As SurveyResponse " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_LOW_SCORE_LASTFIVEQTR\" ";

        public const string CsatCommentsHighScoreFiveQuaterQuery =
            "SELECT  response_date As ResponseDate, casenumberID As CaseNumber, survey_response As SurveyResponse " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_HIGH_SCORE_LASTFIVEQTR\" ";

        public const string CsatCommentsExportFiveQuaterQuery =
            "SELECT  CAST(response_date As NVARCHAR)  Response_Date, CaseNumberID As Case_Number,NAME AS Account_Name ,Theatre,FISCAL_YEAR,FISCAL_QTR_CODE, " +
            "SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH, " +
            "Survey_Response,GlobalTheatre  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_LOW_SCORE_LASTFIVEQTR\" ";

        public const string CsatCommentsHighScoreExportFiveQuaterQuery =
            "SELECT  CAST(response_date As NVARCHAR)  Response_Date, CaseNumberID As Case_Number,NAME AS Account_Name ,Theatre,FISCAL_YEAR,FISCAL_QTR_CODE," +
            "SUBSTRING (FISCAL_MONTH,6,9) As FISCAL_MONTH, Survey_Response,GlobalTheatre  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_CSAT_HIGH_SCORE_LASTFIVEQTR\" ";

        public const string SupportEngineerFiveQuaterQuery =
            "SELECT  Account_Name As AccountName ,Contact_Name As Name,Contact_Email As Email,Theatre, " +
            " Total_Case_Count As TotalCaseCount, Frontline_Case_Count As FrontlineCaseCount ,To_Double(Percentage_Tagged_Frontline)  As PercentFrontline, " +
            " PCNSE As Pcnse, PCNSA As Pcnsa, ACE As Ace, GlobalTheatre " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_SUPPORT_ENGINEERS_LASTFIVEQTR\" ";


        public const string SupportEngineerFiveQuaterTotalCountQuery =
            "SELECT  Count(*) " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_SUPPORT_ENGINEERS_LASTFIVEQTR\" ";

        public const string SupportEngineerFiveQuaterSp =
            "CALL \"SFDC\".\"SP_GCSP_SDFC_SUPPORT_ENGINEERS_PERPAGE_LASTFIVEQTR\" ( '{0}' ,'{1}' ,'{2}','{3}',{4}, {5} ,'{6}','{7}' )";


        #endregion
        public const string DeviceInformationExportQuery =
            "SELECT  ACCOUNTNAME,THEATRE,GLOBALTHEATRE,SERIALNUMBER,AUTHORIZATIONCODE,PARTID,REGISTRATIONDATE,EXPIRATIONDATE,ENDCUSTOMERNAME  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_DEVICE_DETAIL_CURRENT\" ";

        public const string DeviceInformationInternalExportQuery =
            "SELECT  *  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_DEVICE_DETAIL_CURRENT\" ";

        public const string PartnerProfileQuery =
            "SELECT Partner_Name AS PartnerName,RESELLER_TYPE As Type, PARTNER_LEVEL As PartnerLevel, CC_ELITE As Elite," +
            " CC_THEATRE As Theatre, GLOBAL_PARTNER As GlobalPartner, TO_DOUBLE(BOOKING_AMOUNT) As BookingAmount " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH/CV_SFDC_ASC_PARTNERS\" WHERE ACCOUNT_ID = '{0}'";

        public const string FrontLineCurrentQuaterQuery =
            "SELECT  Frontline_Alert As Alert,TO_Double(FRONTLINE_Actual) As Actual, " +
            " TO_DOUBLE(FRONTLINE_Target) As Target,TO_DOUBLE(FRONTLINE_Target) As Target, TO_DOUBLE(Target_Begin) As TargetStart ,TO_DOUBLE(Target_End) As TargetEnd  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_COMPLIANCE_FRONTLINE_CURRQTR\" ";

        public const string ResolutionRateCurrentQuaterQuery =
            "SELECT  Non_Compliance_Flag As Alert,TO_Double(res_rate) As Actual, " +
            " TO_DOUBLE(Target) As Target, TO_DOUBLE(Target_Begin) As TargetStart ,TO_DOUBLE(Target_End) As TargetEnd  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_COMPLIANCE_RESOLUTION_RATE_CURRQTR\" ";

        public const string CustomerSatCurrentQuaterQuery =
            "SELECT  Non_Compliance_Flag As Alert,TO_Double(top3_percent) As Actual, " +
            " TO_DOUBLE(Target) As Target, TO_DOUBLE(Target_Begin) As TargetStart ,TO_DOUBLE(Target_End) As TargetEnd  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_COMPLIANCE_CSAT_TOP3_CURRQTR\" ";

        public const string AscUserListQuery =
            "SELECT  account_id As AccountId, partner_name As PartnerName,CC_THEATRE As Theatre  " +
            " FROM \"_SYS_BIC\".\"GCSP.ITECH/CV_SFDC_ASC_PARTNERS\" WHERE UPPER(ASC__C) != 'NO' ";

        public const string GlobalAscUserListQuery =
            "SELECT  account_id As AccountId, partner_name As PartnerName,CC_THEATRE As Theatre  " +
            " FROM \"_SYS_BIC\".\"GCSP.ITECH/CV_SFDC_ASC_PARTNERS\" WHERE UPPER(GLOBAL_PARTNER) = 'YES' ";

        public const string LastRefreshDateQuery =
            "SELECT REFRESH_DATE As RefreshDate  " +
            "FROM \"_SYS_BIC\".\"GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_LAST_REFRESH\" ";

        public const string AscReminderEmailQuery =
           "SELECT  email As AscSupportEmail " +
           " FROM \"_SYS_BIC\".\"GCSP.ITECH/CV_SFDC_ASC_EMAIL_REMINDER\" WHERE Active = 'Y' ";

        public const string ResolutionRateCurrentMonthQuery = @"SELECT Non_Compliance_Flag As Alert,TO_Double(res_rate) As Actual,
            TO_DOUBLE(Target) As Target, TO_DOUBLE(Target_Begin) As TargetStart, TO_DOUBLE(Target_End) As TargetEnd 
            FROM ""_SYS_BIC"".""GCSP.ITECH.ASC_SELFSERVICE/CV_GCSP_COMPLIANCE_RESOLUTION_RATE_CURR_MONTH"" ";

        #region Theatre
        public const string WorldWideTheatre = "World Wide Average";
        public const string AmericasTheatre = "Americas";
        public const string EmeaTheatre = "EMEA";
        public const string JapanTheatre = "Japan";
        public const string ApacTheatre = "APAC";
        public const string GlobalServicesTheatre = "Global Services Partners";

        public const string Partner = "Partner";
        public const string Target = "TARGET";

        public static List<ConstantList> TheatreList { get; } = new List<ConstantList>()
        {
            new ConstantList() {Id=1,Value = WorldWideTheatre},
            new ConstantList(){Id=2,Value = AmericasTheatre},
            new ConstantList(){Id=3,Value = EmeaTheatre},
            new ConstantList(){Id=4,Value = JapanTheatre},
            new ConstantList(){Id=5,Value = ApacTheatre},
            new ConstantList(){Id=6,Value =GlobalServicesTheatre},
        };

        #endregion
    }

    public class AscScoreCardLogic : IAscScoreCardLogic
    {
        private readonly IAscScoreCardRepository _ascScoreCardRepository;
        private static Database _dataBase = DatabaseFactory.CreateDatabase("SapHanaDB");

        private static readonly HanaConnection connection =
            new HanaConnection(Secrets.SapHanaDb);

        public AscScoreCardLogic(IAscScoreCardRepository ascScoreCardRepository)
        {
            _ascScoreCardRepository = ascScoreCardRepository;
        }

        private static IList<T> RunSapQuery<T>(string query)
        {

            var t1 = DateTime.Now;
            Trace.TraceInformation("Query Started; type :" + typeof(T) + DateTime.Now);

            using (
                HanaConnection connection =
                    new HanaConnection(Secrets.SapHanaDb))
            {
                using (HanaCommand cmd = new HanaCommand(query, connection))
                {
                    connection.Open();
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        Mapper.CreateMap<IDataReader, T>();
                        var item = Mapper.Map<IDataReader, IList<T>>(dr);

                        var t2 = DateTime.Now;

                        Trace.TraceInformation("Query Completed; type :" + typeof(T));

                        return item ?? new List<T>();
                    }
                }
            }
        }


        private static DataTable ExecuteDataTable(string query)
        {
            DataTable dataTable = new DataTable();
            using (HanaCommand command = new HanaCommand(query, connection))
            {
                using (HanaDataAdapter adapter = new HanaDataAdapter(command))
                {

                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }

        public List<DropDownModel> GetAscUserList(string partnerName, string theatre)
        {

            // return _ascScoreCardRepository.GetAscUserList(firstname);

            var query = AscConstants.AscUserListQuery;

            if (!string.IsNullOrEmpty(partnerName) &&
                (partnerName.ToUpper().Equals("ALL ASC") || partnerName.ToUpper().Equals("ALL")))
            {
                partnerName = string.Empty;
            }


            var parameters = string.IsNullOrEmpty(partnerName) && theatre.Trim().ToUpper() != "ALL"
                ? " WHERE "
                : " WHERE UPPER(partner_name) LIKE '" + partnerName?.ToUpper() + "%' ";
            if (!string.IsNullOrEmpty(theatre) && theatre.ToUpper() != "ALL" && !string.IsNullOrEmpty(partnerName))
            {
                parameters = parameters + " AND CC_THEATRE = '" + theatre + "'";
            }
            else if (!string.IsNullOrEmpty(theatre) && theatre.Trim().ToUpper() != "ALL")
            {
                parameters = parameters + "  UPPER(CC_THEATRE) = '" + theatre.Trim().ToUpper() + "'";
            }

            var ascList = RunSapQuery<DropDownModel>(query + parameters).ToList();


            return ascList;
        }

        public DateTime? GetAscDataLastUploadedOn(long ascId)
        {
            return _ascScoreCardRepository.GetAscDataLastUploadedOn(ascId);
        }

        public void InsertAscData(List<AscDataModel> ascData)
        {
            _ascScoreCardRepository.InsertAscData(ascData);
        }

        public bool betaAccess(long supportAccountId)
        {
            return _ascScoreCardRepository.betaAccess(supportAccountId);
        }

        public List<AscPartner> GetAllAscPartnerList()
        {
            var query = AscConstants.AscUserListQuery;
            return RunSapQuery<AscPartner>(query).ToList();
        }

        public List<AscPartner> GetGlobalAscPartnerList()
        {
            var query = AscConstants.GlobalAscUserListQuery;
            return RunSapQuery<AscPartner>(query).ToList();
        }


        public DataSet GetFrontLineMetricsDetail(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);
            string query = string.Empty;
            if (isFiveQuater)
            {
                query = AscConstants.FrontLineMetricsDetailFiveQuaterQuery;
            }
            else
            {
                query = AscConstants.FrontLineMetricsDetailFiveMonthQuery;
            }
            DataTable dt = ExecuteDataTable(query + queryFilter);
            DataSet ds = null;
            if (dt != null)
            {
                ds = new DataSet();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public DataSet GetCustomerComments(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);

            string query = string.Empty;
            if (isFiveQuater)
            {
                query = AscConstants.CsatCommentsExportFiveQuaterQuery;
            }
            else
            {
                query = AscConstants.CsatCommentsExportFiveMonthQuery;
            }
            DataTable dt = ExecuteDataTable(query + queryFilter);
            DataSet ds = null;
            if (dt != null)
            {
                ds = new DataSet();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public DataSet GetHighScoreCustomerComments(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);

            string query = string.Empty;
            if (isFiveQuater)
            {
                query = AscConstants.CsatCommentsHighScoreExportFiveQuaterQuery;
            }
            else
            {
                query = AscConstants.CsatCommentsHighScoreExportFiveMonthQuery;
            }
            DataTable dt = ExecuteDataTable(query + queryFilter);
            DataSet ds = null;
            if (dt != null)
            {
                ds = new DataSet();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public DataSet ExportDeviceInformation(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);

            string query = AscConstants.DeviceInformationExportQuery;
            if (isInternal)
            {
                query = AscConstants.DeviceInformationInternalExportQuery;
            }
            
            DataTable dt = ExecuteDataTable(query + queryFilter);
            DataSet ds = null;
            if (dt != null)
            {
                ds = new DataSet();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public DataSet ExportAscEngineer(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);
            string query = string.Empty;
            if (isFiveQuater)
            {
                query = AscConstants.SupportEngineerFiveQuaterQuery;
            }
            else
            {
                query = AscConstants.SupportEngineerFiveMonthQuery;
            }
            DataTable dt = ExecuteDataTable(query + queryFilter);
            DataSet ds = null;
            if (dt != null)
            {
                ds = new DataSet();
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public DataSet GetResolutionMetricsExport(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);
            var query1 = string.Empty;
            var query2 = string.Empty;
            if (isFiveQuater)
            {
                query1 = AscConstants.ResolutionMetricsPartnerFiveQuaterExportT1Query;
                query2 = AscConstants.ResolutionMetricsPartnerFiveQuaterExportT3Query;
            }
            else
            {
                query1 = AscConstants.ResolutionMetricsPartnerFiveMonthExportT1Query;
                query2 = AscConstants.ResolutionMetricsPartnerFiveMonthExportT3Query;
            }
            DataTable dt1 = ExecuteDataTable(query1 + queryFilter);
            DataTable dt2 = ExecuteDataTable(query2 + queryFilter);
            DataSet ds = new DataSet();
            if (dt1 != null)
            {
                dt1.TableName = "ASC Case Data";
                ds.Tables.Add(dt1);
            }
            if (dt2 != null)
            {
                dt2.TableName = "PANW Case Data";
                ds.Tables.Add(dt2);
            }

            return ds;
        }

        public List<SoftwareEngineer> GetAscSoftwareEngineer(string sfdcId, bool isInternal, string theatre,
            bool isFiveQuater,string isIncludeCurrentPartial, int currentPage, int pageSize, string sortExpression, string sortDirection)
        {

            string query = string.Empty;
            if (isFiveQuater)
            {
                query = AscConstants.SupportEngineerFiveQuaterSp;
            }
            else
            {
                query = AscConstants.SupportEngineerFiveMonthSp;
            }
            DataTable dt =
                ExecuteDataTable(string.Format(query, isInternal ? 1 : 0, isInternal ? theatre : "ALL", sfdcId,isIncludeCurrentPartial,
                    currentPage, pageSize, sortExpression, sortDirection));
            List<SoftwareEngineer> lstSoftwareEngg = null;
            if (dt != null)
            {
                lstSoftwareEngg = (from DataRow dr in dt.Rows
                    select new SoftwareEngineer
                    {
                        Contact_Name = dr["Contact_Name"] == DBNull.Value ? string.Empty : dr["Contact_Name"].ToString(),
                        Contact_Email =
                            dr["Contact_Email"] == DBNull.Value ? string.Empty : dr["Contact_Email"].ToString(),
                        Total_Case_Count =
                            dr["Total_Case_Count"] != DBNull.Value ? Convert.ToInt32(dr["Total_Case_Count"]) : 0,
                        Frontline_Case_Count =
                            dr["Frontline_Case_Count"] != DBNull.Value ? Convert.ToInt32(dr["Frontline_Case_Count"]) : 0,
                        Percentage_Tagged_Frontline =
                            dr["Percentage_Tagged_Frontline"] != DBNull.Value
                                ? Convert.ToDouble(dr["Percentage_Tagged_Frontline"])
                                : 0,
                        Pcnse = dr["PCNSE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["PCNSE"]) : null,
                        Ace = dr["Ace"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["Ace"]) : null,
                        Pcnsa = dr["PCNSA"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["PCNSA"]) : null,
                        //Ace = dr["Ace"] != DBNull.Value ? Convert.ToDouble(dr["Ace"]) : 0,
                        NonCompliant = dr["ALERT_FLAG"] != DBNull.Value && Convert.ToBoolean(dr["ALERT_FLAG"])
                    }).ToList();
            }

            return lstSoftwareEngg;
        }

        public int GetAscSoftwareEngineerTotalCount(string sfdcId, bool isInternal, string theatre, bool isFiveQuater, string isIncludeCurrentPartial)
        {
            var queryFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", isIncludeCurrentPartial);
            var query = string.Empty;
            if (isFiveQuater)
            {
                query = AscConstants.SupportEngineerFiveQuaterTotalCountQuery;
            }
            else
            {
                query = AscConstants.SupportEngineerFiveMonthTotalCountQuery;
            }

            DataTable dt = ExecuteDataTable(query + queryFilter);
            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
            {
                return Convert.ToInt32(dt.Rows[0][0]);
            }
            return 0;
        }

        public string GetLastRefreshDate()
        {
            var ascScorecardObject = new AscScoreCard();

            var lastRefreshDateObj =
                RunSapQuery<LastRefreshDate>(AscConstants.LastRefreshDateQuery).FirstOrDefault();

            if (lastRefreshDateObj != null)
            {
                return String.Format("{0:MMM d, yyyy h:mm tt}", lastRefreshDateObj.RefreshDate);  
            }

            return null;
        }

        public AscScoreCard GetAscScoreCardQtrData(string sfdcId, bool isInternal, string theatre, string includeCurrentPartial)
        {
            var ascQtrFilter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", includeCurrentPartial);

            var ascScorecardObject = new AscScoreCard();
            try
            {

                Parallel.Invoke(
                    () =>
                    {
                        string[] sfdcAccountIdList = sfdcId.Split(',');
                        if (sfdcAccountIdList.Length > 1)
                        {
                            ascScorecardObject.PartnerProfile = new PartnerProfile
                            {
                                Type = string.Empty,
                                PartnerLevel = string.Empty,
                                Elite = string.Empty,
                                Theatre = string.Empty
                            };
                            ascScorecardObject.HasPartnerProfile = false;
                            ascScorecardObject.MultipleAsc = true;
                        }
                        else
                        {
                            var partnerProfileQuery = string.Format(AscConstants.PartnerProfileQuery, sfdcAccountIdList[0].Replace("'",""));
                            ascScorecardObject.PartnerProfile =
                                RunSapQuery<PartnerProfile>(partnerProfileQuery).FirstOrDefault();

                            if (ascScorecardObject.PartnerProfile != null)
                            {
                                ascScorecardObject.HasPartnerProfile = true;
                            }
                            
                            ascScorecardObject.MultipleAsc = false;
                        }
                    },

                    () =>
                    {

                        var deviceCustomerFilter = string.Format(AscConstants.DeviceFilter, sfdcId, isInternal ? 1 : 0,
                            "ALL", includeCurrentPartial);
                        
                        ascScorecardObject.AscDeviceTheatrePartnerChart =
                            RunSapQuery<AscDeviceTheatrePartner>(AscConstants.AscDeviceTheatreLastFiveQuaterQuery +
                                                                 deviceCustomerFilter + " GROUP BY THEATRE, FISCAL_QTR_CODE")
                                .OrderBy(t => t.FiscalCode)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.FrontlineMetrics =
                            RunSapQuery<MetricsData>(AscConstants.FrontLineMetricsFiveQuaterQuery + ascQtrFilter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.FrontlineMetricsPartner =
                            RunSapQuery<MetricsData>(AscConstants.FrontLineMetricsPartnerFiveQuaterQuery + ascQtrFilter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.ResolutionMetrics =
                            RunSapQuery<MetricsData>(AscConstants.ResolutionMetricsFiveQuaterQuery + ascQtrFilter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.ResolutionMetricsPartner =
                            RunSapQuery<MetricsData>(AscConstants.ResolutionMetricsPartnerFiveQuaterQuery + ascQtrFilter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.CsatTop3 =
                            RunSapQuery<PercentLineChartData>(AscConstants.Top3FiveQuaterQuery + ascQtrFilter).ToList();
                    },
                    () =>
                    {
                        var csatFilter = string.Format(AscConstants.CsatFilter, sfdcId, includeCurrentPartial);
                        ascScorecardObject.CsatResponses =
                            RunSapQuery<PercentLineChartData>(AscConstants.CsatResponseFiveQuaterQuery + csatFilter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.Comments =
                            RunSapQuery<Comments>(AscConstants.CsatCommentsFiveQuaterQuery + ascQtrFilter).ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.CommentsResultFromQuest =
                            RunSapQuery<Comments>(AscConstants.CsatCommentsHighScoreFiveQuaterQuery + ascQtrFilter).ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.FrontlineComplianceCurrentQuater =
                            RunSapQuery<ComplianceCurrentQtr>(AscConstants.FrontLineCurrentQuaterQuery + ascQtrFilter)
                                .FirstOrDefault() ?? new ComplianceCurrentQtr();
                    },
                    () =>
                    {
                        ascScorecardObject.ResolutionRateComplianceCurrentQuater =
                            RunSapQuery<ComplianceCurrentQtr>(AscConstants.ResolutionRateCurrentQuaterQuery +
                                                              ascQtrFilter).FirstOrDefault() ?? new ComplianceCurrentQtr();
                    },
                    () =>
                    {
                        ascScorecardObject.CustomerSatComplianceCurrentQuater =
                            RunSapQuery<ComplianceCurrentQtr>(AscConstants.CustomerSatCurrentQuaterQuery + ascQtrFilter)
                                .FirstOrDefault() ?? new ComplianceCurrentQtr();
                    }
                    );

                bool isInternalView = !ascScorecardObject.HasPartnerProfile ;
                MassageAscData(ascScorecardObject, isInternalView,false);
                if (isInternalView && !ascScorecardObject.MultipleAsc)
                {
                    //Device & Customer Information Massage
                    MassageInternalData(ascScorecardObject);
                }

                
            }
            catch (AggregateException ae)
            {
                throw new Exception(string.Join(Environment.NewLine, ae.InnerExceptions.Select(e => e.Message)));
            }
            return ascScorecardObject;

        }

        private static void MassageAscData(AscScoreCard ascScorecardObject, bool isInternal,bool isLastFiveMonth)
        {
            var theatreList = AscConstants.TheatreList.Select(c => c.Value).ToList();
            if (ascScorecardObject.FrontlineMetrics != null && ascScorecardObject.FrontlineMetrics.Count > 0)
            {
                var frontlineCategoryList = (from d in ascScorecardObject.ResolutionMetrics
                                    select new { Category = d.Category, FiscalCode = d.FiscalCode }).Distinct().ToList();

                var category = frontlineCategoryList.OrderBy(t => t.FiscalCode).Select(q => q.Category).ToList();

                string theatreText = string.Empty;
                string partnerTextForChart = string.Empty;
                List<MetricsChart> listFrontlineChart = null;
                List<MultipleAscChart> listMultipleAsc = null;
                if (ascScorecardObject.MultipleAsc){
                    theatreText = "World Wide Average";
                    partnerTextForChart = "Total";
                    listMultipleAsc=new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart=null;
                    foreach (var fiscalQtrCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = fiscalQtrCode,
                            TargetCount =
                                ascScorecardObject.FrontlineComplianceCurrentQuater == null
                                    ? 0
                                    : ascScorecardObject.FrontlineComplianceCurrentQuater.Target,

                            WorldWideAverage = ascScorecardObject.FrontlineMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.Equals(AscConstants.WorldWideTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),

                            TotalCount = ascScorecardObject.FrontlineMetricsPartner.Where(
                                            f =>
                                                f.Category.Equals(fiscalQtrCode) &&
                                                f.PartnerName.ToUpper().Equals("TOTAL"))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault()
                        };
                        listMultipleAsc.Add(objMultipleAscChart);
                    }
                }
                else if (ascScorecardObject.HasPartnerProfile){
                    theatreText = ascScorecardObject.PartnerProfile.Theatre;
                    partnerTextForChart = "Partner";
                    listMultipleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var fiscalQtrCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = fiscalQtrCode,
                            TargetCount =
                                ascScorecardObject.FrontlineComplianceCurrentQuater == null
                                    ? 0
                                    : ascScorecardObject.FrontlineComplianceCurrentQuater.Target,

                            WorldWideAverage = ascScorecardObject.FrontlineMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.ToUpper().Equals(ascScorecardObject.PartnerProfile.Theatre.ToUpper()))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),

                            TotalCount = ascScorecardObject.FrontlineMetricsPartner.Where(
                                            f => f.Category.Equals(fiscalQtrCode))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault()
                        };
                        listMultipleAsc.Add(objMultipleAscChart);
                    }
                }
                else{
                    listFrontlineChart = new List<MetricsChart>();
                    MetricsChart objFrontlineMetricsChart = null;
                    foreach (var fiscalQtrCode in category)
                    {
                        objFrontlineMetricsChart = new MetricsChart()
                        {
                            Category = fiscalQtrCode,
                            TargetCount =
                                ascScorecardObject.FrontlineComplianceCurrentQuater == null
                                    ? 0
                                    : ascScorecardObject.FrontlineComplianceCurrentQuater.Target,
                            //	WorldwideTotalCount = ascScorecardObject.FrontlineMetrics.Where(f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.WorldWideTheatre)).Select(f => f.PercentageTagged).FirstOrDefault(),
                            AmericasTotalCount =
                                ascScorecardObject.FrontlineMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.Equals(AscConstants.AmericasTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            EmeaTotalCount =
                                ascScorecardObject.FrontlineMetrics.Where(
                                    f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.EmeaTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            JapanTotalCount =
                                ascScorecardObject.FrontlineMetrics.Where(
                                    f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.JapanTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            ApacTotalCount =
                                ascScorecardObject.FrontlineMetrics.Where(
                                    f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.ApacTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            GLobalServicesTotalCount =
                                ascScorecardObject.FrontlineMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.Equals(AscConstants.GlobalServicesTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            PartnerTotalCount = (isInternal && !ascScorecardObject.MultipleAsc)
                                ? 0
                                : ascScorecardObject.FrontlineMetricsPartner.Count > 0
                                    ? ascScorecardObject.MultipleAsc
                                        ? (ascScorecardObject.FrontlineMetricsPartner.Where(
                                            f =>
                                                f.Category.Equals(fiscalQtrCode) &&
                                                f.PartnerName.ToUpper().Equals("TOTAL"))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault())
                                        : (ascScorecardObject.FrontlineMetricsPartner.Where(
                                            f => f.Category.Equals(fiscalQtrCode))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault())
                                    : 0
                        };
                        listFrontlineChart.Add(objFrontlineMetricsChart);

                    }
                }
               

                var partnerList =
                    ascScorecardObject.FrontlineMetricsPartner.Select(q => q.PartnerName).Distinct().ToList();

                ascScorecardObject.FrontlineMetricsConsolidated = new MetricsConsolidated
                {
                    Theatre = theatreList,
                    Category = category.ToList(),
                    PartnerList = partnerList,
                    MetricsList = ascScorecardObject.FrontlineMetrics,
                    MetricsPartner = ascScorecardObject.FrontlineMetricsPartner,
                    LineChart = listFrontlineChart,
                    TheatreTextForChart = theatreText,
                    PartnerTextForChart = partnerTextForChart,
                    MultipleAscLineChart = listMultipleAsc
                };
            }

            if (ascScorecardObject.ResolutionMetrics != null && ascScorecardObject.ResolutionMetrics.Count > 0)
            {

                var resolutionCategoryList = (from d in ascScorecardObject.ResolutionMetrics
                    select new {Category = d.Category, FiscalCode = d.FiscalCode}).Distinct().ToList();

                var category = resolutionCategoryList.OrderBy(t => t.FiscalCode).Select(q => q.Category).ToList();

                string theatreText = string.Empty;
                string partnerTextForChart = string.Empty;
                List<MetricsChart> listResolutionChart = null;
                List<MultipleAscChart> listMultipleAsc = null;
                if (ascScorecardObject.MultipleAsc){
                    theatreText = "World Wide Average";
                    partnerTextForChart = "Total";
                    listMultipleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var fiscalQtrCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = fiscalQtrCode,
                            TargetCount =
                                ascScorecardObject.ResolutionRateComplianceCurrentQuater == null
                                    ? 0
                                    : ascScorecardObject.ResolutionRateComplianceCurrentQuater.Target,

                            WorldWideAverage = ascScorecardObject.ResolutionMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.Equals(AscConstants.WorldWideTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),

                            TotalCount = ascScorecardObject.ResolutionMetricsPartner.Where(
                                            f =>
                                                f.Category.Equals(fiscalQtrCode) &&
                                                f.PartnerName.ToUpper().Equals("TOTAL"))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault()
                        };
                        listMultipleAsc.Add(objMultipleAscChart);
                    }
                }
                else if (ascScorecardObject.HasPartnerProfile){
                    theatreText = ascScorecardObject.PartnerProfile.Theatre;
                    partnerTextForChart = "Partner";
                    listMultipleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var fiscalQtrCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = fiscalQtrCode,
                            TargetCount =
                                ascScorecardObject.ResolutionRateComplianceCurrentQuater == null
                                    ? 0
                                    : ascScorecardObject.ResolutionRateComplianceCurrentQuater.Target,

                            WorldWideAverage = ascScorecardObject.ResolutionMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.ToUpper().Equals(ascScorecardObject.PartnerProfile.Theatre.ToUpper()))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),

                            TotalCount = ascScorecardObject.ResolutionMetricsPartner.Where(
                                            f => f.Category.Equals(fiscalQtrCode))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault()
                        };
                        listMultipleAsc.Add(objMultipleAscChart);
                    }
                }
                else{
                    listResolutionChart = new List<MetricsChart>();
                    MetricsChart objResolutionMetricsChart = null;
                    foreach (var fiscalQtrCode in category)
                    {
                        objResolutionMetricsChart = new MetricsChart()
                        {
                            Category = fiscalQtrCode,
                            //	WorldwideTotalCount = ascScorecardObject.ResolutionMetrics.Where(f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.WorldWideTheatre)).Select(f => f.PercentageTagged).FirstOrDefault(),
                            TargetCount =
                                ascScorecardObject.ResolutionRateComplianceCurrentQuater == null
                                    ? 0
                                    : ascScorecardObject.ResolutionRateComplianceCurrentQuater.Target,
                            AmericasTotalCount =
                                ascScorecardObject.ResolutionMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.Equals(AscConstants.AmericasTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            EmeaTotalCount =
                                ascScorecardObject.ResolutionMetrics.Where(
                                    f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.EmeaTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            JapanTotalCount =
                                ascScorecardObject.ResolutionMetrics.Where(
                                    f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.JapanTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            ApacTotalCount =
                                ascScorecardObject.ResolutionMetrics.Where(
                                    f => f.Category.Equals(fiscalQtrCode) && f.Theatre.Equals(AscConstants.ApacTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            GLobalServicesTotalCount =
                                ascScorecardObject.ResolutionMetrics.Where(
                                    f =>
                                        f.Category.Equals(fiscalQtrCode) &&
                                        f.Theatre.Equals(AscConstants.GlobalServicesTheatre))
                                    .Select(f => f.PercentageTagged)
                                    .FirstOrDefault(),
                            PartnerTotalCount = (isInternal && !ascScorecardObject.MultipleAsc)
                                ? 0
                                : ascScorecardObject.ResolutionMetricsPartner.Count > 0
                                    ? ascScorecardObject.MultipleAsc
                                        ? (ascScorecardObject.ResolutionMetricsPartner.Where(
                                            f =>
                                                f.Category.Equals(fiscalQtrCode) &&
                                                f.PartnerName.ToUpper().Equals("TOTAL"))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault())
                                        : (ascScorecardObject.ResolutionMetricsPartner.Where(
                                            f => f.Category.Equals(fiscalQtrCode))
                                            .Select(f => f.PercentageTagged)
                                            .FirstOrDefault())
                                    : 0

                        };
                        listResolutionChart.Add(objResolutionMetricsChart);

                    }
                }
                
                var partnerList =
                    ascScorecardObject.FrontlineMetricsPartner.Select(q => q.PartnerName).Distinct().ToList();
                ascScorecardObject.ResolutionMetricsConsolidated = new MetricsConsolidated
                {
                    Theatre = theatreList,
                    Category = category.ToList(),
                    PartnerList = partnerList,
                    MetricsList = ascScorecardObject.ResolutionMetrics,
                    MetricsPartner = ascScorecardObject.ResolutionMetricsPartner,
                    LineChart = listResolutionChart,
                    TheatreTextForChart = theatreText,
                    PartnerTextForChart = partnerTextForChart,
                    MultipleAscLineChart = listMultipleAsc
                };
            }

            if (ascScorecardObject.CsatResponses != null && ascScorecardObject.CsatResponses.Count > 0)
            {
                var category = (from d in ascScorecardObject.CsatResponses
                    select new {Category = d.Category, FiscalCode = d.FiscalCode}).Distinct()
                    .OrderBy(q => q.FiscalCode)
                    .ToList()
                    .Select(q => q.Category);

                List<MetricsChart> listCsatLineChart = null;
                List<MultipleAscChart> listMultipleAsc = null;
                List<MultipleAscChart> listSingleAsc = null;
                string theatreText = string.Empty;
                if (ascScorecardObject.MultipleAsc)
                {
                    
                    listMultipleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var categoryCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = categoryCode,
                            
                            TotalCount = ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.Partner))
                                    .Select(f => f.Value)
                                    .FirstOrDefault()
                        };
                        listMultipleAsc.Add(objMultipleAscChart);
                    }
                }
                else if (ascScorecardObject.HasPartnerProfile)
                {
                    theatreText = ascScorecardObject.PartnerProfile.Theatre;

                    listSingleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var categoryCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = categoryCode,
                          
                            WorldWideAverage = ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.ToUpper().Equals(ascScorecardObject.PartnerProfile.Theatre.ToUpper()))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),

                            TotalCount = ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.Partner))
                                    .Select(f => f.Value)
                                    .FirstOrDefault()
                        };
                        listSingleAsc.Add(objMultipleAscChart);
                    }
                }
                else
                {
                    listCsatLineChart = new List<MetricsChart>();
                    MetricsChart objCsatResolutionMetricsChart = null;
                    foreach (var categoryCode in category)
                    {
                        objCsatResolutionMetricsChart = new MetricsChart()
                        {
                            Category = categoryCode,

                            AmericasTotalCount =
                                ascScorecardObject.CsatResponses.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.Equals(AscConstants.AmericasTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            EmeaTotalCount =
                                ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.EmeaTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            JapanTotalCount =
                                ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.JapanTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            ApacTotalCount =
                                ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.ApacTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            GLobalServicesTotalCount =
                                ascScorecardObject.CsatResponses.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.Equals(AscConstants.GlobalServicesTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            PartnerTotalCount =
                                ascScorecardObject.CsatResponses.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.Partner))
                                    .Select(f => f.Value)
                                    .FirstOrDefault()
                        };
                        listCsatLineChart.Add(objCsatResolutionMetricsChart);

                    }
                        
                }
                ascScorecardObject.CsatResponseLineChart=new CsatLineChart()
                    {
                        LineChart = listCsatLineChart,
                        TheatreTextForChart = theatreText,
                        MultipleAscLineChart = listMultipleAsc,
                        SingleAscLineChart = listSingleAsc
                    };
               
            }

            if (ascScorecardObject.CsatTop3 != null && ascScorecardObject.CsatTop3.Count > 0)
            {
                var category = (from d in ascScorecardObject.CsatTop3
                    select new {Category = d.Category, FiscalCode = d.FiscalCode}).Distinct()
                    .OrderBy(q => q.FiscalCode)
                    .ToList()
                    .Select(q => q.Category);

                List<MetricsChart> listCsatTop3LineChart = null;
                List<MultipleAscChart> listMultipleAsc = null;
                List<MultipleAscChart> listSingleAsc = null;
                string theatreText = string.Empty;
                if (ascScorecardObject.MultipleAsc)
                {

                    listMultipleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var categoryCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = categoryCode,
                            TargetCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.ToUpper().Equals(AscConstants.Target))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            TotalCount = ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.Partner))
                                    .Select(f => f.Value)
                                    .FirstOrDefault()
                        };
                        listMultipleAsc.Add(objMultipleAscChart);
                    }
                }
                else if (ascScorecardObject.HasPartnerProfile)
                {
                    theatreText = ascScorecardObject.PartnerProfile.Theatre;

                    listSingleAsc = new List<MultipleAscChart>();
                    MultipleAscChart objMultipleAscChart = null;
                    foreach (var categoryCode in category)
                    {
                        objMultipleAscChart = new MultipleAscChart()
                        {
                            Category = categoryCode,
                            TargetCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.ToUpper().Equals(AscConstants.Target))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            WorldWideAverage = ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.ToUpper().Equals(ascScorecardObject.PartnerProfile.Theatre.ToUpper()))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),

                            TotalCount = ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.Partner))
                                    .Select(f => f.Value)
                                    .FirstOrDefault()
                        };
                        listSingleAsc.Add(objMultipleAscChart);
                    }

                }
                else
                {
                    listCsatTop3LineChart = new List<MetricsChart>();
                    MetricsChart objTop3MetricsChart = null;
                    foreach (var categoryCode in category)
                    {
                        objTop3MetricsChart = new MetricsChart()
                        {
                            Category = categoryCode,

                            AmericasTotalCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.Equals(AscConstants.AmericasTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            EmeaTotalCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.EmeaTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            JapanTotalCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.JapanTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            ApacTotalCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.ApacTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            GLobalServicesTotalCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.Equals(AscConstants.GlobalServicesTheatre))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            PartnerTotalCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f => f.Category.Equals(categoryCode) && f.Theatre.Equals(AscConstants.Partner))
                                    .Select(f => f.Value)
                                    .FirstOrDefault(),
                            TargetCount =
                                ascScorecardObject.CsatTop3.Where(
                                    f =>
                                        f.Category.Equals(categoryCode) &&
                                        f.Theatre.ToUpper().Equals(AscConstants.Target))
                                    .Select(f => f.Value)
                                    .FirstOrDefault()
                        };
                        listCsatTop3LineChart.Add(objTop3MetricsChart);

                    }

                }
                ascScorecardObject.CsatTop3LineChart = new CsatLineChart()
                {
                    LineChart = listCsatTop3LineChart,
                    TheatreTextForChart = theatreText,
                    MultipleAscLineChart = listMultipleAsc,
                    SingleAscLineChart = listSingleAsc
                };
            }
        }

        public AscScoreCard GetAscScoreCardLastFiveMonthData(string sfdcId, bool isInternal, string theatre, string includeCurrentPartial)
        {
            var filter = string.Format(AscConstants.MetricsFilter, sfdcId, isInternal ? 1 : 0,
                isInternal ? theatre : "ALL", includeCurrentPartial);

            AscScoreCard ascScorecardObject = new AscScoreCard();

            try
            {

                Parallel.Invoke(
                    () =>
                    {
                        string[] sfdcAccountIdList = sfdcId.Split(',');
                        if (sfdcAccountIdList.Length > 1)
                        {
                            ascScorecardObject.PartnerProfile = new PartnerProfile
                            {
                                Type = string.Empty,
                                PartnerLevel = string.Empty,
                                Elite = string.Empty,
                                Theatre = string.Empty
                            };
                            ascScorecardObject.HasPartnerProfile = false;
                            ascScorecardObject.MultipleAsc = true;
                        }
                        else
                        {
                            var partnerProfileQuery = string.Format(AscConstants.PartnerProfileQuery, sfdcAccountIdList[0].Replace("'", ""));
                            ascScorecardObject.PartnerProfile =
                                RunSapQuery<PartnerProfile>(partnerProfileQuery).FirstOrDefault();
                            if (ascScorecardObject.PartnerProfile != null)
                            {
                                ascScorecardObject.HasPartnerProfile = true;
                            }
                           
                            ascScorecardObject.MultipleAsc = false;
                        }
                    },

                    () =>
                    {
                        var deviceCustomerFilter = string.Format(AscConstants.DeviceFilter, sfdcId, isInternal ? 1 : 0,
                            "ALL", includeCurrentPartial);
                        ascScorecardObject.AscDeviceTheatrePartnerChart =
                            RunSapQuery<AscDeviceTheatrePartner>(AscConstants.AscDeviceTheatreLastFiveMonthQuery +
                                                                 deviceCustomerFilter + " GROUP BY THEATRE, FISCAL_MONTH,year_month")
                                .OrderBy(p => p.FiscalCode)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.FrontlineMetrics =
                            RunSapQuery<MetricsData>(AscConstants.FrontLineMetricsFiveMonthQuery + filter).ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.FrontlineMetricsPartner =
                            RunSapQuery<MetricsData>(AscConstants.FrontLineMetricsPartnerFiveMonthQuery + filter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.ResolutionMetrics =
                            RunSapQuery<MetricsData>(AscConstants.ResolutionMetricsFiveMonthQuery + filter).ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.ResolutionMetricsPartner =
                            RunSapQuery<MetricsData>(AscConstants.ResolutionMetricsPartnerFiveMonthQuery + filter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.CsatTop3 =
                            RunSapQuery<PercentLineChartData>(AscConstants.Top3FiveMonthQuery + filter).ToList();
                    },
                    () =>
                    {
                        var csatFilter = string.Format(AscConstants.CsatFilter, sfdcId, includeCurrentPartial);
                        ascScorecardObject.CsatResponses =
                            RunSapQuery<PercentLineChartData>(AscConstants.CsatResponseFiveMonthQuery + csatFilter)
                                .ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.Comments =
                            RunSapQuery<Comments>(AscConstants.CsatCommentsFiveMonthQuery + filter).ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.CommentsResultFromQuest =
                            RunSapQuery<Comments>(AscConstants.CsatCommentsHighScoreFiveMonthQuery + filter).ToList();
                    },
                    () =>
                    {
                        ascScorecardObject.FrontlineComplianceCurrentQuater =
                            RunSapQuery<ComplianceCurrentQtr>(AscConstants.FrontLineCurrentQuaterQuery + filter)
                                .FirstOrDefault() ?? new ComplianceCurrentQtr();
                    },
                    () =>
                    {
                        ascScorecardObject.ResolutionRateComplianceCurrentQuater =
                            RunSapQuery<ComplianceCurrentQtr>(AscConstants.ResolutionRateCurrentMonthQuery + filter)
                                .FirstOrDefault() ?? new ComplianceCurrentQtr();
                    },
                    () =>
                    {
                        ascScorecardObject.CustomerSatComplianceCurrentQuater =
                            RunSapQuery<ComplianceCurrentQtr>(AscConstants.CustomerSatCurrentQuaterQuery + filter)
                                .FirstOrDefault() ?? new ComplianceCurrentQtr();
                    }
                    );


                bool isInternalView = !ascScorecardObject.HasPartnerProfile;
                MassageAscData(ascScorecardObject, isInternalView,true);
                if (isInternalView && !ascScorecardObject.MultipleAsc)
                {
                    //Device & Customer Information Massage
                    MassageInternalData(ascScorecardObject);
                }

            }
            catch (AggregateException ae)
            {
                throw new Exception(string.Join(Environment.NewLine, ae.InnerExceptions.Select(e => e.Message)));
            }



            return ascScorecardObject;

        }


        private static void MassageInternalData(AscScoreCard ascScorecardObject)
        {

            var category = (from d in ascScorecardObject.AscDeviceTheatrePartnerChart
                select new {Category = d.Category, FiscalCode = d.FiscalCode}).Distinct()
                .OrderBy(q => q.FiscalCode)
                .ToList()
                .Select(q => q.Category);


            var enumerable = category as IList<string> ?? category.ToList();

            var panwDeviceCountFiveMonthList = (from categoryCode in enumerable
                select (from a in ascScorecardObject.AscDeviceTheatrePartnerChart
                    where a.Category.Equals(categoryCode)
                    select a).ToList()
                into ascDeviceTheatreLastFiveMonthLst
                where ascDeviceTheatreLastFiveMonthLst.Count > 0
                select new InternalInformation
                {
                    Category = ascDeviceTheatreLastFiveMonthLst[0].Category,
                    ApacCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.ApacTheatre))
                            .Select(c => c.DeviceCount)
                            .FirstOrDefault(),
                    AmericasCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.AmericasTheatre))
                            .Select(c => c.DeviceCount)
                            .FirstOrDefault(),
                    EmeaCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.EmeaTheatre))
                            .Select(c => c.DeviceCount)
                            .FirstOrDefault(),
                    GlobalCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.GlobalServicesTheatre))
                            .Select(c => c.DeviceCount)
                            .FirstOrDefault(),
                    JapanCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.JapanTheatre))
                            .Select(c => c.DeviceCount)
                            .FirstOrDefault()
                }).ToList();

            var panwCustomerCountFiveMonthList = (from categoryCode in enumerable
                select (from a in ascScorecardObject.AscDeviceTheatrePartnerChart
                    where a.Category.Equals(categoryCode)
                    select a).ToList()
                into ascDeviceTheatreLastFiveMonthLst
                where ascDeviceTheatreLastFiveMonthLst.Count > 0
                select new InternalInformation
                {
                    Category = ascDeviceTheatreLastFiveMonthLst[0].Category,
                    ApacCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.ApacTheatre))
                            .Select(c => c.CustomerCount)
                            .FirstOrDefault(),
                    AmericasCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.AmericasTheatre))
                            .Select(c => c.CustomerCount)
                            .FirstOrDefault(),
                    EmeaCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.EmeaTheatre))
                            .Select(c => c.CustomerCount)
                            .FirstOrDefault(),
                    GlobalCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.GlobalServicesTheatre))
                            .Select(c => c.CustomerCount)
                            .FirstOrDefault(),
                    JapanCount =
                        ascDeviceTheatreLastFiveMonthLst.Where(a => a.Theatre.Equals(AscConstants.JapanTheatre))
                            .Select(c => c.CustomerCount)
                            .FirstOrDefault()
                }).ToList();

            ascScorecardObject.InternalDeviceInformationChart = panwDeviceCountFiveMonthList;
            ascScorecardObject.InternalCustomerInformationChart = panwCustomerCountFiveMonthList;
        }


        public void AscReminderEmail()
        {
            var i = 0;
            var batchsize = 100;
           
            try
            {
                var query = AscConstants.AscReminderEmailQuery;

                var ascReminderEmailList = RunSapQuery<AscReminderEmail>(query).ToList();

                //var ascEmailList = _ascScoreCardRepository.GetAscList();
                var ascReminderId = _ascScoreCardRepository.InsertAscScorecardReminder("started");
                try
                {
                    if (ascReminderEmailList.Count > 0)
                    {
                        var ascEmailList =
                            ascReminderEmailList.Where(t => string.IsNullOrEmpty(t.AscSupportEmail) != true)
                                .Select(t => t.AscSupportEmail)
                                .ToList();
                        if (ascEmailList.Count > batchsize)
                        {
                            while (i < ascEmailList.Count)
                            {
                                var batchEmailList = ascEmailList.Skip(i).Take(batchsize).ToList();

                                var configValues = new ConfigValues();
                                var applicationUrl = configValues.GetAppSetting(AppSettings.ApplicationUrl);
                                var emailBody= string.Format(@"<br/>Dear ASCs,<br/><br/>The due date for uploading your Palo Alto Networks case data is the 5th of this month.  <br/><br/>
                                    If you haven’t already done so, please be sure to complete these important steps by the deadline:<br/>
                                    <ol><li>Log into your ASC Support Account at <a href='{0}'>{0}</a></li>
                                    <li>On the HOME page, under the TOOLs section, click ‘REPORTS’.  *You MUST have the “ASC User” role to find this*</li>
                                    <li>Find the “Upload Case Data” button on the top right of the page and upload the file containing your total Palo Alto Networks case volume from the prior month.</li></ol>
                                     Review your performance against the ASC program requirements and with the other ASC partners in your theatre.<br/><br/>
                                     Thank you for your partnership in providing world class customer support for Palo Alto Networks.<br/><br/>Sincerely,<br/><br/>ASC Program Team<br/>Palo Alto Networks",applicationUrl);

                                var emailStatus = EmailHelper.SendGridMail(batchEmailList, null, null, 
                                    "ASC Scorecard Monthly Reminder", emailBody);

                                _ascScoreCardRepository.InsertAscScorecardReminderEmailList(ascReminderId,
                                    string.Join(",", batchEmailList),
                                    emailStatus);
                                i = i + batchsize;
                            }
                        }
                        else
                        {
                            _ascScoreCardRepository.InsertAscScorecardReminderEmailList(ascReminderId,
                                string.Join(",", ascEmailList.ToArray()),
                                "Success");
                        }
                    }

                    _ascScoreCardRepository.UpdateAscScorecardReminderStatus(ascReminderId, "Success");
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    _ascScoreCardRepository.UpdateAscScorecardReminderStatus(ascReminderId, "Failed");
                    EmailHelper.SendExceptionEmail(ex);
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                EmailHelper.SendExceptionEmail(ex);
            }
        }
    }
}
