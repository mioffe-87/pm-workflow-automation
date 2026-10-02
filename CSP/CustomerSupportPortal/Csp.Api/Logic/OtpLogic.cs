using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System.Web;
using LINQtoCSV;
using System.Net.Http;

namespace Csp.Api.Logic
{
    public class OtpLogic:IOtpLogic
    {
        #region Constants

        public const int BATCH_SIZE = 250;

        #endregion

        private readonly IOtpDataAccess _otpDataAccess;
        private readonly IApplicationSettings _applicationSettings;
        public OtpLogic(IOtpDataAccess otpDataAccess, IApplicationSettings applicationSettings)
        {
            _otpDataAccess = otpDataAccess;
            _applicationSettings = applicationSettings;
        }

        public List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType)
        {
            return _otpDataAccess.GetDevicesForOtp(supportAccountId, userAccountId, deviceType);
        }

        public OTPOutput GetOneTimePassword(OneTimePasswordModel otpModel)
        {
            var oneTimePassword = string.Empty;
            var otpOutput = new OTPOutput();
            DateTime? expirationDate;

            if (otpModel.OtpType.Equals(OtpType.LoggingService))
            {
                string apiUrl = ConfigurationManager.AppSettings["LicensingApiUrl"].ToString();
                string URL = string.Format("{4}/Otp/GetPassword?custId={0}&productCategory={1}&deviceSerial={2}&lookup={3}", otpModel.SupportAccountId,
                    otpModel.ProductCategory, otpModel.SerialNumber, otpModel.LookUp, apiUrl);
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    request.ContentLength = 0;
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    WebResponse webResponse = request.GetResponse();
                    using (Stream webStream = webResponse.GetResponseStream())
                    {
                        if (webStream != null)
                        {
                            using (StreamReader responseReader = new StreamReader(webStream, Encoding.Default))
                            {
                                string response = responseReader.ReadToEnd();
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                                foreach (var kv in dict)
                                {
                                    if (kv.Key == "otp")
                                    {
                                        response = kv.Value;
                                    }
                                }
                                oneTimePassword = HttpUtility.HtmlEncode(response);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
            else
            {
                oneTimePassword = HttpUtility.HtmlEncode(_otpDataAccess.GenerateOneTimePassword(otpModel.SerialNumber));
            }
            
            if (!string.IsNullOrEmpty(oneTimePassword))
            {
                otpModel.Otp = oneTimePassword;
                var isOtpCreated = _otpDataAccess.AddUpdateOneTimePassword(otpModel, out expirationDate);

                if (isOtpCreated)
                {
                    otpOutput.OneTimePassword = oneTimePassword;
                    otpOutput.ExpiryDate = expirationDate.HasValue ? expirationDate.Value.ToString() : 
                        otpModel.OtpType.Equals(OtpType.LoggingService) ? DateTime.Now.AddMinutes(10).ToString() : DateTime.Now.AddMinutes(60).ToString();
                }
            }

            return otpOutput;
        }

        public bool SaveRegistrationPin(RegistrationPinModel regPinModel)
        {
            return _otpDataAccess.SaveRegistrationPin(regPinModel);
        }

        #region Bulk OTP

        public bool SaveBulkOtpRequest(BulkOtpModel bulkOtpModel)
        {
            return _otpDataAccess.SaveBulkOtpRequest(bulkOtpModel);
        }

        public List<BulkOtpModel> GetPendingBulkOtpRequests()
        {
            var pendingRequests = _otpDataAccess.GetPendingBulkOtpRequests();

            return pendingRequests;
        }

        public List<DeviceOtp> GenerateBulkOtp(BulkOtpModel bulkOtpModel)
        {
            var devicesOtp = _otpDataAccess.GenerateBulkOtp(bulkOtpModel);

            return devicesOtp;
        }

        public void ProcessPendingBulkOtpRequests()
        {
            //Get Pending bulk OTP Requests
            var pendingBulkOtpRequests = GetPendingBulkOtpRequests();

            foreach (var bulkOtpRequest in pendingBulkOtpRequests)
            {
                try
                {
                    //Decode the request payload
                    var requestJson = DecodeBulkOtpPayload(bulkOtpRequest.RequestPayload);
                    var requestPayload = JsonConvert.DeserializeObject<BulkOtpRequestPayload>(requestJson);
                    var deviceOtps = ProcessBulkOtp(bulkOtpRequest, requestPayload);

                    if (deviceOtps.Any())
                    {
                        var responsePayload = new BulkOtpResponsePayload
                        {
                            Version = "1.0",
                            DeviceOtp = deviceOtps
                        };

                        var jsonResponse = JsonConvert.SerializeObject(responsePayload);
                        bulkOtpRequest.ResponsePayload = EncodeBulkOtpPayload(jsonResponse);
                        bulkOtpRequest.ProcessingStatus = ProcessingStatus.Complete;
                        if (deviceOtps.Count < requestPayload.Devices.Count)
                        {
                            bulkOtpRequest.StatusMessage = @"One or more of the Firewalls not registered to CSP.";
                        }
                        else if(deviceOtps.Any(d => !string.IsNullOrEmpty(d.ErrorMessage)))
                        {
                            bulkOtpRequest.StatusMessage = @"One or more of the Firewalls not on the selected account.";
                        }
                    }
                    else
                    {
                        bulkOtpRequest.ProcessingStatus = ProcessingStatus.Failed;
                        bulkOtpRequest.StatusMessage = @"All the Firewalls not registered to CSP.";
                    }
                    
                    
                }
                catch (Exception ex)
                {
                    bulkOtpRequest.ProcessingStatus = ProcessingStatus.Failed;
                    bulkOtpRequest.StatusMessage = @"Unable to generate OTP response. Please try again later or contact customer support.";
                    if (HttpContext.Current != null)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                    else
                    {
                        Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                    }
                }

                //Save the response payload in database, set the expiration date and status
                SaveBulkOtpRequest(bulkOtpRequest);
            }
        }

        public void ProcessOtpExpirationRequest(long userAccountId)
        {
            _otpDataAccess.ProcessOtpExpirationRequest(userAccountId);
        }

        public void ProcessPinExpirationRequest(long userAccountId)
        {
            _otpDataAccess.ProcessPinExpirationRequest(userAccountId);
        }

        public string DecodeBulkOtpPayload(string bulkOtpPayload)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(bulkOtpPayload);
            byte[] originalRequest = ApiCommon.Decompress(encodedDataAsBytes);

            string requestJson = Encoding.ASCII.GetString(originalRequest);

            return requestJson;
        }

        public string EncodeBulkOtpPayload(string bulkOtpPayload)
        {
            byte[] originalData = Encoding.ASCII.GetBytes(bulkOtpPayload);
            byte[] compressedData = ApiCommon.Compress(originalData);

            if (compressedData == null)
            {
                throw new ArgumentNullException("CompressedData is empty");
            }

            var encodedData = Convert.ToBase64String(compressedData, Base64FormattingOptions.InsertLineBreaks);

            return encodedData;
        }

        private List<DeviceOtp> ProcessBulkOtp(BulkOtpModel bulkOtp,BulkOtpRequestPayload requestPayload)
        {
            //var requestJson = DecodeBulkOtpPayload(bulkOtp.RequestPayload);
            //var requestPayload = JsonConvert.DeserializeObject<BulkOtpRequestPayload>(requestJson);
            var deviceOtpList = new List<DeviceOtp>();

            if (requestPayload != null)
            {
                var devices = requestPayload.Devices;
                var deviceBatches = GetBatches(devices, BATCH_SIZE);

                foreach (var deviceBatch in deviceBatches)
                {
                    //Call DB Stored procedure to generate OTP and save in the db
                    bulkOtp.DeviceList = string.Concat(@"'", string.Join(@"','", deviceBatch), @"'");
                    var deviceOtp = GenerateBulkOtp(bulkOtp);
                    deviceOtpList.AddRange(deviceOtp);
                }
            }

            return deviceOtpList;
        }

        private List<List<T>> GetBatches<T>(List<T> source, int batchSize)
        {
            List<List<T>> batches = new List<List<T>>();
            int index = 0;
            while (index < source.Count)
            {
                int count = source.Count - index > batchSize ? batchSize : source.Count - index;
                batches.Add(source.GetRange(index, count));
                index += batchSize;
            }
            return batches;
        }

        #endregion

        public OtpSummary<OtpSummaryItem> GetDeviceRegistrationOtpSummary(long supportAccountId, long userAccountId, DataParameters parameters, bool showHistory = false)
        {
            return _otpDataAccess.GetDeviceRegistrationOtpSummary(supportAccountId, userAccountId, parameters, showHistory);
        }

        public OtpSummary<OtpPinSummaryItem> GetDeviceRegistrationPinSummary(long supportAccountId, long userAccountId, DataParameters parameters)
        {
            return _otpDataAccess.GetDeviceRegistrationPinSummary(supportAccountId, userAccountId, parameters);
        }

        public int DisableDeviceRegistrationPin(long supportAccountId, long userAccountId, string ids)
        {
            return _otpDataAccess.DisableDeviceRegistrationPin(supportAccountId, userAccountId, ids);
        }

        public bool EnableThermiteApiTest()
        {
            return _otpDataAccess.EnableThermiteApiTest();
        }

        public byte[] GetOtpHistoryCsv(long supportAccountId, long userAccountId)
        {
            var dataParameters = new DataParameters();
            dataParameters.PaginationOffset = 1;
            var data = _otpDataAccess.GetDeviceRegistrationOtpSummary(supportAccountId, userAccountId, dataParameters, true);
            var csvContext = new CsvContext();

            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
                IgnoreUnknownColumns = true,
                EnforceCsvColumnAttribute = true
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {
                var otpHistoryCsv = data.Items.Select(i => new OtpHistoryCsv
                {
                    SerialNumber = i.SerialNumber,
                    DeviceType = i.DeviceType,
                    OTPType = i.OTPType,
                    Status = i.Status,
                    ExpirationDate = i.ExpirationDate,
                    Requestor = i.Requestor
                });
                csvContext.Write(otpHistoryCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public CommonApiResponse<GenerateOtpResult> GenerateSingleOtp(SingleOtpRequest singleOtpRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.SingleOtpEndpoint, _applicationSettings.PortalBffHost);
                var jsonPayload = JsonConvert.SerializeObject(singleOtpRequest);
                result = CommonHelper.ProcessWebRequest<GenerateOtpResult>(baseUrl, method, _applicationSettings.PortalBffCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }


        public CommonApiResponse<dynamic> GenerateBulkOtp(BulkOtpRequest singleOtpRequest)
        {
            dynamic result = null;
            try
            {
                var method = HttpMethod.Post.Method;
                var baseUrl = string.Format(ApiConstants.BulkOtpEndpoint, _applicationSettings.CspPubSubEventsHost);
                var jsonPayload = JsonConvert.SerializeObject(singleOtpRequest);
                result = CommonHelper.ProcessWebRequest<dynamic>(baseUrl, method, _applicationSettings.CspPubSubEventsCertThumbPrint, jsonPayload, true);
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = ExceptionHelper.GetWebException(ex);
            }
            return result;
        }
    }
}
