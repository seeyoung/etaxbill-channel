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
using System.Text;

namespace OpenETaxBill.Channel.Library.Security.Encrypt
{
    public class Encryptor : tDESCrypto
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        
        /// <summary>
        /// 
        /// </summary>
        private Encryptor(byte[] PrivateKey, byte[] InitVector)
            : base(PrivateKey, InitVector)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly static Lazy<Encryptor> m_encryptor = new Lazy<Encryptor>(() =>
        {
            byte[] _initVector = OpenETaxBill.Channel.Properties.Resources.initVector;
            byte[] _privateKey = OpenETaxBill.Channel.Properties.Resources.privateKey;

            return new Encryptor(_privateKey, _initVector);
        });

        /// <summary></summary>
        public static Encryptor SNG
        {
            get
            {
                return m_encryptor.Value;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 평문 Base64 문자열을 암호화된 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_plainString"></param>
        /// <returns></returns>
        public string PlainBase64ToChiperBase64(string p_plainString)
        {
            return Convert.ToBase64String(base.Encrypt(Convert.FromBase64String(p_plainString)));
        }
        
        /// <summary>
        /// 암호화된 Base64 문자열을 평문 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_chiperString"></param>
        /// <returns></returns>
        public string ChiperBase64ToPlainBase64(string p_chiperString)
        {
            return Convert.ToBase64String(base.Decrypt(Convert.FromBase64String(p_chiperString)));
        }

        /// <summary>
        /// 평문 바이트 배열을 암호화된 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_plainBytes"></param>
        /// <returns></returns>
        public string PlainBytesToChiperBase64(byte[] p_plainBytes)
        {
            return Convert.ToBase64String(base.Encrypt(p_plainBytes));
        }

        /// <summary>
        /// 암호화된 Base64 문자열을 평문 바이트 배열로 변환 합니다.
        /// </summary>
        /// <param name="p_chiperString"></param>
        /// <returns></returns>
        public byte[] ChiperBase64ToPlainBytes(string p_chiperString)
        {
            return base.Decrypt(Convert.FromBase64String(p_chiperString));
        }

        /// <summary>
        /// 평문 문자열을 암호화된 Base64 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_plainString"></param>
        /// <returns></returns>
        public string PlainStringToChiperBase64(string p_plainString)
        {
            return Convert.ToBase64String(base.Encrypt(Encoding.UTF8.GetBytes(p_plainString)));
        }
           
        /// <summary>
        /// 암호화된 Base64 문자열을 평문 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_chiperString"></param>
        /// <returns></returns>
        public string ChiperBase64ToPlainString(string p_chiperString)
        {
            return Encoding.UTF8.GetString(base.Decrypt(Convert.FromBase64String(p_chiperString)));
        }

        /// <summary>
        /// 평문 문자열을 암호화된 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_plainString"></param>
        /// <returns></returns>
        public string PlainStringToChiperString(string p_plainString)
        {
            return Encoding.UTF8.GetString(base.Encrypt(Encoding.UTF8.GetBytes(p_plainString)));
        }

        /// <summary>
        /// 암호화된 문자열을 평문 문자열로 변환 합니다.
        /// </summary>
        /// <param name="p_chiperString"></param>
        /// <returns></returns>
        public string ChiperStringToPlainString(string p_chiperString)
        {
            return Encoding.UTF8.GetString(base.Decrypt(Encoding.UTF8.GetBytes(p_chiperString)));
        }
        
        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}
