# Dataset Schema: itp-aia-datalake.csp

## Table: accounttype
| Column Name | Type | Mode |
|---|---|---|
| accounttypeid | INTEGER | NULLABLE |
| name | STRING | NULLABLE |
| administrativeroleid | INTEGER | NULLABLE |
| securityclassification | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: alertmessages
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| messageheader | STRING | NULLABLE |
| messagedetails | STRING | NULLABLE |
| active | INTEGER | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| scope | NUMERIC | NULLABLE |
| severity | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: appportalapps
| Column Name | Type | Mode |
|---|---|---|
| appportalappsid | NUMERIC | NULLABLE |
| appinternalname | STRING | NULLABLE |
| appdisplayname | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | TIMESTAMP | NULLABLE |
| updatedby | STRING | NULLABLE |
| updateddate | TIMESTAMP | NULLABLE |
| appportalappstypeid | NUMERIC | NULLABLE |
| haslicense | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: ascdata
| Column Name | Type | Mode |
|---|---|---|
| ascdataid | INTEGER | NULLABLE |
| ascid | INTEGER | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| createdon | DATE | NULLABLE |
| closedon | DATE | NULLABLE |
| casenumber | STRING | NULLABLE |
| customername | STRING | NULLABLE |
| customeremail | STRING | NULLABLE |
| loadedon | TIMESTAMP | NULLABLE |
| loadedbyuseraccountid | INTEGER | NULLABLE |
| processedmonth | STRING | NULLABLE |
| processedon | TIMESTAMP | NULLABLE |
| processedbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: assetactivation
| Column Name | Type | Mode |
|---|---|---|
| assetactivationid | INTEGER | NULLABLE |
| evalid | STRING | NULLABLE |
| salesordernumber | STRING | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdby | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: awsaccountsupportaccountmapping
| Column Name | Type | Mode |
|---|---|---|
| id | NUMERIC | NULLABLE |
| awsaccountid | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: bundle
| Column Name | Type | Mode |
|---|---|---|
| bundleid | INTEGER | NULLABLE |
| bundlepartmasterid | INTEGER | NULLABLE |
| featurepartmasterid | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: casecreationproblemsubcategoryproductsupport
| Column Name | Type | Mode |
|---|---|---|
| subcategoryproductsupportid | INTEGER | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| productid | INTEGER | NULLABLE |
| supporttypeid | INTEGER | NULLABLE |
| source | STRING | NULLABLE |
| issupportlicense | INTEGER | NULLABLE |
| regexpartlike | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: chassiscomponent
| Column Name | Type | Mode |
|---|---|---|
| chassiscomponentid | NUMERIC | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| chassisserialnumber | STRING | NULLABLE |
| componentserialnumber | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| region | STRING | NULLABLE |
| city | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: cirrousercredentials
| Column Name | Type | Mode |
|---|---|---|
| id | NUMERIC | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| username | STRING | NULLABLE |
| password | STRING | NULLABLE |
| url | STRING | NULLABLE |
| issynced | INTEGER | NULLABLE |
| timestamp | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: clouddevices
| Column Name | Type | Mode |
|---|---|---|
| clouddeviceid | INTEGER | NULLABLE |
| clouddeviceuniqueid | STRING | NULLABLE |
| productcode | STRING | NULLABLE |
| regioncode | STRING | NULLABLE |
| cloudaccountid | STRING | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| cloudtypeid | NUMERIC | NULLABLE |
| isactive | STRING | NULLABLE |
| datecreated | DATETIME | NULLABLE |
| createdby | STRING | NULLABLE |
| datedeleted | DATETIME | NULLABLE |
| deletedby | STRING | NULLABLE |
| notes | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: cloudgenixdevice
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| poolid | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| deploymenttype | STRING | NULLABLE |
| sitename | STRING | NULLABLE |
| street | STRING | NULLABLE |
| city | STRING | NULLABLE |
| state | STRING | NULLABLE |
| zip | STRING | NULLABLE |
| country | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: cloudgenixmapping
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| cgtenantid | STRING | NULLABLE |
| tenantsize | STRING | NULLABLE |
| devicetype | STRING | NULLABLE |
| firstname | STRING | NULLABLE |
| lastname | STRING | NULLABLE |
| adminemail | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: country
| Column Name | Type | Mode |
|---|---|---|
| countryid | INTEGER | NULLABLE |
| name | STRING | NULLABLE |
| code | STRING | NULLABLE |
| regionlabel | STRING | NULLABLE |
| postalcodelabel | STRING | NULLABLE |
| postalcodemask | STRING | NULLABLE |
| phonemask | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: csspsku
| Column Name | Type | Mode |
|---|---|---|
| csspskuid | NUMERIC | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| skudescription | STRING | NULLABLE |
| ishighavailability | STRING | NULLABLE |
| csspmodelid | NUMERIC | NULLABLE |
| csspsupporttypeid | NUMERIC | NULLABLE |
| cssptermid | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: cvescanconsent
| Column Name | Type | Mode |
|---|---|---|
| consentid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| expirydatetime | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| updatedon | DATETIME | NULLABLE |
| updatedbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: decommisonedinfo
| Column Name | Type | Mode |
|---|---|---|
| id | NUMERIC | NULLABLE |
| deviceid | NUMERIC | NULLABLE |
| licenseid | NUMERIC | NULLABLE |
| isdecommisoned | INTEGER | NULLABLE |
| reason | STRING | NULLABLE |
| createddate | DATETIME | NULLABLE |
| createdby | STRING | NULLABLE |
| updateddate | DATETIME | NULLABLE |
| updatedby | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| decommissioneddate | DATETIME | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: deploymentprofile
| Column Name | Type | Mode |
|---|---|---|
| deploymentprofileid | INTEGER | NULLABLE |
| deploymentprofiletypeid | NUMERIC | NULLABLE |
| name | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| tokenid | INTEGER | NULLABLE |
| dplicenseid | INTEGER | NULLABLE |
| panos | STRING | NULLABLE |
| firewallquantity | NUMERIC | NULLABLE |
| vcpuquantity | NUMERIC | NULLABLE |
| vmmodel | STRING | NULLABLE |
| panorama | STRING | NULLABLE |
| status | NUMERIC | NULLABLE |
| statusmessage | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| tokensallocated | FLOAT | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| updatedby | INTEGER | NULLABLE |
| updatedon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| quantitynode | NUMERIC | NULLABLE |
| productid | STRING | NULLABLE |
| gentype | STRING | NULLABLE |
| vsysquantity | NUMERIC | NULLABLE |
| isairgapeligible | INTEGER | NULLABLE |
| deploymentprofilesubtypeid | INTEGER | NULLABLE |
| avgtxtrecordsperday | INTEGER | NULLABLE |
| isaiopsmigrated | STRING | NULLABLE |
| istsgmapped | STRING | NULLABLE |
| maxthreatsimulations | INTEGER | NULLABLE |
| maxscans | INTEGER | NULLABLE |
| note | STRING | NULLABLE |
| fedramptype | INTEGER | NULLABLE |
| attributes | STRING | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: deploymentprofilestatus
| Column Name | Type | Mode |
|---|---|---|
| deploymentprofilestatusid | NUMERIC | NULLABLE |
| name | STRING | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: deploymentprofilesubscriptions
| Column Name | Type | Mode |
|---|---|---|
| deploymentprofileid | INTEGER | NULLABLE |
| flexsubscriptionid | NUMERIC | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| updatedby | INTEGER | NULLABLE |
| updatedon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: deploymentprofiletype
| Column Name | Type | Mode |
|---|---|---|
| deploymentprofiletypeid | NUMERIC | NULLABLE |
| name | STRING | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: device
| Column Name | Type | Mode |
|---|---|---|
| deviceid | INTEGER | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| serialnumber | STRING | NULLABLE |
| supportcentersupportaccountid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| registrationdate | DATETIME | NULLABLE |
| shipdate | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| shipmentnumber | INTEGER | NULLABLE |
| devicename | STRING | NULLABLE |
| comments | STRING | NULLABLE |
| devicecategoryid | INTEGER | NULLABLE |
| isrmareturn | INTEGER | NULLABLE |
| isrmareplacement | INTEGER | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| customerservicenotes | STRING | NULLABLE |
| issapsynched | INTEGER | NULLABLE |
| virtualsystemsavailable | NUMERIC | NULLABLE |
| graceperioddays | NUMERIC | NULLABLE |
| billtocountryid | INTEGER | NULLABLE |
| city | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| region | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| isvirtualmachine | INTEGER | NULLABLE |
| cpuid | STRING | NULLABLE |
| uuid | STRING | NULLABLE |
| macaddress | STRING | NULLABLE |
| partmasteridasshipped | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| datacenterregion | STRING | NULLABLE |
| hypervisorversion | STRING | NULLABLE |
| panosversion | STRING | NULLABLE |
| productid | STRING | NULLABLE |
| productrequestconfiguration | STRING | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: devicecategory
| Column Name | Type | Mode |
|---|---|---|
| devicecategoryid | INTEGER | NULLABLE |
| devicecategoryname | STRING | NULLABLE |
| description | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| ispublic | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: deviceinfoztp
| Column Name | Type | Mode |
|---|---|---|
| deviceinfoztpid | NUMERIC | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| hashkey | STRING | NULLABLE |
| claimkey | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: devicetransfers
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| destinationuseraccountid | INTEGER | NULLABLE |
| devicetransferstateid | INTEGER | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| reviewedby | INTEGER | NULLABLE |
| reviewedon | DATETIME | NULLABLE |
| bulkdevicetransferid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: domain
| Column Name | Type | Mode |
|---|---|---|
| domainid | INTEGER | NULLABLE |
| domainname | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: eaexcludeddevices
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| eaquotereferenceid | INTEGER | NULLABLE |
| serialnumber | STRING | NULLABLE |
| createdby | STRING | NULLABLE |
| lastmodified | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: eaquoteinfo
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| cpqquotenumber | STRING | NULLABLE |
| eatype | STRING | NULLABLE |
| salesordernumber | STRING | NULLABLE |
| authorizationcode | STRING | NULLABLE |
| partnumber | STRING | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| issfdcsynced | INTEGER | NULLABLE |
| lastmodified | DATETIME | NULLABLE |
| comments | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: easupportaccounts
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| eaquotereferenceid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| sfdcid | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: easyonboarding
| Column Name | Type | Mode |
|---|---|---|
| easyonboardingid | INTEGER | NULLABLE |
| assetactivationid | INTEGER | NULLABLE |
| license_hash | STRING | NULLABLE |
| product | STRING | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: easyonboardingitem
| Column Name | Type | Mode |
|---|---|---|
| easyonboardingitemid | INTEGER | NULLABLE |
| easyonboardingid | INTEGER | NULLABLE |
| licenseid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| productdisplayname | STRING | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| subgroup | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: ems_migrated_entitlement_ids
| Column Name | Type | Mode |
|---|---|---|
| id | NUMERIC | NULLABLE |
| entitlement_id | STRING | NULLABLE |
| entitlement_group_id | STRING | NULLABLE |
| is_migrated | INTEGER | NULLABLE |
| created_by | STRING | NULLABLE |
| created_date | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: entitlement_group
| Column Name | Type | Mode |
|---|---|---|
| id | NUMERIC | NULLABLE |
| entitlement_group_id | STRING | NULLABLE |
| status | STRING | NULLABLE |
| created_for | STRING | NULLABLE |
| created_by | STRING | NULLABLE |
| created_date | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: entitlementsfornetsec
| Column Name | Type | Mode |
|---|---|---|
| entitlementid | STRING | NULLABLE |
| sourceid | STRING | NULLABLE |
| sourcesystem | STRING | NULLABLE |
| sourcetype | STRING | NULLABLE |
| correlationid | STRING | NULLABLE |
| entitlementgroupid | STRING | NULLABLE |
| entitlementversion | INTEGER | NULLABLE |
| supportaccountid | STRING | NULLABLE |
| lineitemid | STRING | NULLABLE |
| entitlementstatus | STRING | NULLABLE |
| sku | STRING | NULLABLE |
| istrial | INTEGER | NULLABLE |
| startdate | DATETIME | NULLABLE |
| enddate | DATETIME | NULLABLE |
| quantity | INTEGER | NULLABLE |
| ascaccountname | STRING | NULLABLE |
| ascaccountid | STRING | NULLABLE |
| supporttype | STRING | NULLABLE |
| supportenddate | DATETIME | NULLABLE |
| iscancelled | INTEGER | NULLABLE |
| creationreason | STRING | NULLABLE |
| acknowledgementdate | DATETIME | NULLABLE |
| admincontacts | STRING | NULLABLE |
| replaces | STRING | NULLABLE |
| replacementreason | STRING | NULLABLE |
| created_by | STRING | NULLABLE |
| updated_by | STRING | NULLABLE |
| created_at | TIMESTAMP | NULLABLE |
| updated_at | TIMESTAMP | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: epagentinfo
| Column Name | Type | Mode |
|---|---|---|
| epagentinfoid | NUMERIC | NULLABLE |
| eptelemetryid | NUMERIC | NULLABLE |
| osversion | STRING | NULLABLE |
| agentcount | NUMERIC | NULLABLE |
| createdate | DATETIME | NULLABLE |
| ostype | STRING | NULLABLE |
| osname | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: epagentinfolog
| Column Name | Type | Mode |
|---|---|---|
| epagentinfologid | NUMERIC | NULLABLE |
| eptelemetryid | NUMERIC | NULLABLE |
| osversion | STRING | NULLABLE |
| agentcount | NUMERIC | NULLABLE |
| createdate | DATETIME | NULLABLE |
| ostype | STRING | NULLABLE |
| osname | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: eptelemetry
| Column Name | Type | Mode |
|---|---|---|
| eptelemetryid | NUMERIC | NULLABLE |
| serialnumber | STRING | NULLABLE |
| supportaccountid | NUMERIC | NULLABLE |
| agentcount | NUMERIC | NULLABLE |
| ipaddress | STRING | NULLABLE |
| currentosversion | STRING | NULLABLE |
| contentversion | STRING | NULLABLE |
| createdate | DATETIME | NULLABLE |
| updatedon | DATETIME | NULLABLE |
| licensecount | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: event_log
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| event_type | STRING | NULLABLE |
| event_agent_id | STRING | NULLABLE |
| event_location | STRING | NULLABLE |
| notes | STRING | NULLABLE |
| topic | STRING | NULLABLE |
| supplemental | STRING | NULLABLE |
| event_time | DATETIME | NULLABLE |
| created_time | DATETIME | NULLABLE |
| event_resolution_type | STRING | NULLABLE |
| event_resolution_notes | STRING | NULLABLE |
| identifier | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: flexsubscriptions
| Column Name | Type | Mode |
|---|---|---|
| flexsubscriptionid | NUMERIC | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| licidentifier | STRING | NULLABLE |
| displayname | STRING | NULLABLE |
| displayorder | NUMERIC | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: flexusecase
| Column Name | Type | Mode |
|---|---|---|
| flexusecaseid | NUMERIC | NULLABLE |
| name | STRING | NULLABLE |
| deploymentprofiletypeid | NUMERIC | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: flexusecasesubscriptionsmapping
| Column Name | Type | Mode |
|---|---|---|
| id | NUMERIC | NULLABLE |
| flexusecaseid | NUMERIC | NULLABLE |
| flexsubscriptionid | NUMERIC | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: genericrestcalllog
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| application | STRING | NULLABLE |
| server | STRING | NULLABLE |
| url | STRING | NULLABLE |
| retryurlservice | STRING | NULLABLE |
| payload | STRING | NULLABLE |
| httpmethod | STRING | NULLABLE |
| notes | STRING | NULLABLE |
| serialnumber | STRING | NULLABLE |
| authcode | STRING | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| createddate | TIMESTAMP | NULLABLE |
| success | INTEGER | NULLABLE |
| httpstatuscode | NUMERIC | NULLABLE |
| errormsg | STRING | NULLABLE |
| retrycount | NUMERIC | NULLABLE |
| lastupdateddate | TIMESTAMP | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: generictelemetry
| Column Name | Type | Mode |
|---|---|---|
| generictelemetryid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| sourceid | STRING | NULLABLE |
| group | STRING | NULLABLE |
| name | STRING | NULLABLE |
| value | STRING | NULLABLE |
| type | STRING | NULLABLE |
| timestamp | DATETIME | NULLABLE |
| data | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: groupdevices
| Column Name | Type | Mode |
|---|---|---|
| groupid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdateuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: igniteevent
| Column Name | Type | Mode |
|---|---|---|
| eventid | INTEGER | NULLABLE |
| eventname | STRING | NULLABLE |
| startdate | DATETIME | NULLABLE |
| enddate | DATETIME | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| theaterid | NUMERIC | NULLABLE |
| ticketprice | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: ignitelicense
| Column Name | Type | Mode |
|---|---|---|
| licenseid | INTEGER | NULLABLE |
| eventid | INTEGER | NULLABLE |
| promocode | STRING | NULLABLE |
| quantity | NUMERIC | NULLABLE |
| quantityremaining | NUMERIC | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| authorizationcode | STRING | NULLABLE |
| effectivedate | DATETIME | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| shipdate | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| shipmentnumber | INTEGER | NULLABLE |
| billtocountryid | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| iscreditcardauthorization | INTEGER | NULLABLE |
| deviceid | NUMERIC | NULLABLE |
| amount | FLOAT | NULLABLE |
| discount | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: ignitesalesorderitem
| Column Name | Type | Mode |
|---|---|---|
| salesorderitemid | INTEGER | NULLABLE |
| salesorderid | INTEGER | NULLABLE |
| licenseid | INTEGER | NULLABLE |
| description | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: ignitetheater
| Column Name | Type | Mode |
|---|---|---|
| theaterid | NUMERIC | NULLABLE |
| name | STRING | NULLABLE |
| code | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: igniteuser
| Column Name | Type | Mode |
|---|---|---|
| userid | INTEGER | NULLABLE |
| licenseid | INTEGER | NULLABLE |
| registrationdate | DATETIME | NULLABLE |
| city | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| email | STRING | NULLABLE |
| firstname | STRING | NULLABLE |
| lastname | STRING | NULLABLE |
| uniqueidentifier | STRING | NULLABLE |
| jobtitle | STRING | NULLABLE |
| joblevel | STRING | NULLABLE |
| companyname | STRING | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| state | STRING | NULLABLE |
| country | STRING | NULLABLE |
| businessphone | STRING | NULLABLE |
| mobilephone | STRING | NULLABLE |
| customerduration | STRING | NULLABLE |
| relationship | STRING | NULLABLE |
| companysize | STRING | NULLABLE |
| industry | STRING | NULLABLE |
| salestrackingemailordomain | STRING | NULLABLE |
| discountcode | STRING | NULLABLE |
| utmmedium | STRING | NULLABLE |
| utmsource | STRING | NULLABLE |
| status | STRING | NULLABLE |
| utmcampaign | STRING | NULLABLE |
| utmcontent | STRING | NULLABLE |
| utmterm | STRING | NULLABLE |
| utmnetwork | STRING | NULLABLE |
| utmname | STRING | NULLABLE |
| utmsitetarget | STRING | NULLABLE |
| amountpaid | FLOAT | NULLABLE |
| theaterid | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: license
| Column Name | Type | Mode |
|---|---|---|
| licenseid | INTEGER | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| authorizationcode | STRING | NULLABLE |
| licensekey | STRING | NULLABLE |
| supportcentersupportaccountid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| effectivedate | DATETIME | NULLABLE |
| registrationdate | DATETIME | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| graceperioddays | NUMERIC | NULLABLE |
| licensetermmonths | NUMERIC | NULLABLE |
| extensionrequestsalesforceid | INTEGER | NULLABLE |
| parentlicenseid | INTEGER | NULLABLE |
| bundlelicenseid | INTEGER | NULLABLE |
| shipdate | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| notes | STRING | NULLABLE |
| shipmentnumber | INTEGER | NULLABLE |
| issapsynched | INTEGER | NULLABLE |
| sitelicensesupportaccountid | INTEGER | NULLABLE |
| city | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| billtocountryid | INTEGER | NULLABLE |
| vmquantity | NUMERIC | NULLABLE |
| vmstubdeviceid | INTEGER | NULLABLE |
| vmparentlicenseid | INTEGER | NULLABLE |
| vmusecount | NUMERIC | NULLABLE |
| supportcentersapid | NUMERIC | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| transactionid | STRING | NULLABLE |
| prelicenseid | NUMERIC | NULLABLE |
| isprelicensed | INTEGER | NULLABLE |
| countryid | INTEGER | NULLABLE |
| saasquantity | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: licensefeature
| Column Name | Type | Mode |
|---|---|---|
| licensefeatureid | INTEGER | NULLABLE |
| licensefeaturename | STRING | NULLABLE |
| isincludedincasecreation | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: licensingapikeys
| Column Name | Type | Mode |
|---|---|---|
| licensingapikeyid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| apikey | STRING | NULLABLE |
| isactive | STRING | NULLABLE |
| createdby | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| updatedby | INTEGER | NULLABLE |
| updatedon | DATETIME | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| productid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: logdevice
| Column Name | Type | Mode |
|---|---|---|
| logdeviceid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| serialnumber | STRING | NULLABLE |
| supportcentersupportaccountid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| registrationdate | DATETIME | NULLABLE |
| shipdate | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| shipmentnumber | INTEGER | NULLABLE |
| devicename | STRING | NULLABLE |
| comments | STRING | NULLABLE |
| isrmareturn | INTEGER | NULLABLE |
| isrmareplacement | INTEGER | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| customerservicenotes | STRING | NULLABLE |
| issapsynched | INTEGER | NULLABLE |
| virtualsystemsavailable | NUMERIC | NULLABLE |
| graceperioddays | NUMERIC | NULLABLE |
| billtocountryid | INTEGER | NULLABLE |
| city | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| isvirtualmachine | INTEGER | NULLABLE |
| cpuid | STRING | NULLABLE |
| uuid | STRING | NULLABLE |
| macaddress | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| reason | STRING | NULLABLE |
| logdate | DATETIME | NULLABLE |
| logbyuser | STRING | NULLABLE |
| devicecategoryid | INTEGER | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: logmembership
| Column Name | Type | Mode |
|---|---|---|
| logmembershipid | INTEGER | NULLABLE |
| membershipid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| description | STRING | NULLABLE |
| activationdate | DATETIME | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| salesforceloginid | STRING | NULLABLE |
| salesforcecontactid | STRING | NULLABLE |
| jiveid | STRING | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| groupid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| membershipstatus | INTEGER | NULLABLE |
| membershipresponseguid | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| reason | STRING | NULLABLE |
| logdate | TIMESTAMP | NULLABLE |
| logbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: logmembershiproles
| Column Name | Type | Mode |
|---|---|---|
| logmembershiprolesid | INTEGER | NULLABLE |
| membershipid | INTEGER | NULLABLE |
| roleid | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| reason | STRING | NULLABLE |
| logdate | TIMESTAMP | NULLABLE |
| logbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: loguseraccount
| Column Name | Type | Mode |
|---|---|---|
| loguseraccountid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| email | STRING | NULLABLE |
| displayname | STRING | NULLABLE |
| title | STRING | NULLABLE |
| firstname | STRING | NULLABLE |
| lastname | STRING | NULLABLE |
| phonenumber | STRING | NULLABLE |
| emailoptin | INTEGER | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| city | STRING | NULLABLE |
| region | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| emaillastsent | DATETIME | NULLABLE |
| emailresponse | STRING | NULLABLE |
| securityemailoptin | INTEGER | NULLABLE |
| ipaddress | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| reason | STRING | NULLABLE |
| logdate | TIMESTAMP | NULLABLE |
| logbyuseraccountid | INTEGER | NULLABLE |
| isblocked | INTEGER | NULLABLE |
| defaultsupportaccountgroupid | INTEGER | NULLABLE |
| defaultsupportaccountid | INTEGER | NULLABLE |
| jiveid | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: loki_dm_product_mapping
| Column Name | Type | Mode |
|---|---|---|
| salesforceaccountid | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| supportaccountname | STRING | NULLABLE |
| entitlementgroupid | STRING | NULLABLE |
| createdbyuseremail | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| updatedbyuseremail | STRING | NULLABLE |
| updatedon | TIMESTAMP | NULLABLE |
| accounttype | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: membership
| Column Name | Type | Mode |
|---|---|---|
| membershipid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| description | STRING | NULLABLE |
| activationdate | DATETIME | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| groupid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| jiveid | STRING | NULLABLE |
| salesforcecontactid | STRING | NULLABLE |
| salesforceloginid | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| membershipstatus | INTEGER | NULLABLE |
| membershipresponseguid | STRING | NULLABLE |
| ismfaenabled | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: membershiproles
| Column Name | Type | Mode |
|---|---|---|
| membershipid | INTEGER | NULLABLE |
| roleid | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: menuprivileges
| Column Name | Type | Mode |
|---|---|---|
| menuprivilegeid | NUMERIC | NULLABLE |
| menuid | NUMERIC | NULLABLE |
| submenuid | NUMERIC | NULLABLE |
| privilegeid | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: migrateddevice
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| serialnumber | STRING | NULLABLE |
| newserialnumber | STRING | NULLABLE |
| supportcentersupportaccountid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| useraccountid | INTEGER | NULLABLE |
| registrationdate | DATETIME | NULLABLE |
| shipdate | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| shipmentnumber | INTEGER | NULLABLE |
| devicename | STRING | NULLABLE |
| comments | STRING | NULLABLE |
| devicecategoryid | INTEGER | NULLABLE |
| isrmareturn | INTEGER | NULLABLE |
| isrmareplacement | INTEGER | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| customerservicenotes | STRING | NULLABLE |
| issapsynched | INTEGER | NULLABLE |
| virtualsystemsavailable | NUMERIC | NULLABLE |
| graceperioddays | NUMERIC | NULLABLE |
| billtocountryid | INTEGER | NULLABLE |
| city | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| region | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| isvirtualmachine | INTEGER | NULLABLE |
| cpuid | STRING | NULLABLE |
| uuid | STRING | NULLABLE |
| macaddress | STRING | NULLABLE |
| partmasteridasshipped | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| datacenterregion | STRING | NULLABLE |
| hypervisorversion | STRING | NULLABLE |
| panosversion | STRING | NULLABLE |
| productid | STRING | NULLABLE |
| productrequestconfiguration | STRING | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: onlineservicesconfig
| Column Name | Type | Mode |
|---|---|---|
| levelid | NUMERIC | NULLABLE |
| levelname | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: onlineservicessubconfig
| Column Name | Type | Mode |
|---|---|---|
| sublevelid | NUMERIC | NULLABLE |
| sublevelname | STRING | NULLABLE |
| levelid | NUMERIC | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: pa_pricing_migration_output
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| cspid | INTEGER | NULLABLE |
| paetenantid | STRING | NULLABLE |
| serialnumber | STRING | NULLABLE |
| oldauthorizationcode | STRING | NULLABLE |
| newauthorizationcode | STRING | NULLABLE |
| oldpartnumber | STRING | NULLABLE |
| newpartnumber | STRING | NULLABLE |
| registrationdate | DATETIME | NULLABLE |
| expirationdate | DATETIME | NULLABLE |
| saasquantity | NUMERIC | NULLABLE |
| processtime | DATETIME | NULLABLE |
| status | STRING | NULLABLE |
| statusdetail | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: partmaster
| Column Name | Type | Mode |
|---|---|---|
| partmasterid | INTEGER | NULLABLE |
| partnumber | STRING | NULLABLE |
| description | STRING | NULLABLE |
| modelcompatibility | STRING | NULLABLE |
| licensetemplate | STRING | NULLABLE |
| parttypeid | INTEGER | NULLABLE |
| partsubtypeid | INTEGER | NULLABLE |
| productfamily | STRING | NULLABLE |
| usecount | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| devicesetid | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: partsubtype
| Column Name | Type | Mode |
|---|---|---|
| partsubtypeid | INTEGER | NULLABLE |
| parttypeid | INTEGER | NULLABLE |
| pandbidentification | STRING | NULLABLE |
| name | STRING | NULLABLE |
| description | STRING | NULLABLE |
| licensefeaturesname | STRING | NULLABLE |
| licensetype | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| supportdisplayname | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: parttype
| Column Name | Type | Mode |
|---|---|---|
| parttypeid | INTEGER | NULLABLE |
| pandbidentification | STRING | NULLABLE |
| description | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: phoenixgrandparents
| Column Name | Type | Mode |
|---|---|---|
| phoenixgrandparentid | INTEGER | NULLABLE |
| grandparentlicenseid | INTEGER | NULLABLE |
| parentlicenseid | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| addittokensapplieddate | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: productquantity
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| type | STRING | NULLABLE |
| authcode | STRING | NULLABLE |
| projectedquantity | INTEGER | NULLABLE |
| capquantity | INTEGER | NULLABLE |
| burstvalue | INTEGER | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | DATETIME | NULLABLE |
| updatedby | INTEGER | NULLABLE |
| updateddate | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: rmaapplicabledevices
| Column Name | Type | Mode |
|---|---|---|
| serialnumber | STRING | NULLABLE |
| material | STRING | NULLABLE |
| distributerid | STRING | NULLABLE |
| endcustomerid | STRING | NULLABLE |
| customername | STRING | NULLABLE |
| address | STRING | NULLABLE |
| city | STRING | NULLABLE |
| region | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| country | STRING | NULLABLE |
| salesorderno | STRING | NULLABLE |
| deliveryno | STRING | NULLABLE |
| purchaseorderno | STRING | NULLABLE |
| shipdate | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: rmadevice
| Column Name | Type | Mode |
|---|---|---|
| deviceid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| serialnumber | STRING | NULLABLE |
| techsupportfilename | STRING | NULLABLE |
| shippingaddress | STRING | NULLABLE |
| shippingcity | STRING | NULLABLE |
| shippingregion | STRING | NULLABLE |
| shippingcountry | STRING | NULLABLE |
| shippingpostalcode | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: rmadevicejobqueue
| Column Name | Type | Mode |
|---|---|---|
| rmajobqueueid | INTEGER | NULLABLE |
| jobuniqueid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| statuscode | NUMERIC | NULLABLE |
| status | STRING | NULLABLE |
| statusmessage | STRING | NULLABLE |
| verdict | STRING | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: role
| Column Name | Type | Mode |
|---|---|---|
| roleid | INTEGER | NULLABLE |
| name | STRING | NULLABLE |
| description | STRING | NULLABLE |
| category | NUMERIC | NULLABLE |
| securityclassification | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: roleprivileges
| Column Name | Type | Mode |
|---|---|---|
| roleid | INTEGER | NULLABLE |
| privilegeid | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: salesorder
| Column Name | Type | Mode |
|---|---|---|
| salesorderid | INTEGER | NULLABLE |
| salesdate | DATETIME | NULLABLE |
| sapcustomerid | INTEGER | NULLABLE |
| sapsalesordernumber | STRING | NULLABLE |
| purchaseordernumber | STRING | NULLABLE |
| ordertype | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: salesorderitem
| Column Name | Type | Mode |
|---|---|---|
| salesorderitemid | INTEGER | NULLABLE |
| salesorderid | INTEGER | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| licenseid | INTEGER | NULLABLE |
| description | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: sapsupportaccountmapping
| Column Name | Type | Mode |
|---|---|---|
| sapcustomermappingid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| sapid | NUMERIC | NULLABLE |
| isprimary | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: secaascertrequest
| Column Name | Type | Mode |
|---|---|---|
| secaascertrequestid | INTEGER | NULLABLE |
| productcategory | STRING | NULLABLE |
| endentityid | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| saasotpid | INTEGER | NULLABLE |
| certserialnumber | STRING | NULLABLE |
| certdn | STRING | NULLABLE |
| certexpirationdate | DATETIME | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | DATETIME | NULLABLE |
| issuccess | STRING | NULLABLE |
| isrenew | STRING | NULLABLE |
| certissuerdn | STRING | NULLABLE |
| sourceipaddress | STRING | NULLABLE |
| notes | STRING | NULLABLE |
| isrevoked | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: secaaspanoramaregistration
| Column Name | Type | Mode |
|---|---|---|
| secaaspanoramaregistrationid | NUMERIC | NULLABLE |
| panoramaserialnumber | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| registrationnumber | NUMERIC | NULLABLE |
| createddate | DATETIME | NULLABLE |
| tenantid | STRING | NULLABLE |
| secondaryserialnumber | STRING | NULLABLE |
| isha | STRING | NULLABLE |
| isactive | STRING | NULLABLE |
| updateddate | DATETIME | NULLABLE |
| updatedby | STRING | NULLABLE |
| ispurging | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: secaaspanoramaregistration_bkp
| Column Name | Type | Mode |
|---|---|---|
| bkpid | INTEGER | NULLABLE |
| panoramaserialnumber | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| registrationnumber | NUMERIC | NULLABLE |
| createddate | DATETIME | NULLABLE |
| tenantid | STRING | NULLABLE |
| secondaryserialnumber | STRING | NULLABLE |
| isha | STRING | NULLABLE |
| isactive | STRING | NULLABLE |
| updateddate | DATETIME | NULLABLE |
| updatedby | STRING | NULLABLE |
| ispurging | INTEGER | NULLABLE |
| bkp_when | TIMESTAMP | NULLABLE |
| bkp_by | STRING | NULLABLE |
| bkp_reason | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: skusupportmapping
| Column Name | Type | Mode |
|---|---|---|
| skusupportmappingid | NUMERIC | NULLABLE |
| supporttype | STRING | NULLABLE |
| partmasterid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: softwareupdates
| Column Name | Type | Mode |
|---|---|---|
| softwareupdateid | INTEGER | NULLABLE |
| filename | STRING | NULLABLE |
| versionnumber | STRING | NULLABLE |
| modelssupported | STRING | NULLABLE |
| releasedate | DATETIME | NULLABLE |
| releasenotes | STRING | NULLABLE |
| platform | STRING | NULLABLE |
| product | STRING | NULLABLE |
| devicesetid | INTEGER | NULLABLE |
| downloadurl | STRING | NULLABLE |
| filesize | FLOAT | NULLABLE |
| minimumrequiredversion | STRING | NULLABLE |
| maximimsupportedversion | STRING | NULLABLE |
| md5key | STRING | NULLABLE |
| formatversion | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| createdon | TIMESTAMP | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | TIMESTAMP | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: supportaccount
| Column Name | Type | Mode |
|---|---|---|
| supportaccountid | INTEGER | NULLABLE |
| sapid | NUMERIC | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| accounttypeid | INTEGER | NULLABLE |
| name | STRING | NULLABLE |
| phonenumber | STRING | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| city | STRING | NULLABLE |
| region | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| isoptoutofallemails | INTEGER | NULLABLE |
| supportaccountlogo | STRING | NULLABLE |
| logocontenttype | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| isjivetrainingcentermember | INTEGER | NULLABLE |
| hasjivedirectcustomersupport | INTEGER | NULLABLE |
| isjivecpsp | INTEGER | NULLABLE |
| isjivesupportcentermember | INTEGER | NULLABLE |
| customernumber | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| ispartner | INTEGER | NULLABLE |
| regrequiressuapproval | INTEGER | NULLABLE |
| iscssp | INTEGER | NULLABLE |
| ismfaenabled | INTEGER | NULLABLE |
| ispublicsector | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: supportaccountgroup
| Column Name | Type | Mode |
|---|---|---|
| groupid | INTEGER | NULLABLE |
| salesforceid | STRING | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| name | STRING | NULLABLE |
| description | STRING | NULLABLE |
| phonenumber | STRING | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| city | STRING | NULLABLE |
| region | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: tenantassociation
| Column Name | Type | Mode |
|---|---|---|
| tenantassociationid | NUMERIC | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| associatedtenantid | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isactive | INTEGER | NULLABLE |
| updatedon | DATETIME | NULLABLE |
| serialnumber | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: tenantlookup
| Column Name | Type | Mode |
|---|---|---|
| tenantlookupid | INTEGER | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| appportalappsid | NUMERIC | NULLABLE |
| serialnumber | STRING | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | DATETIME | NULLABLE |
| updatedby | STRING | NULLABLE |
| updateddate | DATETIME | NULLABLE |
| tenantinstancename | STRING | NULLABLE |
| tenantregion | STRING | NULLABLE |
| oauthconsent | STRING | NULLABLE |
| ispurging | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: tenantlookup_bkp
| Column Name | Type | Mode |
|---|---|---|
| bkpid | INTEGER | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| isactive | INTEGER | NULLABLE |
| appportalappsid | NUMERIC | NULLABLE |
| serialnumber | STRING | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | TIMESTAMP | NULLABLE |
| updatedby | STRING | NULLABLE |
| updateddate | TIMESTAMP | NULLABLE |
| tenantinstancename | STRING | NULLABLE |
| tenantregion | STRING | NULLABLE |
| oauthconsent | STRING | NULLABLE |
| bkp_when | TIMESTAMP | NULLABLE |
| bkp_by | STRING | NULLABLE |
| bkp_reason | STRING | NULLABLE |
| ispurging | INTEGER | NULLABLE |
| isnovatenant | INTEGER | NULLABLE |
| isfirewallcdl | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: tenantplatformmapping
| Column Name | Type | Mode |
|---|---|---|
| tenantplatformmappingid | NUMERIC | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | DATETIME | NULLABLE |
| platformid | INTEGER | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: tenantplatformmapping_bkp
| Column Name | Type | Mode |
|---|---|---|
| bkpid | NUMERIC | NULLABLE |
| tenantid | INTEGER | NULLABLE |
| createdby | STRING | NULLABLE |
| createddate | TIMESTAMP | NULLABLE |
| platformid | INTEGER | NULLABLE |
| bkp_by | STRING | NULLABLE |
| bkp_when | TIMESTAMP | NULLABLE |
| bkp_reason | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: useraccount
| Column Name | Type | Mode |
|---|---|---|
| useraccountid | INTEGER | NULLABLE |
| email | STRING | NULLABLE |
| displayname | STRING | NULLABLE |
| title | STRING | NULLABLE |
| firstname | STRING | NULLABLE |
| lastname | STRING | NULLABLE |
| phonenumber | STRING | NULLABLE |
| isoptinemail | INTEGER | NULLABLE |
| address1 | STRING | NULLABLE |
| address2 | STRING | NULLABLE |
| city | STRING | NULLABLE |
| region | STRING | NULLABLE |
| postalcode | STRING | NULLABLE |
| countryid | INTEGER | NULLABLE |
| emaillastsent | DATETIME | NULLABLE |
| emailresponse | STRING | NULLABLE |
| isoptinsecurityemail | INTEGER | NULLABLE |
| ipaddress | STRING | NULLABLE |
| isactive | INTEGER | NULLABLE |
| isblocked | INTEGER | NULLABLE |
| istempmessageviewed | INTEGER | NULLABLE |
| createdon | DATETIME | NULLABLE |
| createdbyuseraccountid | INTEGER | NULLABLE |
| lastupdate | DATETIME | NULLABLE |
| lastupdatebyuseraccountid | INTEGER | NULLABLE |
| defaultsupportaccountid | INTEGER | NULLABLE |
| isatc | INTEGER | NULLABLE |
| isasc | INTEGER | NULLABLE |
| isdirectsupport | INTEGER | NULLABLE |
| selectedmembershipid | INTEGER | NULLABLE |
| selectedsalesforceloginid | STRING | NULLABLE |
| selectedsalesforcecontactid | STRING | NULLABLE |
| selectedsalesforceaccountid | STRING | NULLABLE |
| jiveid | STRING | NULLABLE |
| actualemail | STRING | NULLABLE |
| iscpsp | INTEGER | NULLABLE |
| isoptinpanosupdateemail | INTEGER | NULLABLE |
| ismfaenabled | INTEGER | NULLABLE |
| multifactorauthtype | NUMERIC | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: vmelatokenexchangerate
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| model | STRING | NULLABLE |
| partsubtypeid | INTEGER | NULLABLE |
| exchangerate | NUMERIC | NULLABLE |
| startutcdatetime | DATETIME | NULLABLE |
| endutcdatetime | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: vmevals
| Column Name | Type | Mode |
|---|---|---|
| vmevalid | INTEGER | NULLABLE |
| devicecode | STRING | NULLABLE |
| evalid | STRING | NULLABLE |
| salesforceaccountid | STRING | NULLABLE |
| createdon | DATETIME | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: vmrenewal
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| supportaccountid | INTEGER | NULLABLE |
| authorizationcode | STRING | NULLABLE |
| serialnumber | STRING | NULLABLE |
| selectiontype | INTEGER | NULLABLE |
| requestdate | DATETIME | NULLABLE |
| quotenumber | STRING | NULLABLE |
| quotedate | DATETIME | NULLABLE |
| status | INTEGER | NULLABLE |
| renewalauthcode | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: vmrenewalunprovisioned
| Column Name | Type | Mode |
|---|---|---|
| id | INTEGER | NULLABLE |
| authorizationcode | STRING | NULLABLE |
| requestdate | DATETIME | NULLABLE |
| quotenumber | STRING | NULLABLE |
| quotedate | DATETIME | NULLABLE |
| renewalquantity | NUMERIC | NULLABLE |
| downgradequantity | NUMERIC | NULLABLE |
| forfeitquantity | NUMERIC | NULLABLE |
| status | INTEGER | NULLABLE |
| renewalauthcode | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

## Table: vmupgrade
| Column Name | Type | Mode |
|---|---|---|
| vmupgradeid | NUMERIC | NULLABLE |
| deviceid | INTEGER | NULLABLE |
| transactionid | STRING | NULLABLE |
| frommodel | STRING | NULLABLE |
| tomodel | STRING | NULLABLE |
| originalauthcode | STRING | NULLABLE |
| upgradedauthcode | STRING | NULLABLE |
| createddate | TIMESTAMP | NULLABLE |
| createdby | STRING | NULLABLE |
| lastmodified | DATETIME | NULLABLE |
| vmupgradestatus | STRING | NULLABLE |
| isdeleted | BOOLEAN | NULLABLE |
| record_ingestion_time | TIMESTAMP | NULLABLE |
| rno | INTEGER | NULLABLE |

