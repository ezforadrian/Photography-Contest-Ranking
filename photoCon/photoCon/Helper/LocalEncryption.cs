using System.Security.Cryptography;
using System.Text;

namespace photoCon.Helper
{
    public class LocalEncryption
    {

        // Method to generate a CSRF token
        public string GenerateCSRFToken()
        {
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }


        public string ANEncrypt(string data, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] encryptedData;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(data);
                        }
                    }
                    encryptedData = ms.ToArray();
                }

                var ivAndEncryptedData = new byte[aesAlg.IV.Length + encryptedData.Length];
                Array.Copy(aesAlg.IV, ivAndEncryptedData, aesAlg.IV.Length);
                Array.Copy(encryptedData, 0, ivAndEncryptedData, aesAlg.IV.Length, encryptedData.Length);

                // Convert the byte array to a hexadecimal string
                return BitConverter.ToString(ivAndEncryptedData).Replace("-", "");
            }
        }

        public string ANDecrypt(string ciphertext, string key)
        {
            try
            {
                // Convert the hexadecimal string to a byte array
                byte[] ivAndEncryptedData = new byte[ciphertext.Length / 2];
                for (int i = 0; i < ciphertext.Length; i += 2)
                {
                    ivAndEncryptedData[i / 2] = Convert.ToByte(ciphertext.Substring(i, 2), 16);
                }

                // Extract the IV from the data
                byte[] iv = new byte[16]; // The IV size is typically 16 bytes for AES
                Array.Copy(ivAndEncryptedData, iv, iv.Length);

                // Create an AES instance with the same key and IV
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Decrypt the data
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(ivAndEncryptedData, iv.Length, ivAndEncryptedData.Length - iv.Length);
                        }

                        byte[] decryptedData = ms.ToArray();

                        // Convert the decrypted bytes to a string (assuming it's text data)
                        return Encoding.UTF8.GetString(decryptedData);
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Decryption failed: " + ex.Message);
            }
        }

    }
}
