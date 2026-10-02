/* GOAL: Resolve Coveo vs Hash conflict (Coveo keeps state in Url hash)
 * 
 * Usage:
 *   - you must set variable selector_SearchInterface
 *   - you must call coveoHashHandler() before coveo.init() function
 * 
 * OPEN PROBLEMS from sample: 
 *  - TODO: how to replace URI.parseQuery in parseState (https://medialize.github.io/URI.js/docs.html#static-parseQuery)
 * RESOLVED PROBLEMS:
 *  - RESOLVED: how to replace Coveo.HashUtils in parseState; Coveo.HashUtils exists and is accessible in JSUI "1.2537.19""
 */

var selector_SearchInterface = '#search';

function coveoHashHandler() {

    //coveoIgnoreNextHashChange: prevent interferences with Angular
    this.coveoIgnoreNextHashChange = false;

    var searchInterface = Coveo.$$(document).find(selector_SearchInterface);

    //afterInitialization: on landing on the page, take what was passed as hash and store it in Coveo right now
    Coveo.$$(searchInterface).on("afterInitialization", function (e, args) {
        //$log.debug('afterInitialization call');

        //call parseState utils (in this unit)
        //var parsedState = parseState(location.hash) // Comments By LK On 07/20
        //Coveo.state(searchInterface, parsedState); // Comments By LK On 07/20

        //Auto-executeQuery: in the layout we set " data-enable-history="false"  on CoveoSearchInterface element to prevent conflict but this prevents from executing the query automatically; doing it manually.
        //TODO: log Usage Analytics (must do when calling executeQuery manually)
        //Coveo.executeQuery(searchInterface);  // Comments By LK On 07/20
        //$log.debug('afterInitialization parsed the state and executed the query');

        //on state change: call handleStateChange
        Coveo.$$(searchInterface).on('state:change', function (e, data) {
            //$log.debug('state:change call. calling handleStateChange function');
            handleStateChange();
        });

        var oldUrl = window.location.href;

        $(window).on('hashchange', function (e) {
            var newUrl = window.location.href;

            //TODO: replace URI
            var parseNew = URI.parse(newUrl);
            //TODO: replace URI
            var parseOld = URI.parse(oldUrl);

            if (parseNew.path === location.pathname) { //if (location.pathname !== "/search.html") {
                handleHashChange(parseNew.fragment);
            }
        }).trigger('hashchange');
    });

    /* parseState
    *    Coveo utility to transform hash into state
    */
    function parseState(state) {
        //TODO replace URI
        var decoded = URI.parseQuery(decodeURIComponent(state));

        var parsedState = {};
        Coveo._.each(decoded, function (value, key) {
            //TODO: replace Coveo.HashUtils
            parsedState[key] = Coveo.HashUtils.getValue(key, '#' + state);
        })

        return parsedState;
    }

    /* handleStateChange
    *    Coveo utility;
    */
    function handleStateChange() {
        if (this.coveoIgnoreNextHashChange) {
            this.coveoIgnoreNextHashChange = false;
            return;
        }
        //$log.debug('Handle state change');
        var searchState = Coveo.state(searchInterface).getAttributes();
        var queryString = Coveo.HashUtils.encodeValues(searchState);

        this.ignoreNextHashChange = true;

        if (queryString) {
            window.location.hash = queryString;
        }
    }

    function handleHashChange(fragment) {
        if (this.ignoreNextHashChange) {
            this.ignoreNextHashChange = false;
            return;
        }

        this.coveoIgnoreNextHashChange = true;

        // Comments By LK On 07/20
        var parsedState = parseState(fragment);
        var defaultAttributes = Coveo.state(searchInterface).defaultAttributes;
        Coveo.state(searchInterface, Coveo._.defaults(parsedState, defaultAttributes));

        // Comments By LK On 09/14
        // As per JP, we don't need to trigger an executeQuery
        /*var si = Coveo.$('.CoveoSearchInterface').coveo('get','SearchInterface');
        
        Coveo.Initialization.logFirstQueryCause(si);
        Coveo.executeQuery(searchInterface);*/

        /*var newState = parseState(fragment);

        var search = $('#search');
        var currentState = search.coveo('state');
        search.coveo('state', _.defaults(newState, currentState.defaultAttributes));
        search.coveo('executeQuery');*/
    }
}

/*onDoneBuildingQuery_AddTokenFilter
    TEMPORARY FUNCTION: until server-sdie coee (DNN/Salesforce) inject searchToekn, this reusable function used to inject proper filter
*/

// Comments By LK On 09/07/18
// New function to pass userType and envType context variables
var onBuildingQuery_AddContextVariables = function (root) {
    var contextVariables = {
        "envType": Coveo.context.envType,
        "isAuthUser": Coveo.context.isAuthUser
    };

    contextVariables[Coveo.context.userTypeKey] = Coveo.context.userType;
   
    Coveo.$$(root).on("buildingQuery", function (e, args) {
        args.queryBuilder.addContext(contextVariables);
    });
}

var onDoneBuildingQuery_AddTokenFilter = function (root) {
    //tokenFilter: shall be added to searchToken by server-side code (DNN/Salesforce)
    Coveo.$$(root).on('doneBuildingQuery', function (e, args) {
        args.queryBuilder.constantExpression.add(Coveo.context.tokenFilter);
    });
}

/*COVEO: initialization of site-wide searchbox*/
document.addEventListener('coveoLoaded', function () {
    Coveo.SearchEndpoint.configureCloudEndpoint(Coveo.context.organization, Coveo.context.token, Coveo.context.restEndpointUrl);

    if (!Coveo.$$(document).find('#search.CoveoSearchInterface')) {
        var root = Coveo.$$(document).find("#searchbox");

        if (root) {
            Coveo.initSearchbox(root, Coveo.context.searchPageUrl, {
                Searchbox: {
                    placeholder: "What are you looking for?"
                },
                Analytics: {
                    searchHub: Coveo.context.searchHub
                }
            });
        }

        setTimeout(function () {
            var mobileRoot = Coveo.$$(document).find("#mobile-searchbox");

            if (mobileRoot) {
                Coveo.initSearchbox(mobileRoot, Coveo.context.searchPageUrl, {
                    Searchbox: {
                        placeholder: "What are you looking for?"
                    },
                    Analytics: {
                        searchHub: Coveo.context.searchHub
                    }
                });
            }
        }, 0);
    }
});
/*COVEO: END initialization of site-wide searchbox*/

/*
 * hideDupResultOfFakeSmartSnippets: prevent duplicate result when a FakeSmartSnippet is displayed 
 * (Coveo PS Ref: #249714)
 */
document.addEventListener('coveoLoaded', function () {
    function hideDupResultOfFakeSmartSnippets() {
        var listSS = document.querySelectorAll('.CoveoSmartSnippet .CoveoResultLink.coveo-smart-snippet-source-title');
        listSS.forEach(function (elSS) {
            //href to find in results
            var href = elSS.getAttribute('href');
            if (href) {
                //elResultLink: href of result to remove (is duplicate of fake smart snippet)
                var elResultLink = document.querySelector('.CoveoResultList .CoveoResultLink[href="' + href + '"]');
                if (elResultLink) {
                    //find its CoveoResult parent
                    var elResult = elResultLink.closest('.CoveoResult');
                    if (elResult) {
                        //remove the result 
                        elResult.remove();
                    }
                }
            }
        })
    }
    var elRoot = document.querySelector('#search');
    if (elRoot) {
        Coveo.$$(elRoot).on(Coveo.QueryEvents.deferredQuerySuccess, hideDupResultOfFakeSmartSnippets)
    }
})

/*COVEO: initialization of csp-search-results */
// BEGIN : Configuring By Date filter which is in lithum
var dateRange = [];
var sDate = new Date(1990, 2, 1);
var eDate = new Date();
eDate.setMonth(eDate.getMonth() + 1);
var obj = {
    start: sDate,
    end: eDate,
    label: "All",
    endInclusive: false
};
dateRange.push(obj);
var eDate = new Date();
eDate.setMonth(eDate.getMonth() + 1);
eDate.setDate(eDate.getDate() - 2);
var obj = {
    start: sDate,
    end: eDate,
    label: "A day ago",
    endInclusive: false
};
dateRange.push(obj);
var eDate = new Date();
eDate.setMonth(eDate.getMonth() + 1);
eDate.setDate(eDate.getDate() - 8);
var obj = {
    start: sDate,
    end: eDate,
    label: "A week ago",
    endInclusive: false
};
dateRange.push(obj);
var eDate = new Date();
eDate.setMonth(eDate.getMonth() + 1);
eDate.setDate(eDate.getDate() - 31);
var obj = {
    start: sDate,
    end: eDate,
    label: "A month ago",
    endInclusive: false
};
dateRange.push(obj);
var eDate = new Date();
eDate.setMonth(eDate.getMonth() + 1);
eDate.setDate(eDate.getDate() - 365);
var obj = {
    start: sDate,
    end: eDate,
    label: "A year ago",
    endInclusive: false
};
dateRange.push(obj);

document.addEventListener('coveoLoaded', function () {
    // BEGIN : Override Coveo error messages
    String.toLocaleString({
        "en": {
            "OopsError": "Please log in to access these pages."
        }
    });
    // END : Override Coveo error messages

    // END : Configuring By Date filter which is in lithum
    //BEGIN : Renaming Source names
    var envType = (window.PortalContext.envType) ? " " + window.PortalContext.envType.toUpperCase() : "";
    var kbSourceName = (envType === "") ? "Salesforce - Knowledge - Production" : "Salesforce - Knowledge - GCS" + envType;
    var valueCaption = {
        "Fluid Topics - Cortex Technical Documentation": "Cortex Technical Documentation",
        "Lithium - https://live.paloaltonetworks.com/t5/Discussions/ct-p/members": "Discussions",
        "Web - PATechDocs": "Technical Documentation",
        "Web - PAWebResources": "Palo Alto Networks Website",
        "YouTube - PAN Tutorials": "Video Tutorials",
        "Lithium - Blogs": "Blogs",
        "LIVEcommunity": "LIVEcommunity",
        "LIVEcommunity Public": "LIVEcommunity",
        "Live Community prod - SmartConX": "LIVEcommunity",
        "TechDocs Sitemap": "Technical Documentation",
        "PAN techDOCS": "Technical Documentation",
        "PANW Prisma Cloud Docs": "Prisma Cloud Technical Documentation",
        "Salesforce Knowledge SIT": "Knowledge Base",
        "Salesforce - Knowledge - GCS UAT": "Knowledge Base",
        "Salesforce Knowledge QAQ3": "Knowledge Base",
        "Salesforce Knowledge QA": "Knowledge Base",
        "Salesforce Knowledge Production": "Knowledge Base",
        "Salesforce Knowledge": "Knowledge Base",
        "Sitemap - PAWebResources": "Palo Alto Networks Website",
        "Intercom - Okyo - Prod": "Okyo Help Center",
        "TD pan.dev Docs": "Developer Docs",
        "Unit42 Web": "Unit 42",
    };

    valueCaption[kbSourceName] = "Knowledge Base";

    //END : Renaming Source names

    var root = Coveo.$$(document).find("#search");
    if (root) {
        //BEGIN : Renaming Source names for Field Value
        Coveo.TemplateHelpers.registerTemplateHelper("fieldValueCaptionHelper", (value, options) =>  {
            return valueCaption[value] ? valueCaption[value] : value;
        });
        //END : Renaming Source names for Field Value

        var mySearchBox = Coveo.$$(document).find("#searchbox");

        onBuildingQuery_AddContextVariables(root);

        /*onDoneBuildingQuery_AddTokenFilter
            TEMPORARY FUNCTION: until server-side code (DNN/Salesforce) inject searchToken, this reusable function used to inject proper filter
        */
        onDoneBuildingQuery_AddTokenFilter(root);

        coveoHashHandler();

        // Disable the resultLink title tooltip
        // https://dev.azure.com/CoveoPS/Palo%20Alto%20-%20Coveo%20Migration/_queries/edit/154862/?triage=true
        Coveo.$$(root).on("newResultDisplayed", function(e, args) {
            try {
                args.item.querySelector(".CoveoResultLink").setAttribute("title", "");
            } catch (error) {
                console.warn('Unable to deacitvate CoveoResultLink title tooltip', error);
            }
        });

        Coveo.init(root, {
            //externalComponents: [mySearchBox, anaytics],
            externalComponents: [mySearchBox],
            Searchbox: {
                placeholder: "What are you looking for?"
            },
            Analytics: {
                searchHub: Coveo.context.searchHub
            },
            Facet: {
                valueCaption: valueCaption,
            },
            FacetRange: {
                field: "@@sysdate",
                ranges: dateRange
            }
        });
    }
});
/*COVEO: END initialization of csp-search-results */

/*
 * CoveoQueryPropertyOutput: Sets the innerText of the element 
 * Specify property to output with data-property; allowed values: totalCountFiltered, query
 *  - totalCountFiltered: outputs how many items in the index are matching the query for the current user (security trimming applied) after filtering has been applied. (https://developers.coveo.com/display/public/SearchREST/Query+Results)
 *  - query: outputs the query expression entered by the user (https://developers.coveo.com/display/public/SearchREST/Query+Parameters#QueryParameters-q)
 */
document.addEventListener('coveoLoaded', function () {
    //List of allowed "data_property": totalCountFiltered,query
    var propertyHandlers = {
        totalCountFiltered: function (root, element) {
            Coveo.$$(root).on('querySuccess', function (e, args) {
                //toLocaleString provide separator
                element.innerText = args.results.totalCountFiltered.toLocaleString();
            });
        },
        query: function (root, element) {
            Coveo.$$(root).on('querySuccess', function (e, args) {
                var query = Coveo.state(root, 'q');
                element.innerText = query;
            });
        }
    }
    //initialization of all instances
    var el = Coveo.$$(document).findAll(".CoveoQueryPropertyOutput");
    if (el.length > 0) {
        el.forEach(function (element) {
            //retrieve desired property and validate support
            var dataproperty = element.getAttribute('data-property')
            var propertyHandler = propertyHandlers[dataproperty];
            if (propertyHandler) {
                //call the desired handler
                var root = Coveo.$$(element).closest('.CoveoSearchInterface');
                propertyHandler(root, element)
            }
        }, this);
    }
})

/*START: CoveoUtils*/
var CoveoUtils = (CoveoUtils || {});
CoveoUtils.flattenAttrExclObjects = function (targetobject, sourceobject, nameprefix) {
    var prefix = nameprefix ? nameprefix + '_' : '';
    for (var prop in sourceobject) {
        var item = sourceobject[prop];
        var itemname = prefix + prop;
        if (typeof item != 'object') {
            targetobject[itemname] = item;
        }
        else {
            //"attributes" from SF serialization we don't want pushed to Coveo UA/ML
            if (prop == 'attributes') {
                continue;
            }

            //recurse
            targetobject = CoveoUtils.flattenAttrExclObjects(targetobject, item, itemname)
        }
    }
    return targetobject;
}
/*END: CoveoUtils*/