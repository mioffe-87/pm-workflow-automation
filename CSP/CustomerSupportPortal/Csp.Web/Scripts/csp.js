var addDeviceToGroupWindow = "";
var deviceWindowId = "";
var deviceGridId = "";
var registerDeviceWindow = "";
var membershipWindow = "";
var userAccountWindow = "";
var membershipUserAccountId = "";
var licenseGridId = "";


function setMembershipId(id) {
    $(membershipUserAccountId).val(id);
}

function SetAntiForgery() {
    return { "__RequestVerificationToken": $('input[name=__RequestVerificationToken]').val() };
}


function SelectAllRoles(id) {
    $("#" + id + " option").prop('selected', true);
}

function AddRemoveRole(id, addRole, removeRole, availableRoles, assignedRoles) {
    var selectFrom = id == addRole ? "#" + availableRoles : "#" + assignedRoles;
    var moveTo = id == removeRole ? "#" + availableRoles : "#" + assignedRoles;
    var selectedItems = $(selectFrom + " :selected").toArray();
    $(moveTo).append(selectedItems);
    $(moveTo).focus();
    $("#" + assignedRoles + " option").prop('selected', true);
    /*All options have the selected property as they are mapped to begin with even 
    though they are not actually selected by the user. When they are removed all are removed 
    from the bound id property but not the selection box. By adding all items back that have 
    the selected property it ensures they are in the bound id property.*/
    selectedItems = $("#" + assignedRoles).toArray();
    $("#" + assignedRoles).append(selectedItems);
}

function AddRemoveAllRoles(id, addAllRoles, removeAllRoles, availableRoles, assignedRoles) {
    var selectFrom = id == addAllRoles ? "#" + availableRoles : "#" + assignedRoles;
    var moveTo = id == removeAllRoles ? "#" + availableRoles : "#" + assignedRoles;
    var selectedItems = $(selectFrom + " option").toArray();
    $(moveTo).append(selectedItems);
    $(moveTo).focus();
    $("#" + assignedRoles + " option").prop('selected', true);
    /*All options have the selected property as they are mapped to begin with even 
    though they are not actually selected by the user. When they are removed all are removed 
    from the bound id property but not the selection box. By adding all items back that have 
    the selected property it ensures they are in the bound id property.*/
    selectedItems = $("#" + assignedRoles).toArray();
    $("#" + assignedRoles).append(selectedItems);

}


var AscScoreCard = function () {
    return {
        AscTimeframeClick: function (timeframe, isInternal) {
            var url = "/AscScorecard/LoadLastFiveQuaterPartnerData";
            if (timeframe.value == 1) {

                url = "/AscScorecard/LoadLastFiveMonthPartnerData";

            }
           
            AscScoreCard.AscGenerateReport(url, isInternal);
            if (timeframe.value == 1) {

                $('#divIncludeCurrentPartial span.chkIncludeCurrentPartialText').html('Include Current Partial Month');
            } else {
                $('#divIncludeCurrentPartial span.chkIncludeCurrentPartialText')
                    .html('Include Current Partial Quarter');
            }
        },
        AscIncludeCurrentPartialChkClick: function (isInternal) {
            var url = "/AscScorecard/LoadLastFiveQuaterPartnerData";
            var timeframe = $('input:radio[name=ascTimeframe]:checked').val();
            if (timeframe == 1) {

                url = "/AscScorecard/LoadLastFiveMonthPartnerData";
            }

            AscScoreCard.AscGenerateReport(url, isInternal);
        },
        AscGenerateReport: function (url, isInternal) {
          
            var msg = $('<h4 style="width: 100%; height: 100%; top: 0px; left: 0px;"><img style="height: 40px; vertical-align: middle; padding-right:10px" src="../../Images/spinner.gif" />Loading data. Please wait...</h4>');
            $.blockUI({
                message: msg
            });

            var isIncludeCurrentPartial = "0";
            if ($("#chkIncludeCurrentPartial").is(":checked")) {
                isIncludeCurrentPartial = "1";
            }

            var parameters = "?sfdcAccountId=" + $("#hdnSFDCAccountId").val() + "&isInternal=" + isInternal + "&theatre=" + $("#TheatreList").kendoDropDownList().val() + "&isIncludeCurrentPartial=" + isIncludeCurrentPartial;

            url = url + parameters;

            $("#AscScoreCardData").load(url, "",
                       function (responseText, textStatus, XMLHttpRequest) {
                           if (textStatus == 'error') {
                               $.unblockUI();
                               if (XMLHttpRequest != null && XMLHttpRequest.status == 400) {
                                   // ShowStaticNotification('<h2>Attention</h2> <p>' + XMLHttpRequest.statusText + '</p>', "info", null, false);
                               } else {
                                   //  ShowStaticNotification('Internal Server Error Occurred. Please try again later. If the error persists, please open a support ticket.', "error", null, false);
                               }
                           }
                           else {
                               $.unblockUI();
                           }
                       });
            
        },

        InternalGeneratReportClick: function () {
            var url = "/AscScorecard/LoadInternalData?IsLastFiveMonth=false";

            if ($('input:radio[name=ascTimeframe]:checked').val() == 1) {

                url = "/AscScorecard/LoadInternalData?IsLastFiveMonth=true";
            }

            var isIncludeCurrentPartial = "0";
           
            if ($("#chkIncludeCurrentPartial").is(":checked")) {
                isIncludeCurrentPartial = "1";
            }
            var parameters = "&userAccountId=" + $("#hdnSFDCAccountId").val() + "&theatre=" + $("#TheatreList").val() + "&isIncludeCurrentPartial=" + isIncludeCurrentPartial;
            
           
            url = url + parameters;
           
            var msg = $('<h4 style="width: 100%; height: 100%; top: 0px; left: 0px;"><img style="height: 40px; vertical-align: middle; padding-right:10px" src="../../Images/spinner.gif" />Loading data. Please wait...</h4>');
            $.blockUI({
                message: msg
            });

            $("#AscScoreCardData").load(url, "",
                       function (responseText, textStatus, XMLHttpRequest) {
                           if (textStatus == 'error') {
                               $.unblockUI();
                               if (XMLHttpRequest != null && XMLHttpRequest.status == 400) {
                                   // ShowStaticNotification('<h2>Attention</h2> <p>' + XMLHttpRequest.statusText + '</p>', "info", null, false);
                               } else {
                                   //  ShowStaticNotification('Internal Server Error Occurred. Please try again later. If the error persists, please open a support ticket.', "error", null, false);
                               }
                           }
                           else {
                               $.unblockUI();
                           }
                       });

        },
        SearchParameter: function (isInternal) {
            var isIncludeCurrentPartial = "0";
            if ($("#chkIncludeCurrentPartial").is(":checked")) {
                isIncludeCurrentPartial = "1";
            }

            if ($('input[name="ascTimeframe"]:checked').val() == 1) {
                return {
                    isLastFiveQtr: "false",
                    sfdcAccountId: $("#hdnSFDCAccountId").val(),
                    isInternal: isInternal,
                    theatre: $("#TheatreList").val(),
                    isIncludeCurrentPartial: isIncludeCurrentPartial
                };
            }


            return {
                isLastFiveQtr: "true",
                sfdcAccountId: $("#hdnSFDCAccountId").val(),
                isInternal: isInternal,
                theatre: $("#TheatreList").val(),
                isIncludeCurrentPartial: isIncludeCurrentPartial
            };
        },
        ascUserListParameter: function () {
           
            return {
                name: $("#ascUserList").data("kendoMultiSelect").value(),
                   // $("#ascUserList").data("kendoComboBox").text(),
                theatre: $("#TheatreList").val()
            };
        },

        FiscalCalendar: function (e) {

            SupportPortal.Help(e, 21, { 
                title: 'Fiscal Calendar'
            }
            );

        },

        onChangeTheatreList: function () {
            //var combobox = $("#ascUserList").data("kendoComboBox");
            var combobox = $("#ascUserList").data("kendoMultiSelect");
            combobox.dataSource.read();
            combobox.value("");
            $("#hdnSFDCAccountId").val("");
        },

        userListOnSelect: function (e) {
           
            $("#hdnSFDCAccountId").val(this.value());

            var items = this.ul.find("li");
            AscScoreCard.checkCheckBoxInputs(items);

        },

        clearFilter: function () {
            $("#TheatreList").val("ALL");

            var combobox = $("#ascUserList").data("kendoMultiSelect");
            combobox.dataSource.read();
            combobox.value("");
            $("#hdnSFDCAccountId").val("");
        },

        ascUserDataBound: function () {
            var items = this.ul.find("li");
            setTimeout(function () {
                AscScoreCard.checkCheckBoxInputs(items);
            });
        },

        checkCheckBoxInputs: function (elements) {
        elements.each(function () {
            var element = $(this);
            var input = element.children("input");

            input.prop("checked", element.hasClass("k-state-selected"));
        });
        },

        SupportEnggNonCompliant: function (data, colName) {

            var now = new Date();
           
            if (data.NonCompliant == true) {
                if (colName == 'Total_Case_Count') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0}</div>',
                        data.Total_Case_Count);
                } else if (colName == 'Frontline_Case_Count') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0}</div>',
                        data.Frontline_Case_Count);
                } else if (colName == 'Percentage_Tagged_Frontline') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0}%</div>',
                        data.Percentage_Tagged_Frontline);
                }
            } else {
                if (colName == 'Total_Case_Count') {
                    return data.Total_Case_Count;
                } else if (colName == 'Frontline_Case_Count') {
                    return data.Frontline_Case_Count;
                } else if (colName == 'Percentage_Tagged_Frontline') {
                    return kendo.format('{0}%',
                        data.Percentage_Tagged_Frontline);
                }
            }

            if (colName == 'Pcnse' && data.Pcnse) {
                if (data.PcnseCompliance == 'AboutToExpire') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0:MM/dd/yyyy}</div>',
                        data.Pcnse);
                }
                else if (data.PcnseCompliance == 'Expired') {
                    return kendo.format('<div style="background-color: #FF0000;color: #000000;">{0:MM/dd/yyyy}</div>',
                        data.Pcnse);
                }
                else {
                    return kendo.format('{0:MM/dd/yyyy}', data.Pcnse);
                }
            }

            if (colName == 'Pcnsa' && data.Pcnsa) {
                if (data.PcnsaCompliance == 'AboutToExpire') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0:MM/dd/yyyy}</div>',
                        data.Pcnsa);
                }
                else if (data.PcnsaCompliance == 'Expired') {
                    return kendo.format('<div style="background-color: #FF0000;color: #000000;">{0:MM/dd/yyyy}</div>',
                        data.Pcnsa);
                }
                else {
                    return kendo.format('{0:MM/dd/yyyy}', data.Pcnse);
                }
            }

            if (colName == 'Ace' && data.Ace) {
                if (data.AceCompliance == 'AboutToExpire') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0:MM/dd/yyyy}</div>',
                        data.Ace);
                }
                else if (data.AceCompliance == 'Expired') {
                    return kendo.format('<div style="background-color: #FF0000;color: #000000;">{0:MM/dd/yyyy}</div>',
                        data.Ace);
                }
                else {
                    return kendo.format('{0:MM/dd/yyyy}', data.Ace);
                }
            }

          if (colName == 'TrafficRedirector' && data.TrafficRedirector) {
                if (data.TrafficRedirectorCompliance == 'AboutToExpire') {
                    return kendo.format('<div style="background-color: #FEFF03;color: #000000;">{0:MM/dd/yyyy}</div>', data.TrafficRedirector);
                }
                else if (data.TrafficRedirectorCompliance == 'Expired') {
                    return kendo.format('<div style="background-color: #FF0000;color: #000000;">{0:MM/dd/yyyy}</div>', data.TrafficRedirector);
                }
                else {
                    return kendo.format('{0:MM/dd/yyyy}', data.TrafficRedirector);
                }
            }

            return "";
        }

    };
}();

$(function () {
    $(document).on("click", "a.exportToExcelDownload", function () {

        var msg = $('<h4 style="width: 100%; height: 100%; top: 0px; left: 0px;"><img style="height: 40px; vertical-align: middle; padding-right:10px" src="../../Images/spinner.gif" />Downloading  file. Please wait...</h4>');

       
        $.blockUI({
            message: msg
        });
        
        $.fileDownload($(this).prop('href'), {
            successCallback: function (url) {
               
                $.unblockUI();

                //var notification = $("#notification").data("kendoNotification");
                //notification.show({
                //    title: "Download Notification",
                //    message: 'Excel file successfully downloaded!'
                //}, "info");
            },
            failCallback: function (responseHtml, url) {
               
                if (responseHtml == undefined || responseHtml == "") {
                    responseHtml = "There was an error generating your file, please refresh your report and try again.";
                }

                $.unblockUI();
                var errorMsg = $('<h4 style="width: 100%; height: 100%; top: 0px; left: 0px;">' + responseHtml + '</h4>');


                $.blockUI({
                    message: errorMsg
                });
                $('.blockOverlay').attr('title', 'Click to unblock').click($.unblockUI);
            }
        });



        return false;
    });

});

//$(window).scroll(function () {
//    if ($(this).scrollTop() >= 50) { // If page is scrolled more than 50px
//        $('#return-to-top').fadeIn(200); // Fade in the arrow
//    } else {
//        $('#return-to-top').fadeOut(200); // Else fade out the arrow
//    }
//});
$('#return-to-top').click(function () { // When arrow is clicked
    $('body,html').animate({
        scrollTop: 0 // Scroll to top of body
    }, 500);
});


var CsspTabs = {
    CsspHome: 0,
    ShoppingCart: 1,
    VmProvisioningAuthCodes: 2,
    PreLicenseDevices: 3,
    EnduserDevices: 4,
    OrderHistory: 5
}
