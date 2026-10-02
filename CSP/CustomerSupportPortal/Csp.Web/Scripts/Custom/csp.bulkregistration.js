var BulkRegistrationManageUtil = function () {
    return {
        UrlBulkRegistrationDiscardDraft: null,
        UrlGetPrepBulkRegistration: null,

        Init: function (args) {
            // initialize defaults
            var self = this;

            self.UrlBulkRegistrationDiscardDraft = args.urlBulkRegistrationDiscardDraft;
            self.UrlGetPrepBulkRegistration = args.urlGetPrepBulkRegistration;
        },

        DiscardDraft: function (e) {
            var self = this;

            e.preventDefault ? e.preventDefault() : e.returnValue = false;
            $(e).prop('disabled', true);

            SupportPortal.ClearNotification();
            SupportPortal.BlockUI();

            $.ajax({
                url: self.UrlBulkRegistrationDiscardDraft,
                type: 'POST',
                dataType: 'json',
                data: SetAntiForgery(),
                cache: false
            }).fail(function (e) {
                SupportPortal.Notification("Failed to discard bulk registration.", SupportPortal.NotificationType.Error);
            }).done(function (e) {
                if (e.Success) {
                    location.href = self.UrlGetPrepBulkRegistration;
                }
            }).always(function () {
                SupportPortal.UnBlockUI();
                $(e).prop('disabled', false);
            });
        }
    };
}();

var PrepBulkRegistrationManageUtil = function () {
    return {
        FormId: null,
        FilterTypeId: null,
        ClaimMorePurchasesId: null,
        HdnFilterTypeId: null,
        PrepBulkRegGridId: null,
        PrepBulkRegAvailableLicensesGridId: null,
        PrepBulkRegDeviceAndLicensesGridId: null,
        ModelCompatibilityId: null,
        UrlPrepBulkRegSaveDraft: null,
        UrlGetCustomizeBulkRegistration: null,
        UrlGetPrepBulkRegistration: null,

        Init: function (args) {
            // initialize defaults
            var self = this;

            self.FormId = args.formId,
            self.FilterTypeId = args.filterTypeId,
            self.ClaimMorePurchasesId = args.claimMorePurchasesId,
            self.HdnFilterTypeId = args.hdnFilterTypeId,
            self.PrepBulkRegGridId = args.prepBulkRegGridId,
            self.PrepBulkRegAvailableLicensesGridId = args.prepBulkRegAvailableLicensesGridId,
            self.PrepBulkRegDeviceAndLicensesGridId = args.prepBulkRegDeviceAndLicensesGridId,
            self.ModelCompatibilityId = args.modelCompatibilityId,
            self.UrlPrepBulkRegSaveDraft = args.urlPrepBulkRegSaveDraft;
            self.UrlGetCustomizeBulkRegistration = args.urlGetCustomizeBulkRegistration;
            self.UrlGetPrepBulkRegistration = args.urlGetPrepBulkRegistration;
            self.UrlClaimMorePurchases = args.urlClaimMorePurchases;

            if (self.FilterTypeId === "RegisteredDevices") {
                self.ToggleDeviceDefaultsSectionDisplay(true);
            }

            self.BindEvents();
        },

        BindEvents: function () {
            var self = this;

            $('input[name=' + self.FilterTypeId + ']').change(self.OnFilterTypeChange);

            $('#' + self.ClaimMorePurchasesId).click(self.OnClaimMorePurchasesClick);

            $('#' + self.PrepBulkRegGridId).on('click', 'button.selectall', self.AvailableDevicesGrid.OnSelectAllClick);
            $('#' + self.PrepBulkRegGridId).on('click', 'button.unselectall', self.AvailableDevicesGrid.OnUnselectAllClick);
            $('#' + self.PrepBulkRegGridId).on('click', 'input.selectone', self.AvailableDevicesGrid.OnSelectOneClick);

            $('#' + self.PrepBulkRegAvailableLicensesGridId).on('click', 'a.apply-license', self.AvailableLicensesGrid.OnSelectLicenseClick);
            $('.device-defaults-header').on('click', self.ToggleDeviceDefaultsDisplay);
        },

        GetLastModelCompatibility: function () {
            return $('#' + PrepBulkRegistrationManageUtil.ModelCompatibilityId).val();
        },

        SetLastModelCompatibility: function (value) {
            $('#' + PrepBulkRegistrationManageUtil.ModelCompatibilityId).val(value);
        },

        ReloadAvailableLicensesGrid: function () {
            $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegAvailableLicensesGridId).data('kendoGrid').dataSource.read();
        },

        OnFilterTypeChange: function () {
            var filterType = $(this).val();
            var devicesAndLicensesGrid = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid');
            var lastFilterType = $('#' + PrepBulkRegistrationManageUtil.HdnFilterTypeId).val();

            var changeAction = function () {
                PrepBulkRegistrationManageUtil.SetLastModelCompatibility("");
                PrepBulkRegistrationManageUtil.AvailableDevicesGrid.Reload();
                $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegAvailableLicensesGridId).data('kendoGrid').dataSource.read();
                devicesAndLicensesGrid.dataSource.data([]);
                $('#' + PrepBulkRegistrationManageUtil.HdnFilterTypeId).val(filterType);

                if (filterType === "RegisteredDevices") {
                    PrepBulkRegistrationManageUtil.ToggleDeviceDefaultsSectionDisplay(true);
                } else {
                    PrepBulkRegistrationManageUtil.ToggleDeviceDefaultsSectionDisplay(false);
                }
            };

            var cancelAction = function () {
                $('input[name=' + PrepBulkRegistrationManageUtil.FilterTypeId + '][value=' + lastFilterType + ']').prop('checked', true);

                if (lastFilterType === "RegisteredDevices") {
                    PrepBulkRegistrationManageUtil.ToggleDeviceDefaultsSectionDisplay(true);
                } else {
                    PrepBulkRegistrationManageUtil.ToggleDeviceDefaultsSectionDisplay(false);
                }
            };

            if (filterType !== lastFilterType && devicesAndLicensesGrid.dataSource.total() > 0) {
                SupportPortal.Confirmation("Changing filter type will discard your changes. Are you sure you want to continue?", function () { changeAction(); }, function () { cancelAction(); });
            } else {
                changeAction();
            }
        },

        OnClaimMorePurchasesClick: function (e) {
            var self = this;

            e.preventDefault ? e.preventDefault() : e.returnValue = false;

            var devicesAndLicensesGrid = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid');

            if (devicesAndLicensesGrid.dataSource.total() > 0) {
                $.proxy(PrepBulkRegistrationManageUtil.SaveDraft, { isDraft: true, redirectToClaimedAssets: true }, $('.bulk-registration-prep-save-draft'))();
            }
        },

        SaveDraft: function (e) {
            var self = this;

            e.preventDefault ? e.preventDefault() : e.returnValue = false;
            
            var devicefilter=$('#' + PrepBulkRegistrationManageUtil.HdnFilterTypeId).val();
            var validator = $('#' + PrepBulkRegistrationManageUtil.FormId).data("kendoValidator");

            if (!self.isDraft && !(validator.validate() || devicefilter === 'RegisteredDevices')) {
                if ($('.device-defaults-header').find('.toggle').hasClass('plus')) {
                    $('.device-defaults-header').trigger('click');
                }

                return false;
            }

            var data = SetAntiForgery();
            data.DevsAndLics = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid').dataSource.data();
            data = $.extend(data, PrepBulkRegistrationManageUtil.DeviceDefaults.GetDefaults());
            data.SelectDevicesFilter = $('#' + PrepBulkRegistrationManageUtil.HdnFilterTypeId).val();

            $(e).prop('disabled', true);
            SupportPortal.ClearNotification();
            SupportPortal.BlockUI();

            $.ajax({
                url: PrepBulkRegistrationManageUtil.UrlPrepBulkRegSaveDraft,
                type: 'post',
                dataType: 'json',
                contentType: "application/json",
                traditional: true,
                data: JSON.stringify(data)
            }).fail(function (e) {
                SupportPortal.Notification("Failed to save your bulk registration.", SupportPortal.NotificationType.Error);
            }).done(function (e) {
                if (e.Success) {
                    if (self.isDraft) {
                        if (self.redirectToClaimedAssets) {
                            location.href = PrepBulkRegistrationManageUtil.UrlClaimMorePurchases + '&bulkRegistrationId=' + e.BulkRegistrationId;
                        } else {
                            location.href = PrepBulkRegistrationManageUtil.UrlGetPrepBulkRegistration + '&bulkRegistrationId=' + e.BulkRegistrationId;
                        }
                    } else {
                        location.href = PrepBulkRegistrationManageUtil.UrlGetCustomizeBulkRegistration + '&bulkRegistrationId=' + e.BulkRegistrationId;
                    }
                } else {
                    SupportPortal.Notification(e.Message, SupportPortal.NotificationType.Error);
                }
            }).always(function () {
                SupportPortal.UnBlockUI();
                $(e).prop('disabled', false);
            });
        },

        ToggleDeviceDefaultsDisplay: function (e) {
            var srcElement = $(e.currentTarget);
            var targetElement = srcElement.find('.toggle');

            targetElement.toggleClass('plus minus');

            if (targetElement.hasClass('minus')) {
                srcElement.siblings('.device-defaults').slideDown("slow");
            } else {
                srcElement.siblings('.device-defaults').slideUp("slow");
            }
        },

        ToggleDeviceDefaultsSectionDisplay: function (isHidden) {
            if (isHidden) {
                $('.device-defaults-section').hide();
            } else {
                $('.device-defaults-section').show();
            }
        }
    };
}();

PrepBulkRegistrationManageUtil.AvailableDevicesGrid = function () {

    function clearOtherModelCompatibilitySelections(newModelCompatibility, checkAll) {
        var otherModelCompatibilities = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegGridId).find('input.selectone').filter(function () {
            var checkbox = $(this);
            var thisModelCompat = checkbox.data("modelcompatibility");
            var isDiff = thisModelCompat !== newModelCompatibility;
            var isChecked = (checkAll && !isDiff);

            checkbox.prop('checked', isChecked);

            return isDiff; //only return different ones
        });

        otherModelCompatibilities.prop('checked', false);

        var lastModelCompat = PrepBulkRegistrationManageUtil.GetLastModelCompatibility();

        if ($('#' + PrepBulkRegistrationManageUtil.PrepBulkRegGridId + ' input.selectone:checked').length === 0) {
            newModelCompatibility = "";
        }

        if (lastModelCompat !== newModelCompatibility.toString()) {
            var devicesAndLicensesGrid = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid');

            //var changeAction = function() {
            PrepBulkRegistrationManageUtil.SetLastModelCompatibility(newModelCompatibility);
            PrepBulkRegistrationManageUtil.ReloadAvailableLicensesGrid();
            devicesAndLicensesGrid.dataSource.data([]);
            //PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.AddDevice();
            //};

            //if (devicesAndLicensesGrid.dataSource.total() > 0) {
            //    SupportPortal.Confirmation("Changing model compatibility will discard your changes. Are you sure you want to continue?", function() { changeAction(); }, null);
            //} else {
            //    changeAction();
            //}
        }

        PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.AddDevice();
    }

    return {
        GetParams: function () {
            var p = SetAntiForgery();
            p.selectDevicesFilter = $('input[name=' + PrepBulkRegistrationManageUtil.FilterTypeId + ']:checked').val();
            return p;
        },

        GetSelectedDeviceIds: function () {
            var devIds = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegGridId + ' input.selectone:checked').map(function () {
                return this.value;
            }).get();

            return devIds;
        },

        OnSelectAllClick: function () {
            var modelCompat = $(this).data('modelcompatibility');

            clearOtherModelCompatibilitySelections(modelCompat, true);
        },

        OnUnselectAllClick: function () {
            var modelCompat = $(this).data('modelcompatibility');

            clearOtherModelCompatibilitySelections(modelCompat, false);
        },

        OnSelectOneClick: function () {
            var checkbox = $(this);
            var modelCompat = checkbox.data('modelcompatibility');

            clearOtherModelCompatibilitySelections(modelCompat);
        },

        OnGridAvailableDevicesRequestStart: function (e) {
            SupportPortal.BlockUI();

            if (e.type === "read") {

            }
        },

        OnGridAvailableDevicesRequestEnd: function (e) {
            SupportPortal.UnBlockUI();

            if (e.type === "read") {

            }
        },

        OnGridAvailableDevicesDataBound: function (e) {
            var grid = e.sender;

            if (grid.dataSource.total() === 0) {
                var columnCount = grid.columns.length;
                $(e.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="grid-empty-message-row"><td colspan="' + columnCount + '" class="grid-empty-message">No devices found</td></tr>');
            }
        },

        Reload: function () {
            $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegGridId).data('kendoGrid').dataSource.read({
                reload: true
            });
        }
    };
}();

PrepBulkRegistrationManageUtil.AvailableLicensesGrid = function () {

    return {
        GetParams: function () {
            var lastModelCompat = PrepBulkRegistrationManageUtil.GetLastModelCompatibility();

            var p = {};
            p.modelCompatibility = lastModelCompat;
            return p;
        },

        OnSelectLicenseClick: function (e) {
            var selectLink = $(e.currentTarget);
            var salesordernumber = selectLink.data('salesordernumber').toString();
            var partnumber = selectLink.data('partnumber');
            var grid = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegAvailableLicensesGridId).data('kendoGrid');
            var data = grid.dataSource.data();
            var devsAndLicsGrid = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid');
            var devsAndLicsGridData = devsAndLicsGrid.dataSource.data();
            //alert(salesordernumber + ' - ' + partnumber);

            //var coreLicType = getCoreLicType(partnumber);
            //if (!coreLicType) {
            //    return;
            //}

            var row = _.findWhere(data, { SalesOrderNumber: salesordernumber, PartNumber: partnumber });
            if (!row) {
                throw 'Error: no row found';
            }

            while (parseInt(row.Quantity) > 0) {

                var licenseId = row.LicenseIds[0];
                var bundles = row.Bundles;

                var result = PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.AddLicense(devsAndLicsGridData, licenseId, partnumber, bundles);
                if (!result.success) {
                    break;
                }

                row.LicenseIds.splice(0, 1); //remove the first licenseID from the list
                row.Quantity--;
            }

            grid.refresh();
            devsAndLicsGrid.refresh();
        },

        OnGridAvailableLicensesDataBound: function (e) {
            var grid = e.sender;

            if (grid.dataSource.total() === 0) {
                var columnCount = grid.columns.length;
                $(e.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="grid-empty-message-row"><td colspan="' + columnCount + '" class="grid-empty-message">No licenses found</td></tr>');
            }
        }
    };
}();

PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid = function () {

    var licConsts = {
        SVC: 'SVC',
        TP: 'TP',
        URL: 'URL',
        GPPortal: 'GP Portal',
        GPGateway: 'GP Gateway',
        WF: 'WF',
        BND: 'BND',
        WRNT: 'WRNT',
        DNS: 'DNS',
        ADNS: 'ADNS',
        SDWAN: 'SDWAN',
        AWF: 'AWF',
        ADVURL: 'ADVURL',
        ATP: 'ATP',      
        ASDWAN:'ASDWAN',
        PAA: 'PAA',
        PanoramaManagement:'PanoramaManagement'

    };

    var licTypes = [
        { regex: /-SVC-WRNT/, type: licConsts.WRNT },
        { regex: /-SVC-/, type: licConsts.SVC },
        { regex: /-SVC$/, type: licConsts.SVC },

        { regex: /-TP-/, type: licConsts.TP },
        { regex: /-TP$/, type: licConsts.TP },

        { regex: /-ATP-/, type: licConsts.TP },
        { regex: /-ATP$/, type: licConsts.TP },

        { regex: /-URL2-/, type: licConsts.URL },
        { regex: /-URL2$/, type: licConsts.URL },
        { regex: /-URL4-/, type: licConsts.URL },
        { regex: /-URL4$/, type: licConsts.URL },
        { regex: /-ADVURL/, type: licConsts.URL },
        { regex: /-ADVURL$/, type: licConsts.URL },

        //-PORTAL- MUST be listed BEFORE -GP-
        { regex: /-PORTAL-/, type: licConsts.GPPortal },
        { regex: /-PORTAL$/, type: licConsts.GPPortal },

        { regex: /-GP-/, type: licConsts.GPGateway },
        { regex: /-GP$/, type: licConsts.GPGateway },

        { regex: /-WF-/, type: licConsts.WF },
        { regex: /-WF$/, type: licConsts.WF },
        { regex: /-AWF-/, type: licConsts.WF },
        { regex: /-AWF$/, type: licConsts.WF },

        { regex: /-DNS-/, type: licConsts.DNS },
        { regex: /-DNS$/, type: licConsts.DNS },
        { regex: /-ADNS-/, type: licConsts.DNS },
        { regex: /-ADNS$/, type: licConsts.DNS },

        { regex: /-SDWAN-/, type: licConsts.SDWAN },
        { regex: /-SDWAN$/, type: licConsts.SDWAN },
        { regex: /-ASDWAN-/, type: licConsts.SDWAN },
        { regex: /-ASDWAN$/, type: licConsts.SDWAN },

        { regex: /-PAA-/, type: licConsts.PAA },
        { regex: /-PAA$/, type: licConsts.PAA },

        { regex: /-M-P-\d+/, type: licConsts.PanoramaManagement },
        { regex: /-M-\d+-P-\d+/, type: licConsts.PanoramaManagement },

        { regex: /-BND/, type: licConsts.BND }
    ];

    function getGrid() {
        return $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid');
    }

    function getDataSource() {
        var grid = getGrid();

        return (grid) ? grid.dataSource : [];
    }

    function getCoreLicType(partNumber) {

        for (var i = 0; i < licTypes.length; i++) {
            var curLicType = licTypes[i];
            if (curLicType.regex.test(partNumber)) {
                return curLicType.type;
            }
        }

        return null;
    }

    function getLicType(partNumber) {
        var licType = '';
        var coreLicType = getCoreLicType(partNumber);
        if (coreLicType === licConsts.SVC) {
            licType = 'StandardSupport';
        }
        else if (coreLicType === licConsts.TP) {
            licType = 'ThreatProtection';
        }
        else if (coreLicType === licConsts.URL) {
            licType = 'Url';
        }
        else if (coreLicType === licConsts.GPPortal) {
            licType = 'GpPortal';
        }
        else if (coreLicType === licConsts.GPGateway) {
            licType = 'GpGateway';
        }
        else if (coreLicType === licConsts.WF) {
            licType = 'Wildfire';
        }
        else if (coreLicType === licConsts.WRNT) {
            licType = 'Warranty';
        }
        else if (coreLicType === licConsts.DNS) {
            licType = 'Dns';
        }
        else if (coreLicType === licConsts.SDWAN) {
            licType = 'Sdwan';
        }
        else if (coreLicType === licConsts.PAA) {
            licType = 'Paa';
        }
        else if (coreLicType === licConsts.PanoramaManagement) {
            licType = 'PanoramaManagement';
        }
        else if (coreLicType === licConsts.ADVURL) {
            licType = 'Url';
        }
        else if (coreLicType === licConsts.AWF) {
            licType = 'Wildfire';
        }
        else if (coreLicType === licConsts.ATP) {
            licType = 'ThreatProtection';
        }
        else if (coreLicType === licConsts.ADNS) {
            licType = 'Dns';
        }

        return licType;
    }

    function hasActivatedLicense(data, group) {
        var hasActiveLicense = false;

        if (data.ActivatedLicenses) {
            var activatedLics = data.ActivatedLicenses;
            var activatedLicsLength = activatedLics.length;

            for (var i = 0; i < activatedLicsLength; i++) {
                var licType = getLicType(activatedLics[i].PartNumber);

                if (licType === group) {
                    hasActiveLicense = true;
                    break;
                } else if (activatedLics[i].Bundles) {
                    var activatedFeatureLics = activatedLics[i].Bundles;
                    var activatedFeatureLicsLength = activatedFeatureLics.length;

                    for (var j = 0; j < activatedFeatureLicsLength; j++) {
                        licType = getLicType(activatedFeatureLics[j].PartNumber);

                        if (licType === group) {
                            hasActiveLicense = true;
                            break;
                        }
                    }
                }
            }
        }

        return hasActiveLicense;
    }

    function hasActivatedBundleLicense(data, bundles) {
        var hasActiveLicense = false;
        var featureLicTypeName;

        for (var i = 0; i < bundles.length; i++) {
            featureLicTypeName = getLicType(bundles[i].PartNumber);

            if (hasActivatedLicense(data, featureLicTypeName)) {
                hasActiveLicense = true;
                break;
            }
        }

        return hasActiveLicense;
    }

    function tryApplyLic(data, licTypeName, licenseId, partNumber) {
        var licenseIdFieldName = licTypeName + 'LicenseId';
        var displayNameFieldName = licTypeName + 'DisplayName';
        var searchFilter = {};

        searchFilter[licenseIdFieldName] = null;

        var row = _.findWhere(data, searchFilter);

        if (row && !hasActivatedLicense(row, licTypeName)) {
            row[licenseIdFieldName] = licenseId;
            row[displayNameFieldName] = partNumber;

            return { success: true };
        }

        return { success: false };
    }

    function tryApplyBundleLic(data, licTypeName, licenseId, partNumber, bundles) {
        var licenseIdFieldName = licTypeName + 'LicenseId';
        var displayNameFieldName = licTypeName + 'DisplayName';

        var searchFilter = {};
        var i;
        var featureLicTypeName;
        var featureLicenseIdFieldName;
        var featureDisplayNameFieldName;

        if (bundles) {
            for (i = 0; i < bundles.length; i++) {
                featureLicTypeName = getLicType(bundles[i].PartNumber);
                featureLicenseIdFieldName = featureLicTypeName + 'LicenseId';

                searchFilter[featureLicenseIdFieldName] = null;
            }

            var row = _.findWhere(data, searchFilter);

            if (row && !hasActivatedBundleLicense(row, bundles)) {
                row[licenseIdFieldName] = licenseId;
                row[displayNameFieldName] = partNumber;

                for (i = 0; i < bundles.length; i++) {
                    featureLicTypeName = getLicType(bundles[i].PartNumber);
                    featureLicenseIdFieldName = featureLicTypeName + 'LicenseId';
                    featureDisplayNameFieldName = featureLicTypeName + 'DisplayName';

                    row[featureLicenseIdFieldName] = licenseId;
                    row[featureDisplayNameFieldName] = partNumber;
                }

                return { success: true };
            }
        }

        return { success: false };
    }

    function isBundleLicense(partNumber) {
        return (partNumber && partNumber.indexOf('BND') !== -1);
    }

    function addDevice() {
        var deviceIds = _.map(PrepBulkRegistrationManageUtil.AvailableDevicesGrid.GetSelectedDeviceIds(), function (id) {
            return parseInt(id);
        });
        var availableDevicesData = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegGridId).data('kendoGrid').dataSource.data().toJSON();;
        var devicesAndLicensesData = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid').dataSource.data().toJSON();;

        var existingDeviceIds = _.pluck(devicesAndLicensesData, 'DeviceId');
        var newDeviceIds = _.difference(deviceIds, existingDeviceIds);
        var remDeviceIds = _.difference(existingDeviceIds, deviceIds);

        var availDevicesData = _.flatten(_.map(availableDevicesData, function (data) {
            return data.items;
        }));

        if (remDeviceIds.length > 0) {
            devicesAndLicensesData = _.reject(devicesAndLicensesData, function (data) {
                if (remDeviceIds.indexOf(data.DeviceId) !== -1) {
                    removeDevice(data);
                }

                return (remDeviceIds.indexOf(data.DeviceId) !== -1);
            });
        }

        if (newDeviceIds.length > 0) {
            var query = new kendo.data.Query(availDevicesData);
            var deviceFilter = { logic: "or", filters: [] };

            for (var i = 0; i < newDeviceIds.length; i++) {
                deviceFilter.filters.push({ field: "DeviceId", operator: "eq", value: parseInt(newDeviceIds[i]) });
            }

            var filteredData = query.filter(deviceFilter).data;
            //query.filter(deviceFilter).data;
            //kendo.data.Query.process(availableDevicesData, deviceFilter);

            for (var i = 0; i < filteredData.length; i++) {

                devicesAndLicensesData.push({
                    BulkRegistrationDeviceId: null,
                    DeviceId: filteredData[i].DeviceId,
                    SerialNumber: filteredData[i].SerialNumber,
                    AssetPartNumber: filteredData[i].PartNumber,
                    AssetDisplayName: filteredData[i].SerialNumber + ' ' + filteredData[i].PartNumber,
                    StandardSupportLicenseId: null,
                    StandardSupportAuthCode: null,
                    StandardSupportExpiration: null,
                    StandardSupportDisplayName: null,
                    ThreatProtectionLicenseId: null,
                    ThreatProtectionAuthCode: null,
                    ThreatProtectionExpiration: null,
                    ThreatProtectionDisplayName: null,
                    UrlLicenseId: null,
                    UrlAuthCode: null,
                    UrlExpiration: null,
                    UrlDisplayName: null,
                    GpPortalLicenseId: null,
                    GpPortalAuthCode: null,
                    GpPortalExpiration: null,
                    GpPortalDisplayName: null,
                    GpGatewayLicenseId: null,
                    GpGatewayAuthCode: null,
                    GpGatewayExpiration: null,
                    GpGatewayDisplayName: null,
                    WildfireLicenseId: null,
                    WildfireAuthCode: null,
                    WildfireExpiration: null,
                    WildfireDisplayName: null,
                    BundleLicenseId: null,
                    BundleAuthCode: null,
                    BundleExpiration: null,
                    BundleDisplayName: null,
                    DnsLicenseId: null,
                    DnsAuthCode: null,
                    DnsExpiration: null,
                    DnsDisplayName: null,
                    SdwanLicenseId: null,
                    SdwanAuthCode: null,
                    SdwanExpiration: null,
                    SdwanDisplayName: null,
                    PaaLicenseId: null,
                    PaaAuthCode: null,
                    PaaExpiration: null,
                    PaaDisplayName: null,
                    PanoramaManagementLicenseId: null,
                    PanoramaManagementAuthCode: null,
                    PanoramaManagementExpiration: null,
                    PanoramaManagementDisplayName: null,
                    ActivatedLicenses: filteredData[i].ActivatedLicenses
                });
            }
        }

        $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid').dataSource.data(devicesAndLicensesData);
    }

    function removeDevice(device) {
        var grid = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegAvailableLicensesGridId).data('kendoGrid');
        var data = grid.dataSource.data();
        var licenseMap = {};

        if (!isBundleLicense(device['StandardSupportDisplayName']) && device['StandardSupportLicenseId']) {
            licenseMap[device['StandardSupportDisplayName']] = device['StandardSupportLicenseId'];
        }

        if (!isBundleLicense(device['ThreatProtectionDisplayName']) && device['ThreatProtectionLicenseId']) {
            licenseMap[device['ThreatProtectionDisplayName']] = device['ThreatProtectionLicenseId'];
        }

        if (!isBundleLicense(device['UrlDisplayName']) && device['UrlLicenseId']) {
            licenseMap[device['UrlDisplayName']] = device['UrlLicenseId'];
        }

        if (!isBundleLicense(device['GpPortalDisplayName']) && device['GpPortalLicenseId']) {
            licenseMap[device['GpPortalDisplayName']] = device['GpPortalLicenseId'];
        }

        if (!isBundleLicense(device['GpGatewayDisplayName']) && device['GpGatewayLicenseId']) {
            licenseMap[device['GpGatewayDisplayName']] = device['GpGatewayLicenseId'];
        }

        if (!isBundleLicense(device['WildfireDisplayName']) && device['WildfireLicenseId']) {
            licenseMap[device['WildfireDisplayName']] = device['WildfireLicenseId'];
        }

        if (!isBundleLicense(device['DnsDisplayName']) && device['DnsLicenseId']) {
            licenseMap[device['DnsDisplayName']] = device['DnsLicenseId'];
        }

        if (!isBundleLicense(device['SdwanDisplayName']) && device['SdwanLicenseId']) {
            licenseMap[device['SdwanDisplayName']] = device['SdwanLicenseId'];
        }

        if (!isBundleLicense(device['PaaDisplayName']) && device['PaaLicenseId']) {
            licenseMap[device['PaaDisplayName']] = device['PaaLicenseId'];
        }

        if (!isBundleLicense(device['PanoramaManagementDisplayName']) && device['PanoramaManagementLicenseId']) {
            licenseMap[device['PanoramaManagementDisplayName']] = device['PanoramaManagementLicenseId'];
        }

        if (device['BundleLicenseId']) {
            licenseMap[device['BundleDisplayName']] = device['BundleLicenseId'];
        }

        var partNumbers = _.keys(licenseMap);

        if (partNumbers.length > 0) {
            $.map(data, function (item, index) {
                if (partNumbers.indexOf(item.PartNumber) !== -1) {
                    item.LicenseIds.push(licenseMap[item.PartNumber]);
                    item.Quantity++;
                }
            });

            grid.refresh();
        }
    }

    function addLicense(data, licenseId, partNumber, bundles) {
        var licType = getCoreLicType(partNumber);
        if (!licType) {
            return { success: false };
        }

        //loop through devices looking for a dev that the lic can be applied to
        switch (licType) {
            case licConsts.SVC:
                return tryApplyLic(data, 'StandardSupport', licenseId, partNumber);
            case licConsts.TP:
                return tryApplyLic(data, 'ThreatProtection', licenseId, partNumber);
            case licConsts.ATP:
                return tryApplyLic(data, 'ThreatProtection', licenseId, partNumber);
            case licConsts.URL:
                return tryApplyLic(data, 'Url', licenseId, partNumber);
            case licConsts.ADVURL:
                return tryApplyLic(data, 'Url', licenseId, partNumber);
            case licConsts.GPPortal:
                return tryApplyLic(data, 'GpPortal', licenseId, partNumber);
            case licConsts.GPGateway:
                return tryApplyLic(data, 'GpGateway', licenseId, partNumber);
            case licConsts.WF:
                return tryApplyLic(data, 'Wildfire', licenseId, partNumber);
            case licConsts.AWF:
                return tryApplyLic(data, 'Wildfire', licenseId, partNumber);
            case licConsts.DNS:
                return tryApplyLic(data, 'Dns', licenseId, partNumber);
            case licConsts.ADNS:
                return tryApplyLic(data, 'Dns', licenseId, partNumber);
            case licConsts.SDWAN:
                return tryApplyLic(data, 'Sdwan', licenseId, partNumber);
            case licConsts.PAA:
                return tryApplyLic(data, 'Paa', licenseId, partNumber);
            case licConsts.PanoramaManagement:
                return tryApplyLic(data, 'PanoramaManagement', licenseId, partNumber);
            case licConsts.BND:
                return tryApplyBundleLic(data, 'Bundle', licenseId, partNumber, bundles);
        }

        return { success: false };
    }

    function swapLicenses(sourceRow, destRow, fieldName) {
        var destLicId = destRow[fieldName + 'LicenseId'];
        var destDisplay = destRow[fieldName + 'DisplayName'];
        var destAuthCode = destRow[fieldName + 'AuthCode'];
        var destExpiration = destRow[fieldName + 'Expiration'];

        destRow[fieldName + 'LicenseId'] = sourceRow[fieldName + 'LicenseId'];
        destRow[fieldName + 'DisplayName'] = sourceRow[fieldName + 'DisplayName'];
        destRow[fieldName + 'AuthCode'] = sourceRow[fieldName + 'AuthCode'];
        destRow[fieldName + 'Expiration'] = sourceRow[fieldName + 'Expiration'];

        sourceRow[fieldName + 'LicenseId'] = destLicId;
        sourceRow[fieldName + 'DisplayName'] = destDisplay;
        sourceRow[fieldName + 'AuthCode'] = destAuthCode;
        sourceRow[fieldName + 'Expiration'] = destExpiration;

        return destLicId;
    }

    function moveLicense(sourceSn, destSn, sourceLicId) {
        var grid = getGrid();
        var dataSource = getDataSource();
        var data = dataSource.data();
        var destSearchFilter = {
            SerialNumber: destSn
        };

        var destRow = _.findWhere(data, destSearchFilter);

        var sourceSearchFilter = {
            SerialNumber: sourceSn
        };

        var sourceRow = _.findWhere(data, sourceSearchFilter);

        var returnedDestLicIds = [];
        var destLicId;

        //check for ALL source sn licenses (not just first match - incase it's listed in multiple places, like a BND)
        if (sourceRow.StandardSupportLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'StandardSupport');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.ThreatProtectionLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'ThreatProtection');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.UrlLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Url');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.GpPortalLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'GpPortal');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.GpGatewayLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'GpGateway');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.WildfireLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Wildfire');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.DnsLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Dns');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.SdwanLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Sdwan');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.PaaLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Paa');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.PanoramaManagementLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'PanoramaManagement');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        if (sourceRow.BundleLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Bundle');

            if (destLicId) {
                returnedDestLicIds.push(destLicId);
            }
        }

        // if (licId still exists in dest row in any column) execute a swapLicenses method for that column
        for (var i = 0; i < returnedDestLicIds.length; i++) {
            var licId = returnedDestLicIds[i];

            if (destRow.StandardSupportLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'StandardSupport');
            }

            if (destRow.ThreatProtectionLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'ThreatProtection');
            }

            if (destRow.UrlLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'Url');
            }

            if (destRow.GpPortalLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'GpPortal');
            }

            if (destRow.GpGatewayLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'GpGateway');
            }

            if (destRow.WildfireLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'Wildfire');
            }

            if (destRow.DnsLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'Dns');
            }

            if (destRow.SdwanLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'Sdwan');
            }

            if (destRow.PaaLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'Paa');
            }

            if (destRow.PanoramaManagementLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'PanoramaManagement');
            }

            if (destRow.BundleLicenseId === licId) {
                swapLicenses(destRow, sourceRow, 'Bundle');
            }
        }

        grid.refresh();
    }

    return {
        GetLicType: function (partNumber) {
            return getLicType(partNumber);
        },

        AddDevice: function () {
            addDevice();
        },

        AddLicense: function (data, licenseId, partNumber, bundles) {
            return addLicense(data, licenseId, partNumber, bundles);
        },

        MoveLicense: function (sourceSn, destSn, sourceLicId) {
            if (sourceSn) {
                sourceSn = sourceSn.toString();
            }

            if (destSn) {
                destSn = destSn.toString();
            }

            moveLicense(sourceSn, destSn, sourceLicId);
        },

        OnDeviceAndLicensesDataBound: function (e) {
            var grid = e.sender;

            if (grid.dataSource.total() === 0) {
                var columnCount = grid.columns.length;
                $(e.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="grid-empty-message-row"><td colspan="' + columnCount + '" class="grid-empty-message">No licenses found</td></tr>');

                return;
            }

            PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.DragAndDrop.BindEvents();
        }
    };
}();

PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.DragAndDrop = function () {

    function highlightDropLocations(className, thisSerialNumber) {
        $('.' + className + '[data-serialnumber!=' + thisSerialNumber + ']').addClass('droppable');
    }

    function unhighlightDropLocations(className) {
        $('.' + className).removeClass('droppable');
    }

    function getDragGroup(target) {
        if (target.hasClass('draggable_license_disabled')) {
            return '';
        }

        var classList = target.prop('class').split(/\s+/);

        for (var i = 0; i < classList.length; i++) {

            var className = classList[i];
            var isMatch = /^drag_group_/.test(className);

            if (isMatch) {
                return className;
            }
        }
        return '';
    }

    function draggableOnDragStart(e) {
        var target = $(e.currentTarget[0]);
        var dragGroup = getDragGroup(target);

        if (dragGroup === '') {
            return;
        }

        var serialNumber = target.data('serialnumber');
        highlightDropLocations(dragGroup, serialNumber);
        target.addClass('dragging_license_shadow');
    }

    function draggableOnDragEnd(e) {
        var target = $(e.currentTarget[0]);
        var dragGroup = getDragGroup(target);
        unhighlightDropLocations(dragGroup);
        target.removeClass('dragging_license_shadow');
    }

    function droptargetOnDragEnter(e) {
        $(e.draggable.hint).addClass('draggable_hover');
        $(e.dropTarget).addClass('droppable_hover');
    }

    function droptargetOnDragLeave(e) {
        $(e.draggable.hint).removeClass('draggable_hover');
        $(e.dropTarget).removeClass('droppable_hover');
    }

    function droptargetOnDrop(e) {
        var target = $(e.target);
        var dropTarget = $(e.dropTarget);

        dropTarget.removeClass('droppable_hover');

        var sourceSn = target.data('serialnumber');
        var sourceLicId = target.data('licenseid');
        var destSn = dropTarget.data('serialnumber');

        if (sourceSn && destSn) {
            PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.MoveLicense(sourceSn, destSn, sourceLicId);
        }
    }

    var groups = [];

    function bindDragAndDrop() {
        for (var i = 0; i < groups.length; i++) {
            var groupName = groups[i];

            var dragDroppable = $("." + groupName);

            dragDroppable.kendoDraggable({
                group: groupName,
                hint: function (element) {
                    var clone = element.clone();
                    clone.addClass('draggable_license_dragging');
                    return clone;
                },
                dragstart: draggableOnDragStart,
                dragend: draggableOnDragEnd
            });

            dragDroppable.kendoDropTarget({
                group: groupName,
                dragenter: droptargetOnDragEnter,
                dragleave: droptargetOnDragLeave,
                drop: droptargetOnDrop
            });
        }
    }

    function trackUniqueGroups(group) {
        if ($.inArray(group, groups) === -1) {
            groups.push(group);
        }
    }

    function renderMovableCell(data, group) {
        var groupName = 'drag_group_' + group;
        trackUniqueGroups(groupName);

        var licIdFieldName = group + 'LicenseId';
        var licId = data[licIdFieldName];
        var displayNameFieldName = group + 'DisplayName';
        var displayName = data[displayNameFieldName];

        if (licId == null) {
            var hasActiveLicense = false;
            var activeLicId = '';

            if (data.ActivatedLicenses) {
                var activatedLics = data.ActivatedLicenses;
                var activatedLicsLength = activatedLics.length;

                for (var i = 0; i < activatedLicsLength; i++) {
                    var licType = PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.GetLicType(activatedLics[i].PartNumber);

                    if (licType === group) {
                        hasActiveLicense = true;
                        activeLicId = activatedLics[i].LicenseId;
                        break;
                    } else if (activatedLics[i].Bundles) {
                        var activatedFeatureLics = activatedLics[i].Bundles;
                        var activatedFeatureLicsLength = activatedFeatureLics.length;

                        for (var j = 0; j < activatedFeatureLicsLength; j++) {
                            licType = PrepBulkRegistrationManageUtil.DevicesAndLicensesGrid.GetLicType(activatedFeatureLics[j].PartNumber);

                            if (licType === group) {
                                hasActiveLicense = true;
                                activeLicId = activatedLics[i].LicenseId;
                                break;
                            }
                        }
                    }
                }
            }

            if (hasActiveLicense) {
                return kendo.format("<div class=\"draggable_license_disabled\" data-serialnumber=\"{0}\">&nbsp;<span class=\"k-icon k-update license-details\" data-lic-id=\"{1}\"></span></div>",
                    data.SerialNumber,
                    activeLicId
                );
            } else {
                return kendo.format("<div class=\"{0}\" data-serialnumber=\"{1}\">&nbsp;</div>",
                    groupName,
                    data.SerialNumber
                );
            }
        }

        var html = kendo.format("<span class=\"draggable_license {2}\" data-licenseid=\"{1}\" data-serialnumber=\"{3}\">{0} <i class=\"icon-move\"></i></span>",
            displayName,
            licId,
            groupName,
            data.SerialNumber
        );

        return html;
    }

    return {
        BindEvents: function () {
            bindDragAndDrop();

            // bind kendo tooltip to display license info on hover
            $('.license-details').kendoTooltip({
                position: "right",
                animation: {
                    open: {
                        effects: "fade:in",
                        duration: 1000
                    },
                    close: {
                        effects: "fade:out"
                    }
                },
                content: function(e) {
                    var dataItem = $('#' + PrepBulkRegistrationManageUtil.PrepBulkRegDeviceAndLicensesGridId).data('kendoGrid').dataItem($(e.target).closest("tr"));
                    var tooltipContents = '';

                    if (dataItem) {
                        var activeLicId = $(e.target).attr('data-lic-id');
                        var activeLic = (activeLicId) ? _.findWhere(dataItem.ActivatedLicenses, { LicenseId: parseInt(activeLicId) }) : null;
                        var template = kendo.template($("#LicenseDetailsTemplate").html());

                        if (activeLic) {
                            tooltipContents = template(activeLic);
                        }
                    }

                    return tooltipContents;
                }
            });
        },

        RenderMovableCell: function (data, group) {
            return renderMovableCell(data, group);
        }
    };
}();

PrepBulkRegistrationManageUtil.DeviceDefaults = function () {
    return {
        DeviceNameId: null,
        DeviceTagId: null,
        address1Id: null,
        address2Id: null,
        CityId: null,
        RegionTextId: null,
        RegionTextHiddenId: null,
        RegionTextDivId: null,
        RegionDropDownDivId: null,
        RegionDropDownId: null,
        CountryDropDownId: null,
        DropDownRegionSelect: null,
        PostalCodeId: null,
        RemarksId: null,
        IsOverrideDefaultsId: null,

        Init: function (args) {
            // initialize defaults
            var self = this;

            self.DeviceNameId = args.deviceNameId;
            self.DeviceTagId = args.deviceTagId;
            self.Address1Id = args.address1Id;
            self.Address2Id = args.address2Id;
            self.CityId = args.cityId;
            self.RegionTextId = args.regionTextId;
            self.RegionTextHiddenId = args.regionTextHiddenId;
            self.RegionTextDivId = args.regionTextDivId;
            self.RegionDropDownDivId = args.regionDropDownDivId;
            self.RegionDropDownId = args.regionDropDownId;
            self.CountryDropDownId = args.countryDropDownId;
            self.DropDownRegionSelect = args.dropDownRegionSelect;
            self.PostalCodeId = args.postalCodeId;
            self.RemarksId = args.remarksId;
            self.IsOverrideDefaultsId = args.isOverrideDefaultsId;
        },

        GetDefaults: function () {
            var self = PrepBulkRegistrationManageUtil.DeviceDefaults;

            var data = {
                DefaultDeviceName: $('#' + self.DeviceNameId).val(),
                DefaultDeviceCategoryId: $('#' + self.DeviceTagId).val(),
                DefaultDeviceAddress1: $('#' + self.Address1Id).val(),
                DefaultDeviceAddress2: $('#' + self.Address2Id).val(),
                DefaultDeviceCity: $('#' + self.CityId).val(),
                DefaultDeviceCountryId: $('#' + self.CountryDropDownId).val(),
                DefaultDeviceOtherRegion: $('#' + self.RegionTextHiddenId).val(),
                DefaultDeviceRegionId: $('#' + self.RegionDropDownId).val(),
                DefaultDevicePostalCode: $('#' + self.PostalCodeId).val(),
                DefaultDeviceComments: $('#' + self.RemarksId).val(),
                IsOverrideDefaults: $('#' + self.IsOverrideDefaultsId).is(':checked')
            };

            return data;
        },

        OnCountryDropdownChange: function () {
            var self = PrepBulkRegistrationManageUtil.DeviceDefaults;

            SupportPortal.OnCountryDropdownChange(self.RegionTextId, self.RegionTextHiddenId, self.RegionTextDivId, self.RegionDropDownDivId, self.CountryDropDownId);
        },

        OnRegionDropdownChange: function () {
            var self = PrepBulkRegistrationManageUtil.DeviceDefaults;

            SupportPortal.OnRegionDropdownChange(self.RegionTextId, self.RegionTextHiddenId, self.RegionDropDownId, self.RegionDropDownDivId, self.DropDownRegionSelect);
        },

        OnRegionTextChange: function () {
            var self = PrepBulkRegistrationManageUtil.DeviceDefaults;

            SupportPortal.OnRegionTextChange(self.RegionTextHiddenId, self.RegionTextId);
        },

        FilterCountry: function () {
            var self = PrepBulkRegistrationManageUtil.DeviceDefaults;

            return { countryId: $("#" + self.CountryDropDownId).val() };
        }
    };
}();

var CustomizeBulkRegistrationManageUtil = function () {

    return {
        UniqueId: null,
        IsReadOnly: null,
        DeviceListViewModel: null,
        Countries: null,
        Regions: null,
        DeviceCategories: null,
        UrlGetBulkRegistrationDevicesWithLicenses: null,
        Validator: null,

        Init: function (args) {
            // initialize defaults
            var self = this;

            self.UniqueId = args.uniqueId;
            self.IsReadOnly = args.isReadOnly;
            self.Countries = args.countries;
            self.Regions = args.regions;
            self.DeviceCategories = args.deviceCategories;
            self.UrlGetBulkRegistrationDevicesWithLicenses = args.urlGetBulkRegistrationDevicesWithLicenses;

            self.BindDeviceListViewModel();
        },

        BindDeviceListViewModel: function () {
            var self = this;

            self.DeviceListViewModel = kendo.observable({
                IsReadOnly: self.IsReadOnly,
                Countries: new kendo.data.DataSource({
                    data: self.Countries
                }),
                Regions: new kendo.data.DataSource({
                    data: self.Regions
                }),
                DeviceCategories: new kendo.data.DataSource({
                    data: self.DeviceCategories
                }),
                Devices: new kendo.data.DataSource({
                    pageSize: 20,
                    transport: {
                        read: {
                            url: self.UrlGetBulkRegistrationDevicesWithLicenses
                        }
                    },
                    schema: {
                        model: {
                            id: "DeviceId",
                            fields: {
                                SerialNumber: { editable: false },
                                Model: { editable: false }
                            }
                        }
                    },
                    change: function () {
                        self.DeviceListViewModel.TargetDevices.data(this.data());
                    },
                    requestStart: function (e) {
                        SupportPortal.BlockUI();
                    },
                    requestEnd: function (e) {
                        SupportPortal.UnBlockUI();
                    }
                }),
                TargetDevices: new kendo.data.DataSource(),
                Move: function (e) {
                    var self = this;
                    var row = $(e.target).closest("tr");
                    var grid = row.closest("div.grid-licenses").data("kendoGrid");
                    var deviceUid = grid.element.closest('div.bulk-registration-device').attr('data-uid');
                    var deviceDataItem = $('div.list-customize-bulk-activation').data('kendoListView').dataSource.getByUid(deviceUid);
                    var licenseDataItem = grid.dataItem(row);

                    self.TargetDevices.filter({ field: "DeviceId", operator: "neq", value: deviceDataItem.DeviceId });

                    var viewModel = kendo.observable({
                        DeviceId: deviceDataItem.DeviceId,
                        LicenseId: licenseDataItem.LicenseId,
                        SerialNumber: deviceDataItem.SerialNumber,
                        AuthorizationCode: licenseDataItem.AuthorizationCode,
                        Type: licenseDataItem.Type,
                        ExpectedExpirationDate: SupportPortal.FormatDate(licenseDataItem.ExpectedExpirationDate),
                        BulkRegistrationDevices: self.TargetDevices,
                        Select: function (e) {
                            SupportPortal.ClearNotification($('.customize-notification'));

                            var self = this;
                            var row = $(e.target).closest("tr");
                            var grid = row.closest("div.grid-devices").data("kendoGrid");

                            var dataItem = grid.dataItem(row);
                            var sourceDeviceId = self.DeviceId;
                            var targetDeviceId = dataItem.get('DeviceId');
                            var devicesDataSource = $('div.list-customize-bulk-activation').data('kendoListView').dataSource;
                            var sourceDevice = devicesDataSource.get(sourceDeviceId);
                            var targetDevice = devicesDataSource.get(targetDeviceId);

                            var license = _.findWhere(sourceDevice.BulkRegistrationLicenses, { LicenseId: self.LicenseId });

                            if (targetDevice.BulkRegistrationLicenses && targetDevice.BulkRegistrationLicenses.length > 0) {
                                var hasTargetLicense = false;
                                var licenseType = license.Type;
                                var i, j, k;
                                var targetLics, targetLicsCnt;
                                var srcFeatureLicPartNumber;
                                var targetLic, targetLicType;
                                var targetBundles, targetBundlesCount;
                                var targetFeatureLicPartNumber;

                                if (licenseType.indexOf('BND') !== -1) {
                                    var bundles = license.Bundles;
                                    var bundlesCount = bundles.length;

                                    for (i = 0; i < bundlesCount; i++) {
                                        targetLics = targetDevice.BulkRegistrationLicenses;
                                        targetLicsCnt = targetLics.length;
                                        srcFeatureLicPartNumber = bundles[i].PartNumber;

                                        for (j = 0; j < targetLicsCnt; j++) {
                                            targetLic = targetLics[j];
                                            targetLicType = targetLic.Type;

                                            if (targetLicType.indexOf('BND') !== -1) {
                                                targetBundles = targetLic.Bundles;
                                                targetBundlesCount = targetBundles.length;

                                                for (k = 0; k < targetBundlesCount; k++) {
                                                    targetFeatureLicPartNumber = targetBundles[k].PartNumber;

                                                    if (srcFeatureLicPartNumber === targetFeatureLicPartNumber) {
                                                        hasTargetLicense = true;
                                                        break;
                                                    }
                                                }

                                                if (hasTargetLicense) {
                                                    break;
                                                }
                                            } else {
                                                if (srcFeatureLicPartNumber === targetLicType) {
                                                    hasTargetLicense = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (hasTargetLicense) {
                                            break;
                                        }
                                    }
                                } else {
                                    targetLics = targetDevice.BulkRegistrationLicenses;
                                    targetLicsCnt = targetLics.length;

                                    for (i = 0; i < targetLicsCnt; i++) {
                                        targetLic = targetLics[i];
                                        targetLicType = targetLic.Type;

                                        if (targetLicType.indexOf('BND') !== -1) {
                                            targetBundles = targetLic.Bundles;
                                            targetBundlesCount = targetBundles.length;

                                            for (j = 0; j < targetBundlesCount; j++) {
                                                targetFeatureLicPartNumber = targetBundles[j].PartNumber;

                                                if (licenseType === targetFeatureLicPartNumber) {
                                                    hasTargetLicense = true;
                                                    break;
                                                }
                                            }

                                            if (hasTargetLicense) {
                                                break;
                                            }
                                        } else {
                                            if (licenseType === targetLicType) {
                                                hasTargetLicense = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (hasTargetLicense) {
                                    SupportPortal.Notification("License already exists", SupportPortal.NotificationType.Error, $('.customize-notification'));
                                    return;
                                }
                            }

                            if (license) {
                                sourceDevice.BulkRegistrationLicenses.splice(_.indexOf(sourceDevice.BulkRegistrationLicenses, license), 1);

                                license.set('BulkRegistrationDeviceId', targetDevice.get('Id'));
                                targetDevice.BulkRegistrationLicenses.push(license);
                            }

                            self.CloseWindow();
                        },
                        CloseWindow: function () {
                            $("#MoveLicenseWindow").data("kendoWindow").close();
                        }
                    });

                    var targetDevicesGridElement = $("div.bulk-registration-move-license").find("div.grid-devices");
                    var targetDevicesGrid = targetDevicesGridElement.data("kendoGrid");

                    if (targetDevicesGrid) {
                        targetDevicesGrid.destroy();
                        targetDevicesGridElement.empty();
                    }

                    kendo.bind($("div.bulk-registration-move-license"), viewModel);

                    var window = $("#MoveLicenseWindow").data("kendoWindow");
                    window.setOptions({
                        minHeight: 450
                    });

                    window.open();
                    window.center();
                },
                Remove: function (e) {
                    var row = $(e.target).closest("tr");
                    var grid = row.closest("div.grid-licenses").data("kendoGrid");

                    var dataItem = grid.dataItem(row);

                    grid.dataSource.remove(dataItem);
                },
                IsOtherRegion: function (e) {
                    var country = e.get('CustomValueCountry.Name');

                    return (country !== "United States" && country !== "Canada") ? true : false;
                },
                OnCountryChange: function (e) {
                    var self = this;
                    var countryId = e.data.get('CustomValueCountry.Id');
                    var country = e.data.get('CustomValueCountry.Name');

                    if (country !== "United States" && country !== "Canada") {
                        e.data.set('CustomValueOtherRegion', null);
                        e.data.set('CustomValueRegion', null);
                    } else {
                        e.data.set('CustomValueOtherRegion', null);
                        e.data.set('CustomValueRegion', null);
                        self.Regions.filter({
                            logic: "or",
                            filters: [
                                { field: "CountryId", operator: "eq", value: countryId },
                                { field: "CountryId", operator: "eq", value: 0 }
                            ]
                        });
                    }

                    self.trigger("IsOtherRegion", e.data);
                },
                ShowHideActionColumn: function (e) {
                    var grid = e.sender;

                    if (this.IsReadOnly) {
                        grid.hideColumn(3);
                    } else {
                        grid.showColumn(3);
                    }
                },
                OnDataBinding: function (e) {
                    var self = this;
                    self.ShowHideActionColumn(e);
                },
                OnDataBound: function (e) {
                    var grid = e.sender;

                    if (grid.dataSource.total() === 0) {
                        var columnCount = grid.columns.length;
                        $(e.sender.wrapper)
                            .find('tbody')
                            .append('<tr class="grid-empty-message-row"><td colspan="' + columnCount + '" class="grid-empty-message">No licenses to display</td></tr>');
                    }
                },
                ToggleDisplay: function (e) {
                    var srcElement = $(e.target).closest('.device-info-header');
                    var targetElement = srcElement.find('.toggle');

                    targetElement.toggleClass('plus minus');

                    if (targetElement.hasClass('minus')) {
                        srcElement.siblings('.device-info').slideDown("slow");
                    } else {
                        srcElement.siblings('.device-info').slideUp("slow");
                    }
                },
                RegistrationDate: function (data) {
                    return (data.RegisteredOn) ? SupportPortal.FormatDate(data.RegisteredOn) : "";
                }
            });

            kendo.bind($("#" + self.UniqueId), self.DeviceListViewModel);
        },

        FormatDate: function (data) {
            var self = this;

            return (data && data.get(self.field)) ? kendo.toString(kendo.parseDate(data.get(self.field), SupportPortal.DateFormat), SupportPortal.DateFormat) : "";
        },

        OnWindowOpen: function () {
            SupportPortal.ClearNotification($('.customize-notification'));
        }
    };
}();