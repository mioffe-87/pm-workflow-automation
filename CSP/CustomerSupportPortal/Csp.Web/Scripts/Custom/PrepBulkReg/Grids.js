//var exampleInput = {
//    prepBulkRegGridId: '@(prepBulkRegGridId)',
//    selectDevicesFilter: '@(selectDevicesFilter)',
//    prepBulkRegAvailableLicensesGridId: '@(prepBulkRegAvailableLicensesGridId)',
//    prepBulkRegDeviceAndLicensesGridId: '@(prepBulkRegDeviceAndLicensesGridId)',
//    hdnModelCompatibilityId: '@(hdnModelCompatibilityId)',
//    reloadAvailableLicensesDataFromServer: availableLicensesGrid.reloadDataFromServer,
//    reloadDevicesAndLicensesDataFromServer: devicesAndLicensesGrid.reloadDataFromServer
//};

var availableDevicesGridConstructor = (function (input) {

    var lastModelCompat = $('#' + input.hdnModelCompatibilityId);

    function filterChanged() {

        $('#' + input.prepBulkRegGridId).data('kendoGrid').dataSource.read();
    }

    function getAvailableDevicesGridParams() {

        var p = SetAntiForgery();
        p.selectDevicesFilter = $('input[name=' + input.selectDevicesFilter +']:checked').val();
        return p;
    }

    function reloadAvailableLicensesGrid() {
        input.reloadAvailableLicensesDataFromServer();
    }

    function clearOtherModelCompatibilitySelections(newModelCompatibility, checkAll) {

        if (lastModelCompat.val() != newModelCompatibility) {
            lastModelCompat.val(newModelCompatibility);

            reloadAvailableLicensesGrid();
        }

        var otherModelCompatibilities = $('#' + input.prepBulkRegGridId).find('input.selectone').filter(function () {
            var checkbox = $(this);
            var thisModelCompat = checkbox.data("modelcompatibility");
            var isDiff = thisModelCompat !== newModelCompatibility;
            if (checkAll && !isDiff) {
                checkbox.prop('checked', true);
            }
            return isDiff; //only return different ones
        });

        otherModelCompatibilities.prop('checked', false);
    }

    function reloadDevicesAndLicensesGrid() {
        input.reloadDevicesAndLicensesDataFromServer();
    }

    function selectAllClick() {
        var modelCompat = $(this).data('modelcompatibility');

        var checkAll = true;
        clearOtherModelCompatibilitySelections(modelCompat, checkAll);

        reloadDevicesAndLicensesGrid();
    }

    function selectOneClick() {
        var checkbox = $(this);
        var modelCompat = checkbox.data('modelcompatibility');

        clearOtherModelCompatibilitySelections(modelCompat);

        reloadDevicesAndLicensesGrid();
    }

    return {
        getAvailableDevicesGridParams: getAvailableDevicesGridParams,
        filterChanged: filterChanged,
        selectAllClick: selectAllClick,
        selectOneClick: selectOneClick
    };
});

//var exampleInput = {
//    hdnModelCompatibilityId: '@(hdnModelCompatibilityId)',
//    prepBulkRegDeviceAndLicensesGridId: '@(prepBulkRegDeviceAndLicensesGridId)',
//    prepBulkRegAvailableLicensesGridId: '@(prepBulkRegAvailableLicensesGridId)',
//    jsPrefix: '@(uniqueId)',
//    getLicsUrl: '@(Url.Action("GetAvailableLicensesForPrepBulkReg", "BulkRegistration", new {id = Model.ActionHelper.Id}))',
//    devsAndLicsGridAddLicense: devicesAndLicensesGrid.addLicense
//};

var availableLicensesGridConstructor = (function (input) {

    var data = [];

    var dataSource = new kendo.data.DataSource({
        transport: {
          read: function(e) {
              e.success(data);
          }
        }
        , schema: gridSchemas.availableLicsDsSchema
    });

    

    var lastModelCompat = $('#' + input.hdnModelCompatibilityId);

    function getAvailableLicensesGridParams() {
        var p = SetAntiForgery();
        p.modelCompatibility = lastModelCompat.val();
        return p;
    }

    function selectAvailableLicense(e) {
        var selectLink = $(e);
        var salesordernumber = selectLink.data('salesordernumber');
        var partnumber = selectLink.data('partnumber');

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

            var result = input.devsAndLicsGridAddLicense(licenseId, partnumber);
            if (!result.success) {
                break;
            }
            row.LicenseIds.splice(0, 1); //remove the first licenseID from the list
            row.Quantity--;
        }

        dataSource.read();
    }

    var reqHandle;
    function cancelAnyInProgressAjaxRequest() {
        if (reqHandle && reqHandle.readyState != 4) {
            reqHandle.abort();
        }
    }

    function reloadDataFromServer() {
        var grid = $('#' + input.prepBulkRegAvailableLicensesGridId);

        cancelAnyInProgressAjaxRequest();

        kendo.ui.progress(grid, true);
        reqHandle = $.ajax({
            url: input.getLicsUrl,
            type: 'post',
            dataType: 'json',
            data: getAvailableLicensesGridParams()
        }).done(function (e) {
            data = e;
            dataSource.read();
        }).always(function() {
            kendo.ui.progress(grid, false);
        });
    }

    function initializeGrid() {
        var grid = $('#' + input.prepBulkRegAvailableLicensesGridId);
        grid.kendoGrid({
            columns: gridSchemas.availableLicsGridColumns(input),
            scrollable: false,
            autoBind: false,
            dataSource: dataSource
        });

        dataSource.read();
    }

    return {
        getAvailableLicensesGridParams: getAvailableLicensesGridParams,
        selectAvailableLicense: selectAvailableLicense,
        initializeGrid: initializeGrid,
        reloadDataFromServer: reloadDataFromServer
    };
});

//var exampleInput = {
//    prepBulkRegGridId: '@(prepBulkRegGridId)',
//    prepBulkRegDeviceAndLicensesGridId: '@(prepBulkRegDeviceAndLicensesGridId)',
//    getDevsAndLicsUrl: '@(Url.Action("GetAppliedLicensesForPrepBulkReg", "BulkRegistration", new {id = Model.ActionHelper.Id}))',
//    renderMovableCell: '@(UniqueId).renderMovableCell',
//    deviceAndLicensesDataBound: deviceAndLicensesDataBound
//};

var devicesAndLicensesGridConstructor = (function (input) {

    var data = [];

    var dataSource = new kendo.data.DataSource({
        transport: {
            read: function (e) {
                e.success(data);
            }
        }
        , schema: gridSchemas.devAndLicsDsSchema
    });

    function getDeviceIds() {
        var devIds = $('#' + input.prepBulkRegGridId + ' input.selectone:checked').map(function () {
            return this.value;
        }).get();

        return devIds;
    }

    function getDeviceAndLicensesGridParams() {
        var p = SetAntiForgery();
        p.deviceIds = getDeviceIds();
        return p;
    }

    var reqHandle;
    function cancelAnyInProgressAjaxRequest() {
        if (reqHandle && reqHandle.readyState != 4) {
            reqHandle.abort();
        }
    }

    function reloadDataFromServer() {
        var grid = $('#' + input.prepBulkRegDeviceAndLicensesGridId);

        cancelAnyInProgressAjaxRequest();

        kendo.ui.progress(grid, true);
        reqHandle = $.ajax({
            url: input.getDevsAndLicsUrl,
            type: 'post',
            dataType: 'json',
            traditional: true,
            data: getDeviceAndLicensesGridParams()
        }).done(function (e) {
            data = e;
            dataSource.read();
        }).always(function () {
            kendo.ui.progress(grid, false);
        });
    }

    function initializeGrid() {
        var grid = $('#' + input.prepBulkRegDeviceAndLicensesGridId);
        grid.kendoGrid({
            columns: gridSchemas.devAndLicsGridColumns(input.renderMovableCell),
            scrollable: false,
            autoBind: false,
            dataSource: dataSource,
            dataBound: function() {
                input.deviceAndLicensesDataBound();
            }
        });
        dataSource.read();
    }

    var licConsts = {
        SVC: 'SVC',
        TP: 'TP',
        URL: 'URL',
        GPPortal: 'GP Portal',
        GPGateway: 'GP Gateway',
        WF: 'WF',
        BND: 'BND'
    };

    var licTypes = [
        { regex: /-SVC-/, type: licConsts.SVC },
        { regex: /-SVC$/, type: licConsts.SVC },

        { regex: /-TP-/, type: licConsts.TP },
        { regex: /-TP$/, type: licConsts.TP },

        { regex: /-URL2-/, type: licConsts.URL },
        { regex: /-URL2$/, type: licConsts.URL },
        { regex: /-URL4-/, type: licConsts.URL },
        { regex: /-URL4$/, type: licConsts.URL },

        //-PORTAL- MUST be listed BEFORE -GP-
        { regex: /-PORTAL-/, type: licConsts.GPPortal },
        { regex: /-PORTAL$/, type: licConsts.GPPortal },

        { regex: /-GP-/, type: licConsts.GPGateway },
        { regex: /-GP$/, type: licConsts.GPGateway },

        { regex: /-WF-/, type: licConsts.WF },
        { regex: /-WF$/, type: licConsts.WF },

        { regex: /-BND/, type: licConsts.BND }
    ];

    function getCoreLicType(partNumber) {

        for (var i = 0; i < licTypes.length; i++) {
            var curLicType = licTypes[i];
            if (curLicType.regex.test(partNumber)) {
                return curLicType.type;
            }
        }

        return null;
    }

    function tryApplyLic(licTypeName, licenseId, partNumber) {

        var licenseIdFieldName = licTypeName + 'LicenseId';
        var displayNameFieldName = licTypeName + 'DisplayName';

        var searchFilter = {};
        searchFilter[licenseIdFieldName] = null;

        var row = _.findWhere(data, searchFilter);
        if (row) {
            row[licenseIdFieldName] = licenseId;
            row[displayNameFieldName] = partNumber;
            dataSource.read();
            return { success: true };
        }
        return { success: false };
    }

    function addLicense(licenseId, partNumber) {

        var licType = getCoreLicType(partNumber);
        if (!licType) {
            return { success: false };
        }

        //loop through devices looking for a dev that the lic can be applied to
        switch(licType) {
            case licConsts.SVC:
                return tryApplyLic('StandardSupport', licenseId, partNumber);

            case licConsts.TP:
                return tryApplyLic('ThreatProtection', licenseId, partNumber);

            case licConsts.URL:
                return tryApplyLic('Url', licenseId, partNumber);

            case licConsts.GPPortal:
                return tryApplyLic('GpPortal', licenseId, partNumber);

            case licConsts.GPGateway:
                return tryApplyLic('GpGateway', licenseId, partNumber);

            case licConsts.WF:
                return tryApplyLic('Wildfire', licenseId, partNumber);

            case licConsts.BND:
                // TODO Decode the bnd lic type from the description field
                // TODO Call tryApplyLic x times to apply the appropriate lics and if any fails it should roll it back and the other lics and return success false
                return tryApplyLic('Wildfire', licenseId, partNumber);
        }

        return { success: false };
    }

    function swapLicenses(sourceRow, destRow, fieldName) {
        var destLicId = destRow[fieldName + 'LicenseId'];
        var destDisplay = destRow[fieldName + 'DisplayName'];

        destRow[fieldName + 'LicenseId'] = sourceRow[fieldName + 'LicenseId'];
        destRow[fieldName + 'DisplayName'] = sourceRow[fieldName + 'DisplayName'];

        sourceRow[fieldName + 'LicenseId'] = destLicId;
        sourceRow[fieldName + 'DisplayName'] = destDisplay;

        return destLicId;
    }

    function moveLicense(sourceSn, destSn, sourceLicId) {

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

            returnedDestLicIds.push(destLicId);
        }

        if (sourceRow.ThreatProtectionLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'ThreatProtection');

            returnedDestLicIds.push(destLicId);
        }

        if (sourceRow.UrlLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Url');

            returnedDestLicIds.push(destLicId);
        }

        if (sourceRow.GpPortalLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'GpPortal');

            returnedDestLicIds.push(destLicId);
        }

        if (sourceRow.GpGatewayLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'GpGateway');

            returnedDestLicIds.push(destLicId);
        }

        if (sourceRow.WildfireLicenseId === sourceLicId) {
            destLicId = swapLicenses(sourceRow, destRow, 'Wildfire');

            returnedDestLicIds.push(destLicId);
        }

        //for (var licId in returnedDestLicIds) {
            // TODO: if (licId still exists in dest row in any column) execute a swapLicenses method for that column
        //}

        dataSource.read();
    }

    function getData() {
        return data;
    }

    return {
        getDeviceAndLicensesGridParams: getDeviceAndLicensesGridParams,
        reloadDataFromServer: reloadDataFromServer,
        initializeGrid: initializeGrid,
        addLicense: addLicense,
        moveLicense: moveLicense,
        getData: getData
    }
});