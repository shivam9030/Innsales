using System.Security.Cryptography;
using System.Text;

namespace InnSales.Services
{
    public static class TokenHasher
    {
        // Hash a token using SHA256
        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // Verify a token against a hashed value
        public static bool VerifyToken(string token, string hashedToken)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(hashedToken))
                return false;

            return HashToken(token) == hashedToken;
        }
    }
}
