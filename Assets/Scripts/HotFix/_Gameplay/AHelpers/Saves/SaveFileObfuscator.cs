using System;
using System.Text;

namespace MoreMountains
{
    public class SaveFileObfuscator
    {
        public static string key = "key";

        public static string encode(string s, string key)
        {
            var bytesContent = Encoding.UTF8.GetBytes(s);
            var bytesKey = Encoding.UTF8.GetBytes(key);
            return base64Encode(xorWithKey(bytesContent, bytesKey));
        }

        public static string decode(string s, string key)
        {
            var bytesKey = Encoding.UTF8.GetBytes(key);
            return Encoding.UTF8.GetString(xorWithKey(base64Decode(s), bytesKey));
        }

        private static byte[] xorWithKey(byte[] a, byte[] key)
        {
            byte[] result = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = (byte)(a[i] ^ key[i % key.Length]);
            return result;
        }

        private static byte[] base64Decode(string s)
        {
            //先将字符串转换为 UTF-8 编码的字节数组
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(s);

            //将字节数组转换为 Base64 编码的字符串
            string base64String = Convert.ToBase64String(utf8Bytes);

            //将 Base64 字符串转换为字节数组
            byte[] base64Bytes = Encoding.UTF8.GetBytes(base64String);

            return base64Bytes;
        }

        private static string base64Encode(byte[] bytes)
        {
            //Base64 字节数组转换回 Base64 字符串
            string base64StringDecoded = Encoding.UTF8.GetString(bytes);

            //Base64 字符串转换回原始字节数组
            byte[] originalBytes = Convert.FromBase64String(base64StringDecoded);

            //将原始字节数组转换回原字符串
            string decodedString = Encoding.UTF8.GetString(originalBytes);

            return decodedString;
        }

        public static bool isObfuscated(string data)
        {
            return !data.Contains("{");
        }
    }
}