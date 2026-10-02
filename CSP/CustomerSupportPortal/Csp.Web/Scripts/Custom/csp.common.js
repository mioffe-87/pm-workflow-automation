var SupportPortal = function () {

    var nameDetailsWindow;

    function bindHeaderEvents() {
        $('#WelcomeNameLink').click(function (e) {
            e.preventDefault();

            var kendowWindow;
            var userAccountId = $('#WelcomeNameLink').data('id');
            var url = '/UserAccount/Details/' + userAccountId;

            //Set so the window knows how to update it's title
            userAccountWindow = '#welcomeNameWindow';

            if (nameDetailsWindow) {
                nameDetailsWindow.data("kendoWindow").refresh(url);
                nameDetailsWindow.data("kendoWindow").setOptions({
                    minHeight: 450
                });

                kendowWindow = nameDetailsWindow.data("kendoWindow");
                kendowWindow.content(SupportPortal.AjaxLoadingMessage());
                kendowWindow.open();
            } else {
                nameDetailsWindow = $('<div id="welcomeNameWindow"></div>').appendTo('body');
                nameDetailsWindow.kendoWindow({
                    content: url,
                    title: 'User Profile',
                    width: '1030px',
                    minHeight: '450px',
                    modal: true
                });
            }

            kendowWindow = nameDetailsWindow.data("kendoWindow");
            kendowWindow.content(SupportPortal.AjaxLoadingMessage());
            kendowWindow.center();
        });
    }

    function preventDoubleSubmission() {
        $(document).on('submit', 'form', function (e) {
            var $form = $(this);

            if ($form.valid()) {

                var alreadySubmitted = $form.data('submitted') === true;
                if (alreadySubmitted) {
                    // prevent submission if the form is already submitted
                    e.preventDefault();
                } else {
                    // mark the form as submitted
                    $form.find(':submit').prop('disabled', true);
                    $form.data('submitted', true);
                }

                $form.find(':submit').prop('disabled', false);
            }
        });
    }
    
    var millisInMinute = 60 * 1000;

    function convertLocalDateTimeToPacificTime(localDateTime) {

        var localTimezoneValue = moment(localDateTime);
        var localTimezoneMilliseconds = localTimezoneValue.valueOf();

        var offsetToUtcMin = localTimezoneValue.zone();
        var offsetToUtcMilli = offsetToUtcMin * millisInMinute;

        var offsetToPacificTimeMin = -1 * localTimezoneValue.tz('America/Los_Angeles').zone();
        var offsetToPacificTimeMilli = offsetToPacificTimeMin * millisInMinute;

        var pacificTime = localTimezoneMilliseconds + offsetToUtcMilli + offsetToPacificTimeMilli;

        return '\/Date(' + pacificTime + ')\/';
    }

    var jsonDateTimeFormat = /^\/Date\(\d+\)\/$/;

    function kendoGridTimezoneToPst(e) {
        if (e.type === "read" && e.response) {
            var rows = e.response.Data;
            if (rows != null) {

                if (rows.length > 0) {

                    for (var i = 0; i < rows.length; i++) {

                        var row = rows[i];

                        for (var col in row) {
                            var cellValue = row[col];

                            if (jsonDateTimeFormat.test(cellValue)) {
                                row[col] = convertLocalDateTimeToPacificTime(cellValue);
                            }
                        }
                    }
                }
            }
        }
    }

    return {
        SpinnerImageUrl: new String(),
        LogOnUrl: new String(),
        ValidationMessageTemplate: new String(),
        ActiveHelpWindow: undefined,
        DateFormat: 'M/d/yyyy',
        DateTimeFormat: 'M/d/yyyy h:mm tt',

        Init: function (args) {
            this.SpinnerImageUrl = args.SpinnerImageUrl;
            this.LogOnUrl = args.LogOnUrl;
            this.ValidationMessageTemplate = args.ValidationMessageTemplate;
            this.DateFormat = args.DateFormat;
            this.DateTimeFormat = args.DateTimeFormat;

            // prevent double submission on all forms
            preventDoubleSubmission();

            // Initiate AjaxGlobalHandler
            //AjaxGlobalHandler.Init();

            $('.k-link').unbind('ajaxStart ajaxStop');
        },

        RedirectToLogon: function () {
            window.location.href = SupportPortal.LogOnUrl;
        },

        SessionKeys: {
            Key: "Key"
        },

        NotificationType: {
            Success: 1,
            Information: 2,
            Warning: 3,
            Error: 4
        },

        HasHtml5StorageSupport: function () {
            return (typeof (Storage) !== "undefined");
        },

        GetSession: function (key) {
            var result = null;

            if (typeof (Storage) !== "undefined") {
                var value = sessionStorage.getItem(key);

                result = (value != null) ? $.parseJSON(value) : null;
            }

            return result;
        },

        SetSession: function (key, value) {
            if (typeof (Storage) !== "undefined") {
                sessionStorage.setItem(key, JSON.stringify(value));
            }
        },

        Tooltip: function (selector, message, options) {
            $.fn.KendoTooltip(selector, message, options);
        },

        Help: function (selector, id, options) {
            $.fn.KendoHelpWindow(selector, id, options);
        },

        Confirmation: function (message, yesCallback, noCallback, options) {
            if (options == undefined) {
                options = new Option();
                options.width = 400;
                options.height = 150;
            }

            if (options.title == undefined || options.title.length == 0) {
                options.title = "Confirmation";
            }

            $.fn.KendoConfirmation({ title: options.title, html: message, minHeight: options.height, minWidth: options.width, yesTitle: options.yesTitle, noTitle: options.noTitle, actions: options.actions }, yesCallback, noCallback);
        },

        Message: function (message, options, okCallback) {
            if (options == undefined) {
                options = new Option();
                options.width = 350;
                options.height = 106;
            }

            if (options.title == undefined || options.title.length == 0) {
                options.title = "Notification";
            }

            $.fn.KendoMessage({ title: options.title, html: message, minHeight: options.height, minWidth: options.width, okTitle: options.okTitle, actions: options.actions }, okCallback);
        },

        LengthyMessage: function (message, options) {
            if (options == undefined) {
                options = new Option();
                options.width = 500;
                options.height = 120;
            }

            if (options.title == undefined || options.title.length == 0) {
                options.title = "Notification";
            }

            $.fn.KendoMessage({ title: options.title, html: message, minHeight: options.height, minWidth: options.width, okTitle: options.okTitle, actions: options.actions }, null);
        },

        Notification: function (message, type, selector) {
            var className = "";
            selector = (selector) ? $(selector) : $(".response-notification:last");
            $('.response-notification').css('display', 'block');
            if (type === SupportPortal.NotificationType.Success) {
                className = "success-notification notification";
            }
            else if (type === SupportPortal.NotificationType.Information) {
                className = "information-notification notification";
            }
            else if (type === SupportPortal.NotificationType.Warning) {
                className = "warning-notification notification";
            }
            else if (type === SupportPortal.NotificationType.Error) {
                className = "error-notification notification";
            } else {
                className = "warning-notification notification";
            }

            SupportPortal.ClearNotification(selector);

            if (message.includes("Cannot transfer this device with")) {
                $(selector).append('<div style="position: relative;"><div class="' + className + ' ascupload-notification-height"><div class="notification-text wrapText ascupload-notification-text">' + message + '</div><input type="button" value="x" class="notification-button" onclick="closeNotification(this)" style="position:absolute"/></div></div>');
            }
            else {
                $(selector).append('<div style="position: relative;"><div class="' + className + '"><div class="notification-text">' + message + '</div><input type="button" value="x" class="notification-button" onclick="closeNotification(this)" style="position:absolute"/></div></div>');
            }
            if (type === SupportPortal.NotificationType.Success) {
                SupportPortal.FadeOutNotification(selector);
            }
        },

        ClearNotification: function (selector) {
            selector = (selector) ? $(selector) : $(".response-notification");

            $(selector).empty();
        },

        FadeOutNotification: function (selector) {
            selector = (selector) ? $(selector) : $(".success-notification");

            setTimeout(function () {
                $(selector).fadeOut(700);
            }, 5000);
        },

        AjaxLoadingMessage: function () {
            return '<div style="text-align: center; position: absolute; top: 50%; left: 0px; width: 100%; height: 1px; overflow: visible; display: block;"><div style="margin-left: -125px; position: absolute; top: -35px; left: 50%; width: 250px; height: 70px;"><img style="height: 40px; vertical-align: middle;" src="' + SupportPortal.SpinnerImageUrl + '" /> Please wait...</div></div>';
        },

        CustomLoadingMessageWithSpinner: function (message) {
            return '<div style="text-align: center; position: absolute; top: 50%; left: 0px; width: 100%; height: 1px; overflow: visible; display: block;"><div style="margin-left: -100px; position: absolute; top: -25px; left: 30%; width: 350px; height: 70px;"><img style="height: 40px; vertical-align: middle;" src="' + SupportPortal.SpinnerImageUrl + '" /> ' + message + '</div></div>';
        },

        BlockUI: function () {
            var msg = $('<h3 style="width: 100%; height: 100%; top: 0px; left: 0px;"><img style="height: 40px; vertical-align: middle;" src="' + SupportPortal.SpinnerImageUrl + '" /> Please wait...</h3>');

            $.blockUI({ message: msg, baseZ: 7777777 });
            /*
            Kendo sets to 10000+ on the popups which in some cases the value of 9999 did not work properly. Since that was arbitrary value
            I figured seven 7s was a great way to keep it magical. :) -Jason Sacks
            */
        },

        UnBlockUI: function () {
            $.unblockUI();
        },

        AddOverlay: function (element) {
            var pos = element.position();
            jQuery('<div/>', {
                width: element.outerWidth(),
                height: element.outerHeight(),
                "class": "tabStripOverlay"
            })
                .css({ left: pos.left, top: pos.top, position: 'absolute', zIndex: 20000 })
                .appendTo(document.body);
        },

        RemoveOverlay: function () {
            $(".tabStripOverlay").remove();
        },

        Serialize: function (data) {
            for (var property in data) {
                if ($.isArray(data[property])) {
                    SupportPortal.SerializeArray(property, data[property], data);
                }
            }

            var antiForgeryToken = SetAntiForgery();
            data.__RequestVerificationToken = antiForgeryToken.__RequestVerificationToken;
        },

        SerializeArray: function (prefix, array, result) {
            for (var i = 0; i < array.length; i++) {
                if ($.isPlainObject(array[i])) {
                    for (var property in array[i]) {
                        result[prefix + "[" + i + "]." + property] = array[i][property];
                    }
                } else {
                    result[prefix + "[" + i + "]"] = array[i];
                }
            }
        },

        FormatDate: function(value) {
            if (value) {
                return kendo.toString(kendo.parseDate(value, SupportPortal.DateFormat), SupportPortal.DateFormat);
            }

            return null;
        },

        stripProps: function (data, propsToStrip) {
            for (var i = 0; i < propsToStrip.length; i++) {
                var prop = propsToStrip[i];
                if (data.hasOwnProperty(prop)) {
                    delete data[prop];
                }    
            }
        },

        OnWindowOpen: function (id) {
            $("#" + id).html(SupportPortal.AjaxLoadingMessage());
        },

        OnWindowClose: function (id) {
            var grid = $("#" + id).data("kendoGrid").dataSource;
            grid.read();
        },

        OnCountryDropdownChange: function (regionTextId, regionTextHiddenId, regionTextDivId, regionDropDownDivId, countryDropDownId) {
            var regionText = $("#" + regionTextId);
            var regionTextHidden = $("#" + regionTextHiddenId);
            var regionTextDiv = $("#" + regionTextDivId);
            var regionDropdownDiv = $("#" + regionDropDownDivId);
            var countryText = $("#" + countryDropDownId).data("kendoDropDownList").text();
            if (countryText == "United States" || countryText == "Canada") {
                if (!regionDropdownDiv.is(':visible')) {
                    regionDropdownDiv.show();
                    regionTextDiv.hide();
                }
            } else {
                if (regionDropdownDiv.is(':visible')) {
                    regionText.val("");
                    regionTextHidden.val("");
                    regionDropdownDiv.hide();
                    regionTextDiv.show();
                }
            }
        },

        OnRegionDropdownChange: function (regionTextId, regionTextHiddenId, regionDropDownId, regionDropDownDivId, regionSelect) {

            // Because of ID conflicts we must set the "Region" bound field dynamically from 
            // dropdown as it changes.
            var regionText = $("#" + regionTextId);
            var regionTextHidden = $("#" + regionTextHiddenId);
            var regionDropdown = $("#" + regionDropDownId);
            var regionDropdownDiv = $("#" + regionDropDownDivId);
            if (regionDropdownDiv.is(':visible')) {
                if (regionDropdown.val() == regionSelect) {
                    regionText.val("");
                    regionTextHidden.val("");
                }
                else {
                    regionText.val(regionDropdown.val());
                    regionTextHidden.val(regionDropdown.val());
                }
            }
        },

        OnRegionTextChange: function (regionTextHiddenId, regionTextId) {
            // Because of ID conflicts we must set the "Region" bound field dynamically from 
            // the text field as it changes.
            var regionTextHidden = $("#" + regionTextHiddenId);
            var regionText = $("#" + regionTextId);
            regionTextHidden.val(regionText.val());
        },

        
        DownloadContent: function (fileType, folderName, fileName) {        
            var fileInfo = { FolderName: folderName, FileName: fileName };
            var formattedurl =  portalData.applicationHost+ "/api/ExternalServices/GetDownloadUrl?useraccountid=" + portalData.userAccountId;
            var requestheaders = {Authorization: "Bearer "+ portalData.authToken};
            SupportPortal.BlockUI();           

            $.ajax({
                url: formattedurl,
                headers: requestheaders,             
                type: 'POST',
                dataType: 'json',
                contentType: "application/json",
                traditional: true,
                data: JSON.stringify(fileInfo),
                cache: false,
                async: false
            }).fail(function (e) {
                SupportPortal.UnBlockUI();
                SupportPortal.Message("Unable to download content. Please contact support.");
            }).done(function (e) {
                if (e.Success) {
                    try {

                        if (fileType !== "notes") {
                            var downloadFrameId = 'iFrmDownload';
                            var downloadFrame = document.getElementById(downloadFrameId);

                            if (downloadFrame === null) {
                                downloadFrame = document.createElement('iframe');
                                downloadFrame.id = downloadFrameId;
                                downloadFrame.style.display = 'none';

                                document.body.appendChild(downloadFrame);
                            }

                            var origin = window.location.protocol +
                                "//" +
                                window.location.hostname +
                                (window.location.port ? ':' + window.location.port : '');
                            //var downloadUrl = (fileType && fileType.toLowerCase() === 'notes')
                            //    ? origin +
                            //    "/updates/DownloadContent?contentFileName=" +
                            //    fileName +
                            //    "&akamaiUrl=" +
                            //    e.DownloadUrl
                            //    : e.DownloadUrl;
                            var downloadUrl = e.DownloadUrl;

                            if (downloadFrame) {
                                if (downloadFrame.src) {
                                    downloadFrame.src = downloadUrl;
                                } else if (downloadFrame.contentWindow !== null &&
                                    downloadFrame.contentWindow.location !== null) {
                                    downloadFrame.contentWindow.location = downloadUrl;
                                } else {
                                    downloadFrame.setAttribute('src', downloadUrl);
                                }
                            }
                        } else {
                            window.open(e.DownloadUrl, "_blank");
                        }
                    }
                    catch (e) {
                    }
                }
                else {
                    SupportPortal.UnBlockUI();
                    SupportPortal.Message("Unable to download content. Please contact support.");
                }
            }).always(function () {
                SupportPortal.UnBlockUI();
            });
        },

        bindHeaderEvents: bindHeaderEvents,

        kendoGridTimezoneToPst: kendoGridTimezoneToPst

    };
}();

// jQuery Global Functions
(function ($) {

    // Kendo Confirmation/Notification Window
    $.fn.KendoWindow = function (options) {
        var defaultOptions = $.extend({}, $.fn.KendoWindow.defaults, options);

        var kendowWindow = $("<div />").kendoWindow({
            title: defaultOptions.title,
            icon: defaultOptions.icon,
            modal: defaultOptions.modal,
            resizable: defaultOptions.resizable,
            draggable: defaultOptions.draggable,
            visible: defaultOptions.visible,
            width: defaultOptions.minWidth,
            height: defaultOptions.minHeight,
            actions: defaultOptions.actions
        }).data('kendoWindow');

        return kendowWindow;
    };

    // Kendo Confirmation Window
    $.fn.KendoConfirmation = function (options, yesCallback, noCallback) {
        var $this = this;
        var kendoWindow = $.fn.KendoWindow(options);

        kendoWindow.content($.fn.KendoConfirmation.format(options));
        kendoWindow.center().open();

        // callback functions
        $('.kendo-confirm-yes').click(function () {
            if (typeof yesCallback == 'function') {
                yesCallback();
            }

            kendoWindow.close();
        });

        $('.kendo-confirm-no').click(function () {
            if (typeof noCallback == 'function') {
                noCallback();
            }

            kendoWindow.close();
        });

        return $this;
    };

    // Kendo Notification Window
    $.fn.KendoMessage = function (options, okCallback) {
        var $this = this;
        var kendoWindow = $.fn.KendoWindow(options);

        kendoWindow.content($.fn.KendoMessage.format(options));
        kendoWindow.center().open();

        // callback functions
        $('.kendo-confirm-ok').click(function () {
            if (typeof okCallback == 'function') {
                okCallback();
            }

            kendoWindow.close();
        });

        return $this;
    };

    // Kendo Confirmation/Notification Window default options
    $.fn.KendoWindow.defaults = {
        title: "Notification",
        html: "",
        icon: "",
        modal: true,
        resizable: false,
        draggable: false,
        visible: false,
        minWidth: 350,
        minHeight: 100
    };

    // html for confirmation box
    $.fn.KendoConfirmation.format = function (options) {
        var yesTitle = (options.yesTitle != undefined) ? options.yesTitle : "Yes";
        var noTitle = (options.noTitle != undefined) ? options.noTitle : "No";

        return '<div class= "kendo-confirmation">' +
                '<br/>' +
                '<p class="kendo-confirm-message">' + options.html + '</p>' +
                '<br/>' +
                '<div class="kendo-confirmation-buttons">' +
                '<button class="kendo-confirm-yes k-button"> ' + yesTitle + '</button>' +
                '&nbsp;&nbsp;&nbsp;' +
                '<button class="kendo-confirm-no k-button"> ' + noTitle + '</button>' +
                '</div>';
    };

    // html for message box
    $.fn.KendoMessage.format = function (options) {
        var okTitle = (options.okTitle != undefined) ? options.okTitle : "OK";

        return '<div class= "kendo-confirmation">' +
                '<br/>' +
                '<p class="kendo-confirm-message">' + options.html + '</p>' +
                '<br/>' +
                '<div class="kendo-confirmation-buttons">' +
                '<button class="kendo-confirm-ok k-button"> ' + okTitle + '</button>' +
                '</div>';
    };

    // Kendo Tooltip
    $.fn.KendoTooltip = function (selector, message, options) {
        var defaultOptions = $.extend({}, $.fn.KendoTooltip.defaults, options);

        // Remove all previous tooltips
        $("[data-role=popup]").remove();

        var kendoTooltip = $("<div />").kendoTooltip({ // $(selector).kendoTooltip({
            autoHide: defaultOptions.autoHide,
            callout: defaultOptions.callout,
            showOn: defaultOptions.showOn,
            position: defaultOptions.position,
            width: defaultOptions.width,
            height: defaultOptions.height,
            content: isNaN(message) ? message : { url: "/Base/CspHelp/" + message },
            animation: {
                close: {
                    effects: "fade:out"
                }
            },
            show: function () {
                /*
                var that = this,
                    tooltips = $("[data-role=popup]");

                tooltips.each(function () {
                    var tooltip = $(this).data("kendoTooltip");
                    if (tooltip && tooltip != that) {
                        tooltip.hide();
                    }
                });*/
            }
        }).data("kendoTooltip");

        kendoTooltip.show($(selector)); // kendoTooltip;
        return true;
    };

    // Kendo Tooltip default options
    $.fn.KendoTooltip.defaults = {
        autoHide: false,
        callout: true,
        filter: ".csp-tooltip",
        showOn: "click",
        position: "right",
        width: 200,
        height: 100
    };

    // Kendo Help Window
    $.fn.KendoHelpWindow = function (selector, id, options) {
        var defaultOptions = $.extend({}, $.fn.KendoHelpWindow.defaults, options);

        // Remove previous help window
        if (SupportPortal.ActiveHelpWindow != undefined) {
            SupportPortal.ActiveHelpWindow.close();
            SupportPortal.ActiveHelpWindow = undefined;
        }

        var offset = $(selector).offset();

        var leftposition = 0;
        if ((offset.left + defaultOptions.minWidth) > $(window).width()) {
            leftposition = (offset.left - defaultOptions.minWidth);
        } else {  leftposition = offset.left; }

        var kendowWindow = $("<div />").kendoWindow({
            title: defaultOptions.title,
            icon: defaultOptions.icon,
            content: id ? "/Base/CspHelp/" + id : "",
            modal: defaultOptions.modal,
            resizable: defaultOptions.resizable,
            draggable: defaultOptions.draggable,
            visible: defaultOptions.visible,
            width: (defaultOptions.width) ? defaultOptions.width : defaultOptions.minWidth,
            height: (defaultOptions.height) ? defaultOptions.height : defaultOptions.minHeight,
            actions: defaultOptions.actions,
            open: function (e) {
                this.wrapper.css({ top: offset.top + $(selector).height() / 2, left: leftposition + $(selector).width() });
            }
        }).data('kendoWindow');

        SupportPortal.ActiveHelpWindow = kendowWindow;

        if (defaultOptions.content) {
            kendowWindow.content(defaultOptions.content);
        }
        else {
            kendowWindow.content(SupportPortal.AjaxLoadingMessage());
        }

        kendowWindow.open();

        return true;
    };

    // Kendo Help window default options
    $.fn.KendoHelpWindow.defaults = {
        title: "Help",
        html: "",
        icon: "",
        modal: false,
        resizable: true,
        draggable: true,
        visible: false,
        minWidth: 350,
        minHeight: 100,
        actions: ["Maximize", "Close"]
    };
})(jQuery);

var global = function () {
    return {
        ShowProgress: true
    };
}();

var AjaxGlobalHandler = {
    Init: function (options) {
        $.ajaxSetup({ cache: false });

        // Ajax events
        $(document).ajaxStart(function () {
            if (document.activeElement) {
                var className = document.activeElement.className;

                if (className.indexOf("k-link") != -1 || className.indexOf("k-icon") != -1 || className.indexOf("k-widget") != -1) {
                    return;
                }
            }

            if (global.ShowProgress) {
                SupportPortal.BlockUI();
            }
        }).ajaxError(function (e, xhr, opts) {
            if (xhr.status === 0 || xhr.readyState === 0) {
                return;
            }

            SupportPortal.Message(options.AjaxErrorMessage);
        }).ajaxComplete(function (e, xhr, opts) {
            if (xhr.status >= 300 && xhr.status < 400) { SupportPortal.RedirectToLogon(); }
        }).ajaxStop(function () {
            if (document.activeElement) {
                var className = document.activeElement.className;

                if (className.indexOf("k-link") != -1 || className.indexOf("k-icon") != -1 || className.indexOf("k-widget") != -1) {
                    return;
                }
            }

            if (global.ShowProgress) {
                SupportPortal.UnBlockUI();
            }
        });
    }
};

$(function () {
    SupportPortal.bindHeaderEvents();

    // HACK fix to disable autocomplete
    $(document).on('focus', ':input', function () {
        $(this).attr('autocomplete', 'off');
    });
});

// Removes whitespace from both ends of the string
// Fix for <= IE8
if (!String.prototype.trim) {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    };
}

/**
* Replace all tokens with the new one
*/
String.prototype.replaceAll = function (token, newToken, ignoreCase) {
    var str, i = -1, _token;
    if ((str = this.toString()) && typeof token === "string") {
        _token = ignoreCase === true ? token.toLowerCase() : undefined;
        while ((i = (_token !== undefined ? str.toLowerCase().indexOf(_token, i >= 0 ? i + newToken.length : 0) : str.indexOf(token, i >= 0 ? i + newToken.length : 0))) !== -1) {
            str = str.substring(0, i)
                .concat(newToken)
                .concat(str.substring(i + token.length));
        }
    }
    return str;
};

//var kendoWindowHelper = (function () {

//    function getOrCreateWindowById(id, windowOptions) {

//        var window = $('#' + id);
//        var kendoWindow;
//        if (window.length == 0) {
//            window = $('<div id="welcomeNameWindow"></div>').appendTo('body');
//            kendoWindow = window.kendoWindow({
//                content: windowOptions.url,
//                title: windowOptions.title,
//                width: windowOptions.width,
//                minHeight: windowOptions.minHeight,
//                modal: windowOptions.modal
//            });
//        } else {
//            kendoWindow = window.data("kendoWindow");

//            kendoWindow.content(' ');
//        }

//        if (windowOptions.openEvent) {
//            kendoWindow.bind("open", windowOptions.openEvent);
//        }

//        if (windowOptions.closeEvent) {
//            kendoWindow.bind("close", windowOptions.closeEvent);
//        }

//        kendoWindow.open();
//        kendoWindow.cneter();

//        return kendoWindow;
//    }

//    function getDefaultWindowOptions() {
//        return {
//            url: '',
//            title: '',
//            width: '500px',
//            minHeight: '450px',
//            modal: true,
//            scrollable: false,
//            draggable: true,
//            openEvent: null,
//            closeEvent: null
//        };
//    }

//    return {
//        getOrCreateWindowById: getOrCreateWindowById,
//        getDefaultWindowOptions: getDefaultWindowOptions
//    };
//})();

