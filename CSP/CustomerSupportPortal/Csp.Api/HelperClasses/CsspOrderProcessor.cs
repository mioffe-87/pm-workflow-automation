using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Csp.Api.Model;
using Csp.Api.SAPService;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.Net;
using System.ServiceModel;
using System.Web.Services.Protocols;
using System.Web;
using System.Xml.Linq;

namespace Csp.Api.HelperClasses
{
    public class CsspOrderProcessor
    {
        private static DateTime CsspEndDate = DateTime.Parse("12/31/2036");

        public static bool PushToSAP(CsspOrderSapInput sapInput, out string inputXML, out string sapReturnMessage, out string errorMessage)
        {
            errorMessage = string.Empty;
            sapReturnMessage = string.Empty;
            inputXML = string.Empty;


            try
            {
                //Create instance of SAP Service  
                var saprequest = GetSAPServiceRequest();
                //var req = GetSAPSWebRequest();
                var order = GetOrderInfo(sapInput);
                if (order != null)
                {

                    #region Get XML

                    XmlDocument xmlDoc = new XmlDocument();   //Represents an XML document, 
                    // Initializes a new instance of the XmlDocument class.

                    // Comments By LK On 12/04/2018
                    // Fix to prevent XML External Entity attacks
                    xmlDoc.XmlResolver = null;

                    // XmlSerializer xmlSerializer = new XmlSerializer(order.GetType());
                    // Creates a stream whose backing store is memory. 
                   
                    XmlSerializer xmlSerializer = new XmlSerializer(order.GetType());                  
                    using (MemoryStream xmlStream = new MemoryStream())
                    {
                        xmlSerializer.Serialize(xmlStream, order);
                        xmlStream.Position = 0;
                        //Loads the XML document from the specified string.
                        xmlDoc.Load(xmlStream);
                        inputXML = xmlDoc.InnerXml;
                        Debug.WriteLine(inputXML);
                    }

                    #endregion                  
                     var response = saprequest.SI_QuoteCreate_OB(order);
                     var isSuccess = string.IsNullOrEmpty(response.Response) == false && response.Response.ToUpper() == "S";
                     sapReturnMessage = response.ErrorMsg;
                    return isSuccess;
                }
            }
            catch (Exception oex)
            {
                errorMessage = oex.ToString();
                return false;
            }

            return false;
        }

        private static string GetCSSPMonths()
        {
            return (((CsspEndDate.Year - DateTime.Now.Year) * 12) + CsspEndDate.Month - DateTime.Now.Month).ToString();
        }

        private static string PadZeroIfNeeded(int val)
        {
            if (val.ToString().Length == 1)
            {
                return "0" + val.ToString();
            }
            return val.ToString();
        }

        private static DT_QuoteCreate GetOrderInfo(CsspOrderSapInput sapInput)
        {
            string salesEmpId = ConfigurationManager.AppSettings["CsspSalesEmpId"];
            string salesOrg = CsspConstants.SALES_ORG;
            bool hasBacklineSupport = false;
            #region SAP Order Object Construction

            var orderInfo = new DT_QuoteCreate();
            var orderHeader = new DT_QuoteCreateHeader();
            var orderItems = new List<DT_QuoteCreateItem>();

            #region Populate Order Header
            var todaysDate = System.DateTime.Now.Year + "-" + PadZeroIfNeeded(System.DateTime.Now.Month) + "-" + PadZeroIfNeeded(System.DateTime.Now.Day);


            orderHeader.DOC_DATE = todaysDate;
            orderHeader.SALES_DOC_TYPE = CsspConstants.SALES_DOC_TYPE;
            orderHeader.SALES_ORG = sapInput.SoldToSalesOrg.HasValue ? sapInput.SoldToSalesOrg.Value.ToString() : salesOrg.ToString();
            orderHeader.PO_NO = sapInput.CartNumber;
            orderHeader.PO_DATE = todaysDate;
            orderHeader.SOURCE = CsspConstants.SOURCE;
            #region Order Header Text Collection
            var headerTexts = new List<DT_QuoteCreateHeaderTEXT>()
                {
                    new DT_QuoteCreateHeaderTEXT()
                    {
                        TEXT_QUALIFIER = CsspConstants.ORDER_HEADER_TEXT_QUALIFIER1,
                        TEXT_VALUE = sapInput.SupportAccountId.ToString()
                    },
                    new DT_QuoteCreateHeaderTEXT()
                    {
                        TEXT_QUALIFIER = CsspConstants.ORDER_HEADER_TEXT_QUALIFIER2,
                        TEXT_VALUE = sapInput.OrderedByUser
                    }
                };
            #endregion
            orderHeader.HEADER_TEXT = headerTexts.ToArray();

            #endregion

            #region Populate Order Line Item

            int idxItems = 0;
            foreach (var item in sapInput.OrderItems)
            {
                if (item.Contains("-BKLN-"))
                {
                    hasBacklineSupport = true;
                }
                idxItems = idxItems + 1;
                orderItems.Add(new DT_QuoteCreateItem()
                {
                    ITEM_NO = idxItems.ToString(),
                    MATERIAL = item,
                    QTY = "1",
                    ADDTL_DATA = new DT_QuoteCreateItemADDTL_DATA[]
                    {
                        new DT_QuoteCreateItemADDTL_DATA()
                        {
                            QUALIFIER = "ZTDM",
                            VALUE = GetCSSPMonths()
                        }
                    }
                });
            }

            #endregion

            #region Order Partners
            var orderPartners = new List<DT_QuoteCreatePartner>()
            {
                 new DT_QuoteCreatePartner()
                    {
                        CUST_ID =  string.IsNullOrEmpty(sapInput.SalesRepAccountId) ? salesEmpId : sapInput.SalesRepAccountId,
                        PATNER_FUNCTION = CsspConstants.PARTNER_FUNC_Z3 //  Sales Employee
                    },
                new DT_QuoteCreatePartner()
                    {
                        CUST_ID = sapInput.DistiSfdcAccountId,
                        PATNER_FUNCTION = CsspConstants.PARTNER_FUNC_Z4 //  DISTI IS MANDATORY. IF NO DISTI THEN SEND END USER
                    },
                new DT_QuoteCreatePartner()
                    {
                        CUST_ID = sapInput.DistiSfdcAccountId,
                        PATNER_FUNCTION = CsspConstants.PARTNER_FUNC_AG // Rag: Remember the evening when Thuy and Senthil drove you crazy!! -> SOLD TO (IF DISTI THEN DISTI IS THE SOLD TO ELSE ITS THE CSSP)
                    }
            };

            orderPartners.Add(new DT_QuoteCreatePartner()
            {
                CUST_ID = sapInput.SpSfdcAccountId,
                PATNER_FUNCTION = CsspConstants.PARTNER_FUNC_SH // SHIP TO
            });


            orderPartners.Add(new DT_QuoteCreatePartner()
            {
                CUST_ID = sapInput.SpSfdcAccountId,
                PATNER_FUNCTION = CsspConstants.PARTNER_FUNC_Z1 // END USER
            });
            if (hasBacklineSupport) // Fix Error => E - Order has no BKLN Material, Please Remove ASC(Z6) Partner
            {
                orderPartners.Add(new DT_QuoteCreatePartner()
                {
                    CUST_ID = sapInput.AscSfdcAccountId,
                    PATNER_FUNCTION = CsspConstants.PARTNER_FUNC_Z6 // ASC
                });
            }

            #endregion

            orderInfo.Header = orderHeader;
            orderInfo.Item = orderItems.ToArray();
            orderInfo.Partner = orderPartners.ToArray();

            #endregion

            return orderInfo;
        }

        private static SI_QuoteCreate_OBService GetSAPServiceRequest()
        {
            try
            {
                var partialshipmentThumbprint = ConfigurationManager.AppSettings["PartialShipmentThumbprint"];
                var partialshipmentEndPoint = ConfigurationManager.AppSettings["PartialShipmentApiUrl"];
                var sapRequest = new SI_QuoteCreate_OBService();
                if (string.IsNullOrEmpty(partialshipmentThumbprint))
                {
                    return sapRequest;
                }

                if (!string.IsNullOrEmpty(partialshipmentThumbprint))
                {
                    var clientCertificate = CertificateManager.GetCertificateByCnName(partialshipmentThumbprint);

                    if (clientCertificate == null)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
                    }
                    var url = string.Format("{0}/QuoteCreate", partialshipmentEndPoint);
                    sapRequest.Url = url;
                    sapRequest.ClientCertificates.Add(clientCertificate);
                    sapRequest.SoapVersion = SoapProtocolVersion.Soap11;
                }
                return sapRequest;
            }


            catch (Exception oex)
            {
                var ex = oex;
                throw;
            }
        }

        private static HttpWebRequest GetSAPSWebRequest()
        {
            try
            {
                string partialshipmentEndPoint = ConfigurationManager.AppSettings["PartialShipmentApiUrl"];
                var partialshipmentThumbprint = ConfigurationManager.AppSettings["PartialShipmentThumbprint"];
                var url = string.Format("{0}/QuoteCreate", partialshipmentEndPoint);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);               
                req.Method = "POST";
                req.ContentType = "text/xml;charset=utf-8";               

                if (!string.IsNullOrEmpty(partialshipmentThumbprint))
                {
                    var clientCertificate = CertificateManager.GetCertificate(partialshipmentThumbprint);
                    req.ClientCertificates.Add(clientCertificate);
                }

                return req;
            }
            catch (Exception oex)
            {
                var ex = oex;
                throw;
            }
        }
    }
}
