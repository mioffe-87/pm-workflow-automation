using System;
using System.Collections.Generic;
using System.Data;
using Csp.Api.Model.Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.DataAccess
{
    public class AddressValidationDataAccess: BaseDataAccess,IAddressValidationDataAccess       
    {
        public IList<AddressAPIResponse> ValidateRegioncode(IList<AddressAPIResponse> addressAPIResposneList)
        {
            IList<AddressAPIResponse> addressList = new List<AddressAPIResponse>();
            try
            {
                var getRegion = "";
                foreach (var item in addressAPIResposneList)
                {

                    if (!string.IsNullOrEmpty(item.Address1) && !string.IsNullOrEmpty(item.State) && !string.IsNullOrEmpty(item.City) && !string.IsNullOrEmpty(item.Country))
                    {

                        getRegion = @"SELECT r.Name RegionName,r.code regioncode,c.Name Country FROM csp.region  r
                                    JOIN csp.country c ON r.CountryId = c.CountryId
                                    WHERE (UPPER(TRIM(r.NAME)) =@P_RegionName OR UPPER(TRIM(r.code))=@P_RegionName) 
                                    AND (UPPER(TRIM(c.NAME))=@p_Country OR UPPER(TRIM(c.code))=@p_Country)";


                        using (var command = Connection.GetSqlStringCommand(getRegion))
                        {
                            Connection.AddInParameter(command, "@p_Country", DbType.String, item.Country.Trim().ToUpper());
                            Connection.AddInParameter(command, "@P_RegionName", DbType.String, item.State.Trim().ToUpper());

                            using (var dataReader = Connection.ExecuteReader(command))
                            {
                                while (dataReader.Read())
                                {
                                    addressList.Add(item);
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return addressList;

        }

        public bool ValidateCountryforAddressAPI(int countryId)
        {
            bool isAddressValidationRequired = false;
            try
            {
                var strSQL = "SELECT a.CountryId FROM csp.addressvalidationcountries a WHERE a.CountryId=@p_Country ";
                using (var command = Connection.GetSqlStringCommand(strSQL))
                {
                    Connection.AddInParameter(command, "@p_Country", DbType.String, countryId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            isAddressValidationRequired = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return isAddressValidationRequired;

        }

    }
}
