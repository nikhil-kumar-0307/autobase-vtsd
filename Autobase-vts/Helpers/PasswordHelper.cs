using System;
using System.Security.Cryptography;
using System.Text;

namespace autobase.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            string hash = HashPassword(password);
            return string.Equals(hash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
