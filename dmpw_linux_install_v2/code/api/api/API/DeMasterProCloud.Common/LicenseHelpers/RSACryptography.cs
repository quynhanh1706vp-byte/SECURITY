using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.Common.LicenseHelpers
{
    public class RSACryptography
    {
        public static string GetDeMasterProKey()
        {
            string key = "";

            try
            {
                key = File.ReadAllText(Constants.VerifyLicenseSetting.FileNameDemasterProKey);
                key = key.Replace(Constants.VerifyLicenseSetting.TextBeginDemasterProKey, "");
                key = key.Replace(Constants.VerifyLicenseSetting.TextEndDemasterProKey, "");
                key = key.Trim();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return key;
        }

        public static List<byte[]> SplitDataDecryptByKeySize(List<byte> dataEncrypt, int keySize = Constants.VerifyLicenseSetting.KeySize / 8)
        {
            List<byte[]> result = new List<byte[]>();
            int count = dataEncrypt.Count / keySize;
            count = dataEncrypt.Count == keySize * count ? count : count + 1;
            for (int i = 0; i < count; i++)
            {
                if (i == count - 1)
                {
                    result.Add(dataEncrypt.GetRange(i * keySize, dataEncrypt.Count - i * keySize).ToArray());
                }
                else
                {
                    result.Add(dataEncrypt.GetRange(i * keySize, keySize).ToArray());
                }
            }

            return result;
        }

        public static byte[] DecryptData(byte[] data, string key)
        {
            try
            {
                RSAParameters rsaKey = ConvertStringToRsaKey(key);
                return RSADecrypt(data, rsaKey, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return null;
            }
        }
        
        private static RSAParameters ConvertStringToRsaKey(string stringKey)
        {
            //get a stream from the string
            var sr = new StringReader(stringKey.Base64Decode());
            //we need a deserializer
            var xs = new XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            return (RSAParameters)xs.Deserialize(sr);
        }
        
        private static byte[] RSADecrypt(byte[] data, RSAParameters rsaKeyInfo, bool doOaepPadding = false)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(Constants.VerifyLicenseSetting.KeySize))
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    rsa.ImportParameters(rsaKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = rsa.Decrypt(data, doOaepPadding);
                }

                return decryptedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return null;
            }
        }
    }
}