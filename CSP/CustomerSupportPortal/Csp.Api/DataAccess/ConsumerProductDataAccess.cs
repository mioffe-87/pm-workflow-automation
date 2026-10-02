using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.ConsumerProduct;
using Csp.Api.Model.ExtendedExpertise;
using Csp.Api.Services.Contracts;
using System.Data;
using Csp.Api.DataAccess.HelperClasses;
using MySql.Data.MySqlClient;

namespace Csp.Api.DataAccess
{
    public class ConsumerProductDataAccess : BaseDataAccess, IConsumerProductDataAccess
    {
        public ConsumerProductResponse<ConsumerProductOkyoGardeParent> GetOkyoProductData(ConsumerProductRequest request)
        {
            ConsumerProductResponse<ConsumerProductOkyoGardeParent> returnData = new ConsumerProductResponse<ConsumerProductOkyoGardeParent>();
            List<ConsumerProductOkyoGardeParent> result = new List<ConsumerProductOkyoGardeParent>();
            int totalRecordsCount = 0;

            try
            {
                using (var command = Connection.GetStoredProcCommand("csp.GetConsumerProductOkyo"))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, request.SupportAccountId);
                    Connection.AddInParameter(command, "@p_Searchfield", DbType.String, request.SearchField);
                    Connection.AddInParameter(command, "@p_SearchValue", DbType.String, request.SearchValue);
                    Connection.AddInParameter(command, "@p_SortField", DbType.String, request.SortField);
                    Connection.AddInParameter(command, "@p_SortDirection", DbType.String, request.SortOrder);
                    Connection.AddInParameter(command, "@p_PaginationOffset", DbType.Int32, request.Page);
                    Connection.AddInParameter(command, "@p_PaginationCount", DbType.Int32, request.PerPage);
                    Connection.AddOutParameter(command, "@p_TotalRecordCount", DbType.Int64, Int32.MaxValue);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        // Parent resultSet 
                        while (dataReader.Read())
                        {
                            var okya = new ConsumerProductOkyoGardeParent()
                            {
                                Id = dataReader.GetColumnValue<long>("TenantLookupId"),
                                TenantSerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                Description = dataReader.GetColumnValue<string>("Description"),
                                PartNumber = dataReader.GetColumnValue<string>("PartNumber"),
                                ServiceLevel = dataReader.GetColumnValue<string>("ServiceLayer"),
                                Region = dataReader.GetColumnValue<string>("Region"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime?>("ExpirationDate"),
                                TotalActivatedQuantity = dataReader.GetColumnValue<int>("ActivatedQuantity"),
                                TotalAvailableQuantity = dataReader.GetColumnValue<int>("availableQuantity"),
                                TotalQuantity = dataReader.GetColumnValue<int>("TotalQuantity"),
                                DeviceId = dataReader.GetColumnValue<long>("DeviceId"),
                                Children = new List<ConsumerProductOkyoGardeChild>()
                            };
                            result.Add(okya);
                        }
                        //Children result set
                        dataReader.NextResult();
                        while (dataReader.Read())
                        {
                            long deviceId = dataReader.GetColumnValue<long>("DeviceId");
                            var target = result.Find(x => x.DeviceId == deviceId);
                            if (target != null && target.DeviceId > 0)
                            {
                                List<ConsumerProductOkyoGardeChild> child = (List<ConsumerProductOkyoGardeChild>)target.Children;
                                child.Add(
                                    new ConsumerProductOkyoGardeChild
                                    {
                                        Id = dataReader.GetColumnValue<long>("LicenseId"),
                                        AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                        Description = dataReader.GetColumnValue<string>("Description"),
                                        PartNumber = dataReader.GetColumnValue<string>("PartNumber"),
                                        Quantity = dataReader.GetColumnValue<int>("Quantity"),
                                        ActivatedQuantity = dataReader.GetColumnValue<int>("ActivatedQuantity"),
                                        AvailableQuantity = dataReader.GetColumnValue<int>("AvailableQuantity")
                                    }
                                    );
                            }
                        }
                    }

                    if (command.Parameters["@p_TotalRecordCount"].Value != DBNull.Value)
                    {
                        totalRecordsCount = Convert.ToInt32(command.Parameters["@p_TotalRecordCount"].Value);
                    }
                }

                returnData.Items = result;
                returnData.Total = totalRecordsCount;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return returnData;
        }
    }
}
