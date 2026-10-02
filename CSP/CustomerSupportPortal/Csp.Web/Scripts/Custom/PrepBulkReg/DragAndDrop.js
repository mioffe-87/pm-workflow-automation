//var input = {
//    moveLicense: devicesAndLicensesGrid.moveLicense
//};
var dragAndDropConstructor = (function (input) {

    function highlightDropLocations(className, thisSerialNumber) {
        $('.' + className + '[data-serialnumber!=' + thisSerialNumber + ']').addClass('droppable');
    }

    function unhighlightDropLocations(className) {
        $('.' + className).removeClass('droppable');
    }

    function getDragGroup(target) {
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


        input.moveLicense(sourceSn, destSn, sourceLicId);
    }

    var groups = [];

    function deviceAndLicensesDataBound() {

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
            return kendo.format("<div class=\"{0}\" data-serialnumber=\"{1}\">&nbsp;</div>",
                groupName,
                data.SerialNumber
            );
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
        deviceAndLicensesDataBound: deviceAndLicensesDataBound,
        renderMovableCell: renderMovableCell
    }
});