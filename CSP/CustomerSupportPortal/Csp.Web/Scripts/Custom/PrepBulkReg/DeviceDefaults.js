//var exampleInput = {
//    regionTextId: "@regionTextId",
//    regionTextHiddenId: "@regionTextHiddenId",
//    regionTextDivId: "@regionTextDivId",
//    regionDropDownDivId: "@regionDropDownDivId",
//    regionDropDownId: "@regionDropDownId",
//    countryDropDownId: "@countryDropDownId",
//    dropDownRegionSelect: "@Constants.DropDownRegionSelect"
//};

var deviceDefaultsConstructor = (function (input) {

    function onCountryDropdownChange() {
        SupportPortal.OnCountryDropdownChange(input.regionTextId, input.regionTextHiddenId, input.regionTextDivId, input.regionDropDownDivId, input.countryDropDownId);
    }

    function onRegionDropdownChange() {
        SupportPortal.OnRegionDropdownChange(input.regionTextId, input.regionTextHiddenId, input.regionDropDownId, input.regionDropDownDivId, input.dropDownRegionSelect);
    }

    function onRegionTextChange() {
        SupportPortal.OnRegionTextChange(input.regionTextHiddenId, input.regionTextId);
    }

    function filterCountry() {
        return { countryId: $("#" + input.countryDropDownId).val() };
    }

    return {
        onCountryDropdownChange: onCountryDropdownChange,
        onRegionDropdownChange: onRegionDropdownChange,
        onRegionTextChange: onRegionTextChange,
        filterCountry: filterCountry
    }
});