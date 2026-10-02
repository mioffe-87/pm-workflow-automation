using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess
{
    class CsspDataAccess : BaseDataAccess, ICsspDataAccess
    {
        public IEnumerable<CsspProvisionedDevices> GetCsspProvisionedDevices(long supportAccountId, string serialNumber = "", string authorizationCode = "", string endUserAccountId = "", string endUserCompany = "", string contactEmail = "", DataParameters parameters = null)
        {

           var provisionedDevices = new List<CsspProvisionedDevices>();

            using (var command = Connection.GetStoredProcCommand("GetCsspProvisionedDevices"))
            {
                Connection.AddInParameter(command, "p_SupportAccountId",DbType.Int64,supportAccountId);
                Connection.AddInParameter(command, "p_AuthorizationCode", DbType.String, authorizationCode == ""? null:authorizationCode);
                Connection.AddInParameter(command, "p_SerialNumber", DbType.String, serialNumber == ""? null:serialNumber);
                Connection.AddInParameter(command, "p_EndUserAccountId", DbType.String, endUserAccountId == ""?null:endUserAccountId);
                Connection.AddInParameter(command, "p_EndUserCompany", DbType.String, endUserCompany == ""? null:endUserCompany);
                Connection.AddInParameter(command, "p_EndUserEmail", DbType.String, contactEmail == ""? null:contactEmail);

                using (var dataReader = Connection.ExecuteReader(command))
                {


                    while (dataReader.Read())
                    {

                       provisionedDevices.Add(new CsspProvisionedDevices()
                       {
                           DeviceId = Convert.ToInt32(dataReader["deviceid"]),
                           MasterAuthCode = dataReader["authorizationcode"].ToString(),
                           SerialNumber = dataReader["serialnumber"].ToString(),
                                            Model = dataReader["modelcompatibility"].ToString(),
                                            DeviceName = dataReader["devicename"].ToString(),
                                            EndUserCustomerAccountId = Convert.ToString(dataReader["CustomerReferenceId"]),
                                            EndUserAddress = Convert.ToString(dataReader["Address"]),
                                            EndUserCompany = Convert.ToString(dataReader["CompanyName"]),
                                            EndUserContactEmail = Convert.ToString(dataReader["ContactEmail"]),
                                            EndUserCity = Convert.ToString(dataReader["City"]),
                                            EndUserState = Convert.ToString(dataReader["State"]),
                                            EndUserPostalCode = Convert.ToString(dataReader["PostalCode"]),
                                            EndUserCountry = Convert.ToString(dataReader["Country"]),
                                            EndUserPhoneNumber = Convert.ToString(dataReader["PhoneNumber"]),
                                            EndUserCompanyWebsite =Convert.ToString(dataReader["CompanyWebsite"]),
                                            EndUserIndustry = Convert.ToString(dataReader["CompanyWebsite"]),
                                            DnbNumber = Convert.ToString(dataReader["DnBNumber"]),
                                            ProvisionedDate = Convert.ToString(dataReader["registrationdate"]),
                                            HasEndUserInfo = Convert.ToString(dataReader["CsspEndUserInformationId"])!= string.Empty ? true : false

                       });
                    }
                }

               
            }

            var devices = provisionedDevices.ToList();

            if (parameters != null)
            {
                parameters.TotalRecordCount = devices.Count;
                return devices.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount).ToArray();
            }
            else
            {
                return devices;
            }

            //return provisionedDevices;
        }

    }
}

