using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Common.Infrastructure
{
    /// <summary>
    /// Encrypt data using .NET Cryptography Framework.
    /// </summary>
    public class Encryptor
    {
        readonly UTF8Encoding _enc;
        readonly Aes _rcipher;
        readonly byte[] _key, _iv;
        byte[] _pwd, _ivBytes;

        /***
         * Encryption mode enumeration
         */
        public enum EncryptMode
        {
            Encrypt,
            Decrypt
        };

        static readonly char[] CharacterMatrixForRandomIvStringGeneration =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
        };

        public Encryptor()
        {
            _enc = new UTF8Encoding();
            // SECURITY: Replaced RijndaelManaged with AES for better security
            _rcipher = Aes.Create();
            _rcipher.Mode = CipherMode.CBC;
            _rcipher.Padding = PaddingMode.PKCS7;
            _rcipher.KeySize = 256;
            _rcipher.BlockSize = 128;
            _key = new byte[32];
            _iv = new byte[_rcipher.BlockSize / 8]; //128 bit / 8 = 16 bytes
            _ivBytes = new byte[16];
        }

        #region Hashing a string using Microsoft.AspNetCore.Cryptography.KeyDerivation allows us to use PBKDF2 which is far harder to brute force.

        public static string Encrypt(string plainText, string encryptKey)
        {
            try
            {
                var iv = GetHashSha256(encryptKey, 16); //16 bytes = 128 bits
                var key = GetHashSha256(encryptKey, 32); //32 bytes = 256 bits
                return new Encryptor().Encrypt(plainText, key, iv);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in Encrypt");
                return null;
            }
        }

        public static string Decrypt(string plainText, string encryptKey)
        {
            try
            {
                var iv = GetHashSha256(encryptKey, 16); //16 bytes = 128 bits
                var key = GetHashSha256(encryptKey, 32); //32 bytes = 256 bits
                return new Encryptor().Decrypt(plainText, key, iv);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in Decrypt");
                return null;
            }
        }

        /**
         * This function generates random string of the given input length.
         * 
         * @param _plainText
         *            Plain text to be encrypted
         * @param _key
         *            Encryption Key. You'll have to use the same key for decryption
         * @return returns encrypted (cipher) text
         */
        public static string GenerateRandomIv(int length)
        {
            try
            {
                var iv = new char[length];
                var randomBytes = new byte[length];

                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(
                        randomBytes); //Fills an array of bytes with a cryptographically strong sequence of random values.
                }

                for (int i = 0; i < iv.Length; i++)
                {
                    var ptr = randomBytes[i] % CharacterMatrixForRandomIvStringGeneration.Length;
                    iv[i] = CharacterMatrixForRandomIvStringGeneration[ptr];
                }

                return new string(iv);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in GenerateRandomIv");
                return null;
            }
        }

        /**
         * 
         * @param _inputText
         *            Text to be encrypted or decrypted
         * @param _encryptionKey
         *            Encryption key to used for encryption / decryption
         * @param _mode
         *            specify the mode encryption / decryption
         * @param _initVector
         *               initialization vector
         * @return encrypted or decrypted string based on the mode
         */
        public string EncryptDecrypt(string inputText, string encryptionKey, EncryptMode mode, string initVector)
        {
            try
            {
                var _out = ""; // output string
                //_encryptionKey = MD5Hash (_encryptionKey);
                _pwd = Encoding.UTF8.GetBytes(encryptionKey);
                _ivBytes = Encoding.UTF8.GetBytes(initVector);

                var len = _pwd.Length;
                if (len > _key.Length)
                {
                    len = _key.Length;
                }

                var ivLenth = _ivBytes.Length;
                if (ivLenth > _iv.Length)
                {
                    ivLenth = _iv.Length;
                }

                Array.Copy(_pwd, _key, len);
                Array.Copy(_ivBytes, _iv, ivLenth);
                _rcipher.Key = _key;
                _rcipher.IV = _iv;

                if (mode.Equals(EncryptMode.Encrypt))
                {
                    //encrypt
                    var plainText = _rcipher.CreateEncryptor()
                        .TransformFinalBlock(_enc.GetBytes(inputText), 0, inputText.Length);
                    _out = Convert.ToBase64String(plainText);
                }

                if (mode.Equals(EncryptMode.Decrypt))
                {
                    //decrypt
                    var plainText = _rcipher.CreateDecryptor().TransformFinalBlock(Convert.FromBase64String(inputText),
                        0, Convert.FromBase64String(inputText).Length);
                    _out = _enc.GetString(plainText);
                }

                _rcipher.Dispose();
                return _out; // return encrypted/decrypted string
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in EncryptDecrypt");
                return null;
            }
        }

        /**
         * This function encrypts the plain text to cipher text using the key
         * provided. You'll have to use the same key for decryption
         * 
         * @param _plainText
         *            Plain text to be encrypted
         * @param _key
         *            Encryption Key. You'll have to use the same key for decryption
         * @return returns encrypted (cipher) text
         */
        public string Encrypt(string plainText, string key, string initVector)
        {
            try
            {
                return EncryptDecrypt(plainText, key, EncryptMode.Encrypt, initVector);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in Encrypt");
                return null;
            }
        }

        /***
         * This funtion decrypts the encrypted text to plain text using the key
         * provided. You'll have to use the same key which you used during
         * encryprtion
         * 
         * @param _encryptedText
         *            Encrypted/Cipher text to be decrypted
         * @param _key
         *            Encryption key which you used during encryption
         * @return encrypted value
         */

        public string Decrypt(string encryptedText, string key, string initVector)
        {
            try
            {
                return EncryptDecrypt(encryptedText, key, EncryptMode.Decrypt, initVector);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in Decrypt");
                return null;
            }
        }

        /***
         * This function decrypts the encrypted text to plain text using the key
         * provided. You'll have to use the same key which you used during
         * encryption
         * 
         * @param _encryptedText
         *            Encrypted/Cipher text to be decrypted
         * @param _key
         *            Encryption key which you used during encryption
         */
        public static string GetHashSha256(string text, int length)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                var hashstring = new SHA256Managed();
                var hash = hashstring.ComputeHash(bytes);
                var hashString = hash.Aggregate(string.Empty, (current, x) => current + $"{x:x2}");

                if (length > hashString.Length)
                    return hashString;
                return hashString.Substring(0, length);
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger(typeof(Encryptor));
                logger.LogError(ex, "Error in GetHashSha256");
                return null;
            }
        }
    }
    #endregion
}