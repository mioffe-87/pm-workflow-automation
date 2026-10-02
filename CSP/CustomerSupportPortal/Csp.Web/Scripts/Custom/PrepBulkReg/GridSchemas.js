var gridSchemas = (function() {

    var availableLicsDsSchema = {
        model: {
            fields: {
                SalesOrderNumber: {
                    type: "string"
                },
                ShipDate: {
                    type: "date"
                },
                PartNumber: {
                    type: "string"
                },
                Quantity: {
                    type: "number"
                },
                Term: {
                    type: "string"
                }
            }
        }
    };

    var availableLicsGridColumns = function(input) {
        return [
            {
                title: '&nbsp',
                'template': '<a href="javascript:void(0)" onclick="' + input.jsPrefix + '.selectAvailableLicense(this);" data-salesordernumber="#=SalesOrderNumber#" data-partnumber="#=PartNumber#">Apply</a>',
                'field': 'SalesOrderNumber',
                'sortable': false,
                'filterable': false,
                'encoded': true
            }, {
                'title': 'Sales<br/>Order',
                'field': 'SalesOrderNumber',
                'filterable': {},
                'encoded': true
            }, {
                'title': 'Ship<br/>Date',
                'field': 'ShipDate',
                'format': '{0:MM/dd/yyyy}',
                'filterable': {},
                'encoded': true
            }, {
                'title': 'Part<br/>Number',
                'field': 'PartNumber',
                'filterable': {},
                'encoded': true
            }, {
                'title': 'Quantity',
                'field': 'Quantity',
                'filterable': {},
                'encoded': true
            }, {
                'title': 'Term',
                'field': 'Term',
                'filterable': {},
                'encoded': true
            }
        ];
    }

    var devAndLicsDsSchema = {
        model: {
            fields: {
                SerialNumber: {
                    type: 'string'
                },
                AssetPartNumber: {
                    type: 'string'
                },
                AssetDisplayName: {
                    type: 'string'
                },
                StandardSupportLicenseId: {
                    type: 'number'
                },
                StandardSupportAuthCode: {
                    type: 'string'
                },
                StandardSupportExpiration: {
                    type: 'date'
                },
                StandardSupportDisplayName: {
                    type: 'string'
                },
                ThreatProtectionLicenseId: {
                    type: 'number'
                },
                ThreatProtectionAuthCode: {
                    type: 'string'
                },
                ThreatProtectionExpiration: {
                    type: 'date'
                },
                ThreatProtectionDisplayName: {
                    type: 'string'
                },
                UrlLicenseId: {
                    type: 'number'
                },
                UrlAuthCode: {
                    type: 'string'
                },
                UrlExpiration: {
                    type: 'date'
                },
                UrlDisplayName: {
                    type: 'string'
                },
                GpPortalLicenseId: {
                    type: 'number'
                },
                GpPortalAuthCode: {
                    type: 'string'
                },
                GpPortalExpiration: {
                    type: 'date'
                },
                GpPortalDisplayName: {
                    type: 'string'
                },
                GpGatewayLicenseId: {
                    type: 'number'
                },
                GpGatewayAuthCode: {
                    type: 'string'
                },
                GpGatewayExpiration: {
                    type: 'date'
                },
                GpGatewayDisplayName: {
                    type: 'string'
                },
                WildfireLicenseId: {
                    type: 'number'
                },
                WildfireAuthCode: {
                    type: 'string'
                },
                WildfireExpiration: {
                    type: 'date'
                },
                WildfireDisplayName: {
                    type: 'string'
                }
            }
        }
    };

    var devAndLicsGridColumns = function (renderMovableCell) {
        return [
            {
                title: 'Asset',
                field: 'AssetDisplayName',
                filterable: {},
                encoded: true
            }, {
                title: 'Service',
                template: '#=' + renderMovableCell + '(data, "StandardSupport")#',
                field: 'StandardSupportDisplayName',
                filterable: {},
                encoded: true
            }, {
                title: 'Threat<br/>Protection',
                template: '#=' + renderMovableCell + '(data, "ThreatProtection")#',
                field: 'ThreatProtectionDisplayName',
                filterable: {},
                encoded: true
            }, {
                title: 'Url<br/>Filtering',
                template: '#=' + renderMovableCell + '(data, "Url")#',
                field: 'UrlDisplayName',
                filterable: {},
                encoded: true
            }, {
                title: 'GP<br/>Portal',
                template: '#=' + renderMovableCell + '(data, "GpPortal")#',
                field: 'GpPortalDisplayName',
                filterable: {},
                encoded: true
            }, {
                title: 'GP<br/>Gateway',
                template: '#=' + renderMovableCell + '(data, "GpGateway")#',
                field: 'GpGatewayDisplayName',
                filterable: {},
                encoded: true
            }, {
                title: 'WildFire',
                template: '#=' + renderMovableCell + '(data, "Wildfire")#',
                field: 'WildfireDisplayName',
                filterable: {},
                encoded: true
            }
        ];
    }

    return {
        availableLicsDsSchema: availableLicsDsSchema,
        availableLicsGridColumns: availableLicsGridColumns,
        devAndLicsDsSchema: devAndLicsDsSchema,
        devAndLicsGridColumns: devAndLicsGridColumns
    };
})();