using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using AutoMapper;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using LINQtoCSV;

namespace Csp.Api.Logic {
	public class VirtualMachineLogic : IVirtualMachineLogic {
		private readonly IVirtualMachineDataAccess _virtualMachineDataAccess;
		private readonly IDeviceRepository _deviceRepository;
		private readonly ILicenseLogic _licenseLogic;
		private readonly IDeviceLogic _deviceLogic;
		private readonly IUserRegistrationRepository _userRegistrationRepository;
		private readonly ISupportAccountRepository _supportAccountRepository;
		private readonly IVmRegistrationRepository _vmRegistrationRepository;
	    private readonly ISiteLicenseRepository _siteLicenseRepository;
	    private readonly ISupportAccountLogic _supportAccountLogic;
        private const string vm_cpuid = "vm-cpuid";
        private const string vm_uuid = "vm-uuid";

   
		public VirtualMachineLogic(IVirtualMachineDataAccess virtualMachineDataAccess, IDeviceRepository deviceRepository, ILicenseLogic licenseLogic, 
            IUserRegistrationRepository userRegistrationRepository, ISupportAccountRepository supportAccountRepository, IDeviceLogic deviceLogic,
            IVmRegistrationRepository vmRegistrationRepository, ISiteLicenseRepository sieLicenseRepository, ISupportAccountLogic supportAccountLogic)
        {
			_virtualMachineDataAccess = virtualMachineDataAccess;
			_deviceRepository = deviceRepository;
			_licenseLogic = licenseLogic;
			_deviceLogic = deviceLogic;
			_vmRegistrationRepository = vmRegistrationRepository;
			_userRegistrationRepository = userRegistrationRepository;
			_supportAccountRepository = supportAccountRepository;
		    _siteLicenseRepository = sieLicenseRepository;
            _supportAccountLogic = supportAccountLogic;
        }

        public AddVmAuthCodeResult AddVmAuthCode(EVmType vmType, string authCode, long supportAccountId, long userAccountId)
        {
            var result = CanRegisterDevice(vmType, authCode, supportAccountId, true);
			if (!string.IsNullOrEmpty(result.ErrorMessage)) {
				return result;
			}

			return UpdateVmStub(vmType, authCode, supportAccountId, userAccountId);
		}

		/// <summary>
		/// Checks whether the current auth code can be registered
		/// 1) It's not null or empty
		/// 2) Not already registered
		/// 3) There is one and only one record of the auth code in the license table
		/// 4) It is active, doesn't have a vm stub device, and doesn't have a reg or exp date
		/// 5) Has the proper VM identifier (not a physical device)
		/// </summary>
		/// <param name="authCode"></param>
		/// <param name="supportAccountId"></param>
		/// <returns>An auth code result.  Check the ErrorMessage property for success</returns>
        private AddVmAuthCodeResult CanRegisterDevice(EVmType vmType, string authCode, long supportAccountId, bool blockVmElaActivation = false)
        {
			if (string.IsNullOrEmpty(authCode)) {
				return new AddVmAuthCodeResult(VmAuthCodeMessages.EmptyAuthCode);
			}

			//Is Auth Code Already Registered?
			if (_virtualMachineDataAccess.IsAuthCodeAlreadyRegistered(vmType, authCode)) {
				return new AddVmAuthCodeResult(VmAuthCodeMessages.AlreadyInUse);
			}

			if (_supportAccountRepository.IsEvalSupportAccount(supportAccountId)) {
				if (!_userRegistrationRepository.IsEvalDeviceCode(authCode)) {
					return new AddVmAuthCodeResult("To register a new purchased asset, the Eval account must be converted to an active CSP account.  Please go to the Company Account page to activate your account.");
				}
			}

			//Get details
			var authCodeDetails = _virtualMachineDataAccess.GetAuthCodeDetails(vmType, authCode);
			if (authCodeDetails == null) {
				return new AddVmAuthCodeResult(VmAuthCodeMessages.AuthCodeNotFound);
			}

			//Is code already in use?
			if (authCodeDetails.IsActive == false || (authCodeDetails.VmStubDeviceId ?? 0) > 0
					|| authCodeDetails.RegistrationDate.HasValue || authCodeDetails.ExpirationDate.HasValue) {
				return new AddVmAuthCodeResult(VmAuthCodeMessages.AuthCodeAlreadyAssociated);
			}

			//Is this code definitely a VM (not a physical device)
		    var dbIdentifier = authCodeDetails.PanDbIdentification.ToUpper();
            if (vmType == EVmType.Cn 
                && !dbIdentifier.StartsWith("TOKEN") 
                || vmType == EVmType.Vm
                && !dbIdentifier.StartsWith("PHOENIX") 
                && !dbIdentifier.Equals("VMELA"))
            {
				return new AddVmAuthCodeResult(VmAuthCodeMessages.NotAVmDevice);
			}

            // Do not allow VM ELA auth code activation
            if (blockVmElaActivation
                && (authCodeDetails.PartNumber.Contains("PAN-VM-ELA-BND2")
                || authCodeDetails.PartNumber.Contains("PAN-CN-X-ELA")))
		    {
                return new AddVmAuthCodeResult(VmAuthCodeMessages.VmElaAuthCode);
		    }

			//No problems, allow registration
			return new AddVmAuthCodeResult();
		}

		/// <summary>
		/// Generates a new "Stub" Serial Number
		/// Creates a device using the stub
		/// Updates the license to point to the new device with VMStubDeviceID
		/// </summary>
		/// <param name="authCode"></param>
		/// <param name="supportAccountId"></param>
		/// <returns></returns>
        private AddVmAuthCodeResult UpdateVmStub(EVmType vmType, string authCode, long supportAccountId, long userAccountId)
        {
			// a) vmStubId = select max(Id) from genedvmstub
			// b) vmSerialNo = "VM" + vmStubId++ (Padded 10 spaces)
			// d) insert into genedvmstub (vmStubSN) values('$vmSerialNo');
			var newStubSn = _virtualMachineDataAccess.GetNewStubSn(vmType);

			// c) shipDate = select ship_date from lic_features where auth_code like '$serialnum'
			var shipDate = _virtualMachineDataAccess.GetShipDate(vmType, authCode);


			// e) insert into device_master (serialnum,acctid,isVM,part_id,ship_date,regdate,grouping,ispublic) values ('$vmSerialNo',$acctID,1,'PAN-PA-VM-SW','$shipDate',CURRENT_TIMESTAMP,1,1);
			// f) lid = select lid from device_master where serialnum like '$vmSerialNo'
			// g) update lic_features set VmStubId='$lid' where auth_code like '$serialnum'
			_virtualMachineDataAccess.CreateStubDevice(vmType, authCode, newStubSn, shipDate, supportAccountId, userAccountId);

			// Check to see if the support account is AWS and it doesnt have a SFDC Account Id, SAP ID yet.

			// Comments By LK On 02/08/2020
			// Disable the logic to update SalesforceAccountId programatically
			//_licenseLogic.UpdateSapIdSfdcIdForAwsSupportAccount(supportAccountId, authCode, userAccountId);

			return new AddVmAuthCodeResult();
		}

        public IEnumerable<ReleasedLicenseItem> GetReleasedLicenses(EVmType vmType, DataParameters parameters, long supportAccountId)
        {
            var releasedLicenses = _licenseLogic.GetReleasedVMLicenses(parameters, supportAccountId);

            return releasedLicenses;
        }

        public IEnumerable<AvailableVm> GetAvailableVms(DataParameters parameters, long supportAccountId, long userAccountId, string authCode)
        {
            var availableVms = _vmRegistrationRepository.GetAvailableVms(parameters, supportAccountId, userAccountId, authCode);

            return availableVms;
        }

        public IEnumerable<AvailableVirtualSeries<AvailableCn>> GetAvailableCns(DataParameters parameters, long supportAccountId, long userAccountId, string authCode, string serialNumber)
        {
            var availableVms = _vmRegistrationRepository.GetAvailableCns(parameters, supportAccountId, userAccountId, authCode, serialNumber);

            return availableVms;
        }

        public async Task<byte[]> GetAvailableCnsCsv(long supportAccountId, long userAccountId, string authCode, string serialNumber)
        {
            return await Task.Run(() =>
            {
                var dataParameters = new DataParameters();
                var data = GetAvailableCns(dataParameters, supportAccountId, userAccountId, authCode, serialNumber);
                var flattenedData = data.SelectMany(c => c.Vms.Select(p => new AvailableCnCsv(c, p))).ToArray();
                var cc = new CsvContext();
                var outputFileDescription = new CsvFileDescription
                {
                    SeparatorChar = ',',
                    FirstLineHasColumnNames = true,
                    IgnoreUnknownColumns = true,
                    EnforceCsvColumnAttribute = true,
                    QuoteAllFields = true,
                };
                using (var ms = new MemoryStream())
                {
                    using (TextWriter tw = new StreamWriter(ms))
                    {
                        cc.Write(flattenedData, tw, outputFileDescription);
                        tw.Flush();
                        ms.Position = 0;
                        return ms.ToArray();
                    }
                }
            });
        }

        public async Task<byte[]> GetAvailableVmsCsv(long supportAccountId, long userAccountId, string authCode)
        {
            var dataParameters = new DataParameters();

            var data = GetAvailableVms(dataParameters, supportAccountId, userAccountId, authCode);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                IgnoreUnknownColumns = true,
                EnforceCsvColumnAttribute = true,
                QuoteAllFields = true,
            };
            using (var ms = new MemoryStream())
            {
                using (TextWriter tw = new StreamWriter(ms))
                {
                    var availableVms = Mapper.Map<AvailableVmCalc[]>(data);
                    foreach (var item in availableVms)
                    {
                        var parentCsvData = Mapper.Map<AvailableVmCsv>(item);
                        var csvData = await _vmRegistrationRepository.GetChildAuthCodesInfo(EVmType.Vm, supportAccountId, parentCsvData);
                        cc.Write(csvData, tw, outputFileDescription);
                        outputFileDescription.FirstLineHasColumnNames = false;
                    }

                    tw.Flush();
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
		}

        public ParseVmFileResult ParseVmFile(EVmType vmType, HttpPostedFileBase file)
        {

            if ((file.FileName.EndsWith(".txt") && file.FileName.EndsWith(".xml")) || (file.ContentType != "text/plain" && file.ContentType != "text/xml"))
		    {
		        return new ParseVmFileResult
		        {
		            Message = HttpUtility.HtmlEncode("The file format is not supported")
		        };
		    }

		    var result = new ParseVmFileResult();
		    if (file.ContentType == "text/plain")
		    {
		        string content;
                using (var sr = new StreamReader(file.InputStream, Encoding.Default))
		        {
		            content = sr.ReadToEnd();
		        }

		        //Standardize line endings on \n
		        content = Regex.Replace(content, "\r\n", "\n").Replace('\r', '\n');
		        var keyValuePairs = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		        foreach (var kvp in keyValuePairs)
		        {

		            var kvpArray = kvp.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
		            if (kvpArray.Length != 2) continue;

		            var key = kvpArray[0];
		            var value = kvpArray[1];

		            switch (key)
		            {
		                case vm_cpuid:
		                    result.CpuId = HttpUtility.HtmlEncode(value);
		                    break;

		                case vm_uuid:
		                    result.UuId = HttpUtility.HtmlEncode(value);
		                    break;
		            }
		        }		        
		    }
		    else
		    {
		        result = ParseXMLFile(file, result);
		    }

			var hasMissingValues = string.IsNullOrEmpty(result.CpuId) || string.IsNullOrEmpty(result.UuId);

			return hasMissingValues
							 ? new ParseVmFileResult {
								 Message = "File parsing error"
							 }
							 : result;
		}

        public RegisterVmResult RegisterVm(EVmType vmType, string uuId, string cpuId, string authCode, long supportAccountId, long userAccountId, 
            string hyperVisor = null, string panOs = null, bool isCssp = false)
        {
            var isSiteLicenseGranted = false;
            LicensingService.licenseRet[] results;

		    if (isCssp)
		    {
                if (string.IsNullOrEmpty(uuId) || string.IsNullOrEmpty(cpuId) || string.IsNullOrEmpty(authCode))
                    return new RegisterVmResult { ErrorMessage = "Please fill in all fields" };
		    }
		    else
		    {
                if (string.IsNullOrEmpty(uuId) || string.IsNullOrEmpty(cpuId) || string.IsNullOrEmpty(authCode) 
                    || string.IsNullOrEmpty(hyperVisor) || string.IsNullOrEmpty(panOs))
                    return new RegisterVmResult { ErrorMessage = "Please fill in all fields" };
		    }


            if (String.IsNullOrEmpty(hyperVisor) && String.IsNullOrEmpty(panOs)) {
                results = _licenseLogic.LicenseIt(string.Empty, cpuId, uuId, authCode);
            } else {
                results = _licenseLogic.LicenseIt(string.Empty, cpuId, uuId, authCode, hyperVisor, panOs);
            }


			if (!results.Any()) return new RegisterVmResult { ErrorMessage = "Failed to register device" };
			var firstResult = results.First();
			if (!string.IsNullOrEmpty(firstResult.errmsg)) return new RegisterVmResult { ErrorMessage = firstResult.errmsg };

			var newVmAuthCode = firstResult.auth_code;
			var newDeviceData = string.IsNullOrEmpty(newVmAuthCode) 
                ? _virtualMachineDataAccess.GetDeviceDataFromLfid(vmType, Convert.ToInt64(firstResult.lfid)) 
                : _virtualMachineDataAccess.GetDeviceDataFromAuthCode(vmType, newVmAuthCode);
			results = _licenseLogic.LicenseIt(newDeviceData.SerialNumber);
			if (!results.Any()) return new RegisterVmResult { ErrorMessage = "Failed to register and license device" };
			firstResult = results.First();
			if (!string.IsNullOrEmpty(firstResult.errmsg)) return new RegisterVmResult { ErrorMessage = firstResult.errmsg };

            //Update the user who activated VM
            _virtualMachineDataAccess.UpdateVmActivationUser(newDeviceData.SerialNumber, userAccountId);
            //Update the supportAccountId by serial number because the trigger can't do this
            _deviceRepository.UpdateSupportAccountIdBySerialNumber(newDeviceData.SerialNumber, supportAccountId);

            // grant site license for the newly registered VM
            isSiteLicenseGranted = _licenseLogic.GrantSiteLicenses(supportAccountId);

			return new RegisterVmResult { NewDeviceId = newDeviceData.DeviceId };
		}


		public static class VmAuthCodeMessages {
			public const string Success = "VM Auth Code registered successfully!";
			public const string EmptyAuthCode = "Auth Code cannot be empty";
			public const string AlreadyInUse = "This Auth Code is already associated with another user";
			public const string AuthCodeNotFound = "The Auth Code could not be found";
			public const string AuthCodeAlreadyAssociated = "This Auth Code is already associated with another user";
			public const string NotAVmDevice = "The Auth Code is invalid";
            public const string VmElaAuthCode = "This auth code cannot be activated on this page. Please try activating the auth code on the Enterprise Agreement page.";
		}


        public bool CanProvisionPanorama(EVmType vmType, string authCode)
        {
			return _virtualMachineDataAccess.CanProvisionPanorama(vmType, authCode);
		}


        public bool ProvisionPanorama(EVmType vmType, string authCode, long supportAccountId, long userAccountId)
        {
			bool isSuccess = false;
			#region generate serialnum
			string serialNum = _virtualMachineDataAccess.GetPraSerialNumber(vmType);
			if (string.IsNullOrEmpty(serialNum)) {
				return false;
			}
			string newSerialNum = (Convert.ToInt32(serialNum) + 1).ToString();
			if (!_virtualMachineDataAccess.UpdateNewSerialNumber(vmType, serialNum, newSerialNum)) {
				return false;
			}
			//newSerialNum = "0007NSX" + Convert.ToInt32(newSerialNum).ToString("00000");
			newSerialNum = "000710" + Convert.ToInt32(newSerialNum).ToString("000000");
			#endregion
			#region create device and free subscription
			//Create Nsx device and free subscription
			string partId = "PAN-PRA-1000-CP";
			if (_deviceLogic.PushToDeviceMaster(newSerialNum, Convert.ToInt32(userAccountId), partId)) {
				//Update user account in pandb
				_virtualMachineDataAccess.UpdateAccountIdForPraDevice(vmType, newSerialNum, userAccountId);
				//license it
				bool licensingFailed = false;
				try {
					var resultArray = _licenseLogic.LicenseIt(newSerialNum);
					if (resultArray.Any(message => message.errmsg != null)) {
						licensingFailed = true;
						Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + resultArray.First().errmsg));
					}
				} catch (Exception oex) {
					licensingFailed = true;
					Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + oex.Message));
				}
				if (licensingFailed) {
					Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Failed to License - " + newSerialNum));
				}
				// Update bundlelfid license
				_licenseLogic.UpdateLicFeaturesForNsxProvisioning(authCode, newSerialNum);
				// Now update support account id in csp.
				_deviceRepository.UpdateSupportAccountIdBySerialNumber(newSerialNum, supportAccountId);
                //Update support if VM ELA Panorama
                //(Panorama provisioned for VM(NSX and ELA) by default have premium support. Update support for VM ELA panorama to have same support as VM ELA Authcode)
                _licenseLogic.UpdateSupportForVMELABundles(authCode, newSerialNum);
                // update site licenses
                _licenseLogic.GrantSiteLicenses(supportAccountId);
                // update licensekey after updating the expiration date
                _licenseLogic.Relicenseit(newSerialNum);
				isSuccess = true;
			} else {
				isSuccess = false;
			}
			#endregion
			return isSuccess;
		}

        public IEnumerable<PraProvisioningRec> GetProvisionedPraDevices(EVmType vmType, string authCode)
        {
			return _virtualMachineDataAccess.GetNsxProvisionedPanoramas(vmType, authCode);
		}

        public bool IsInTheMiddelOfUpgrade(EVmType vmType, string serialNumber)
        {
            return _vmRegistrationRepository.IsInTheMiddelOfUpgrade(vmType, serialNumber);
        }

        public void GetVmUpgradeModels(EVmType vmType, string vmSerialNumber, string upgradeAuthCode, out string fromModel, out string toModel)
        {
            _vmRegistrationRepository.GetVmUpgradeModels(vmType, vmSerialNumber, upgradeAuthCode,out fromModel, out toModel);
        }


        public bool RollbackUpgrade(EVmType vmType, string transactionId)
        {
            return _licenseLogic.RollbackUpgrade(transactionId);
        }


        public bool StoreVmUpgrade(EVmType vmType, long deviceId, string transactionId, string fromModel, string toModel, string originalAuthCode, string upgradeAuthCode, string createdBy)
        {
            return _vmRegistrationRepository.StoreVmUpgrade(vmType, deviceId, transactionId, fromModel, toModel,
                originalAuthCode, upgradeAuthCode, createdBy);
        }


        public string GetVmAuthCodeBeforeUpgrade(EVmType vmType, string serialNumber)
        {
            return _vmRegistrationRepository.GetVmAuthCodeBeforeUpgrade(vmType, serialNumber);
        }


        public void GetVmUpgradeModels(EVmType vmType, long deviceId, string transactionId, out string originalVmAuthCode, out string currentVmAuthCode, out string fromModel, out string toModel)
        {
            _vmRegistrationRepository.GetVmUpgradeModels(vmType, deviceId, transactionId, out originalVmAuthCode,
                out currentVmAuthCode, out fromModel, out toModel);
        }


        public bool IsUpgradeComplete(EVmType vmType, long deviceId)
        {
            //return _vmRegistrationRepository.IsUpgradeComplete(deviceId);
            return _virtualMachineDataAccess.IsUpgradeComplete(vmType, deviceId);
        }


        public string GetOriginalModelAfterRollback(EVmType vmType, string transactionId, string upgradeAuthcode)
        {
            return _vmRegistrationRepository.GetOriginalModelAfterRollback(vmType, transactionId, upgradeAuthcode);
        }


        public bool CanUserUpgradeVm(EVmType vmType, string email)
        {
            return _vmRegistrationRepository.CanUserUpgradeVm(vmType, email);
        }

        public int GetVmUseCountByAuthCode(EVmType vmType, string authCode)
	    {
	        return _vmRegistrationRepository.GetVmUseCountByAuthCode(vmType, authCode);
	    }

        private ParseVmFileResult ParseXMLFile(HttpPostedFileBase file, ParseVmFileResult result)
        {
            try
            {
                var xmlDoc = new XmlDocument();

                // Comments By LK On 12/04/2018
                // Fix to prevent XML External Entity attacks
                xmlDoc.XmlResolver = null;

                xmlDoc.Load(file.InputStream);
                var node = xmlDoc.SelectSingleNode(string.Format("//{0}", vm_uuid));
                if(node != null)
                    result.UuId = HttpUtility.HtmlEncode(node.InnerText);
                node = xmlDoc.SelectSingleNode(string.Format("//{0}", vm_cpuid));
                if (node != null)
                    result.CpuId = HttpUtility.HtmlEncode(node.InnerText);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return result;
        }

        public VmAuthCodeDetails GetAuthCodeDetails(EVmType vmType, string authCode)
	    {
	        return _virtualMachineDataAccess.GetAuthCodeDetails(vmType, authCode);
	    }

        public string GetLatestQuoteInfo(EVmType vmType, long supportAcctId, string authCode, out bool selectAll, out bool lockUi, out bool hasPendingRenewals)
        {
            try
            {
                return _vmRegistrationRepository.GetLatestQuoteInfo(vmType, supportAcctId, authCode, out selectAll, out lockUi, out hasPendingRenewals);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetLatestQuoteInfo exception: {0}", e.Message)));
                throw;
            }
        }

        public async Task<IEnumerable<VmRenewalDevice>> GetVmRenewalDetails(EVmType vmType, long supportAcctId, string authcode, string quoteNumber, bool selectAll, bool hasPendingRenewals)
	    {
	        try
	        {
	            if (string.IsNullOrEmpty(authcode))
	                return new List<VmRenewalDevice>();

                if(hasPendingRenewals)
                    return await _vmRegistrationRepository.GetProvisionedVmsWithPendingRenewals(vmType, supportAcctId, authcode, quoteNumber, selectAll);

	            return await _vmRegistrationRepository.GetProvisionedVmsWithFulfilledRenewals(vmType, supportAcctId, authcode, selectAll);
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetVmRenewalDetails exception: {0}", e.Message)));
	            throw;
	        }
	    }

	    //return error message: null or empty mean successful.
        public string SaveRenewalVMs(EVmType vmType, long supportAcctId, long userAcctId, VmRenewalModel model, bool allSelected, string quoteNumber)
	    {
	        try
	        {
                _vmRegistrationRepository.SaveRenewalVMs(vmType, supportAcctId, userAcctId, model, allSelected, quoteNumber);
	            return string.Empty;
	        }
	        catch (Exception e)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("SaveRenewalVMs exception: {0}", e.Message)));
	            return e.Message;
	        }
	    }

        public async Task<IEnumerable<AvailableVmCsv>> GetChildAuthCodesInfo(EVmType vmType, long supportAcctId, AvailableVmCsv parentAuthCode)
	    {
            return await _vmRegistrationRepository.GetChildAuthCodesInfo(vmType, supportAcctId, parentAuthCode);
	    }

        public async Task<VmRenewalModel> GetUnprovisionedRenewalDetails(EVmType vmType, string authcode, string latestQuoteNumber)
	    {
	        return await _vmRegistrationRepository.GetUnprovisionedRenewalDetails(vmType, authcode, latestQuoteNumber);
	    }

        public async Task<int> GetActualProvisionedVms(EVmType vmType, long supportAcctId, string authcode)
	    {
	        return await _vmRegistrationRepository.GetActualProvisionedVms(vmType, supportAcctId, authcode);
	    }

        //do not add stub device for VM-ELA authcode (Grandparent Authcode)
        public async Task<AddVmAuthCodeResult> ActivateVmElaAuthCode(EVmType vmType, string authCode, long supportAccountId, long userAccountId)
	    {
	        var result = CanRegisterDevice(vmType, authCode, supportAccountId);

	        if (!string.IsNullOrEmpty(result.ErrorMessage))
	        {
	            return result;
	        }

	        try
	        {
	            await _siteLicenseRepository.AddVmElaMoreTokens(authCode);
	        }
	        catch (Exception e)
	        {
	            result.ErrorMessage = "Encounter an error during add 50% total tokens.";
	            return result;
	        }

	        return new AddVmAuthCodeResult();
	    }

	    //Generate authcodes for the VM 5 Models
        public RegisterVmResult GenerateVmElaModels(EVmType vmType, string authCode, long supportAccountId, long userAccountId)
	    {
	        try
	        {
	            //the logic below is almost the same as those in VM's UpdateVmStub(string authCode, long supportAccountId, long userAccountId)
	            //except that CreateStubDevice call with true for isVmELaStubDevice
	            var newStubSn = _virtualMachineDataAccess.GetNewStubSn(vmType);
	            var shipDate = _virtualMachineDataAccess.GetShipDate(vmType, authCode);
	            _virtualMachineDataAccess.CreateStubDevice(vmType, authCode, newStubSn, shipDate, supportAccountId, userAccountId, true);

							// Comments By LK On 02/08/2020
							// Disable the logic to update SalesforceAccountId programatically
	            //_licenseLogic.UpdateSapIdSfdcIdForAwsSupportAccount(supportAccountId, authCode, userAccountId);

	            //generate 5 VM authcodes
	            var results = _licenseLogic.LicenseIt(newStubSn, string.Empty, string.Empty, authCode);
	            if (!results.Any())
	                return new RegisterVmResult { ErrorMessage = "Failed to generate 5 VM models" };

                //update additional tokens applied date
	            _siteLicenseRepository.UpdateVmElaAdditionalTokensAppliedDate(authCode);
	            return new RegisterVmResult();
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GenerateVmElaModels exception: {0}, {1}", e.Message, e.StackTrace)));
                return new RegisterVmResult { ErrorMessage = "Failed to generate 5 VMs" };
	        }
	    }

        //get the token customer can allocate
        public async Task<VmElaTokenModel> GetVmElaTokens(EVmType vmType, string authCode)
	    {
	        try
	        {
	            if (string.IsNullOrEmpty(authCode))
	                return new VmElaTokenModel{ GrandParentAuthCode = authCode };
	            var result = await _siteLicenseRepository.GetVmElaTokens(authCode);
                var isUnbounded = CheckVmElaUnboundedStatus(vmType, authCode, result);
	            if (isUnbounded && result.AdditionalTokenAppliedDate == null)
	            {
	                await _siteLicenseRepository.ApplyVmElaAdditionalTokens(authCode, result.TotalTokens);
	                result.AdditionalTokenAppliedDate = DateTime.Now;
	            }

	            return result;
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetVmElaTokens exception: {0}, {1}", e.Message, e.StackTrace)));
	            return new VmElaTokenModel { GrandParentAuthCode = authCode };
	        }
	    }

        //check if it is in unbounded period or not, and return the total token if it is in unbounded period.
        public static bool CheckVmElaUnboundedStatus(EVmType vmType, string authCode, VmElaTokenModel model)
	    {
	        if (model.AdditionalTokenAppliedDate.HasValue)
	            return true;

	        bool unBoundedPeriod = false;
            //If the VM ELA contract term is <=12 months; No Mid-term CAP; Unbounded the customer receives 500,000 tokens 
            //in addition to their total token quantity.
	        if (model.TermMonths <= 12)
	            unBoundedPeriod = true;
            //If the VM ELA contract term is >12 months & <=36 months; The customer is limited to their total token quantity up until 
            //the Mid-term CAP is over, then the remaining period is unbounded and the customer receives an additional 500,000 tokens.
            else if (model.TermMonths > 12 && model.TermMonths <= 36)
	        {
	            if (model.RegistrationDate != null)
	            {
	                var midTerm = model.RegistrationDate.Value + new TimeSpan(model.TermMonths * 15, 0, 0, 0);
	                if (DateTime.Now > midTerm)
	                    unBoundedPeriod = true;
	            }
	        }
            //If the VM ELA contract term is > 36 months & <=60 months; The customer is limited to their total token quantity up 
            //until 60% of the term is over, then the remaining period is unbounded and the customer receives an additional 500,000 tokens.
            else if (model.TermMonths > 36 && model.TermMonths <= 72)
	        {
	            if (model.RegistrationDate != null)
	            {
	                var midTerm = model.RegistrationDate.Value + new TimeSpan(model.TermMonths * 18, 0, 0, 0);
	                if (DateTime.Now > midTerm)
	                    unBoundedPeriod = true;
	            }
	        }

	        //If the customer reaches 10% left of their Total Token Amount in the Unbounded Period, the system should add 500,000 Tokens only once.
	        if (unBoundedPeriod && model.AdditionalTokenAppliedDate == null)
	        {
	            model.TotalTokens += 500000L;                    
	        }

	        return unBoundedPeriod;
	    }

        public async Task<IEnumerable<VmElaChildModelInfo>> GetCurrentAccountVmElaModels(EVmType vmType, string grandParentAuthCode, long supportAccountId)
	    {
	        try
	        {
	            return await _siteLicenseRepository.GetCurrentAccountVmElaModels(grandParentAuthCode, supportAccountId);
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetCurrentAccountVmElaModels exception: {0}, {1}", e.Message, e.StackTrace)));
	            return new List<VmElaChildModelInfo>();
	        }
	    }

        public async Task<string> SaveVmElaAllocatedQuantities(EVmType vmType, string grandParentAuthCode, long totalAllocatedTokens, Dictionary<string, IEnumerable<VmElaChildModelInfo>> models)
	    {
	        try
	        {
	            await _siteLicenseRepository.SaveVmElaAllocatedQuantities(grandParentAuthCode,totalAllocatedTokens, models);
	            return string.Empty;
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("SaveVmElaAllocatedQuantities exception: {0}, {1}", e.Message, e.StackTrace)));
	            return e.Message;
	        }
	    }

        public async Task<string> GrantVmElaTokens(EVmType vmType, string grandParentAuthCode, long curUserId, string destinationEmail, bool notifyUser, long supportAccountId)
	    {
	        try
	        {
	            var userAccountDataAccess = new UserAccountDataAccess();
                var emailExists = userAccountDataAccess.IsEmailExists(destinationEmail);
	            if (!emailExists)
	                return "The destination email must be a registered user.";

               
	            var result = await _siteLicenseRepository.GrantVmElaTokens(grandParentAuthCode, curUserId, destinationEmail);

                long tokenId = _siteLicenseRepository.GetVmElaTokenId(grandParentAuthCode,curUserId,destinationEmail);
               
                if (string.IsNullOrEmpty(result) && notifyUser)
                {
                    EmailHelper.SendVmElaNotification(curUserId, destinationEmail, grandParentAuthCode, supportAccountId, ApiConstants.VM_ELA_GRANT_TOKEN_ACCESS);
                }

                if (string.IsNullOrEmpty(result) && tokenId > 0)
                {
                    #region Publish Event to Pubsub

                    AppNotificationData newUserAppNotificationData = new AppNotificationData()
                    {
                        AccountId = Convert.ToString(supportAccountId),
                        EntityId = Convert.ToString(tokenId),
                        UserId = Convert.ToString(curUserId)
                    };

                    AppNotification newUserNotification = new AppNotification()
                    {
                        AppData = newUserAppNotificationData,
                        Category = NotificationCategory.VMELATokenGrant.ToDescription(),
                        DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription() },
                        Source = NotificationSource.Csp.ToDescription()
                    };
					EventPublisher eventPublisher = new EventPublisher();
                    await eventPublisher.PublishEvent(newUserNotification);
                    #endregion
                }

	            return result;
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GrantVmElaTokens exception: {0}, {1}", e.Message, e.StackTrace)));
	            return "Encountered an error during token grant process.";
	        }
	    }

        private void NotifyUser(EVmType vmType, long curUserId, string destinationEmail, string grandParentAuthCode)
        {
            var userAccountDataAccess = new UserAccountDataAccess();

            var sourceUser = userAccountDataAccess.Read(curUserId, curUserId);

            const string subject = "Palo Alto Networks - A VM-ELA Auth-Code has been granted to you";

            var sbBody = string.Format(@"{0} has granted you to use this VM-ELA Auth-Code: {1}.<br/>
You can accept it by logging into your Palo Alto Networks Support account at
http://support.paloaltonetworks.com.<br/>
<br/>
For questions about this grant, please contact {0}.<br/>
<br/>
For other questions, please contact Palo Alto Networks support at<br/>
support@paloaltonetworks.com or call us at<br/>
US: 1.866.898.9087 <br/>
Outside the US: +1.408.738.7799.<br/>
<br/>
This message comes from an automated system using an unmonitored mailbox.<br/>
Please do not respond to this message directly.<br/>", sourceUser.Email, grandParentAuthCode);

            EmailHelper.SendMail(destinationEmail, subject, sbBody.ToString(), false,"VM ELA Token");
            
        }

        public bool HasPendingTokensGrantRequest(EVmType vmType, long supportAccountId, long userId)
	    {
	        if (userId <= 0)
	            return false;
	        try
	        {
	            var result = _siteLicenseRepository.HasPendingTokensGrantRequest(supportAccountId, userId);
	            return result;
	        }
	        catch (Exception e)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("HasPendingTokensGrantRequest exception: {0}, {1}", e.Message, e.StackTrace)));
	            return false;
	        }
	    }

        public async Task<IEnumerable<ElaTokensGrantModel>> GetPendingElaTokensGrantItems(EVmType vmType, long supportAccountId, long userId)
	    {
	        try
	        {
                if(supportAccountId < 0 || userId < 0)
                    return new List<ElaTokensGrantModel>();

	            return await _siteLicenseRepository.GetPendingElaTokensGrantItems(supportAccountId, userId);
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetPendingElaTokensGrantItems exception: {0}, {1}", e.Message, e.StackTrace)));
                return new List<ElaTokensGrantModel>();
	        }
	    }

        public async Task<string> AcceptElaTokenGrant(EVmType vmType, ElaTokensGrantModel elaTokensGrantModel, bool accepted, long userAccountId)
	    {
	        try
	        {
                //generate the 5-Models' license
	            if (accepted)
	            {
                    var activatedBefore = await _siteLicenseRepository.VmElaModelsExist(elaTokensGrantModel.AuthorizationCode, elaTokensGrantModel.SupportAccountId);
	                if (activatedBefore)
	                {
	                    //update DB
                        await _siteLicenseRepository.AcceptElaTokenGrant(elaTokensGrantModel.AuthorizationCode, userAccountId, true);
	                    return string.Empty;
	                }

                    var result = GenerateVmElaModels(vmType, elaTokensGrantModel.AuthorizationCode, elaTokensGrantModel.SupportAccountId, userAccountId);
	                if (string.IsNullOrEmpty(result.ErrorMessage))
	                {
	                    //update DB
                        await _siteLicenseRepository.AcceptElaTokenGrant(elaTokensGrantModel.AuthorizationCode, userAccountId, true);
                        //Notify ELA Admin after grant is accepted
                        EmailHelper.SendVmElaNotification(userAccountId, elaTokensGrantModel.SourceUser, elaTokensGrantModel.AuthorizationCode, 
                            elaTokensGrantModel.SupportAccountId, ApiConstants.VM_ELA_GRANT_TOKEN_ACCEPTED, elaTokensGrantModel.GrantDate);
	                }
	                return result.ErrorMessage;	                    
	            }
	            else
	            {
                    await _siteLicenseRepository.AcceptElaTokenGrant(elaTokensGrantModel.AuthorizationCode, userAccountId, false);
	                return string.Empty;
	            }

	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("AcceptElaTokenGrant exception: {0}, {1}", e.Message, e.StackTrace)));
	            return e.Message;
	        }
	    }

        public async Task<IEnumerable<VmItemToDeactivate>> GetProvisionedVmsToDeactivate(EVmType vmType, long supportAccountId, string parentAuthCode)
	    {
	        try
	        {
                if(string.IsNullOrEmpty(parentAuthCode))
                    return new List<VmItemToDeactivate>();
	            return await _vmRegistrationRepository.GetProvisionedVmsToDeactivate(vmType, supportAccountId, parentAuthCode);
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetProvisionedVmsToDeactivate exception: {0}, {1}", e.Message, e.StackTrace)));
	            return new List<VmItemToDeactivate>();
	        }
	    }

        public async Task<string> DeactivateProvisionedVms(EVmType vmType, IEnumerable<VmItemToDeactivate> items, string email)
	    {
	        try
	        {
	            await _vmRegistrationRepository.DeactivateProvisionedVms(vmType, items, email);
	            return string.Empty;
	        }
	        catch (Exception e)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("DeactivateProvisionedVms exception: {0}, {1}", e.Message, e.StackTrace)));
	            return e.Message;
	        }
	    }

        public string RevokeVmElaLicense(EVmType vmType, string authCode, long supportAccountId)
	    {
	        try
	        {
	            var result = _virtualMachineDataAccess.RevokeVmElaLicense(vmType, authCode, supportAccountId);
                return result;
	        }
	        catch (Exception e)
	        {
	            var errorMessage = "An error occurred while revoking VM ELA license.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("RevokeVmElaLicense exception: {0}, {1}", e.Message, e.StackTrace)));
                return errorMessage;
	        }
	    }

        public ParseVmFileResult ParseVmFileByContent(EVmType vmType, string fileContent, string fileType)
        {

            var result = new ParseVmFileResult();
            if (fileType == "text/plain")
            {

                //Standardize line endings on \n
                string content = Regex.Replace(fileContent, "\r\n", "\n").Replace('\r', '\n');
                var keyValuePairs = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kvp in keyValuePairs)
                {

                    var kvpArray = kvp.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (kvpArray.Length != 2) continue;

                    var key = kvpArray[0];
                    var value = kvpArray[1];

                    switch (key)
                    {
                        case vm_cpuid:
                            result.CpuId = value;
                            break;

                        case vm_uuid:
                            result.UuId = value;
                            break;
                    }
                }
            }
            else
            {
                var xmlDoc = new XmlDocument();

                // Comments By LK On 12/04/2018
                // Fix to prevent XML External Entity attacks
                xmlDoc.XmlResolver = null;
                xmlDoc.InnerXml = fileContent;
                //  xmlDoc.Load(fileContent);
                var node = xmlDoc.SelectSingleNode(string.Format("//{0}", vm_uuid));
                if (node != null)
                    result.UuId = node.InnerText;
                node = xmlDoc.SelectSingleNode(string.Format("//{0}", vm_cpuid));
                if (node != null)
                    result.CpuId = node.InnerText;
            }

            var hasMissingValues = string.IsNullOrEmpty(result.CpuId) || string.IsNullOrEmpty(result.UuId);

            return hasMissingValues
                             ? new ParseVmFileResult
                             {
                                 Message = "File parsing error"
                             }
                             : result;
        }

        public IEnumerable<ElaTokensGrantModel> GetPendingElaTokensByUser(EVmType vmType, long supportAccountId, long userId, long entityId)
        {
            try
            {
                if (supportAccountId < 0 || userId < 0)
                    return new List<ElaTokensGrantModel>();

                return _siteLicenseRepository.GetPendingElaTokensByUser(supportAccountId, userId,entityId);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetPendingElaTokensByUser exception: {0}, {1}", e.Message, e.StackTrace)));
                return new List<ElaTokensGrantModel>();
            }
        }

        public async Task<CnAllocateAuthCodeTokenDetails> GetCnAuthCodeTokenDetails(long supportAccountId)
        {
            try
            {
                return await _vmRegistrationRepository.GetCnAuthCodeTokenDetails(supportAccountId);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetCnAuthCodeTokenDetails exception: {0}, {1}", e.Message, e.StackTrace)));
                return new CnAllocateAuthCodeTokenDetails();
            }
        }

        public async Task<List<CnAvailablePanorama>> GetCnAvailablePanorama(long supportAccountId)
        {
            try
            {
                return await _vmRegistrationRepository.GetCnAvailablePanorama(supportAccountId);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("GetCnAvailablePanorama exception: {0}, {1}", e.Message, e.StackTrace)));
                return new List<CnAvailablePanorama>();
            }
        }
    }
}
