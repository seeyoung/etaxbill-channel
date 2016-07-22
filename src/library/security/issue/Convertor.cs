﻿/*
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
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace OpenETaxBill.Channel.Library.Security.Issue
{
    public class Convertor
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        
        /// <summary>
        /// 
        /// </summary>
        private Convertor()
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<Convertor> m_convertor = new Lazy<Convertor>(() =>
        {
            return new Convertor();
        });

        /// <summary></summary>
        public static Convertor SNG
        {
            get
            {
                return m_convertor.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        private char[] digitChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private char[] hexaChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public string ToHexString(byte[] bytes)
        {
            char[] _chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int _byte = bytes[i];
                _chars[i * 2] = hexaChar[_byte >> 4];
                _chars[i * 2 + 1] = hexaChar[_byte & 0xF];
            }

            return new string(_chars);
        }

        /// <summary>
        /// base64 인코딩 규칙 중 한줄의 길이는 76문자를 넘기지 않아야 한다.
        /// </summary>
        /// <param name="p_digestValue">인코딩 할 byte array</param>
        /// <returns>포맷에 맞게 인코딩 된 base64 문자열</returns>
        public string FormatEncodedString(byte[] p_digestValue)
        {
            const int MAX_CHAR_LEN = 76;
            int index = 0;
            string s = Convert.ToBase64String(p_digestValue);

            var sb = new StringBuilder();
            while ((index + MAX_CHAR_LEN) < s.Length)
            {
                sb.AppendFormat("{0}\n", s.Substring(index, MAX_CHAR_LEN));
                index += MAX_CHAR_LEN;
            }

            sb.AppendFormat("{0}", s.Substring(index, s.Length - index));
            return sb.ToString();
        }

        public string GetRandHexString(int p_length)
        {
            if (p_length > 32)
            {
                var _randString = new StringBuilder();

                var _random = new Random(DateTime.Now.Millisecond);
                for (int i = 0; i < p_length; i++)
                    _randString.Append(this.hexaChar[_random.Next(16)]);

                return _randString.ToString();
            }
            else
            {
                return Guid.NewGuid().ToString("N").Substring(0, p_length);
            }
        }

        public string GetRandNumString(int p_length)
        {
            var _randString = new StringBuilder();

            var _random = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < p_length; i++)
                _randString.Append(this.digitChar[_random.Next(10)]);

            return _randString.ToString();
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        public MemoryStream EnvelopedTransformToStream(Stream p_sourceXml)
        {
            MemoryStream _result = new MemoryStream();

            XmlDsigEnvelopedSignatureTransform _envTransform = new XmlDsigEnvelopedSignatureTransform(true);
            {
                _envTransform.Algorithm = SignedXml.XmlDsigEnvelopedSignatureTransformUrl;

                Type[] _validInTypes = _envTransform.InputTypes;

                for (int i = 0; i < _validInTypes.Length; i++)
                {
                    if (_validInTypes[i] == typeof(System.IO.Stream))
                    {
                        _envTransform.LoadInput(p_sourceXml);
                        break;
                    }
                }

                Type[] _validOutTypes = _envTransform.OutputTypes;

                for (int i = 0; i < _validOutTypes.Length; i++)
                {
                    if (_validOutTypes[i] == typeof(XmlDocument))
                    {
                        Type streamType = typeof(XmlDocument);
                        
                        XmlDocument _doc = (XmlDocument)_envTransform.GetOutput(streamType);
                        _result = new MemoryStream(Encoding.UTF8.GetBytes(_doc.OuterXml));

                        break;
                    }
                }
            }

            return _result;
        }

        public XmlElement EnvelopedTransformToStream(XmlElement p_sourceXml)
        {
            MemoryStream _canStream = new MemoryStream(Encoding.UTF8.GetBytes(p_sourceXml.OuterXml));
            _canStream = this.EnvelopedTransformToStream(_canStream);

            _canStream.Seek(0, SeekOrigin.Begin);

            XmlDocument _candoc = new XmlDocument(p_sourceXml.OwnerDocument.NameTable);
            _candoc.PreserveWhitespace = true;
            _candoc.Load(_canStream);

            return _candoc.DocumentElement;
        }

        public MemoryStream Dsig14NTransformToStream(Stream p_sourceXml)
        {
            MemoryStream _result = new MemoryStream();

            XmlDsigC14NTransform _14nTransform = new XmlDsigC14NTransform(true);
            {
                _14nTransform.Algorithm = SignedXml.XmlDsigExcC14NTransformUrl;

                Type[] _validInTypes = _14nTransform.InputTypes;

                for (int i = 0; i < _validInTypes.Length; i++)
                {
                    if (_validInTypes[i] == typeof(System.IO.Stream))
                    {
                        _14nTransform.LoadInput(p_sourceXml);
                        break;
                    }
                }

                Type[] _validOutTypes = _14nTransform.OutputTypes;

                for (int i = 0; i < _validOutTypes.Length; i++)
                {
                    if (_validOutTypes[i] == typeof(System.IO.Stream))
                    {
                        Type streamType = typeof(System.IO.Stream);
                        _result = (MemoryStream)_14nTransform.GetOutput(streamType);
                        break;
                    }
                }
            }

            return _result;
        }

        public MemoryStream TransformToStream(Stream p_sourceXml)
        {
            return this.Dsig14NTransformToStream(p_sourceXml);
        }

        public XmlElement TransformToElement(XmlElement p_sourceXml)
        {
            MemoryStream _canStream = new MemoryStream(Encoding.UTF8.GetBytes(p_sourceXml.OuterXml));
            _canStream = this.Dsig14NTransformToStream(_canStream);

            _canStream.Seek(0, SeekOrigin.Begin);

            XmlDocument _candoc = new XmlDocument(p_sourceXml.OwnerDocument.NameTable);
            _candoc.PreserveWhitespace = true;
            _candoc.Load(_canStream);

            return _candoc.DocumentElement;
        }

        public XmlDocument TransformToDocument(XmlDocument p_sourceXml)
        {
            return this.TransformToElement(p_sourceXml.DocumentElement).OwnerDocument;
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// XML Document의 정규화 과정을 수행한다.
        /// 정규화 알고리즘 : http://www.w3.org/TR/2001/REC-xml-c14n-20010315
        /// </summary>
        /// <param name="p_sourceXml">정규화 과정을 거칠 XML Document</param>
        /// <param name="p_indent">요소의 들여쓰기 여부</param>
        /// <param name="p_indentChars">들여쓰기에 사용 할 문자열</param>
        /// <returns>정규화 되어진 스트림</returns>
        public MemoryStream Canonicalization(Stream p_sourceXml, string p_IndentChars)
        {
            MemoryStream _result = new MemoryStream();

            var _xmldoc = new XmlDocument();
            _xmldoc.PreserveWhitespace = false;
            _xmldoc.Load(p_sourceXml);

            XmlWriterSettings _settings = new XmlWriterSettings();
            _settings.Indent = true;
            _settings.IndentChars = p_IndentChars;
            _settings.NewLineChars = "\n";
            _settings.OmitXmlDeclaration = false;
            _settings.Encoding = Encoding.UTF8;

            using (XmlWriter _xw = XmlWriter.Create(_result, _settings))
            {
                _xmldoc.WriteTo(_xw);
                _xw.Flush();
            }

            _result.Seek(0, SeekOrigin.Begin);

            return _result;
        }
        
        public MemoryStream CanonicalizationToStream(Stream p_sourceXml, string p_IndentChars)
        {
            return this.Canonicalization(p_sourceXml, p_IndentChars);
        }

        public MemoryStream CanonicalizationToStream(Stream p_sourceXml)
        {
            return this.Canonicalization(p_sourceXml, "  ");
        }

        public XmlElement CanonicalizationToElement(XmlElement p_sourceXml, string p_IndentChars)
        {
            MemoryStream _canStream = this.CanonicalizationToStream(
                            new MemoryStream(Encoding.UTF8.GetBytes(p_sourceXml.OuterXml)),
                            p_IndentChars
                            );

            XmlDocument _candoc = new XmlDocument(p_sourceXml.OwnerDocument.NameTable);
            _candoc.PreserveWhitespace = true;
            _candoc.Load(_canStream);

            return _candoc.DocumentElement;
        }

        public XmlElement CanonicalizationToElement(XmlElement p_sourceXml)
        {
            return this.CanonicalizationToElement(p_sourceXml, "  ");
        }

        public XmlDocument CanonicalizationToDocument(XmlDocument p_sourceXml, string p_IndentChars)
        {
            return this.CanonicalizationToElement(p_sourceXml.DocumentElement, p_IndentChars).OwnerDocument;
        }

        public XmlDocument CanonicalizationToDocument(XmlDocument p_sourceXml)
        {
            return this.CanonicalizationToDocument(p_sourceXml, "  ");
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}