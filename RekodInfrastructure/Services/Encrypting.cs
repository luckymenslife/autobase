using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rekod.Services
{
    public static class Encrypting
    {
        public static string Encrypt(byte[] data, string key)
        {
            SymmetricAlgorithm sa = Rijndael.Create();
            ICryptoTransform ct = sa.CreateEncryptor(
                (new PasswordDeriveBytes(key, null)).GetBytes(16),
                new byte[16]);

            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);

            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }
        public static string Encrypt(string data, string key)
        {
            return Encrypt(Encoding.UTF8.GetBytes(data), key);
        }

        public static string Decrypt(string data, string key)
        {
            CryptoStream cs = InternalDecrypt(Convert.FromBase64String(data), key);
            StreamReader sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        public static string TryDecrypt(string data, string key)
        {
            try
            {
                CryptoStream cs = InternalDecrypt(Convert.FromBase64String(data), key);
                StreamReader sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return data;
            }
        }
        
        private static CryptoStream InternalDecrypt(byte[] data, string key)
        {
            SymmetricAlgorithm sa = Rijndael.Create();
            ICryptoTransform ct = sa.CreateDecryptor(
                (new PasswordDeriveBytes(key, null)).GetBytes(16),
                new byte[16]);

            MemoryStream ms = new MemoryStream(data);
            return new CryptoStream(ms, ct, CryptoStreamMode.Read);
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
