using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    public static class CryptographyHelper
    {
        public static string GetMD5Hash(string text)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();

                //compute hash from the bytes of text
                md5.ComputeHash(Encoding.ASCII.GetBytes(text));

                //get hash result after compute it
                byte[] result = md5.Hash;

                StringBuilder strBuilder = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    //change it into 2 hexadecimal digits
                    //for each byte
                    strBuilder.Append(result[i].ToString("x2"));
                }

                return strBuilder.ToString();
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyHelper));
                logger.LogError(ex, "Error in GetMD5Hash");
                return null;
            }
        }
        
        public static bool VerifyMD5Hash(string input, string hash)
        {
            try
            {
                // Hash the input.
                string hashOfInput = GetMD5Hash(input);

                // Create a StringComparer an compare the hashes.
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                return 0 == comparer.Compare(hashOfInput, hash);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyHelper));
                logger.LogError(ex, "Error in VerifyMD5Hash");
                return false;
            }
        }

        public static string GetHmacH256(string text, string secretKey)
        {
            try
            {
                // Convert the key and message into byte arrays
                var keyBytes = Encoding.UTF8.GetBytes(secretKey);
                var messageBytes = Encoding.UTF8.GetBytes(text);

                // Use HMACSHA256 class to compute the HMAC
                using (var hmacsha256 = new HMACSHA256(keyBytes))
                {
                    var hashBytes = hmacsha256.ComputeHash(messageBytes);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyHelper));
                logger.LogError(ex, "Error in GetHmacH256");
                return null;
            }
        }
        
        public static bool VerifyHmacSha256Hash(string key, string message, string expectedHmac)
        {
            try
            {
                string computedHmac = GetHmacH256(message, key);

                // Securely compare the two HMACs
                return AreEqual(computedHmac, expectedHmac);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(CryptographyHelper));
                logger.LogError(ex, "Error in VerifyHmacSha256Hash");
                return false;
            }
        }

        private static bool AreEqual(string a, string b)
        {
            // Ensure constant-time comparison to avoid timing attacks
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}