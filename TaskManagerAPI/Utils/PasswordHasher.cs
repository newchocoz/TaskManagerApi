using System.Security.Cryptography;
using System.Text;

namespace TaskManagerAPI.Utils
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;   // 128 bit
        private const int KeySize = 32;    // 256 bit
        private const int Iterations = 100_000; // recommended >=100k
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string HashPassword(string password)
        {
            // generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // derive key
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            // store as: {iterations}.{salt}.{hash}
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] hash = Convert.FromBase64String(parts[2]);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                Algorithm,
                hash.Length
            );

            // compare securely
            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }
    }
}
