using System;
using System.Security.Cryptography;
using System.Text;


namespace photoCon.Services
{
    public class EncryptionService
    {
        public string Encrypt(string data, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] encryptedData;

                using (var ms = new System.IO.MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new System.IO.StreamWriter(cs))
                        {
                            sw.Write(data);
                        }
                    }
                    encryptedData = ms.ToArray();
                }

                var ivAndEncryptedData = new byte[aesAlg.IV.Length + encryptedData.Length];
                Array.Copy(aesAlg.IV, ivAndEncryptedData, aesAlg.IV.Length);
                Array.Copy(encryptedData, 0, ivAndEncryptedData, aesAlg.IV.Length, encryptedData.Length);

                return Convert.ToBase64String(ivAndEncryptedData);
            }
        }

        public string Decrypt(string ciphertext, string key)
        {
            try
            {
                // Convert the Base64-encoded ciphertext to bytes
                byte[] ivAndEncryptedData = Convert.FromBase64String(ciphertext);

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
                    using (var ms = new System.IO.MemoryStream())
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
