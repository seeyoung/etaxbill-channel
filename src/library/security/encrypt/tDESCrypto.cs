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

using System.Security.Cryptography;

namespace OpenETaxBill.Channel.Library.Security.Encrypt
{
    public class tDESCrypto
    {
        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        public tDESCrypto()
        {
            // TripleDESCryptoServiceProvider 는 기본이 CBC 모드이며, 자동으로 랜덤 대칭키와 초기벡터를 생성한다.
            // 따라서, 추가적인 설정없이 바로 암호화, 복호화하는데 사용하면 된다.
            m_tripleCryptor = new TripleDESCryptoServiceProvider();
            //tDes.Padding = PaddingMode.None;
        }

        public tDESCrypto(byte[] p_key, byte[] p_iv)
        {
            m_tripleCryptor = new TripleDESCryptoServiceProvider
            {
                Key = p_key,
                IV = p_iv
            };
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        private readonly TripleDESCryptoServiceProvider m_tripleCryptor = null;

        public byte[] Key
        {
            get
            {
                return m_tripleCryptor.Key;
            }
        }

        public byte[] IV
        {
            get
            {
                return m_tripleCryptor.IV;
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
        
        /// <summary>
        /// 3-DES 알고리즘을 이용하여 주어진 데이터를 encrypt 한다.
        /// </summary>
        /// <param name="plainData">원본 데이터</param>
        /// <returns></returns>
        public byte[] Encrypt(byte[] p_plainData)
        {
            ICryptoTransform _icrypto = m_tripleCryptor.CreateEncryptor();
            return _icrypto.TransformFinalBlock(p_plainData, 0, p_plainData.Length);              
        }

        /// <summary>
        /// 3-DES 알고리즘을 이용하여 주어진 데이터를 decrypt 한다.
        /// </summary>
        /// <param name="encryptedData">암호화된 데이터</param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] p_encrypted)
        {
            ICryptoTransform _icrypto = m_tripleCryptor.CreateDecryptor();
            return _icrypto.TransformFinalBlock(p_encrypted, 0, p_encrypted.Length);      
        }

        //-------------------------------------------------------------------------------------------------------------------------//
        // 
        //-------------------------------------------------------------------------------------------------------------------------//
    }
}
