using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AppFitnessTrackerReal.sec
{
    internal class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
        public static bool VerifyPassword(string password,string storedHash)
        {
            string[] parts = storedHash.Split(':');

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hash = Convert.FromBase64String(parts[1]);

            byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);
            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
    }
}
