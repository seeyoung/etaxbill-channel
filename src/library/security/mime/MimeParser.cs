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
using System.Linq;
using System.Text;
using System.IO;

using OpenETaxBill.Channel.Library.Utility;

namespace OpenETaxBill.Channel.Library.Security.Mime
{
    public class MimeParser
    {
        public byte[] CarriageReturnLineFeed = new byte[] { (byte)'\r', (byte)'\n' };
        public byte[] EndOfPartsDelimeter = new byte[] { (byte)'-', (byte)'-' };

        public Encoding ParserEncoding
        {
            get;
            set;
        }
        public Encoding SoapXmlEncoding
        {
            get;
            set;
        }

        public MimeParser()
        {
            ParserEncoding = Encoding.UTF8;
            SoapXmlEncoding = Encoding.UTF8;
        }

        public MimeParser(Encoding encoding)
            : this()
        {
            ParserEncoding = encoding;
        }

        public MimeParser(Encoding encoding, Encoding soapEncoding)
            : this(encoding)
        {
            SoapXmlEncoding = soapEncoding;
        }

        public byte[] SerializeMimeContent(MimeContent p_content)
        {
            MemoryStream _contentStream = new MemoryStream();
            this.SerializeMimeContent(p_content, _contentStream);
            return _contentStream.ToArray();
        }

        public void SerializeMimeContent(MimeContent p_icontent, Stream p_ostream)
        {
            byte[] _writeHelper;

            //
            // Prepare some bytes written more than once
            //
            byte[] _boundaryBytes = ParserEncoding.GetBytes("--" + p_icontent.Boundary);

            //
            // Write every part into the stream
            //
            foreach (var item in p_icontent.Parts)
            {
                //
                // First of all write the boundary
                //
                p_ostream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
                p_ostream.Write(_boundaryBytes, 0, _boundaryBytes.Length);
                p_ostream.Write(CarriageReturnLineFeed, 0, 2);

                //
                // Write the content-type for the current element
                //
                var _builder = new StringBuilder();
                _builder.Append(String.Format("Content-Type: {0}", item.ContentType));
                if (!String.IsNullOrEmpty(item.CharSet))
                    _builder.Append(String.Format("; charset={0}", item.CharSet));
                _builder.Append(new char[] { '\r', '\n' });

                if (!String.IsNullOrEmpty(item.TransferEncoding))
                {
                    _builder.Append(String.Format("Content-Transfer-Encoding: {0}", item.TransferEncoding));
                    _builder.Append(new char[] { '\r', '\n' });
                }

                _builder.Append(String.Format("Content-ID: {0}", item.ContentId));

                _writeHelper = ParserEncoding.GetBytes(_builder.ToString());
                p_ostream.Write(_writeHelper, 0, _writeHelper.Length);

                p_ostream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
                p_ostream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);

                //
                // Write the actual content
                //
                p_ostream.Write(item.Content, 0, item.Content.Length);
            }

            //
            // Write one last content boundary
            //
            p_ostream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
            p_ostream.Write(_boundaryBytes, 0, _boundaryBytes.Length);
            p_ostream.Write(EndOfPartsDelimeter, 0, EndOfPartsDelimeter.Length);
            p_ostream.Write(CarriageReturnLineFeed, 0, CarriageReturnLineFeed.Length);
        }

        public MimeContent DeserializeMimeContent(string p_httpContentType, byte[] p_binaryContent)
        {
            //
            // First of all parse the http content type
            //
            string _mimeType = null, _mimeBoundary = null, _mimeStart = null;
            ParseHttpContentTypeHeader(p_httpContentType, ref _mimeType, ref _mimeBoundary, ref _mimeStart);

            //
            // Create the mime-content
            //
            MimeContent _content = new MimeContent()
            {
                Boundary = _mimeBoundary
            };

            // 
            // Start finding the parts in the mime message
            // Note: in MIME RFC a "--" represents the end of something
            //
            int _endBoundaryHelperNdx = 0;
            byte[] _mimeBoundaryBytes = ParserEncoding.GetBytes("--" + _mimeBoundary);
            for (int i = 0; i < p_binaryContent.Length; i++)
            {
                if (AreArrayPartsForTextEqual(_mimeBoundaryBytes, 0, p_binaryContent, i, _mimeBoundaryBytes.Length))
                {
                    _endBoundaryHelperNdx = i + _mimeBoundaryBytes.Length;
                    if ((_endBoundaryHelperNdx + 1) < p_binaryContent.Length)
                    {
                        // The end of the MIME-message is the boundary followed by "--"
                        if (p_binaryContent[_endBoundaryHelperNdx] == '-' && p_binaryContent[_endBoundaryHelperNdx + 1] == '-')
                        {
                            break;
                        }
                    }
                    else
                    {
                        throw new ProxyException("Invalid MIME content parsed, premature End-Of-File detected!");
                    }

                    // Start reading the mime part after the boundary
                    MimePart _part = ReadMimePart(p_binaryContent, ref i, _mimeBoundaryBytes);
                    if (_part != null)
                    {
                        _content.Parts.Add(_part);
                    }
                }
            }

            //
            // Finally return the ready-to-use object model
            //
            _content.SetAsStartPart(_mimeStart);
            return _content;
        }

        private void ParseHttpContentTypeHeader(string p_httpContentType, ref string p_mimeType, ref string p_mimeBoundary, ref string p_mimeStart)
        {
            string[] _contentTypeParsed = p_httpContentType.Split(new char[] { ';' });
            for (int i = 0; i < _contentTypeParsed.Length; i++)
            {
                string _contentTypePart = _contentTypeParsed[i].Trim();

                int _equalsNdx = _contentTypePart.IndexOf('=');
                if (_equalsNdx <= 0)
                    continue;

                string _key = _contentTypePart.Substring(0, _equalsNdx);
                string _value = _contentTypePart.Substring(_equalsNdx + 1);
                if (_value[0] == '\"')
                    _value = _value.Remove(0, 1);
                if (_value[_value.Length - 1] == '\"')
                    _value = _value.Remove(_value.Length - 1);

                switch (_key.ToLower())
                {
                    case "type":
                        p_mimeType = _value;
                        break;

                    case "start":
                        p_mimeStart = _value;
                        break;

                    case "boundary":
                        p_mimeBoundary = _value;
                        break;
                }
            }

            if ((p_mimeType == null) || (p_mimeStart == null) || (p_mimeBoundary == null))
            {
                throw new ProxyException("Invalid HTTP content header - please verify if type, start and boundary are available in the multipart/related content type header!");
            }
        }

        private MimePart ReadMimePart(byte[] p_binaryContent, ref int p_currentIndex, byte[] p_mimeBoundaryBytes)
        {
            byte[] _contentTypeKeyBytes = ParserEncoding.GetBytes("Content-Type:");
            byte[] _transferEncodingKeyBytes = ParserEncoding.GetBytes("Content-Transfer-Encoding:");
            byte[] _contentIdKeyBytes = ParserEncoding.GetBytes("Content-ID:");

            //
            // Find the appropriate content header indexes
            //
            int _contentTypeNdx = -1, _transferEncodingNdx = -1, _contentIdNdx = -1;
            int _contentTypeLen = -1, _transferEncodingLen = -1, _contentIdLen = -1;
            while (p_currentIndex < p_binaryContent.Length)
            {
                // Try compare for keys
                if (_contentTypeNdx < 0)
                {
                    if (AreArrayPartsForTextEqual(_contentTypeKeyBytes, 0, p_binaryContent, p_currentIndex, _contentTypeKeyBytes.Length) == true)
                    {
                        _contentTypeNdx = p_currentIndex;
                        _contentTypeLen = this.GetLengthToCRLF(p_binaryContent, _contentTypeNdx + _contentTypeKeyBytes.Length);
                    }
                }

                if (_transferEncodingNdx < 0)
                {
                    if (AreArrayPartsForTextEqual(_transferEncodingKeyBytes, 0, p_binaryContent, p_currentIndex, _transferEncodingKeyBytes.Length) == true)
                    {
                        _transferEncodingNdx = p_currentIndex;
                        _transferEncodingLen = this.GetLengthToCRLF(p_binaryContent, _transferEncodingNdx + _transferEncodingKeyBytes.Length);
                    }
                }

                if (_contentIdNdx < 0)
                {
                    if (AreArrayPartsForTextEqual(_contentIdKeyBytes, 0, p_binaryContent, p_currentIndex, _contentIdKeyBytes.Length) == true)
                    {
                        _contentIdNdx = p_currentIndex;
                        _contentIdLen = this.GetLengthToCRLF(p_binaryContent, _contentIdNdx + _contentIdKeyBytes.Length);
                    }
                }

                // All content headers found, last content header split by Carriage Return Line Feed
                // TODO: Check index out of bounds!
                if (p_binaryContent[p_currentIndex] == 13 && p_binaryContent[p_currentIndex + 1] == 10)
                {
                    if (p_binaryContent[p_currentIndex + 2] == 13 && p_binaryContent[p_currentIndex + 3] == 10)
                        break;
                }

                // Next array index
                p_currentIndex++;
            }

            // After the last content header, we have \r\n\r\n, always
            p_currentIndex += 4;

            //
            // If not all indices found, error
            //
            //if (!((_contentTypeNdx >= 0) && (_transferEncodingNdx >= 0) && (_contentIdNdx >= 0)))
            //{
            //    // A '0' at the end of the message indicates that the previous part was the last one
            //    if (p_binaryContent[p_currentIndex - 1] == 0)
            //        return null;
            //    else if (p_binaryContent[p_currentIndex - 2] == 13 && p_binaryContent[p_currentIndex - 1] == 10)
            //        return null;
            //    else
            //        throw new ProxyException("Invalid mime content passed into mime parser! Content-Type, Content-Transfer-Encoding or ContentId headers for mime part are missing!");
            //}

            // 
            // Convert the content header information into strings
            //
            string _contentType = "";
            string _charSet = "";

            if (_contentTypeNdx > 0)
            {
                _contentType = ParserEncoding.GetString(p_binaryContent, _contentTypeNdx + _contentTypeKeyBytes.Length, _contentTypeLen).Trim();

                if (_contentType.Contains(';'))
                {
                    int _contentTypeSplitIdx = _contentType.IndexOf(';');
                    int _equalsCharSetNdx = _contentType.IndexOf('=', _contentTypeSplitIdx + 1);

                    if (_equalsCharSetNdx < 0)
                        _charSet = _contentType.Substring(_contentTypeSplitIdx + 1).Trim();
                    else
                        _charSet = _contentType.Substring(_equalsCharSetNdx + 1).Trim();

                    _contentType = _contentType.Substring(0, _contentTypeSplitIdx).Trim();
                }
            }

            string _transferEncoding = "";
            if (_transferEncodingNdx > 0)
            {
                _transferEncoding = ParserEncoding.GetString(p_binaryContent, _transferEncodingNdx + _transferEncodingKeyBytes.Length, _transferEncodingLen).Trim();
            }

            string _contentId = "";
            if (_contentIdNdx > 0)
            {
                _contentId = ParserEncoding.GetString(p_binaryContent, _contentIdNdx + _contentIdKeyBytes.Length, _contentIdLen).Trim();
            }

            //
            // Current mime content starts now, therefore find the end
            //
            int _startContentIndex = p_currentIndex;
            int _endContentIndex = -1;
            while (p_currentIndex < p_binaryContent.Length)
            {
                if (AreArrayPartsForTextEqual(p_mimeBoundaryBytes, 0, p_binaryContent, p_currentIndex, p_mimeBoundaryBytes.Length))
                {
                    _endContentIndex = p_currentIndex - 1;
                    break;
                }

                p_currentIndex++;
            }
            if (_endContentIndex == -1)
                _endContentIndex = p_currentIndex - 1;

            // 
            // Tweak start- and end-indexes, cut all Carriage Return Line Feeds
            //
            while (true)
            {
                if ((p_binaryContent[_startContentIndex] == 13) && (p_binaryContent[_startContentIndex + 1] == 10))
                    _startContentIndex += 2;
                else
                    break;

                if (_startContentIndex > p_binaryContent.Length)
                    throw new ProxyException("Error in content, start index cannot go beyond overall content array!");
            }

            while (true)
            {
                if ((p_binaryContent[_endContentIndex - 1] == 13) && (p_binaryContent[_endContentIndex] == 10))
                    _endContentIndex -= 2;
                else
                    break;

                if (_endContentIndex < 0)
                    throw new ProxyException("Error in content, end content index cannot go beyond smallest index of content array!");
            }

            //
            // Now create a byte array for the current mime-part content
            //
            MimePart _mimePart = new MimePart()
            {
                ContentId = _contentId,
                TransferEncoding = _transferEncoding,
                ContentType = _contentType,
                CharSet = _charSet,
                Content = new byte[_endContentIndex - _startContentIndex + 1]
            };

            Array.Copy(p_binaryContent, _startContentIndex, _mimePart.Content, 0, _mimePart.Content.Length);

            // Go to the last sign before the next boundary starts
            p_currentIndex--;

            return _mimePart;
        }

        private bool AreArrayPartsForTextEqual(byte[] p_firstArray, int p_firstOffset, byte[] p_secondArray, int p_secondOffset, int p_length)
        {
            var _result = false;

            // Check array boundaries
            if ((p_firstOffset + p_length) <= p_firstArray.Length && (p_secondOffset + p_length) <= p_secondArray.Length)
            {
                _result = true;

                // Run through the arrays and compare byte-by-byte
                for (int i = 0; i < p_length; i++)
                {
                    char c1 = Char.ToLower((char)p_firstArray[p_firstOffset + i]);
                    char c2 = Char.ToLower((char)p_secondArray[p_secondOffset + i]);
                    if (c1 != c2)
                    {
                        _result = false;
                        break;
                    }
                }
            }

            return _result;
        }

        private int GetLengthToCRLF(byte[] p_arrary, int p_offset)
        {
            int _result = 0;

            for (int i = p_offset; i < p_arrary.Length; i++)
            {
                if (p_arrary[i] == 13 && p_arrary[i + 1] == 10)
                    break;

                _result++;
            }

            return _result;
        }
    }
}