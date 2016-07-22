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

using System.IO;
using System.Text;
using OpenETaxBill.Channel.Library.Utility;

namespace OpenETaxBill.Channel.Library.Security.Mime
{
    public class MimePart
    {
        public string ContentType
        {
            get; set;
        }
        public string TransferEncoding
        {
            get; set;
        }
        public string ContentId
        {
            get; set;
        }
        public string CharSet
        {
            get; set;
        }

        private byte[] m_content;
        public byte[] Content
        {
            get
            {
                return m_content;
            }
            set
            {
                m_content = value;
            }
        }

        public string GetContentAsString()
        {
            var _result = "";

            using (StreamReader _sr = new StreamReader(GetContentAsStream(), Encoding.UTF8))
            {
                _result = _sr.ReadToEnd();
            }

            return _result;
        }

        public MemoryStream GetContentAsStream()
        {
            if (this.Content != null)
                return new MemoryStream(this.Content);
            else
                throw new ProxyException("Content is not initialized, no message-part loaded or de-serialized!");
        }
    }
}