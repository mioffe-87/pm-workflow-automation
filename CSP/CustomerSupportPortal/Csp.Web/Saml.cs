using Csp.Api.HelperClasses;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Saml
{
	public partial class Response
	{
		protected XmlDocument _xmlDoc = new XmlDocument();
		protected readonly X509Certificate2 _certificate;
		protected XmlNamespaceManager _xmlNameSpaceManager; 

		public string Xml { get { return _xmlDoc.OuterXml; } }

	    public Response(string certThumbprint, string responseString)
        {
            _certificate = CertificateManager.GetCertificateByCnName(certThumbprint);
            LoadXml(responseString);
		}

		public void LoadXml(string xml)
		{
            if (xml != null)
            {
                UTF8Encoding enc = new UTF8Encoding();
                xml = enc.GetString(Convert.FromBase64String(xml));
                string formattedXml = Regex.Unescape(xml);

                _xmlDoc = new XmlDocument();
                _xmlDoc.PreserveWhitespace = true;
                _xmlDoc.XmlResolver = null;
                _xmlDoc.LoadXml(formattedXml);
                _xmlNameSpaceManager = GetNamespaceManager(); 
            }
        }

		public bool IsValid()
        {
            if (_xmlDoc == null)
            {
                return false;

            }
			XmlNodeList nodeList = _xmlDoc.SelectNodes("//ds:Signature", _xmlNameSpaceManager);

			SignedXml signedXml = new SignedXml(_xmlDoc);

			if (nodeList.Count == 0) return false;

			signedXml.LoadXml((XmlElement)nodeList[0]);
			return ValidateSignatureReference(signedXml) && signedXml.CheckSignature(_certificate, true) && !IsExpired();
		}

		private bool ValidateSignatureReference(SignedXml signedXml)
		{
			if (signedXml.SignedInfo.References.Count != 1) //no ref at all
				return false;

			var reference = (Reference)signedXml.SignedInfo.References[0];
			var id = reference.Uri.Substring(1);

			var idElement = signedXml.GetIdElement(_xmlDoc, id);

			if (idElement == _xmlDoc.DocumentElement)
				return true;
			else 
			{
				var assertionNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion", _xmlNameSpaceManager) as XmlElement;
				if (assertionNode != idElement)
					return false;
			}

			return true;
		}

		private bool IsExpired()
		{
			DateTime expirationDate = DateTime.MaxValue;
			XmlNode node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", _xmlNameSpaceManager);
			if (node != null && node.Attributes["NotOnOrAfter"] != null)
			{
				DateTime.TryParse(node.Attributes["NotOnOrAfter"].Value, out expirationDate);
			}
			return DateTime.UtcNow > expirationDate.ToUniversalTime();
		}

		public string GetNameID()
		{
			XmlNode node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:NameID", _xmlNameSpaceManager);
			return node.InnerText;
		}

		public virtual string GetUpn()
		{
			return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
		}

		public virtual string GetEmail()
        {
            return GetCustomAttribute("User.email")
                   ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
        }

		public virtual string GetFirstName()
        {
            return GetCustomAttribute("firstName")
                   ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
                   ?? GetCustomAttribute("User.FirstName");
        }

		public virtual string GetLastName()
		{
			return GetCustomAttribute("lastName")
				?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
				?? GetCustomAttribute("User.LastName");
		}

		public virtual string GetDepartment()
		{
			return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/department")
				?? GetCustomAttribute("department");
		}

		public virtual string GetPhone()
		{
			return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone")
				?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/telephonenumber");
		}

		public virtual string GetCompany()
		{
			return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/companyname")
				?? GetCustomAttribute("organization")
				?? GetCustomAttribute("User.CompanyName");
		}

		public virtual string GetLocation()
		{
			return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/location")
				?? GetCustomAttribute("physicalDeliveryOfficeName");
		}

		public string GetCustomAttribute(string attr)
		{
			XmlNode node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:AttributeStatement/saml:Attribute[@Name='" + attr + "']/saml:AttributeValue", _xmlNameSpaceManager);
			return node == null ? null : node.InnerText;
		}

        public virtual string GetTitle()
        {
            return GetCustomAttribute("User.title")
                   ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/title");
        }

        public virtual string GetTenantId() {

            return GetCustomAttribute("azureTenantId")
                 ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/azureTenantId")
                 ?? GetCustomAttribute("User.azureTenantId");
        }

        private XmlNamespaceManager GetNamespaceManager()
		{
			XmlNamespaceManager manager = new XmlNamespaceManager(_xmlDoc.NameTable);
			manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
			manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
			manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

			return manager;
		}
	}

	public class AuthRequest
	{
		private readonly string _id;
		private readonly string _issue_instant;

		private readonly string _issuer;

		public enum AuthRequestFormat
		{
			Base64 = 1
		}

		public AuthRequest(string issuer, string assertionConsumerServiceUrl)
		{
			_id = "_" + Guid.NewGuid().ToString();
			_issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
			_issuer = issuer;
			
		}

		public string GetRequest(AuthRequestFormat format)
		{
			using (StringWriter sw = new StringWriter())
			{
				XmlWriterSettings xws = new XmlWriterSettings();
				xws.OmitXmlDeclaration = true;

				using (XmlWriter xw = XmlWriter.Create(sw, xws))
				{
					xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
					xw.WriteAttributeString("ID", _id);
					xw.WriteAttributeString("Version", "2.0");
					xw.WriteAttributeString("IssueInstant", _issue_instant);
					xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");

					xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
					xw.WriteString(_issuer);
					xw.WriteEndElement();

					xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
					xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
					xw.WriteAttributeString("AllowCreate", "true");
					xw.WriteEndElement();
					xw.WriteEndElement();
				}

				if (format == AuthRequestFormat.Base64)
				{
					var memoryStream = new MemoryStream();
					var writer = new StreamWriter(new DeflateStream(memoryStream, CompressionMode.Compress, true), new UTF8Encoding(false));
					writer.Write(sw.ToString());
					writer.Close();
					string result = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, Base64FormattingOptions.None);
					return result;
				}

				return null;
			}
		}

		public string GetRedirectUrl(string samlEndpoint, string relayState = null)
		{
			var queryStringSeparator = samlEndpoint.Contains("?") ? "&" : "?";

			var url = samlEndpoint + queryStringSeparator + "SAMLRequest=" + Uri.EscapeDataString(GetRequest(AuthRequestFormat.Base64));

			if (!string.IsNullOrEmpty(relayState)) 
			{
				url += "&RelayState=" + Uri.EscapeDataString(relayState);
			}

			return url;
		}
	}
}