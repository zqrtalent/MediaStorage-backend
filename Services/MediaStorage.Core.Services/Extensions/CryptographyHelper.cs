using System;
using System.Security.Cryptography;
using System.Text;

namespace MediaStorage.Common.Extensions
{
    public static class CryptographyHelper
    {
        public static string ComputeHashSHA1(string computeString)
        {
            StringBuilder sb = null;
            using(var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(System.Text.UTF8Encoding.UTF8.GetBytes(computeString));
                sb = new StringBuilder(hash.Length*2 + 1);
                var btSingleArray = new byte[1];
                foreach(var b in hash)
                {
                    btSingleArray[0] = b; 
                    sb.Append(BitConverter.ToString(btSingleArray));
                }
            }

            return sb?.ToString() ?? string.Empty;
        }
    }
}
