/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using OpenETaxBill.Channel.Library.Security.Issue;

namespace OpenETaxBill.Channel.Library.Security.Signature
{
    public class XSignature
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        
        /// <summary>
        /// 
        /// </summary>
        private XSignature()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        private static volatile XSignature m_instance = null;
        private static object m_syncRoot = new object();

        /// <summary></summary>
        public static XSignature SNG
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_instance == null)
                        {
                            m_instance = new XSignature();
                        }
                    }
                }

                return m_instance;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
        private const string TaxInvoiceXPath = "not(self::*[name() = 'TaxInvoice'] | ancestor-or-self::*[name() = 'ExchangedDocument'] | ancestor-or-self::ds:Signature)";

        private volatile NameValueCollection m_nameCollections = null;
        public NameValueCollection SignNameCollections
        {
            get
            {
                if (m_nameCollections == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_nameCollections == null)
                        {
                            m_nameCollections = new NameValueCollection();

                            m_nameCollections.Add("", "urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0");
                            m_nameCollections.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
                            m_nameCollections.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        }
                    }
                }

                return m_nameCollections;
            }
        }

        private volatile XmlNamespaceManager m_signNSpaces = null;
        public XmlNamespaceManager SignNamespaces
        {
            get
            {
                if (m_signNSpaces == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_signNSpaces == null)
                        {
                            m_signNSpaces = new XmlNamespaceManager(new NameTable());

                            foreach (string _key in this.SignNameCollections.AllKeys)
                                m_signNSpaces.AddNamespace(_key, this.SignNameCollections[_key]);
                        }
                    }
                }

                return m_signNSpaces;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // Step 2 (3.정규화 Transform 결과 -> 4.XPath Transform 결과)
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Signature 대상이 되는 문서를 XPath 필터링 한다.
        /// </summary>
        /// <param name="p_canonicalXml">정규화된 원본 XML 문서</param>
        /// <returns>필터링 처리 된 Stream 객체</returns>
        public MemoryStream GetXPathTransformStream(MemoryStream p_canonicalXml)
        {
            MemoryStream _result = new MemoryStream();
            {
                //-----------------------------------------------------------------------------------------------------------------
                //
                //-----------------------------------------------------------------------------------------------------------------
                XmlTextWriter _xtw = new XmlTextWriter(_result, new UTF8Encoding());

                XPathDocument _xpd = new XPathDocument(XmlReader.Create(p_canonicalXml), XmlSpace.Preserve);
                XPathNavigator _xpn = _xpd.CreateNavigator();
                _xpn.MoveToRoot();

                XPathExpression _xpe = XPathExpression.Compile(XSignature.TaxInvoiceXPath);
                {
                    XmlNamespaceManager _xmlmgr = new XmlNamespaceManager(_xpn.NameTable);
                    foreach (string _key in this.SignNameCollections.AllKeys)
                        _xmlmgr.AddNamespace(_key, this.SignNameCollections[_key]);

                    _xpe.SetContext(_xmlmgr);
                }

                //-----------------------------------------------------------------------------------------------------------------
                // NameSpaces
                //-----------------------------------------------------------------------------------------------------------------
                bool _isnext = _xpn.MoveToFirstChild();

                IDictionary<string, string> _nameSpaces = _xpn.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
                {
                    var _nameColl = from _name in _nameSpaces.Keys
                                    orderby _nameSpaces[_name] descending
                                    select _name;

                    foreach (string _name in _nameColl)
                    {
                        string _nameSpace = " xmlns";
                        if (String.IsNullOrEmpty(_name) == false)
                            _nameSpace += ":" + _name;
                        _nameSpace += "=\"" + _nameSpaces[_name] + "\"";

                        _xtw.WriteString(_nameSpace);
                    }
                }

                //-----------------------------------------------------------------------------------------------------------------
                // <TaxInvoiceDocument>, <TaxInvoiceTradeSettlement>, <TaxInvoiceTradeLineItem>
                //-----------------------------------------------------------------------------------------------------------------
                var _isSkip = false;

                _isnext = _xpn.MoveToFirstChild();
                while (_isnext == true)
                {
                    string _outxml = _xpn.OuterXml.Replace("\r\n", "\n");

                    if (_xpn.NodeType == XPathNodeType.Whitespace)
                    {
                        if (_isSkip == false)
                            _xtw.WriteString(_outxml);
                    }
                    else if (_xpn.NodeType == XPathNodeType.Element)
                    {
                        if ((bool)_xpn.Evaluate(_xpe) == true)
                        {
                            _xtw.WriteRaw(_outxml.Replace(" xmlns=\"" + _nameSpaces[""] + "\"", ""));
                            //_isSkip = false;
                        }
                        else
                        {
                            // _isSkip = true;
                        }
                    }

                    _isnext = _xpn.MoveToNext();
                }

                _xtw.Flush();
            }

            return _result;
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // Step 3
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 세금계산서 문서를 digest하여 SignedInfo의 DigestValue를 채운 후 
        /// SignedInfo 전체를 전자서명 한다.
        /// </summary>
        /// <param name="p_targetXml">다이제스트 처리 할 전자세금계산서</param>
        /// <param name="p_x509cert">공인인증서</param>
        /// <returns>전자서명된 처리결과</returns>
        public XmlElement GetSignedXmlElement(Stream p_targetXml, X509Certificate2 p_x509cert
            //#if DEBUG
            //            , string p_outpath
            //#endif
            )
        {
            XmlDocument _signedXml = new XmlDocument();
            _signedXml.PreserveWhitespace = true;

            AsymmetricSignatureFormatter _signer = new RSAPKCS1SignatureFormatter(p_x509cert.PrivateKey);
            HashAlgorithm _digester;
            string _signMethod, _digMethod;

            if (p_x509cert.PrivateKey.KeySize == 2048)
            {
                _signer.SetHashAlgorithm("SHA256");
                _digester = new SHA256CryptoServiceProvider();

                _signMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                _digMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
            }
            else
            {
                _signer.SetHashAlgorithm("SHA1");
                _digester = new SHA1CryptoServiceProvider();

                _signMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                _digMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
            }

            byte[] _digestValue = _digester.ComputeHash(p_targetXml);

            string _signedInfoXml = ""
                    + "<ds:SignedInfo xmlns=\"urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" + "\n"
                    + "<ds:CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"></ds:CanonicalizationMethod>" + "\n"
                    + "<ds:SignatureMethod Algorithm=\"" + _signMethod + "\"></ds:SignatureMethod>" + "\n"
                    + "<ds:Reference URI=\"\">" + "\n"
                    + "<ds:Transforms>" + "\n"
                    + "<ds:Transform Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"></ds:Transform>" + "\n"
                    + "<ds:Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xpath-19991116\">" + "\n"
                    + "<ds:XPath>" + XSignature.TaxInvoiceXPath + "</ds:XPath>" + "\n"
                    + "</ds:Transform>" + "\n"
                    + "</ds:Transforms>" + "\n"
                    + "<ds:DigestMethod Algorithm=\"" + _digMethod + "\"></ds:DigestMethod>" + "\n"
                    + "<ds:DigestValue>" + Convertor.SNG.FormatEncodedString(_digestValue) + "</ds:DigestValue>" + "\n"
                    + "</ds:Reference>" + "\n"
                    + "</ds:SignedInfo>";

            //#if DEBUG
            //                var _savefile = Path.Combine(p_outpath, "5.txt");
            //                {
            //                    File.WriteAllText(_savefile, _signedInfoXml, Encoding.UTF8);
            //                }
            //#endif 

            byte[] _signatureHash = _digester.ComputeHash(Encoding.UTF8.GetBytes(_signedInfoXml));
            byte[] _signatureSign = _signer.CreateSignature(_signatureHash);

            byte[] _x509data = p_x509cert.GetRawCertData();

            _signedInfoXml = _signedInfoXml.Replace(
                "<ds:SignedInfo xmlns=\"urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">",
                "<ds:SignedInfo>"
                );

            string _signatureXml = ""
                    + "<ds:Signature xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\">" + "\n"
                    + _signedInfoXml + "\n"
                    + "<ds:SignatureValue>" + "\n"
                    + Convertor.SNG.FormatEncodedString(_signatureSign) + "\n"
                    + "</ds:SignatureValue>" + "\n"
                    + "<ds:KeyInfo>" + "\n"
                    + "<ds:X509Data>" + "\n"
                    + "<ds:X509Certificate>" + "\n"
                    + Convertor.SNG.FormatEncodedString(_x509data) + "\n"
                    + "</ds:X509Certificate>" + "\n"
                    + "</ds:X509Data>" + "\n"
                    + "</ds:KeyInfo>" + "\n"
                    + "</ds:Signature>";

            _signedXml.LoadXml(_signatureXml);

            return _signedXml.DocumentElement;
        }

        /// <summary>
        /// 전자세금계산서 발행을 위한 전자서명 후 결과를 리턴한다.
        /// </summary>
        /// <param name="p_istream"></param>
        /// <param name="p_x509cert"></param>
        /// <returns></returns>
        public MemoryStream GetSignedXmlStream(Stream p_istream, X509Certificate2 p_x509cert
//#if DEBUG
//            , string p_outpath
//#endif
            )
        {
            //---------------------------------------------------------------------------------------------------------------------//
            // Step 1
            //---------------------------------------------------------------------------------------------------------------------//
            MemoryStream _canonicalStream = Convertor.SNG.TransformToStream(p_istream);
            _canonicalStream = Convertor.SNG.CanonicalizationToStream(_canonicalStream, "\t");

//#if DEBUG
//            var _savefile = Path.Combine(p_outpath, "3.txt");
//            {
//                _canonicalStream.Seek(0, SeekOrigin.Begin);
//                File.WriteAllBytes(_savefile, _canonicalStream.ToArray());
//            }
//#endif

            //---------------------------------------------------------------------------------------------------------------------//
            // Step 2
            //---------------------------------------------------------------------------------------------------------------------//
            _canonicalStream.Seek(0, SeekOrigin.Begin);
            MemoryStream _transformStream = this.GetXPathTransformStream(_canonicalStream);

//#if DEBUG
//            _savefile = Path.Combine(p_outpath, "4.txt");
//            {
//                _transformStream.Seek(0, SeekOrigin.Begin);
//                File.WriteAllBytes(_savefile, _transformStream.ToArray());
//            }
//#endif

            //---------------------------------------------------------------------------------------------------------------------//
            // Step 3
            //---------------------------------------------------------------------------------------------------------------------//
            _transformStream.Seek(0, SeekOrigin.Begin);
            XmlElement _signatureNode = this.GetSignedXmlElement(_transformStream, p_x509cert
//#if DEBUG
//                , p_outpath
//#endif
                );

            //---------------------------------------------------------------------------------------------------------------------//
            // Step 4
            //---------------------------------------------------------------------------------------------------------------------//
            _canonicalStream.Seek(0, SeekOrigin.Begin);

            string _outerXml = (new StreamReader(_canonicalStream)).ReadToEnd();
            _outerXml = _outerXml.Replace("<TaxInvoiceDocument>", _signatureNode.OuterXml + "<TaxInvoiceDocument>");

            return new MemoryStream(Encoding.UTF8.GetBytes(_outerXml));
        }
        
        /// <summary>
        /// 전자세금계산서 발행을 위한 전자서명 후 결과를 리턴한다.
        /// </summary>
        /// <param name="p_srcfile"></param>
        /// <param name="p_x509cert"></param>
        /// <returns></returns>
        public MemoryStream GetSignedXmlStream(string p_srcfile, X509Certificate2 p_x509cert
//#if DEBUG
//            , string p_outpath
//#endif            
            )
        {
            FileStream _fs = new FileStream(p_srcfile, FileMode.Open, FileAccess.Read);
            return this.GetSignedXmlStream(_fs, p_x509cert
//#if DEBUG
//            , p_outpath
//#endif
            );
        }
        
        //-------------------------------------------------------------------------------------------------------------------------//
        //
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}