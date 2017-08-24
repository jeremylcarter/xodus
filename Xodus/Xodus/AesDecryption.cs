using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Xodus
{
    public class AesEnDecryption
    {
        private string AES_IV = "15CV1/ZOnVI3rY4wk4INBg==";

        // Key with 256 and IV with 16 length
        private readonly string AES_Key = "cXdlcnR5dWlvcGFzZGZnaGprbHp4YzEyMzQ1Njc4OTA=";

        private readonly IBuffer m_iv;
        private readonly CryptographicKey m_key;

        public AesEnDecryption()
        {
            var aes_iv = new byte[16];

            for (var i = 0; i < aes_iv.Length; i++)
                aes_iv[i] = Convert.ToByte('\0');

            var key = Convert.FromBase64String(AES_Key).AsBuffer();
            m_iv = aes_iv.AsBuffer();
            var provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            m_key = provider.CreateSymmetricKey(key);
        }

        public byte[] Encrypt(byte[] input)
        {
            var bufferMsg =
                CryptographicBuffer.ConvertStringToBinary(Encoding.ASCII.GetString(input), BinaryStringEncoding.Utf8);
            var bufferEncrypt = CryptographicEngine.Encrypt(m_key, bufferMsg, m_iv);
            return bufferEncrypt.ToArray();
        }

        public byte[] Decrypt(byte[] input)
        {
            var bufferDecrypt = CryptographicEngine.Decrypt(m_key, input.AsBuffer(), m_iv);
            return bufferDecrypt.ToArray();
        }
    }
}