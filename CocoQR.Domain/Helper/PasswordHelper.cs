using System.Security.Cryptography;
using System.Text;

namespace CocoQR.Domain.Helper
{
    public class PasswordHelper
    {
        public static string HashPasswordThrice(string password)
        {
            var firstHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var secondHash = SHA256.HashData(firstHash);
            var thirdHash = SHA256.HashData(secondHash);
            return Convert.ToBase64String(thirdHash);
        }
    }
}
