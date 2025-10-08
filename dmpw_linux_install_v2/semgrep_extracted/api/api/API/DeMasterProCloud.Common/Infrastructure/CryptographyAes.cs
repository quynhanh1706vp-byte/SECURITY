using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    public class CryptographyAes
    {
        public static byte[] EncryptStringToBytes(byte[] input, string textHexKey, string textHexIv)
        {
            try
            {
                byte[] encrypted;
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 128;
                    aesAlg.BlockSize = 128;

                    aesAlg.Key = StringToByteArray(textHexKey);

                    aesAlg.IV = StringToByteArray(textHexIv);

                    var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption.
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(input, 0, input.Length);

                            //using (var swEncrypt = new StreamWriter(csEncrypt))
                            //{
                            //    //Write all data to the stream.
                            //    swEncrypt.Write(input);
                            //}
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }

                return encrypted;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyAes));
                logger.LogError(ex, "Error in EncryptStringToBytes");
                return null;
            }
        }

        public static byte[] EncryptStringToBytes(string hexText, string textHexKey, string textHexIv)
        {
            try
            {
                byte[] encrypted;
                // byte[] iv;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 128;
                    aesAlg.BlockSize = 128;

                    aesAlg.Key = StringToByteArray(textHexKey);
                    aesAlg.IV = StringToByteArray(textHexIv);
                    // iv = aesAlg.IV;

                    var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption.
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                //Write all data to the stream.
                                swEncrypt.Write(StringToByteArray(hexText));
                            }

                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }

                return encrypted;
                // byte[] combinedIvCt = new byte[iv.Length + encrypted.Length];
                // Array.Copy(iv, 0, combinedIvCt, 0, iv.Length);
                // Array.Copy(encrypted, 0, combinedIvCt, iv.Length, encrypted.Length);
                //
                // // Return the encrypted bytes from the memory stream.
                // return combinedIvCt;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyAes));
                logger.LogError(ex, "Error in EncryptStringToBytes");
                return null;
            }
        }

        public static string DecryptStringFromBytes_Aes(byte[] cipherTextCombined, byte[] key)
        {
            try
            {
                // Declare the string used to hold
                // the decrypted text.
                string plaintext = null;

                // Create an Aes object
                // with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;

                    byte[] iv = new byte[aesAlg.BlockSize / 8];
                    byte[] cipherText = new byte[cipherTextCombined.Length - iv.Length];

                    Array.Copy(cipherTextCombined, iv, iv.Length);
                    Array.Copy(cipherTextCombined, iv.Length, cipherText, 0, cipherText.Length);

                    aesAlg.IV = iv;

                    aesAlg.Mode = CipherMode.CBC;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

                return plaintext;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyAes));
                logger.LogError(ex, "Error in DecryptStringFromBytes_Aes");
                return null;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            try
            {
                StringBuilder hex = new StringBuilder(ba.Length * 2);
                foreach (byte b in ba)
                    hex.AppendFormat("{0:x2}", b);
                return hex.ToString();
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyAes));
                logger.LogError(ex, "Error in ByteArrayToString");
                return null;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            try
            {
                int numberChars = hex.Length;
                byte[] bytes = new byte[numberChars / 2];
                for (int i = 0; i < numberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyAes));
                logger.LogError(ex, "Error in StringToByteArray");
                return null;
            }
        }
    }
}