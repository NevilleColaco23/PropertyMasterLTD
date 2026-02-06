using System.Security.Cryptography;

namespace MyWarehouse.WebApi
{
    public static class Helper
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Generate8CharCode()
        {
            Span<char> chars = stackalloc char[8];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            }
            return new string(chars);
        }
    }
}
